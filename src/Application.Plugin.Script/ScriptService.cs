using Application.Core.Channel;
using Application.Core.Gameplay.Plugins;
using Application.Plugin.Script.Npc;
using Application.Plugin.Script.Quest;
using System.Reflection;

namespace Application.Plugin.Script
{
    internal class ScriptService : PluginServiceBase, IScriptNpcService,
        IScriptQuestService,
        IScriptItemService,
        IScriptPortalService,
        IScriptMapService,
        IScriptReactorService
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> _npcSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _itemSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _questSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _portalSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _mapEnterSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _mapFirstEnterSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _reactorHitSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _reactorActSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _reactorTouchSource;
        Dictionary<string, (Type ObjType, MethodInfo Method)> _reactorUntouchSource;

        public ScriptService(WorldChannelServer node, string pluginName):base(node, pluginName)
        {
            _npcSource = TypeUtils.LoadFromType(typeof(NpcScript));
            _itemSource = TypeUtils.LoadFromType(typeof(ItemScript));
            _questSource = TypeUtils.LoadFromType(typeof(QuestScript));

            _portalSource = TypeUtils.LoadFromType(typeof(PortalScript));
            _mapEnterSource = TypeUtils.LoadFromType(typeof(MapEnterScript));
            _mapFirstEnterSource = TypeUtils.LoadFromType(typeof(MapFirstEnterScript));
            _reactorHitSource = TypeUtils.LoadFromType(typeof(ReactorHitScript));
            _reactorActSource = TypeUtils.LoadFromType(typeof(ReactorActScript));
            _reactorTouchSource = TypeUtils.LoadFromType(typeof(ReactorTouchScript));
            _reactorUntouchSource = TypeUtils.LoadFromType(typeof(ReactorUntouchScript));
        }


        public Dictionary<string, (Type ObjType, MethodInfo Method)> NpcScripts => _npcSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> QuestScripts => _questSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ItemScripts => _itemSource;

        public Dictionary<string, (Type ObjType, MethodInfo Method)> PortalScripts => _portalSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> MapEnterScripts => _mapEnterSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> MapFirstEnterScripts => _mapFirstEnterSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorHitScripts => _reactorHitSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorActScripts => _reactorActSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorTouchScripts => _reactorTouchSource;
        public Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorUntouchScripts => _reactorUntouchSource;

        public override async ValueTask DisposeAsync()
        {
            _itemSource.Clear();
            _mapEnterSource.Clear();
            _mapFirstEnterSource.Clear();
            _npcSource.Clear();
            _portalSource.Clear();
            _reactorActSource.Clear();
            _reactorHitSource.Clear();
            _reactorTouchSource.Clear();
            _reactorUntouchSource.Clear();
        }

        public override Task OnMounted()
        {
            return Task.CompletedTask;
        }
    }
}
