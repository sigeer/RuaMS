using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21739QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21739QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21739 
        public async Task q21739e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Q21739_HaveDefeated));
            await forceCompleteQuest();
            await gainExp(29500);
        }
    }
}
