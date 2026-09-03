using Application.Core.Server.quest;
using scripting.quest;

namespace Application.Core.scripting.quest
{
    public class QuestScriptBase : QuestActionManager
    {
        protected Quest _questObj;
        public QuestScriptBase(IChannelClient c, Quest quest, int npc) : base(c, quest.getId(), npc, false)
        {
            _questObj = quest;
        }
    }
}
