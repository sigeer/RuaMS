
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    // 职业
    internal partial class QuestScript
    {
        // Quest: 1054 
        public Task q1054s()
        {
            return forceStartQuest();
        }

        // Quest: 6030 
        public async Task q6030e()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A10_TeachAbout),
                this.GetTalkMessage(ScriptMessage.A10_WhileScience),
                this.GetTalkMessage(ScriptMessage.A10_FactAble),
                this.GetTalkMessage(ScriptMessage.A10_AlchemyEmployed),
                this.GetTalkMessage(ScriptMessage.A10_RememberMaxima)
                ]);
            await gainMeso(-10000);

            await forceCompleteQuest();
        }
        // Quest: 6031 
        public async Task q6031e()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A10_TeachAboutBasics),
                this.GetTalkMessage(ScriptMessage.A10_ScienceStages),
                this.GetTalkMessage(ScriptMessage.A10_MakesTrue),
                this.GetTalkMessage(ScriptMessage.A10_TakeMind),
                this.GetTalkMessage(ScriptMessage.A10_HasMade)
                ]);
            await gainMeso(-10000);

            await forceCompleteQuest();
        }
        // Quest: 6032 
        public async Task q6032e()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.A10_YouveCome),
                this.GetTalkMessage(ScriptMessage.A10_TeachActual),
                this.GetTalkMessage(ScriptMessage.A10_LetsTake),
                this.GetTalkMessage(ScriptMessage.A10_NeedHand)
                ]);

            await gainMeso(-10000);

            await forceCompleteQuest();
        }
        // Quest: 6033 
        public async Task q6033e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A10_HmClaim));
            if (getQuestProgressInt(6033) == 1 && haveItem(4260003, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A10_IndeedHave));
                await forceCompleteQuest();

                var skillid = (int)Math.Floor(getPlayer().getJob().getId() / 1000.0) * 10000000 + 1007;
                await teachSkill(skillid, 2, 3, -1);
                await gainExp(230000);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A10_WhatsWrong));
                return;

            }
        }
        // Quest: 6036 
        public async Task q6036e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A10_BotheringAgain));
            if (haveItem(4031980, 1))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A10_CraftedComes));

                await forceCompleteQuest();

                await gainItem(4031980, -1);
                var skillid = (int)Math.Floor(getPlayer().getJob().getId() / 1000.0) * 10000000 + 1007;
                await teachSkill(skillid, 3, 3, -1);
                await gainExp(300000);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A10_PleaseStep));
            }

        }
        // Quest: 6700 
        public Task q6700e()
        {
            // TODO
            return Task.CompletedTask;
        }

    }
}