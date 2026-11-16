using Application.Core.Client;
using Application.Core.scripting.Infrastructure;
using Application.Core.scripting.quest;
using Application.Shared.Constants;
using Application.Shared.Constants.Skill;
// 外层命名空间 Features 下有 Features.Aran，裸写 Aran 会先绑定到它而非技能常量类。
using SkillAran = Application.Shared.Constants.Skill.Aran;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21733QuestHandler : QuestScriptBase
    {
        public Q21733QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc) : base(c, quest, npc)
        {
        }

        // Quest: 21733 
        public async Task q21733s()
        {
            await AskMenu(this.GetTalkMessage(ScriptMessage.Q21733_AranLith));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q21733_HaveVery)))
            {
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Q21733_SayingIts));
            }
        }
        // Quest: 21733 
        public async Task q21733e()
        {
            await SaySpeech([
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733_ArghNever), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733_ImSorry), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733_HuhHave), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733_Weakness), NpcTalkSpeaker.PlayerRight),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733_DocumentPuppeteer), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(this.GetTalkMessage(ScriptMessage.Q21733_EvenPosition), NpcTalkSpeaker.PlayerRight)
                    ]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q21733_DontWorry)))
            {
                await gainExp(8000);
                await teachSkill(SkillAran.POLEARM_MASTERY, 0, 20, -1); // polearm mastery

                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.Q21733_MatterMuch));
            }
        }
    }
}
