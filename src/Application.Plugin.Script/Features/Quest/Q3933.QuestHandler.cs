using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3933QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q3933QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 3933 
        public async Task q3933s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Q3933_DidntThink));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q3933_TrulySee)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Q3933_GoodLike));
                await TryStartSoloQuest(getQuest(), forceStartQuest);
            }
        }
    }
}
