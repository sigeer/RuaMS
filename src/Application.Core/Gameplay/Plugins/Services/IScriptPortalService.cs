using System.Reflection;

namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// 传送门脚本服务
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptPortalService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> PortalScripts { get; }
    }
}
