namespace Application.Plugin.Script.Quest
{
    // 勋章
    internal partial class QuestScript
    {
        // Quest: 19000 
        public Task q19000s() => HandleMedalQuest();
        // Quest: 19001 
        public Task q19001s() => HandleMedalQuest();
        // Quest: 19002 
        public Task q19002s() => HandleMedalQuest();
        // Quest: 19005 
        public Task q19005s() => HandleMedalQuest();
        // Quest: 19006 
        public Task q19006s() => HandleMedalQuest();
        // Quest: 29002 
        public Task q29002s() => HandleMedalQuest();
        // Quest: 29002 
        public Task q29002e() => HandleMedalQuest();
        // Quest: 29400 
        public Task q29400s() => HandleMedalQuest();
        // Quest: 29400 
        public Task q29400e() => HandleMedalQuest();
        // Quest: 29500 
        public Task q29500s() => HandleMedalQuest();
        // Quest: 29500 
        public Task q29500e() => HandleMedalQuest();
        // Quest: 29501 
        public Task q29501s() => HandleMedalQuest();
        // Quest: 29501 
        public Task q29501e() => HandleMedalQuest();
        // Quest: 29502 
        public Task q29502s() => HandleMedalQuest();
        // Quest: 29502 
        public Task q29502e() => HandleMedalQuest();
        // Quest: 29503 
        public Task q29503s() => HandleMedalQuest();
        // Quest: 29503 
        public Task q29503e() => HandleMedalQuest();
        // Quest: 29508 
        public Task q29508e() => HandleMedalQuest();
        // Quest: 29900 
        public Task q29900s() => HandleMedalQuest();
        // Quest: 29900 
        public Task q29900e() => HandleMedalQuest();
        // Quest: 29901 
        public Task q29901s() => HandleMedalQuest();
        // Quest: 29901 
        public Task q29901e() => HandleMedalQuest();
        // Quest: 29902 
        public Task q29902s() => HandleMedalQuest();
        // Quest: 29902 
        public Task q29902e() => HandleMedalQuest();

        // Quest: 29903 
        public Task q29903s() => HandleMedalQuest();
        // Quest: 29903 
        public Task q29903e() => HandleMedalQuest();
        // Quest: 29904 
        public Task q29904s() => HandleMedalQuest();
        // Quest: 29905 
        public Task q29905s() => HandleMedalQuest();
        // Quest: 29906 
        public Task q29906s() => HandleMedalQuest();
        // Quest: 29907 
        public Task q29907s() => HandleMedalQuest();
        // Quest: 29908 
        public Task q29908s() => HandleMedalQuest();
        // Quest: 29909 
        public Task q29909s() => HandleMedalQuest();
        // Quest: 29910 
        public Task q29910s() => HandleMedalQuest();
        // Quest: 29911 
        public Task q29911s() => HandleMedalQuest();
        // Quest: 29912 
        public Task q29912s() => HandleMedalQuest();
        // Quest: 29913 
        public Task q29913s() => HandleMedalQuest();
        // Quest: 29914 
        public Task q29914s() => HandleMedalQuest();
        // Quest: 29915 
        public Task q29915s() => HandleMedalQuest();
        // Quest: 29916 
        public Task q29916s() => HandleMedalQuest();
        // Quest: 29917 
        public Task q29917s() => HandleMedalQuest();
        // Quest: 29918 
        public Task q29918s() => HandleMedalQuest();
        // Quest: 29919 
        public Task q29919s() => HandleMedalQuest();
        // Quest: 29920 
        public Task q29920s() => HandleMedalQuest();
        // Quest: 29921 
        public Task q29921s() => HandleMedalQuest();
        // Quest: 29922 
        public Task q29922s() => HandleMedalQuest();
        // Quest: 29923 
        public Task q29923s() => HandleMedalQuest();
        // Quest: 29924 
        public Task q29924s() => HandleMedalQuest();
        // Quest: 29925 
        public Task q29925s() => HandleMedalQuest();
        // Quest: 29926 
        public Task q29926s() => HandleMedalQuest();
        // Quest: 29927 
        public Task q29927s() => HandleMedalQuest();
        // Quest: 29928 
        public Task q29928s() => HandleMedalQuest();
        // Quest: 29933 
        public Task q29933s() => HandleMedalQuest();

        async Task HandleMedalQuest()
        {
            var questObj = server.quest.Quest.getInstance(getQuest());
            if (questObj?.ViewMedalItem > 0)
            {
                var medalname = c.CurrentCulture.GetItemName(questObj.ViewMedalItem);

                await forceStartQuest();
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