using Application.Core.Channel;
using Application.Core.Game.Commands;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Gameplay.Plugins;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.FakeCharacter.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Application.Plugin.FakeCharacter
{
    internal class FakerService : PluginServiceBase, IPluginMapObjectService
    {
        /// <summary>
        /// Channel, PlayerId, Idx, FakePlayer
        /// </summary>
        ConcurrentDictionary<AbstractEventInstanceManager, Dictionary<int, FakePlayer>> _dataSource = new();
        HashSet<CommandBase> _commands = [];

        public FakerService(WorldChannelServer node, string pluginName) : base(node, pluginName)
        {
        }

        public override async ValueTask DisposeAsync()
        {
            // 清理所有假人
            var ds = _dataSource.ToArray();
            _dataSource.Clear();
            foreach (var kw in ds)
            {
                foreach (var id in kw.Value.Keys)
                {
                    await Remove(kw.Key, id);
                    kw.Key.OnDispose -= OnEventDisposed;
                }
            }


            // 清理当前插件的命令
            var commandCenter = _node.ServiceProvider.GetRequiredService<CommandExecutor>();
            foreach (var item in _commands)
            {
                commandCenter.TryRemoveCommand(item);
            }

            _commands.Clear();
        }

        public async Task Summon(Player chr, int idx)
        {
            var eim = chr.getEventInstance();
            if (eim == null)
            {
                return;
            }

            bool shouldCreate = false;
            FakePlayer? fakeChr = null;
            if (_dataSource.TryGetValue(eim, out var eimData))
            {
                if (eimData.TryGetValue(idx, out fakeChr))
                {
                    await fakeChr.Move(chr.getPosition());
                    //var followed = await fakeChr.Follow(chr);
                    //if (!followed)
                    //{
                    //    // 销毁重建
                    //    await Remove(eim, idx);

                    //    shouldCreate = true;
                    //}
                }
                else
                {
                    shouldCreate = true;
                }
            }
            else
            {
                _dataSource[eim] = [];
                shouldCreate = true;
            }

            if (shouldCreate)
            {
                fakeChr = new FakePlayer(chr, chr.MapModel, chr.getPosition(), idx);
                await chr.MapModel.addPlayer(fakeChr);
                await fakeChr.BroadcastIdle();
                _dataSource[eim][idx] = fakeChr;
            }

            if (fakeChr != null)
            {
                await eim.registerPlayer(fakeChr);
                eim.OnDispose += OnEventDisposed;
            }
        }

        void OnEventDisposed(object? sender, EventArgs arg)
        {
            // eim释放，所有玩家都要离开地图，地图也要释放，不需要再调用MapModel.removePlayer
            _dataSource.TryRemove(sender as AbstractEventInstanceManager, out _);
        }

        public async Task Remove(AbstractEventInstanceManager eim, int idx)
        {
            if (_dataSource.GetValueOrDefault(eim, []).Remove(idx, out var fakeChr))
            {
                await fakeChr.MapModel.removePlayer(fakeChr);
                await eim.unregisterPlayer(fakeChr);
                await fakeChr.DisposeAsync();
            }
        }

        public IEnumerable<FakePlayer> GetEventFakePlayers(AbstractEventInstanceManager eim)
        {
            return _dataSource.GetValueOrDefault(eim, []).Values.ToArray();
        }

        public async Task OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            if (mapObject is Player chr && chr is not FakePlayer && chr.isLeader())
            {
                var eim = chr.getEventInstance();
                if (eim == null)
                    return;

                foreach (var fakeChr in GetEventFakePlayers(eim))
                {
                    fakeChr.setPosition(chr.getPosition());
                    await map.addPlayer(fakeChr);

                    // addPlayer 使用 enteringField=true，spawn 包会硬编码 stance=6
                    // 这里再广播一次 idle 包，让其他玩家看到正确的姿态
                    await fakeChr.BroadcastIdle();
                }
            }
        }

        public async Task OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            if (mapObject is Player chr && chr is not FakePlayer && chr.isLeader())
            {
                var eim = chr.getEventInstance();
                if (eim == null)
                    return;

                foreach (var fakeChr in GetEventFakePlayers(eim))
                {
                    await map.removePlayer(fakeChr);
                }
            }
        }

        public override Task OnMounted()
        {
            _commands.Add(new FakeCommand(this));
            var commandCenter = _node.ServiceProvider.GetRequiredService<CommandExecutor>();
            foreach (var item in _commands)
            {
                commandCenter.TryRegisterCommand(item);
            }
            return Task.CompletedTask;
        }
    }
}
