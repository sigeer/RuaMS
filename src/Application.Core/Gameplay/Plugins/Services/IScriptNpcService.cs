using System.Reflection;

using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// NPC 脚本服务
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptNpcService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> NpcScripts { get; }
    }
}
