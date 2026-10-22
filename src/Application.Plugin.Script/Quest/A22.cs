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
            await SayNext("你不是小不点吗？");
            await SayNext("嗯……你也睡不着？");
            await SayNext("嗯……那你怎么这么晚还没睡呢？我昨晚熬了一夜，好像也没睡着……");
            await SayNext("是不是因为那个？因为那一个值得冒险……");
            await SayNext("值得冒险？你在说什么？");
            await SayNext("嗯……");
            await SayNext("(说着说着，感觉有点困了……)");

            if (await SayAcceptDecline("呵呵呵，你怎么这么可爱呢？没有吃饭吗？太瘦了。既然你对冒险感兴趣，去找 #p1013101# 吧。他一定会很开心的。"))
            {
                await forceStartQuest();
                await SayNext("#b#p1013101##k 去 #b#m100030102##k 给猎犬喂食吧。从家里出去就能见到他。");
                await sendImage("UI/tutorial/evan/1/0");
            }
            else
            {
                await SayNext("嗯……你不去找 #p1013101# 吗？那么，兄弟姐妹之间应该好好相处……");
            }
        }

        // Quest: 22000
        public async Task q22000e()
        {
            await SayNext("哦，你是那个小不点？真了不起，这么深的黑眼圈。你是不是没睡觉？什么？值得冒险，什么冒险啊，是参加冒险吗？");
            await SayNext("原来是这样。难怪你大半夜到处跑。果然很机灵。 \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 20 exp");
            await forceCompleteQuest();
            await gainExp(20);
            await sendImage("UI/tutorial/evan/2/0");
        }

        // Quest: 22001
        public async Task q22001s()
        {
            await SayNext("你今天笑得可真开心。你怎么那么开心呢。喂 #p1013102#，你怎么了？");
            await SayNext("什么？你是 #p1013101# 的弟弟！");
            await SayNext("……小不点！我会告诉你的！你知道 #p1013102# 有多可怕吗？……我看看你能不能不被它咬到……喂，它似乎很喜欢你……");

            if (await SayAcceptDecline("……你偷偷跑出来了吧？你想看看哥哥被咬吗？快点！再聊聊吧，我就告诉你怎么走！"))
            {
                await gainItem(4032447, 1, true);
                await forceStartQuest();
                await SayNext("去山头 #b那边#k 喂 #b#p1013102##k。他大概在那边等着要喂。");
                await SayNext("去喂 #p1013102# 吧……");
            }
            else
            {
                await SayNext("……你偷偷跑出来了。你想看看哥哥被咬吗？快点！再聊聊，我就告诉你怎么走！");
            }
        }

        // Quest: 22002
        public async Task q22002s()
        {
            await SayNext("你喂了 #p1013102# 了吗？哎呀，它看起来应该吃饱了。这是 #t2022620#。我帮你做的。如果你和它合不来的话，自己去找 #p1013102# 吧。");

            if (await SayAcceptDecline("……我给你弄了个#b好东西#k，所以#b待会儿再跟你说#k。……不过还有话要说。"))
            {
                await gainItem(2022620, 1, true);
                await forceStartQuest();
                await SayNext("#b(好像还有话要说……先吃了 #t2022620# 再说吧。)#k");
            }

            await sendImage("UI/tutorial/evan/3/0");
        }

        // Quest: 22002
        public async Task q22002e()
        {
            await SayNext("……你来了，安心吧，你怎么能帮我呢？  \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i1003028# 1 #t1003028#  \r\n#i2022621# 5 #t2022621#s \r\n#i2022622# 5 #t2022622# \r\n#fUI/UIWindow.img/QuestIcon/8/0# 60 exp");
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
            if (!await SayAcceptDecline("妈妈#b说#k让你去农场的时候帮忙带点东西。你能帮我把它送去给你爸爸的 #b#m100030300##k，好吗？"))
            {
                await SayNext("好吧，如果你改变主意了，随时来找我。真是个好孩子，回头再说。");
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

            await SayNext("真是我的好帮手，真是个好孩子！记得把#b头上的血迹擦干净#k。快去给你爸爸。你爸爸肯定会很高兴的。");
            await SayNext("如果你不小心弄坏了风向，就来还给我。我再给你做午饭。");
            await sendImage("UI/tutorial/evan/5/0");
        }

        // Quest: 22004
        public async Task q22004s()
        {
            await SayNext("最近农场上的#o1210100#有点多。它们无缘无故发脾气，还弄坏了一些农家的东西。我对此很担心，所以都没能好好睡觉。而且还有一只#o1210100#跑来跑去，很麻烦，你去看看吧。");

            if (await SayAcceptDecline("在我找到#o1210100#之前，你先帮我把农家的东西修好吧。要是没修好可不太妙，只要几样#t4032498#应该就能修好了。小不点，要麻烦你帮我搜集#b3个#t4032498##k就好了。"))
            {
                await forceStartQuest();
                await SayNext("哦，真是谢谢你。#b#t4032498##k可以打周围的#r#o0130100##k来收集。虽然它们不是很强，但不小心的话还是会有危险，一定要好好使用技能啊。");
                await sendImage("UI/tutorial/evan/6/0");
            }
            else
            {
                await SayNext("嗯……照#p1013101#的话，应该就能帮上忙了。");
            }
        }

        // Quest: 22004
        public async Task q22004e()
        {
            await SayNext("#t4032498#搜集齐了吗？真不知道该怎么感谢你才好。对了，这是你的报酬。 \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i3010097# 1 #t3010097# \r\n#i2022621# 15 #t2022621#s \r\n#i2022622# 15 #t2022622#s \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 210 exp");

            if (!isQuestCompleted(22004))
            {
                await gainItem(3010097, 1, true);
                await forceCompleteQuest();
                await gainExp(210);
            }

            await SayNext("好了，用剩下的木头做了个椅子。虽然不是很好看，但很结实，很好用。");
            await sendImage("UI/tutorial/evan/7/0");
        }

        // Quest: 22007
        public async Task q22007e()
        {
            await SayNext("哦，这是 #t4032451# 吗？给我看看。我帮你看看。");

            if (await AskYesNo("好的，交给我吧。我不知道该怎么用，不过大概能…… \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 360 exp"))
            {
                await gainItem(4032451, -1);
                await forceCompleteQuest();
                await gainExp(360);
                await sendImage("UI/tutorial/evan/9/0");
            }
            else
            {
                await SayNext("嗯，是吗。记得安装好了之后确认一下。");
            }
        }

        // Quest: 22008
        public async Task q22008s()
        {
            if (!await SayAcceptDecline("最近小不点们显得很活跃，像以前的风格一样，传来 #t4032451#s。我大概知道它们的来历，它们多半跟那件事有关。他们说每个村子都发生了什么。"))
            {
                await SayNext("别担心……你害怕 #o9300385#es 吗？如果你跟这件事没关系，就不用太担心。");
                return;
            }

            await forceStartQuest();
            await SayNext("……是吗？那就去看看那些家伙吧。前面应该有 #r10 只 #o9300385#es#k 在 #b#m100030103##k 那里。我会告诉你，照顾好那里的东西。现在去 #m100030103#！");
            await sendImage("UI/tutorial/evan/10/0");
        }

        // Quest: 22008
        public async Task q22008e()
        {
            await SayNext("你看到 #o9300385#es 了吗？");
            await SayNext("#b它们好像在做些什么？");
            await SayNext("嗯，那个？嗯……它确实追过来了，但可以肯定地告诉你它在追你。我可不想你被 #o9300385# 吃掉。所以我绕开了。");
            await SayNext("#b你确定不是因为怕弄坏了什么才绕开的吗？");
            await SayNext("什么？你竟然这么说！真没礼貌！");
            await SayNext("#b小心点！#o9300385# 可能会追过来！");
            await SayNext("再见！小不点！");
            await SayNext("#b……");
            await SayNext("……");
            await SayNext("看着你长大，好好看看这个世界。谢谢你，帮我找回了我弟弟的遗物。真的太感谢了！");
            await SayNext("#b(怪不得我听见有人在喊……)");
            await SayNext("来，这个给你。我很高兴你能来。作为奖励，这是很久以前一个冒险家留下的。 \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i1372043# 1 #t1372043# \r\n#i2022621# 25 #t2022621# \r\n#i2022622# 25 #t2022622#s \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 910 exp");

            if (!isQuestCompleted(22008))
            {
                await gainItem(1372043, 1, true);
                await gainItem(2022621, 25, true);
                await gainItem(2022622, 25, true);
                await forceCompleteQuest();
                await gainExp(910);
            }

            await SayNext("#b如果是给魔法师用的，应该是一把魔杖，虽然可能不想要，但既然带着，就看看你需要什么吧。");
            await SayNext("不管怎么说，我该走了，对吧？有多少人，为什么还在这里？你也应该去该去的地方了。");
        }
    }
}
