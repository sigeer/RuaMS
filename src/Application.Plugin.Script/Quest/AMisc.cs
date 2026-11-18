using Application.Shared.Constants.Job;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 其余零散任务的缺失实现。
    /// 对应 JS scripts/quest/{10940,2191,2192,2236,3114,3507,3927,8255,medalQuest,unidentifiedQuest,3414_free10rate}.js。
    /// 这些任务在 Quest.wz 中未声明 startscript/endscript，当前运行期不会分发。
    /// </summary>
    internal partial class QuestScript
    {
        // Quest: 10940
        /// <summary>新手礼包：接受后发放 2430191。</summary>
        public async Task q10940s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AMisc_WelcomeMaple)))
                return;

            await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_OpenInventory));
            await forceStartQuest();
            await forceCompleteQuest();
            await gainItem(2430191, 1, true);
        }





        // Quest: 2236
        /// <summary>睡眠森林的护身符：发放 6 个护身符，进度串初值 "000000"。</summary>
        public async Task q2236s()
        {
            if (!await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.AMisc_FeelForces)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_ComeBack));
                return;
            }

            if (haveItem(4032263))
            {
                await gainItem(4032263, -6);
            }
            await gainItem(4032263, 6);
            await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_TakeCharms));
            await forceStartQuest();
            await setQuestProgress(2236, 1, "000000");
        }

        // Quest: 2236
        public async Task q2236e()
        {
            if (getQuestProgress(2236, 1) == "111111")
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_TooFelt));
                await gainExp(60000);
                await forceCompleteQuest();
                return;
            }

            if (haveItem(4032263))
            {
                await gainItem(4032263, -6);
            }
            await gainItem(4032263, 6);
            await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_GoodStill));
            await setQuestProgress(2236, 1, "000000");
        }

        // Quest: 3114
        public async Task q3114e()
        {
            if (getQuestProgress(3114) != "42")
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.AMisc_HaventFinished));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.AMisc_DoneThank));
            await forceCompleteQuest();
            await gainFame(20);
            await SayNext(this.GetTalkMessage(ScriptMessage.AMisc_GoodbyeHope));
        }

        // Quest: 3507
        public async Task q3507e()
        {
            var memoryConfirmed = isQuestCompleted(3523) || isQuestCompleted(3524) || isQuestCompleted(3525)
                || isQuestCompleted(3526) || isQuestCompleted(3527) || isQuestCompleted(3529) || isQuestCompleted(3539);

            if (!memoryConfirmed)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_HaveYet));
                return;
            }

            await completeQuest();
            await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_FilledAll));
        }

        // Quest: 3927
        /// <summary>沙漠墙上的谜语：答对后给经验。</summary>
        public async Task q3927e()
        {
            if (getQuestProgress(3927) != "1")
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_GuessHavent));
                return;
            }

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.AMisc_WallSay));
            if (sel != 0)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_GuessHavent));
                return;
            }

            await AskMenu(this.GetTalkMessage(ScriptMessage.AMisc_FindWall));
            await SayOK(this.GetTalkMessage(ScriptMessage.AMisc_HadHammer));
            await gainExp(28843);
            await forceCompleteQuest();
        }

        // Quest: 8255
        public async Task q8255s()
        {
            await forceStartQuest();
            await forceCompleteQuest();
        }

        // Quest: 8255
        public async Task q8255e()
        {
            await forceCompleteQuest();
        }

        // Quest: unidentifiedQuest
        /// <summary>任务脚本缺失时的兜底提示。</summary>
        public async Task unidentifiedQuest()
        {
            await getPlayer().TypedMessage(NoticeType.Pink, this.GetTalkMessage(ScriptMessage.AMisc_QuestNotFound, getQuest()));
        }

        // Quest: 3414_free10rate
        /// <summary>欧米加研究任务：按职业发一张 10% 卷轴并给经验。</summary>
        public async Task q3414_free10ratee()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AMisc_3414_free10rate_WhoaSample));

            var style = Application.Shared.Constants.Job.JobFactory.GetJobStyleById(getJobId());
            int[] scrolls = style switch
            {
                JobStyle.WARRIOR or JobStyle.BEGINNER =>
                    [2043002, 2043102, 2043202, 2044002, 2044102, 2044202, 2044402, 2044302],
                JobStyle.MAGICIAN => [2043702, 2043802],
                JobStyle.BOWMAN => [2044502, 2044602],
                JobStyle.THIEF => [2043302, 2044702],
                _ => [2044802, 2044902],
            };

            var menu = this.GetTalkMessage(ScriptMessage.AMisc_3414_free10rate_PleaseSelect);
            for (var i = 0; i < scrolls.Length; i++)
            {
                menu += this.GetTalkMessage(ScriptMessage.AMisc_ScrollEntry, i, scrolls[i], scrolls[i]);
            }

            var sel = await AskMenu(menu);
            if (sel < 0 || sel >= scrolls.Length)
                return;

            await gainItem(scrolls[sel], 1);
            await gainExp(12000);
            await completeQuest();
        }

        // Quest: medalQuest
        /// <summary>
        /// 勋章任务：JS 调用 <c>qm.getMedalName()</c>，C# 侧改从任务的 <c>ViewMedalItem</c> 取勋章名。
        /// </summary>
        public async Task MedalQuest()
        {
            await forceStartQuest();
            await forceCompleteQuest();

            var medalName = GetMedalName();
            await this.Pink(this.GetTalkMessage(ScriptMessage.AMisc_MedalNotFound, medalName.ToString()));
            await this.EarnTitle(this.GetTalkMessage(ScriptMessage.AMisc_MedalClaimed, medalName));
        }

        // Quest: medalQuest
        public async Task MedalQuestEnd()
        {
            await forceCompleteQuest();

            var medalName = GetMedalName();
            await this.Pink(this.GetTalkMessage(ScriptMessage.AMisc_MedalNotFound, medalName.ToString()));
            await this.EarnTitle(this.GetTalkMessage(ScriptMessage.AMisc_MedalClaimed, medalName));
        }

        private string GetMedalName()
        {
            var model = Application.Core.Server.quest.Quest.getInstance(getQuest());
            var medalItemId = model?.ViewMedalItem ?? 0;
            if (medalItemId <= 0)
                return this.GetTalkMessage(ScriptMessage.AMisc_medalQuest_UnknownMedal);

            return Application.Core.Channel.DataProviders.ItemInformationProvider.getInstance()
                .getName(medalItemId) ?? medalItemId.ToString();
        }
    }
}
