using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2192QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q2192QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 2192
        /// <summary>火枪手转职：送到考场。</summary>
        public async Task q2192s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q2192_YouveReached)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Q2192_DontWant));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Q2192_CantJust));
            await SayNext(this.GetTalkMessage(ScriptMessage.Q2192_WayProve));
            await startQuest();
            await warp(108000501, 0);
        }
    }
}
