
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 伊班新手任务线 22000 ~ 22008（从小不点村子到遇见龙）。
    /// 对应 JS scripts/quest/{22000,22001,22002,22003,22004,22007,22008}.js。
    /// 这些任务在 Quest.wz 中未声明 startscript/endscript，当前运行期不会分发。
    /// </summary>
    internal partial class QuestScript
    {
        // Quest: 22000
        public async Task q22000s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_SleepEvan));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_AboutMom));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_SeemTired));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_ItsMom));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_StrangeDream));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_Text));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_ExplainMet));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A22_HahahaDragon)))
            {
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_WentFeed));
                await sendImage("UI/tutorial/evan/1/0");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_HmDont));
            }
        }

        // Quest: 22000
        public async Task q22000e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_EvanWhats));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_MuahahahahahaDragon));
            await forceCompleteQuest();
            await gainExp(20);
            await sendImage("UI/tutorial/evan/2/0");
        }

        // Quest: 22001
        public async Task q22001s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_HahaHad));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_ThatsS));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_LittleBrat));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A22_StopBeing)))
            {
                await gainItem(4032447, 1, true);
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_HurryHead));
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_FeedCome));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_StopBeing));
            }
        }

        // Quest: 22002
        public async Task q22002s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_FeedHave));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A22_IllGive)))
            {
                await gainItem(2022620, 1, true);
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_MomHas));
            }

            await sendImage("UI/tutorial/evan/3/0");
        }

        // Quest: 22002
        public async Task q22002e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_EatBreakfast));
            await forceCompleteQuest();
            await gainItem(1003028, 1, true);
            await gainItem(2022621, 5, true);
            await gainItem(2022622, 5, true);
            await gainExp(60);
            await sendImage("UI/tutorial/evan/4/0");
        }

        // Quest: 22003
        public async Task q22003s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A22_DadForgot)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_GoodKids));
                return;
            }

            if (!isQuestStarted(22003))
            {
                if (!haveItem(4032448))
                {
                    await gainItem(4032448, 1, true);
                }
                await forceStartQuest();
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A22_HeeheeEvan));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_ComeBack));
            await sendImage("UI/tutorial/evan/5/0");
        }

        // Quest: 22004
        public async Task q22004s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_SFarm));

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A22_BeforeGo)))
            {
                await forceStartQuest();
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_ThatsVery));
                await sendImage("UI/tutorial/evan/6/0");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_HmHave));
            }
        }

        // Quest: 22004
        public async Task q22004e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_AhBring));

            if (!isQuestCompleted(22004))
            {
                await gainItem(3010097, 1, true);
                await forceCompleteQuest();
                await gainExp(210);
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A22_MadeNew));
            await sendImage("UI/tutorial/evan/7/0");
        }

        // Quest: 22007
        public async Task q22007e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_BringGive));

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.A22_AlrightGo)))
            {
                await gainItem(4032451, -1);
                await forceCompleteQuest();
                await gainExp(360);
                await sendImage("UI/tutorial/evan/9/0");
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_HmThats));
            }
        }

        // Quest: 22008
        public async Task q22008s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A22_ItsStrange)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.A22_ScaredEs));
                return;
            }

            await forceStartQuest();
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_RightLet));
            await sendImage("UI/tutorial/evan/10/0");
        }

        // Quest: 22008
        public async Task q22008e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_DefeatEs));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_HappenedSlaying));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_HahaChase));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_SureWerent));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_WaySheesh));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_WatchOut));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_EeeekMommy));
            await SayNext("#b……");
            await SayNext("……");
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_LittleBratIm));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_DontWant));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_HmphAnyway));

            if (!isQuestCompleted(22008))
            {
                await gainItem(1372043, 1, true);
                await gainItem(2022621, 25, true);
                await gainItem(2022622, 25, true);
                await forceCompleteQuest();
                await gainExp(910);
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A22_WeaponMagicians));
            await SayNext(this.GetTalkMessage(ScriptMessage.A22_AnywayFoxes));
        }
    }
}
