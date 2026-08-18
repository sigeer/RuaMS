using System.Reflection;

using Application.Core.Gameplay.Plugins;

namespace Application.Core.Gameplay.Plugins.Services
{
    /// <summary>
    /// 任务脚本服务
    /// </summary>
    [PluginInvocation(PluginInvocationMode.FirstMatch)]
    public interface IScriptQuestService : IPluginServiceBase
    {
        Dictionary<string, (Type ObjType, MethodInfo Method)> QuestScripts { get; }
    }
}
