using System.Reflection;

using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// 物品脚本服务
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptItemService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> ItemScripts { get; }
    }
}
