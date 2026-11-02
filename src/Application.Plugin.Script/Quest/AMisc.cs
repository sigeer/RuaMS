using Application.Shared.Constants.Job;

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
            if (!await SayAcceptDecline("你好, #h0#. 欢迎来到枫树世界。现在是活动季，我们欢迎有礼物的新角色。你现在想要你的礼物吗?"))
                return;

            await SayOK("打开你的库存打开它！这些礼物会让你看起来很时髦。哦，还有一件事！你会在30级得到另一份礼物。祝你好运!");
            await forceStartQuest();
            await forceCompleteQuest();
            await gainItem(2430191, 1, true);
        }





        // Quest: 2236
        /// <summary>睡眠森林的护身符：发放 6 个护身符，进度串初值 "000000"。</summary>
        public async Task q2236s()
        {
            if (!await SayAcceptDecline("我能感觉到邪恶的力量。它们深藏在地牢里，非常强大。如果我们想驱赶这个地方的邪恶，我们必须把护身符放在地牢里的巫师岩石上。你愿意为我做这件事吗？"))
            {
                await SayOK("在你决定是否要进行这个任务之后再跟我说。如果你决定不做，你不会错过任何机会。");
                return;
            }

            if (haveItem(4032263))
            {
                await gainItem(4032263, -6);
            }
            await gainItem(4032263, 6);
            await SayOK("拿着这些护身符，把它们放在地牢里的巫师岩石上。我给你总共6个护身符。");
            await forceStartQuest();
            await setQuestProgress(2236, 1, "000000");
        }

        // Quest: 2236
        public async Task q2236e()
        {
            if (getQuestProgress(2236, 1) == "111111")
            {
                await SayOK("我也感觉到了。巫师岩石的力量开始压倒邪恶的力量。我想睡眠森林现在安全了。邪恶已经被消灭。");
                await gainExp(60000);
                await forceCompleteQuest();
                return;
            }

            if (haveItem(4032263))
            {
                await gainItem(4032263, -6);
            }
            await gainItem(4032263, 6);
            await SayOK("哦，不好。我仍然感受到来自内部的不祥预兆。拿着这些护身符，将它们封印在巫师岩石上。我们指望你了。");
            await setQuestProgress(2236, 1, "000000");
        }

        // Quest: 3114
        public async Task q3114e()
        {
            if (getQuestProgress(3114) != "42")
            {
                await SayNext("你还没完成。");
                return;
            }

            await SayNext("完成了吗？谢谢您，冒险家。多亏了您的帮助，我可以安稳入睡了...");
            await forceCompleteQuest();
            await gainFame(20);
            await SayNext("再见，希望我们还能再见到面。");
        }

        // Quest: 3507
        public async Task q3507e()
        {
            var memoryConfirmed = isQuestCompleted(3523) || isQuestCompleted(3524) || isQuestCompleted(3525)
                || isQuestCompleted(3526) || isQuestCompleted(3527) || isQuestCompleted(3529) || isQuestCompleted(3539);

            if (!memoryConfirmed)
            {
                await SayOK("你还没有与你的第一位老师确认你的记忆吗？");
                return;
            }

            await completeQuest();
            await SayOK("你现在已经重新拥有所有的记忆了。你现在可以前往 #m270020000#.");
        }

        // Quest: 3927
        /// <summary>沙漠墙上的谜语：答对后给经验。</summary>
        public async Task q3927e()
        {
            if (getQuestProgress(3927) != "1")
            {
                await SayOK("我猜你还没有找到#p2103001#。无论如何，要找到沙盗对你来说都会很困难...");
                return;
            }

            var sel = await AskMenu("墙壁说什么了？\r\n#L0##b如果我有一把铁锤和一把匕首，一把弓和一支箭...#l\r\n#L1# 拜伦爱西林 #l\r\n#L2# 啊，我忘了。");
            if (sel != 0)
            {
                await SayOK("我猜你还没有找到#p2103001#。无论如何，要找到沙盗对你来说都会很困难...");
                return;
            }

            await AskMenu("你找到墙了吗？\r\n#L0##b我有，但是…我不知道它在说什么。#l");
            await SayOK("如果我有一把铁锤和一把匕首... 一支弓箭... 这是什么意思？你想让我告诉你吗？我自己也不知道。这是你应该考虑的事情。如果你需要线索... 它会是这样的... 一件武器只是一件物品... 直到有人使用它?");
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
            await getPlayer().TypedMessage(5, $"Quest: {getQuest()} 无法找到，请联系管理员。");
        }

        // Quest: 3414_free10rate
        /// <summary>欧米加研究任务：按职业发一张 10% 卷轴并给经验。</summary>
        public async Task q3414_free10ratee()
        {
            await SayNext("哇……就是这个！！！有了这个样本，欧米加部门的那些研究将重新焕发生机！我也无法用语言来形容找到比我更擅长打猎的人。我得重新回到正轨了！总之，为了你的出色表现，我必须好好奖励你。");

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

            var menu = "请选择你想要的卷轴，成功率均为 10%。 \r\n\r\n#r选择道具\r\n#b";
            for (var i = 0; i < scrolls.Length; i++)
            {
                menu += $"\r\n#L{i}# #i{scrolls[i]}# #t{scrolls[i]}#";
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
            await mapMessage(5, $"<{medalName}> 没有找到。");
            await EarnTitle($"<{medalName}> 已领取奖励。");
        }

        // Quest: medalQuest
        public async Task MedalQuestEnd()
        {
            await forceCompleteQuest();

            var medalName = GetMedalName();
            await mapMessage(5, $"<{medalName}> 没有找到。");
            await EarnTitle($"<{medalName}> 已领取奖励。");
        }

        private string GetMedalName()
        {
            var model = Application.Core.Server.quest.Quest.getInstance(getQuest());
            var medalItemId = model?.ViewMedalItem ?? 0;
            if (medalItemId <= 0)
                return "未知勋章";

            return Application.Core.Channel.DataProviders.ItemInformationProvider.getInstance()
                .getName(medalItemId) ?? medalItemId.ToString();
        }
    }
}
