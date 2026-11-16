using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21747QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21747QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21747 
        public async Task q21747s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q21747_HaveThought)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Q21747_SealStone));
                await forceStartQuest();
            }

        }

        // Quest: 21747 
        public async Task q21747e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Q21747_HaveDefeated));
            await SayNext(this.GetTalkMessage(ScriptMessage.Q21747_YetSomething));
            await gainExp(16000);
            await forceCompleteQuest();
        }
    }
}
