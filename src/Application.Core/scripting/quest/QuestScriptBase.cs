using scripting.quest;

namespace Application.Core.scripting.quest
{
    public class QuestScriptBase : QuestActionManager
    {
        protected server.quest.Quest _questObj;
        public QuestScriptBase(IChannelClient c, server.quest.Quest quest, int npc) : base(c, quest.getId(), npc, false)
        {
            _questObj = quest;
        }
    }
}
