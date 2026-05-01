namespace Application.Plugin.Script.Quest
{

    // 彩虹岛
    internal partial class QuestScript
    {
        // Quest: 1021 
        public async Task q1021s()
        {
            await SaySpeech([
                "嗨，我是罗杰，可以教你一些有用的知识。",
                    "你问我为什么在这吗？呵呵！我想要帮助那些来到这里的冒险家们。"
                ]);
            if (await SayAcceptDecline("来。。。开个小玩笑怎么样？咦！"))
            {
                if (getPlayer().HP >= 50)
                {
                    getPlayer().UpdateHP(25);
                }

                if (!haveItem(2010007))
                {
                    gainItem(2010007, 1);
                }

                forceStartQuest();
                await SaySpeech([
                    "是不是吓了一跳？HP跌到0就坏了。来，给你#r#t2010007##k，把它吃掉就会恢复了。你打开道具窗看看",
                        "你要把我给你的#t2010007#全部吃掉，停滞在一个地方什么都不做HP也会恢复的。。。你恢复了全部的HP在跟我聊聊吧。"
                    ]);
                showInfo("UI/tutorial.img/28");
            }
        }
        // Quest: 1021 
        public async Task q1021e()
        {
            if (getPlayer().HP < 50)
            {
                await SayNext("嗨，你的HP还没有完全恢复，使用我给你的苹果来补充吧！快去试试！");
                return;
            }
            else
            {
                await SaySpeech([
                    "消耗道具。。。怎么样？很简单吧？可以在右下角设定#b快捷键#k，你还不知道吧？哈哈~",
                        "不错！学得很好应该给你礼物。这些都是在旅途中必需的，谢谢我吧！危机的时候好好使用。",
                        "我能教你的只有这些了。有点儿舍不得也没办法，到了要离别的时候。路上小心，一路顺风啊！！！\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v2010000# 3 #t2010000#\r\n#v2010009# 3 #t2010009#\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 10 exp"
                    ]);
                if (isQuestCompleted(1021))
                {
                    dropMessage(1, "未知错误");
                }
                else if (canHold(2010000) && canHold(2010009))
                {
                    gainExp(10);
                    gainItem(2010000, 3);
                    gainItem(2010009, 3);
                    forceCompleteQuest();
                }
                else
                {
                    dropMessage(1, "你的背包已经满了。");
                }
            }
        }
        // Quest: 1028 
        public Task q1028s()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Quest: 1048 
        public Task q1048s()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Quest: 1049 
        public Task q1049s()
        {
            // TODO
            return Task.CompletedTask;
        }
        // Quest: 1050 
        public Task q1050s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 1051 
        public Task q1051s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 1052 
        public Task q1052s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 1053 
        public Task q1053s()
        {
            // TODO

            return Task.CompletedTask;
        }

    }
}