using Application.Core.Channel;

namespace Application.Core.Gameplay.Plugins
{
    /// <summary>
    /// 任务脚本服务
    /// </summary>
    public interface IScriptQuestService : IPluginServiceBase
    {
        Task<bool> StartQuest(IChannelClient c, server.quest.Quest questObj, int npcId);
        Task<bool> CompleteQuest(IChannelClient c, server.quest.Quest questObj, int npcId);
    }
}
