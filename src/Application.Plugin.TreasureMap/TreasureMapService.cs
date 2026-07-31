using Application.Core.Channel;
using Application.Core.Gameplay.Plugins;
using Application.Shared.Models;
using System.Reflection;

namespace Application.Plugin.TreasureMap
{
    public class TreasureMapService : PluginServiceBase, IScriptItemService, IScriptNpcService
    {

        Dictionary<string, (Type ObjType, MethodInfo Method)> _itemScripts;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _npcScripts;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ItemScripts => _itemScripts;

        public Dictionary<string, (Type ObjType, MethodInfo Method)> NpcScripts => _npcScripts;

        public TreasureMapService(WorldChannelServer node, string pluginName) : base(node, pluginName)
        {
            _itemScripts = TypeUtils.LoadFromType(typeof(ItemScript));
            _npcScripts = TypeUtils.LoadFromType(typeof(NpcScript));
        }

        public override async ValueTask DisposeAsync()
        {
            _itemScripts.Clear();
            _npcScripts.Clear();

            if (_node.Servers.TryGetValue(Settings.ActiveChannel, out var effectChannel))
            {
                await effectChannel.Send(async c =>
                {
                    c.RemoveScriptableNpc(1052103);
                    await c.FlushScriptableNpc();
                });
            }
        }

        public override async Task OnMounted()
        {
            if (_node.Servers.TryGetValue(Settings.ActiveChannel, out var effectChannel))
            {
                await effectChannel.Send(async c =>
                {
                    c.RegisterScriptableNpc(new ScriptableNpc(1052103, "getTreasureMap", "藏宝图任务"));
                    await c.FlushScriptableNpc();
                });
            }
            await base.OnMounted();
        }
    }
}
