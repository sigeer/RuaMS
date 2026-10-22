namespace Application.Plugin.Script.Quest
{
    // 北斗新手引导（30000 ~ 30005）
    internal partial class QuestScript
    {
        // Quest: 30000
        public async Task q30000s()
        {
            await SayNext("哦，尊贵的冒险者#r#h ##k，欢迎莅临#b《北斗冒险岛》#k这方被神秘与奇迹所笼罩的奇幻之地！吾乃您的引导者，#r小睡#k，一个集智慧与优雅于一身的存在，特此恭候您的到来。");
            await SayNext("哈哈，恭喜您，被命运的骰子扔进了这场荒诞不经的大冒险！您准备好了吗？前方是无尽的搞笑、离奇和不可思议！从玛加提亚城的狂欢派对，直接跳入冰封雪域的冰雪奇缘，再一头扎进森林迷宫的魔幻蘑菇圈，最后飘向星空岛屿的银河漂流瓶大会！");

            if (await SayAcceptDecline("记住，冒险岛不只是冒险，它是一场对常规的无情嘲弄，是对逻辑的彻底颠覆！所以，请放开您的笑点，拥抱这份疯狂，与我们一起，在荒诞与奇幻中，创造属于我们的神经质传奇吧！哈哈，让我们一起，笑对人生，疯癫到底！"))
            {
                await forceStartQuest();
                await gainMeso(100000);
                await forceCompleteQuest();
            }
        }

        // Quest: 30000
        public async Task q30000e()
        {
            await SayOK("看来你已经见过小睡了，欢迎来到北斗，这是给你的启动资金，希望对您的冒险有所帮助\r\n\r\n#fUI/CashShop.img/CSDiscount/bonus# 金币: 1000000");
            await gainMeso(100000);
            await forceCompleteQuest();
        }

        // Quest: 30001
        public async Task q30001s()
        {
            await SayNext("嗨~冒险者，时隔多年您又回到了记忆中的冒险岛呢，还记得吗？这是您刚踏入旅程时的第一站。");
            if (await SayAcceptDecline("也许关于职业和村子您早已烂熟于心，但我还是要向您介绍一下4个职业，因为这是我的职责，希望您能接受。"))
            {
                await SayOK("非常感谢您，您按下#bALT#k键跳上履带，可以回忆大巨变以前的角色，在您参观完毕后，跟我的好友罗杰对话吧。");
                await forceStartQuest();
            }
        }

        // Quest: 30001
        public async Task q30001e()
        {
            await SayNext("您好，亲爱的冒险家，是莎莉让您来的吧？说明您已经熟悉了大巨变前的4大基本职业了，目前版本对标#bCMS079#k，因此也有了海盗,啊，你问#r战神和龙神#k？那是歪门邪道...");
            await SayOK("亲爱的冒险家，望您回到童年，享受以前无忧无虑的冒险欢乐时光吧！");
            await forceCompleteQuest();
        }

        // Quest: 30002
        public async Task q30002s()
        {
            if (!await SayAcceptDecline("喂,你，新来的冒险家，你要不要了解一下这里？"))
                return;

            var text = "明智的选择，这个地方是一个叫玩具城的地方，这里有很多乐高积木组成的村民和怪物。";
            text += "当然，也有很多怪物，你瞧，右边的就是玩具城最常见的一种,#b#o9409001##k,你帮我狩猎5只，我会给你一些在冒险路上的报酬...";
            await SayOK(text);
            await forceStartQuest();
        }

        // Quest: 30003
        public async Task q30003s()
        {
            await SayNext("嗨，您是新来的冒险家吗？正如你所见，冒险岛世界存在一个着常年被冰雪覆盖的村子 #b冰封雪域#k。关于这里有个小故事，你感兴趣吗？");

            var text = "据说这里的某深处存在着一个恐怖的存在，它的诅咒导致这里常年下雪。曾经有一个小队前去讨伐这传说中的怪物。";
            text += "这个队伍集结了冒险岛世界最强大的4个职业，在到达这个怪物的藏身之处的洞穴迷宫前中了陷阱，战士队长为了队友，永远留在了那里。";
            text += "他们唯一的牧师更是最后感染了那个怪物的火毒而走火入魔。纵使这般，虽然他们获胜了，但也是惨胜。其中的弓箭手和飞侠是一对夫妻。";
            text += "飞侠怀有身孕，殊不知这怪物竟然在死前将自身最后的邪念注入到了这个飞侠体内，想借此重生。";
            text += "他们回到村里后，村长阿尔卡特听闻此事，竟欲处死弓箭手的妻子,他施展魔法禁锢了她，村民将她绑上祭台。";
            text += "最后这二人也终于死了，讨伐怪物的远征队，无一活口。。。据说这个怪物还是重生了，最后见过它的人，只隐约看到它有8只手。。。";
            await SayNext(text);

            if (await SayAcceptDecline("怎么样，故事有趣吗？不过这对你来说还是有难度的，怎么样，想热热身吗？"))
            {
                await SayOK("好的，你去狩猎5只#b#o9409000##k！");
                await forceStartQuest();
            }
        }

        // Quest: 30003
        public async Task q30003e()
        {
            await SayOK("不错嘛,任务完成!这是给你的奖励,希望你能用到它\r\n\r\n#fUI/CashShop.img/CSDiscount/bonus# 金币x50000");
            await forceCompleteQuest();
            await gainMeso(50000);
        }

        // Quest: 30004
        public async Task q30004s()
        {
            if (await SayAcceptDecline("亲爱的冒险家，您的旅程还顺利吗，是否决定离开这里了呢？"))
            {
                await SayOK("好的，如果你真的决定好离开，请再和我对话吧");
                await forceStartQuest();
            }
        }

        // Quest: 30004
        public async Task q30004e()
        {
            await SayOK("好的，我这就送您出去。一路顺风！");
            await forceCompleteQuest();
            await warp(0);
        }

        // Quest: 30005
        public async Task q30005s()
        {
            await SayNext("大事不好了，邪恶的气息正在侵袭冒险岛世界，勇士，请听我说...");

            var text = "本来冒险岛世界的怪物都非常祥和，可是不知道发生了什么事，最近北斗气象台检测到冒险世界正在被一股强大的黑暗力量吞噬，最先发现异常的地区是位于#b魔法密林郊区#k的猴子森林！";
            text += "猴子们受到黑暗力量的影响一个接一个的死去，并且产生变异。有人甚至看到死去的猴子竟复活过来，对人类进行无差别的攻击！这可能还只是个开始。";
            await SayNext(text);

            if (!await SayAcceptDecline("勇士，我知道您很强大，还请#r帮帮我们，我可以送您过去！！#k"))
                return;

            if (getLevel() < 40)
            {
                await SayOK("还是等你40级以后再去吧，你现在去会死翘翘的。");
                return;
            }

            if (getMapId() == 300000012)
            {
                await SayOK("等您刑满释放了我再来接您！");
                return;
            }

            await warp(100040103);
            await SayOK("谢谢您，请帮我消灭200只，但愿这样可以让冒险岛世界的黑暗气息能有效地被遏制一些。");
            await forceStartQuest();
        }

        // Quest: 30005
        public async Task q30005e()
        {
            await SayOK("天呐您这么快就消灭了200只，冒险岛世界有救了！谢谢您~！");
            await forceCompleteQuest();
        }
    }
}
