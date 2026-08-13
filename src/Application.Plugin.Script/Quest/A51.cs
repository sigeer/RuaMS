using Application.Core.Channel.QuestRecordEx;

namespace Application.Plugin.Script.Quest
{
    // 勋章
    internal partial class QuestScript
    {
        // Quest: 19000 
        public Task q19000s() => HandleMedalQuestStart();
        // Quest: 19001 
        public Task q19001s() => HandleMedalQuestStart();
        // Quest: 19002 
        public Task q19002s() => HandleMedalQuestStart();
        // Quest: 19005 
        public Task q19005s() => HandleMedalQuestStart();
        // Quest: 19006 
        public Task q19006s() => HandleMedalQuestStart();
        // Quest: 29002 
        public async Task q29002s()
        {
            await startQuest();

            var chr = getPlayer();
            var questEx = new MedalQuest29002Ex(null);
            questEx.PopG = 1000;
            questEx.Popgap = 0;
            await questEx.Flush(chr);
        }
        // Quest: 29002 
        public async Task q29002e()
        {
            var chr = getPlayer();
            var questEx = new MedalQuest29002Ex(chr.AreaInfo.GetValueOrDefault((short)getQuest()));

            if (questEx.Popgap >= questEx.PopG)
            {
                await HandleMedalQuestComplete();
            }
            else
            {
                await SayOK($"要想获得人气王称号，必须在限定时间内使人气度提高1000。如果你觉得太困难，可以放弃任务，挑战其他称号。");
            }
        }
        // Quest: 29400 
        public Task q29400s() => HandleMedalQuestStart();
        // Quest: 29400 
        public Task q29400e() => HandleMedalQuestComplete();
        // Quest: 29500 
        public async Task q29500s()
        {
            await SayNext("#v1142006:# #e#b#t1142006##k\n\n - 人气达到 1000\n\n#n你想挑战这枚勋章吗？");
            await startQuest();
            await SayOK("人气达到 1000 后，请再来找我接受审查。");
        }
        // Quest: 29500 
        public async Task q29500e()
        {
            if (getPlayer().Fame < 1000)
            {
                await SayOK("你的人气还没有达到 1000。");
                return;
            }
            await HandleMedalQuestComplete();
        }
        // Quest: 29501 
        public Task q29501s() => HandleMedalQuestStart();
        // Quest: 29501 
        public Task q29501e() => HandleMedalQuestComplete();
        // Quest: 29502 
        public Task q29502s() => HandleMedalQuestStart();
        // Quest: 29502 
        public Task q29502e() => HandleMedalQuestComplete();
        // Quest: 29503 
        public Task q29503s() => HandleMedalQuestStart();
        // Quest: 29503 
        public Task q29503e() => HandleMedalQuestComplete();
        // Quest: 29505 
        public Task q29505s() => HandleMedalQuestStart();
        // Quest: 29505 
        public Task q29505e() => HandleMedalQuestComplete();
        // Quest: 29506 
        public Task q29506s() => HandleMedalQuestStart();
        // Quest: 29506 
        public Task q29506e() => HandleMedalQuestComplete();
        // Quest: 29508 
        public Task q29508e() => HandleMedalQuestComplete();
        // Quest: 29900 
        public Task q29900s() => HandleMedalQuestStart();
        // Quest: 29900 
        public Task q29900e() => HandleMedalQuestComplete();
        // Quest: 29901 
        public Task q29901s() => HandleMedalQuestStart();
        // Quest: 29901 
        public Task q29901e() => HandleMedalQuestComplete();
        // Quest: 29902 
        public Task q29902s() => HandleMedalQuestStart();
        // Quest: 29902 
        public Task q29902e() => HandleMedalQuestComplete();

        // Quest: 29903 
        public Task q29903s() => HandleMedalQuestStart();
        // Quest: 29903 
        public Task q29903e() => HandleMedalQuestComplete();
        // Quest: 29904 
        public Task q29904s() => HandleMedalQuestStart();
        // Quest: 29905 
        public Task q29905s() => HandleMedalQuestStart();
        // Quest: 29906 
        public Task q29906s() => HandleMedalQuestStart();
        // Quest: 29907 
        public Task q29907s() => HandleMedalQuestStart();
        // Quest: 29908 
        public Task q29908s() => HandleMedalQuestStart();
        // Quest: 29909 
        public Task q29909s() => HandleMedalQuestStart();
        // Quest: 29910 
        public Task q29910s() => HandleMedalQuestStart();
        // Quest: 29911 
        public Task q29911s() => HandleMedalQuestStart();
        // Quest: 29912 
        public Task q29912s() => HandleMedalQuestStart();
        // Quest: 29913 
        public Task q29913s() => HandleMedalQuestStart();
        // Quest: 29914 
        public Task q29914s() => HandleMedalQuestStart();
        // Quest: 29915 
        public Task q29915s() => HandleMedalQuestStart();
        // Quest: 29916 
        public Task q29916s() => HandleMedalQuestStart();
        // Quest: 29917 
        public Task q29917s() => HandleMedalQuestStart();
        // Quest: 29918 
        public Task q29918s() => HandleMedalQuestStart();
        // Quest: 29919 
        public Task q29919s() => HandleMedalQuestStart();
        // Quest: 29920 
        public Task q29920s() => HandleMedalQuestStart();
        // Quest: 29921 
        public Task q29921s() => HandleMedalQuestStart();
        // Quest: 29922 
        public Task q29922s() => HandleMedalQuestStart();
        // Quest: 29923 
        public Task q29923s() => HandleMedalQuestStart();
        // Quest: 29924 
        public Task q29924s() => HandleMedalQuestStart();
        // Quest: 29925 
        public Task q29925s() => HandleMedalQuestStart();
        // Quest: 29926 
        public Task q29926s() => HandleMedalQuestStart();
        // Quest: 29927 
        public Task q29927s() => HandleMedalQuestStart();
        // Quest: 29928 
        public Task q29928s() => HandleMedalQuestStart();
        // Quest: 29933 
        public Task q29933s() => HandleMedalQuestStart();

        async Task NotImplement()
        {
            await SayNext("尚未实现");
        }

        async Task HandleMedalQuestStart()
        {
            await startQuest();
        }

        async Task HandleMedalQuestComplete()
        {
            var questObj = server.quest.Quest.getInstance(getQuest());
            if (questObj?.ViewMedalItem > 0)
            {
                var medalname = c.CurrentCulture.GetItemName(questObj.ViewMedalItem);

                await SayNext($"恭喜你获得了 #b<{medalname}>#k 勋章！继续加油吧勇士.\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n #v{questObj.ViewMedalItem}:# #t{questObj.ViewMedalItem}# 1");
                if (canHold(questObj.ViewMedalItem))
                {
                    await gainItem(questObj.ViewMedalItem);
                    await earnTitle("<" + medalname + "> 奖励已获取.");

                    await forceCompleteQuest();
                }
                else
                {
                    await SayNext("背包空间不足或者已经领取过了");
                }
            }
        }

    }
}