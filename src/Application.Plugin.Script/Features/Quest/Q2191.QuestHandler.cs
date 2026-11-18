using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2191QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q2191QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 2191
        /// <summary>拳手转职：送到考场。</summary>
        public async Task q2191s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q2191_YouveReached)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Q2191_DontWant));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Q2191_CantJust));
            await SayNext(this.GetTalkMessage(ScriptMessage.Q2191_WayProve));
            await startQuest();
            await warp(108000502, 0);
        }
    }
}
