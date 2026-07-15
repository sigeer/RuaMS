using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 传送门脚本服务
    /// </summary>
    public interface IScriptPortalService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> PortalScripts { get; }
    }
}
