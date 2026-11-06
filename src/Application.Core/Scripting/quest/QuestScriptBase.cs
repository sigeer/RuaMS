using Application.Core.Server.quest;
using Application.Core.Server.quest.actions;
using scripting.npc;

namespace Application.Core.scripting.quest
{
    public abstract class QuestScriptBase : NPCConversationManager
    {
        protected Quest _questObj;
        public QuestScriptBase(IChannelClient c, Quest quest, int npc) : base(c, npc)
        {
            _questObj = quest;
        }

        public int getQuest()
        {
            return _questObj.getId();
        }

        public async Task<bool> forceStartQuest()
        {
            return await forceStartQuest(getQuest());
        }

        public async Task<bool> forceCompleteQuest()
        {
            return await forceCompleteQuest(getQuest());
        }

        // For compatibility with some older scripts...
        public async Task startQuest()
        {
            await forceStartQuest();
        }

        // For compatibility with some older scripts...
        public async Task completeQuest()
        {
            await forceCompleteQuest();
        }

        public async Task CompleteQuestN(int? selection = null)
        {
            await Quest.getInstance(getQuest()).complete(c.OnlinedCharacter, npc, selection);
        }

        public override async Task gainExp(int gain)
        {
            await ExpAction.runAction(getPlayer(), gain);
        }

        public override async Task gainMeso(int gain)
        {
            await MesoAction.runAction(getPlayer(), gain);
        }
    }
}
