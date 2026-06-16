using Application.Core.Channel;
using Application.Core.Game.Commands;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Gameplay.Plugins;
using Application.Plugin.FakeCharacter.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Plugin.FakeCharacter
{
    internal class FakerService: IPluginLifeService, IPluginMapService
    {
        Dictionary<int, FakePlayer> _dataSource = new();
        HashSet<CommandBase> _commands = [];

        public ValueTask DisposeAsync()
        {
            var ds = _dataSource.Values.ToArray();
            _dataSource.Clear();
            foreach (var item in ds)
            {
                item.Dispose();
            }
            return ValueTask.CompletedTask;
        }

        public void Summon(Player chr, int idx)
        {
            var fakeId = FakePlayer.GetFakePlayerId(chr, idx);
            if (_dataSource.TryGetValue(fakeId, out var fakeChr))
            {
                fakeChr.Follow(chr);
            }
            else
            {
                fakeChr = new FakePlayer(chr, chr.MapModel, chr.getPosition(), idx);
                chr.MapModel.addPlayer(fakeChr);

                fakeChr.BroadcastIdle();
                _dataSource[fakeId] = fakeChr;
            }
        }

        public void Remove(Player chr, int idx)
        {
            var fakeId = FakePlayer.GetFakePlayerId(chr, idx);
            if (_dataSource.Remove(fakeId, out var fakeChr))
            {
                chr.MapModel.removePlayer(fakeChr);
            }
        }

        public IEnumerable<FakePlayer> GetPlayerFakePlayers(Player chr)
        {
            var keys = _dataSource.Keys.Where(x => (x % 10000) == chr.Id).ToArray();
            foreach (var item in keys)
            {
                yield return _dataSource[item];
            }
        }

        public void OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            if (mapObject is Player chr && chr.isLeader())
            {
                foreach (var fakeChr in GetPlayerFakePlayers(chr))
                {
                    fakeChr.setPosition(chr.getPosition());
                    map.addPlayer(fakeChr);

                    // addPlayer 使用 enteringField=true，spawn 包会硬编码 stance=6
                    // 这里再广播一次 idle 包，让其他玩家看到正确的姿态
                    // （尤其当主人在绳子上 stance=6/7，或静止状态 stance=0/1 时）
                    fakeChr.BroadcastIdle();
                }
            }
        }

        public void OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            if (mapObject is Player chr && chr.isLeader())
            {
                foreach (var fakeChr in GetPlayerFakePlayers(chr))
                {
                    map.removePlayer(fakeChr);
                }
            }
        }

        public void OnMounted(WorldChannelServer node)
        {
            _commands.Add(new FakeCommand(this));
            foreach (var item in _commands)
            {
                node.ServiceProvider.GetRequiredService<CommandExecutor>().TryRegisterCommand(item);
            }
        }

        public void OnUnmounted(WorldChannelServer node)
        {
            foreach (var item in _commands)
            {
                node.ServiceProvider.GetRequiredService<CommandExecutor>().TryRemoveCommand(item);
            }

        }
    }
}
