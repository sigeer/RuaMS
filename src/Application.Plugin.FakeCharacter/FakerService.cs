using Application.Core.Channel;
using Application.Core.Game.Commands;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Gameplay.Plugins;
using Application.Plugin.FakeCharacter.Commands;
using Application.Shared.MapObjects;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Application.Plugin.FakeCharacter
{
    internal class FakerService : IPluginLifeService, IPluginMapObjectService
    {
        Dictionary<int, FakePlayer> _dataSource = new();
        HashSet<CommandBase> _commands = [];

        public async ValueTask DisposeAsync()
        {
            var ds = _dataSource.Values.ToArray();
            _dataSource.Clear();
            foreach (var item in ds)
            {
                await item.DisposeAsync();
            }
        }

        public async Task Summon(Player chr, int idx)
        {
            var fakeId = FakePlayer.GetFakePlayerId(chr, idx);
            if (_dataSource.TryGetValue(fakeId, out var fakeChr))
            {
                await fakeChr.Follow(chr);
            }
            else
            {
                fakeChr = new FakePlayer(chr, chr.MapModel, chr.getPosition(), idx);
                await chr.MapModel.addPlayer(fakeChr);

                await fakeChr.BroadcastIdle();
                _dataSource[fakeId] = fakeChr;
            }
        }

        public async Task Remove(Player chr, int idx)
        {
            var fakeId = FakePlayer.GetFakePlayerId(chr, idx);
            if (_dataSource.Remove(fakeId, out var fakeChr))
            {
                await chr.MapModel.removePlayer(fakeChr);
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

        public async Task OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            if (mapObject is Player chr && chr is not FakePlayer && chr.isLeader())
            {
                foreach (var fakeChr in GetPlayerFakePlayers(chr))
                {
                    fakeChr.setPosition(chr.getPosition());
                    await map.addPlayer(fakeChr);

                    // addPlayer 使用 enteringField=true，spawn 包会硬编码 stance=6
                    // 这里再广播一次 idle 包，让其他玩家看到正确的姿态
                    // （尤其当主人在绳子上 stance=6/7，或静止状态 stance=0/1 时）
                    await fakeChr.BroadcastIdle();
                }
            }
        }

        public async Task OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            if (mapObject is Player chr && chr is not FakePlayer && chr.isLeader())
            {
                foreach (var fakeChr in GetPlayerFakePlayers(chr))
                {
                    await map.removePlayer(fakeChr);
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
