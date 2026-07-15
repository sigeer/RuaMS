using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 地图脚本服务
    /// </summary>
    public interface IScriptMapService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> MapEnterScripts { get; }
        Dictionary<string, (Type ObjType, MethodInfo Method)> MapFirstEnterScripts { get; }
    }
}
