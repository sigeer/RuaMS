using System.Reflection;

using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// 地图脚本服务
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptMapService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> MapEnterScripts { get; }
        Dictionary<string, (Type ObjType, MethodInfo Method)> MapFirstEnterScripts { get; }
    }
}
