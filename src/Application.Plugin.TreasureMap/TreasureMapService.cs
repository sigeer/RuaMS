using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Gameplay.Plugins;
using Application.Shared.Constants.Map;
using System.Reflection;
using tools;

namespace Application.Plugin.TreasureMap
{
    public class TreasureMapService : PluginServiceBase , IScriptItemService, IScriptNpcService, IPluginMapObjectService
    {

        Dictionary<string, (Type ObjType, MethodInfo Method)> _itemScripts;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _npcScripts;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ItemScripts => _itemScripts;

        public Dictionary<string, (Type ObjType, MethodInfo Method)> NpcScripts => _npcScripts;

        public TreasureMapService(WorldChannelServer node, string pluginName):base(node, pluginName)
        {
            _itemScripts = TypeUtils.LoadFromType(typeof(ItemScript));
            _npcScripts = TypeUtils.LoadFromType(typeof(NpcScript));
        }

        public async Task OnMapObjectEnterField(IMap map, IMapObject mapObject)
        {
            if (map.Id == MapId.KERNING_CITY && map.ChannelServer.Id == Settings.ActiveChannel && mapObject is Player chr)
            {
                await chr.SendPacket(PacketCreator.SetNPCScriptable([(1052103, "getTreasureMap")]));
            }
        }

        public Task OnMapObjectLeaveField(IMap map, IMapObject mapObject)
        {
            return Task.CompletedTask;
        }

        public override async ValueTask DisposeAsync()
        {
            _itemScripts.Clear();
            _npcScripts.Clear();

            if (_node.Servers.TryGetValue(Settings.ActiveChannel, out var effectChannel))
            {
                await effectChannel.Send(async c =>
                {
                    var mapObj = await c.getMapFactory().getMap(MapId.KERNING_CITY);
                    foreach (var chr in mapObj.getAllPlayers())
                    {
                        await chr.SendPacket(PacketCreator.SetNPCScriptable([(1052103, "")]));
                    }
                });
            }
        }

        public override async Task OnMounted()
        {
            if (_node.Servers.TryGetValue(Settings.ActiveChannel, out  var effectChannel))
            {
                await effectChannel.Send(async c =>
                {
                    var mapObj = await c.getMapFactory().getMap(MapId.KERNING_CITY);
                    foreach (var chr in mapObj.getAllPlayers())
                    {
                        await chr.SendPacket(PacketCreator.SetNPCScriptable([(1052103, "getTreasureMap")]));
                    }
                });
            }
            await base.OnMounted();
        }
    }
}
