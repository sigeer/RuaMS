using System.Reflection;

using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// Reactor 脚本服务
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptReactorService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorHitScripts { get; }
        Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorActScripts { get; }
        Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorTouchScripts { get; }
        Dictionary<string, (Type ObjType, MethodInfo Method)> ReactorUntouchScripts { get; }
    }
}
