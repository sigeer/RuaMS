
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 蘑菇王国主线任务 2312 ~ 2321（史莱姆菇/奇拉蘑菇孢子线）。
    /// 对应 JS scripts/quest/{2312,2313,2315,2316,2317,2318,2319,2320,2321}.js。
    /// 这些任务已有 <c>Quest.wz</c> 记录但未声明 startscript/endscript，故运行期不会分发；
    /// 按「文件全覆盖」口径补齐。
    /// </summary>
    internal partial class QuestScript
    {
        // Quest: 2312
        public async Task q2312s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_NeedHelp)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_KeepMoving));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_HmmmUnsure));
            }
        }

        // Quest: 2312
        public async Task q2312e()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_TeachRenegade)))
                return;

            await forceCompleteQuest();
            await gainExp(11500);
            await gainItem(4000499, -50);
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_AmazingApologize));
        }

        // Quest: 2313
        public async Task q2313s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_HaveTold)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_SaveKingdom));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_TheresMuch));
            }
        }

        // Quest: 2313
        public async Task q2313e()
        {
            await forceCompleteQuest();
            await gainExp(4000);
        }

        // Quest: 2315
        public async Task q2315s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_PowerfulBarrier)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_PleaseGo));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_PleaseForget));
            }
        }

        // Quest: 2315
        public async Task q2315e()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_InvestigatedBarrier)))
                return;

            await forceCompleteQuest();
            await gainExp(4000);
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_HmmmInteresting));
        }

        // Quest: 2316
        public async Task q2316s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_ThinkIve)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_ConfidentScarrs));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_EvenAsk));
            }
        }

        // Quest: 2316
        public async Task q2316e()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_AhYoure, npc)))
                return;

            await forceCompleteQuest();
            await gainExp(4200);
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_KillerMushroom));
        }

        // Quest: 2317
        public async Task q2317s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_AhMistaken)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_PleaseDefeat));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_BreakingThrough));
            }
        }

        // Quest: 2317
        public async Task q2317e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_HaveGathered));
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_AmazedAble));
            await forceCompleteQuest();
            await gainExp(13500);
            await gainItem(4000500, -100);
        }

        // Quest: 2318
        public async Task q2318s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_HmmmLooked)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_WantDefeat));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_UnderstandIts));
            }
        }

        // Quest: 2318
        public async Task q2318e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_GatherAll));

            if (!haveItem(4000499, 50))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_PleaseGather));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.A23_EnoughMake));
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_KillerMushroomSpores));
            await forceCompleteQuest();
            await gainExp(11500);
            await gainItem(4000499, -50);
            await gainItem(2430014, 1);
        }

        // Quest: 2319
        public async Task q2319s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_AlmostForgot)))
            {
                await forceStartQuest();
                await gainItem(4032389, 1);
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_MinisterMagic));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_KnowIts));
            }
        }

        // Quest: 2319
        public async Task q2319e()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_KillerMushroomSporesFinally)))
                return;

            await forceCompleteQuest();
            await gainExp(4200);
            await gainItem(4032389, -1);
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_KillerMushroomSporesThank));
        }

        // Quest: 2320
        public async Task q2320s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_HaveJust)))
            {
                await forceStartQuest();
                await gainItem(4032389, 1);
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_HonestKiller));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_WantedPersonally));
            }
        }

        // Quest: 2320
        public async Task q2320e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_YoureBehalf));
            await forceCompleteQuest();
            await gainExp(8800);
            await gainItem(4032389, -1);
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_AhhKiller));
        }

        // Quest: 2321
        public async Task q2321s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A23_DontSeem)))
            {
                await forceStartQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_YoullAble));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A23_GoodLuck));
            }
        }

        // Quest: 2321
        public async Task q2321e()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_HaveKeeping));
            await forceCompleteQuest();
            await gainExp(2500);
            await SayOK(this.GetTalkMessage(ScriptMessage.A23_ProblemFigure));
        }
    }
}
