
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    // 北斗新手引导（30000 ~ 30005）
    internal partial class QuestScript
    {
        // Quest: 30000
        public async Task q30000s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A00_NobleAdventurer));
            await SayNext(this.GetTalkMessage(ScriptMessage.A00_HahaCongratulations));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A00_RememberMaplestory)))
            {
                await forceStartQuest();
                await gainMeso(100000);
                await forceCompleteQuest();
            }
        }

        // Quest: 30000
        public async Task q30000e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A00_HaveMet));
            await gainMeso(100000);
            await forceCompleteQuest();
        }

        // Quest: 30001
        public async Task q30001s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A00_AdventurerAfter));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A00_AlreadyKnow)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A00_ThankVery));
                await forceStartQuest();
            }
        }

        // Quest: 30001
        public async Task q30001e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A00_DearAdventurer));
            await SayOK(this.GetTalkMessage(ScriptMessage.A00_DearAdventurerRelive));
            await forceCompleteQuest();
        }

        // Quest: 30002
        public async Task q30002s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A00_NewAdventurer)))
                return;

            var text = this.GetTalkMessage(ScriptMessage.A00_WiseChoice);
            text += this.GetTalkMessage(ScriptMessage.A00_CoursePlenty);
            await SayOK(text);
            await forceStartQuest();
        }

        // Quest: 30003
        public async Task q30003s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A00_NewAdventurerSee));

            var text = this.GetTalkMessage(ScriptMessage.A00_SaySomewhere);
            text += this.GetTalkMessage(ScriptMessage.A00_PartyGathered);
            text += this.GetTalkMessage(ScriptMessage.A00_OnlyCleric);
            text += this.GetTalkMessage(ScriptMessage.A00_ThiefChild);
            text += this.GetTalkMessage(ScriptMessage.A00_BackVillage);
            text += this.GetTalkMessage(ScriptMessage.A00_EndTwo);
            await SayNext(text);

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A00_InterestingStill)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A00_GoodGo));
                await forceStartQuest();
            }
        }

        // Quest: 30003
        public async Task q30003e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A00_BadQuest));
            await forceCompleteQuest();
            await gainMeso(50000);
        }

        // Quest: 30004
        public async Task q30004s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A00_DearAdventurerHas)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A00_AlrightTruly));
                await forceStartQuest();
            }
        }

        // Quest: 30004
        public async Task q30004e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A00_VerySend));
            await forceCompleteQuest();
            await warp(0);
        }

        // Quest: 30005
        public async Task q30005s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A00_SomethingTerrible));

            var text = this.GetTalkMessage(ScriptMessage.A00_MonstersMaple);
            text += this.GetTalkMessage(ScriptMessage.A00_AffectedDark);
            await SayNext(text);

            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A00_WarriorKnow)))
                return;

            if (getLevel() < 40)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A00_BetterWait));
                return;
            }

            if (getMapId() == 300000012)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A00_IllCome));
                return;
            }

            await warp(100040103);
            await SayOK(this.GetTalkMessage(ScriptMessage.A00_ThankPlease));
            await forceStartQuest();
        }

        // Quest: 30005
        public async Task q30005e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A00_GoodnessDestroyed));
            await forceCompleteQuest();
        }
    }
}
