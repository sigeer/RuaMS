using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21301QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21301QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21301 
        public async Task q21301e()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q21301_ManageSlay)))
            {
                await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21301_ReallyDidnt), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21301_CantLet), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21301_EvenGo), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("   #i4001173#", 0),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21301_HopeDreams), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21301_FuriousRight), NpcTalkSpeaker.PlayerRight)
                ]);

                await completeQuest();
            }
        }
    }
}
