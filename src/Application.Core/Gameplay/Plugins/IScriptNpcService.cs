using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// NPC 脚本服务
    /// </summary>
    public interface IScriptNpcService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> NpcScripts { get; }
    }
}
