using Application.Core.Client;
using scripting.quest;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 根据 area 拆分 Etc/QuestCategory
    /// </summary>
    internal partial class QuestScript : QuestActionManager
    {
        protected server.quest.Quest _questObj;
        public QuestScript(IChannelClient c, server.quest.Quest quest, int npc) : base(c, quest.getId(), npc, false)
        {
            _questObj = quest;
        }
    }
}
