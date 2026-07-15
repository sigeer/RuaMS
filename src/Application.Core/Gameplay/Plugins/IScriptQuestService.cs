using System.Reflection;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 任务脚本服务
    /// </summary>
    public interface IScriptQuestService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> QuestScripts { get; }
    }
}
