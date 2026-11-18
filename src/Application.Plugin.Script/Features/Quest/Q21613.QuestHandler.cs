using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21613QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21613QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21613 
        public async Task q21613s()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21613_WerePack), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21613_WerewolfFriend), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q21613_UnderstandWont)))
            {
                await TryStartSoloQuest(getQuest(), forceStartQuest);
            }
        }
    }
}
