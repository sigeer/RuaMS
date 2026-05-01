using Acornima.Ast;
using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants.Job;
using Application.Shared.Items;
using scripting.map;
using server.quest;

namespace Application.Plugin.Script.Quest
{

    // 冒险骑士团
    internal partial class QuestScript
    {
        // Quest: 20000 
        public async Task q20000s()
        {
            await SayNext("啊，你来了。。。这真令人兴奋。我很感激你成为皇家骑士的决定。我等你这样的人等了很久了。有勇气面对黑魔法师而不退缩的人...");
            await SayNext("对抗想吞没整个枫叶世界的黑魔法师的邪恶本性，他的弟子的狡猾本性，以及对抗疯狂怪物的身体战等着你。总有一天，你甚至可以把自己变成仇敌，折磨你 ...");
            await SayOK("但我不担心这些。我相信你一定能战胜这一切，保护冒险岛世界免受黑魔法师的伤害。当然，你必须变得比现在强一点，对吧?");

            // gainItem(1142065, 1); // Noblesse Medal * 1
            gainExp(20); //gain 20 exp!!
            forceStartQuest();
            forceCompleteQuest();
        }
        // Quest: 20001 
        public async Task q20001s()
        {
            await SaySpeech([
                "你好, #h #. 我正式欢迎你加入皇家骑士团。我叫内哈特·冯·鲁比斯汀，年轻女皇的首席战术师。从现在起我会经常和你见面，所以我建议你记住我的名字。哈哈。。。",
                "我知道你没有足够的时间弄清楚作为皇家骑士你真正需要做什么。我会一个接一个地详细地告诉你。我会解释你在哪里，年轻的女皇是谁，我们的职责是什么。。。",
                "你正站在一个叫做圣地的岛上，这是唯一一个由年轻的女皇统治的陆地，也碰巧漂浮在空中。是的，我们说话的时候漂浮在空中。我们住在这里是不必要的，但为了年轻的女皇，它通常像一艘船，在枫树的世界里漂流...",
                "年轻的女皇的确是冒险岛世界的统治者，是这个世界唯一的统治者。什么？你从没听说过这样的事？啊，这是可以理解的。年轻的女皇也许会统治这个世界，但她不是一个笼罩在每个人面前的独裁者。她用圣地作为一种方式，让她以观察员的身份监督世界，而不必太亲力亲为。不管怎样，通常都是这样...",
                "但时不时会出现她必须控制的情况。邪恶的黑魔法师在全世界都有复活的迹象。正是毁灭之王威胁要毁灭我们所知的世界，它正试图重新出现在我们的生活中.",
                "问题是，没人知道。黑魔法师失踪已经很久了，人们已经习惯了世界的和平，不一定知道如果这样的危机来临该怎么办。如果这种情况继续下去，我们的世界将很快面临严重的危险.",
                "就在那时，年轻的女皇决定在这场潜在的危机暴露出来之前挺身而出，控制住它。她决定建立一个骑士团，以防止黑魔法师被完全复活。我相信你知道自从你自愿成为骑士之后会发生什么.",
                "我们的职责很简单。我们需要让自己变得更强大；比我们现在的状态强大得多，这样当黑魔法师回来时，我们将与他战斗，并在他把整个世界置于严重危险之前彻底消灭他。这是我们的目标，我们的使命，因此也是你的"
                ]);
            if (await SayAcceptDecline("这是对这种情况的基本概述. 明白?"))
            {
                if (isQuestCompleted(20001))
                {
                    gainExp(40);
                    gainItem(1052177, 1); // fancy noblesse robe
                }
                forceStartQuest();
                forceCompleteQuest();

                await SaySpeech([
                    "我很高兴你明白我告诉你的，但是。。。你知道吗？根据你现在的等级，你将无法面对黑魔法师。见鬼，你不能面对他的徒弟的奴隶怪物的宠物的假人！你确定你准备好保护冒险岛世界了吗?",
                    "你可能是皇家骑士的一员，但这并不意味着你是骑士。忘了做个骑士吧。你还不是一个训练中的骑士。很多时候你会坐在这里为皇家的骑士们做文书工作，但是...",
                    "但话又说回来，没有人天生强壮。女皇也更喜欢创造一个环境，在那里可以培养和创造一系列强大的骑士，而不是寻找一个超自然的天才骑士。现在，你必须在训练中成为一名骑士，让自己变得更加强大，这样你以后会变得有用。我们将讨论当皇家骑士的职责，一旦你达到这样的能力水平.",
                    "从左边的入口一直走，然后朝前走修炼森林 . 在那里，你会找到#b修炼教官#p1102000##k。下次见到你，我希望你至少等级达到10级."
                    ]);
            }
        }
        // Quest: 20002 
        public async Task q20002s()
        {
            forceStartQuest();
            await SayNext("呵呵……做好变强的准备了吗？");
            await SayNext("骑士团长们也不是一开始就很强的。需要慢慢来。");
            forceCompleteQuest();
        }
        // Quest: 20008 
        public async Task q20008s()
        {
            var selection = await AskMenu("你准备好执行任务了吗？如果你不能通过这个测试，那么你就不能称自己为真正的骑士。你确定你能做到吗？如果你害怕这样做，告诉我。我不会告诉奈哈特的. \r\n #L0#我稍后再试试这个.#l \r\n #L1#我不怕。我们这样做吧.#l");
            if (selection == 0)
            {
                await SayNext("如果你称自己为骑士，那就不要犹豫。向大家展示你有多大的勇气.");
            }
            else if (selection == 1)
            {
                forceStartQuest();

                await AskMenu("我很高兴你没有逃跑，但是。。。你确定你想成为一名见习的骑士吗？我要问的是，你是否愿意加入冒险骑士团，因此时刻与女皇紧密相连？她或许是个女皇，但终究还是个孩子。你确定你能为她而战吗？我不会让内哈特知道的，所以告诉我你的真实想法。\r\n #L2#如果女皇想要冒险岛世界的和平，那我什么都愿意。#l \r\n #L3#只要我能成为一名骑士，我会忍受一切 #l");
                forceCompleteQuest();
            }
        }
        // Quest: 20010 
        public async Task q20010s()
        {
            await SaySpeech([
                        "欢迎光临！你是？哦，你是 #b#h ##k! \r\n很高兴见到你。我一直在等你是来当骑士团的，对吧？我的名字是奇姆，我现在指导像你这样的贵族应女皇的要求.",
                        "如果你想正式成为骑士团的一部分，你必须先见见女皇。她在这个岛的中心，我和我的兄弟们想和你分享一些东西，这些东西在你走之前在冒险岛世界里被认为是#基本知识#K。这样可以吗？",
                        "让我提醒你这是一次探索你可能已经注意到，在冒险岛世界的游戏NPC偶尔会向你寻求各种帮助。这样的帮助被称为#任务#K.当你完成任务时，你将会得到奖励，所以我强烈建议你努力完成枫树交给你的工作。"
                ]);
            if (await SayAcceptDecline("你想见见我吗？ #b奇赞#k, 谁能告诉你打猎的事？你可以沿着左边的箭头找到奇赞。"))
            {
                forceStartQuest();
                guideHint(2);
            }
            else
            {
                await SayNext("哇，哇！你真的拒绝我的提议了吗？好吧，在我们的帮助下你可以更快的提升所以如果你改变主意了就告诉我。即使你拒绝了任务，如果你来和我谈谈，你也可以再次接受任务。");
            }
        }
        // Quest: 20010 
        public async Task q20010e()
        {
            await SayOK("你是我哥哥奇姆派来的贵族吗？很高兴见到你！我是奇赞我给你奇姆要我给你的奖赏。记住，你可以通过按#bI#k键来检查你的库存#K红色药水帮助你恢复惠普，蓝色的帮助你恢复mp。事先学习如何使用它们是个好主意，这样当你处于危险中时，你就能随时准备好了。\r\n\r\n#fUI/UIWindow.img/Quest/reward# \r\n\r\n#v2000020# #z2000020# \r\n#v2000021# #z2000021# \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0#15 exp");
            if (canHold(2000022) && canHold(2000023))
            {
                if (!isQuestCompleted(21010))
                {
                    gainItem(2000020, 5);
                    gainItem(2000021, 5);

                    gainExp(15);
                }
                guideHint(3);
                forceCompleteQuest();
            }
            else
            {
                dropMessage(1, "背包不足");
            }
        }
        // Quest: 20011 
        public async Task q20011s()
        {
            await SaySpeech([
                    "有许多方法打猎，但最基本的方法是用你的 #b普通攻击#k. 所有你需要的是在你的手的武器，因为它只是摆动你的武器在怪物一件简单的事情。",
                        "请按 #bCtrl#k 使用你的普通攻击. 通常下 Ctrl 位于 #b键盘的左下角#k, 但你并不需要我告诉你对不对？ 发现Ctrl 并尝试攻击！",
                    ]);
            if (await SayAcceptDecline("现在，你已经尝试过了，我们一定要测试它。在这方面，你可以找到最薄弱 #r#o100120##k 在耶雷弗, 这是您的最佳选择。尝试狩猎 #r1只#k. 当你回来我给你的奖励。."))
            {
                forceStartQuest();
                guideHint(4);
            }
            else
            {
                await SayNext("你不想？它甚至不是那么难，你会得到特殊的设备作为奖励！好吧，好好想想如果你改变主意了就告诉我。");
            }
        }
        // Quest: 20011 
        public async Task q20011e()
        {
            await SaySpeech([
                    "啊，看来你成功地猎到了 #o100120#. P很简单，对吧？经常攻击可能很容易使用，但它们相当弱。不过，别担心。#p1102006# 会教会你如何使用更强大的技能。等等，你走之前让我给你个应得的奖励。",
                    "这个装备是给贵族用的。比你现在穿的酷多了，不是吗？跟着你左边的箭去见我弟弟 #b#p1102006##k. 你走之前换一套新的贵族服装怎么样？ \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i1002869# #t1002869# - 1 \r\n#i1052177# #t1052177# - 1 \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 30 exp",
                    ]);
            gainItem(1002869, 1);
            gainItem(1052177, 1);
            forceCompleteQuest();
            gainExp(30);
            guideHint(6);
        }
        // Quest: 20012 
        public async Task q20012s()
        {
            await SaySpeech([
                "我一直在等待你, #h0#. 我的名字是 #p1102006# 为了要让你满足我的兄弟。 所以，你已经学会了如何使用普通攻击了？\r\n 好了接下来你会了解 #b如何使用技能#k, 你会发现这对你很有帮助！",
                "当你每次升等你会获得技能点数，这意味这你可以有一些能力了！ 请案 #bK 键#k 看看你的技能. 好好善用你的技能点数在技能上。 #b将技能拉到快捷键上更方便使用。#k."
                ]);
            if (await SayAcceptDecline("时间过得真快，忘了你是要练习了... 接下来你会发现很多的 #o100121# 在这张地图。你需要打倒 #r3只 #o100121##k 使用你的 #b攻击#b 技能 然后给我 1 #b#t4000483##k 作为证明OK？ 我会在这里等你的。"))
            {
                forceStartQuest();
                guideHint(8);
            }
            else
            {
                await SayNext("普通攻击是基本技能，很容易使用。重要的是要记住，使用技巧做真正的狩猎是很重要的。我建议你重新考虑。");
            }
        }
        // Quest: 20012 
        public async Task q20012e()
        {
            gainItem(4000483, -1);
            forceCompleteQuest();
            gainExp(40);

            await SayNext("你已经成功地打败了 #o100121# 并给我带来了 一个 #t4000483#. 这是非常令人印象深刻! #b你善用了 3 个技能点数 当你每一次升级的时候, 你会获得更多技能点数，接下来请照着箭头走去找我的兄弟 #b#p1102007##k, 他将告诉你下一步怎么做。\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#fUI/UIWindow.img/QuestIcon/8/0# 40 经验值");
        }
        // Quest: 20013 
        public async Task q20013s()
        {
            await SaySpeech([
                "#b(*当当*)#k",
                "嘿！你吓到我了！. 我不知道我有一个访客. 你是贵族 #p1102006# 在谈论着. 欢迎! 我是 #p1102007#, 我的兴趣是制作 #b椅子#k. 我正在考虑让一个作为欢迎你的礼物。",
                "别急，我不能给你一个椅子，因为我没有足够的材料。你能找到我需要的材料？在这个区域附近，你会发现很多箱子里面的物品。你能不能给我带回 一个 #t4032267# 和一个  #t4032268# 在那些箱子里面。",
                "你知道怎么从箱子里拿东西吗？你所要做的就是像攻击怪物一样打破盒子。不同的是，你可以用你的技能攻击怪物，但是你可以#只使用常规的攻击来打破盒子#K。"
            ]);
            if (await SayAcceptDecline("请给我 1个 #b#t4032267##k 和 1个 #b#t4032268##k 在那些箱子里面. 然后我就会做一个最棒的椅子给你， 我会在这里等着你！"))
            {
                forceStartQuest();
                guideHint(9);
            }
        }
        // Quest: 20013 
        public async Task q20013e()
        {
            await SayNext("你给我带了一块石头和窗帘吗？让我们来看看。啊，这些正是我需要的！他们确实是一个#t 4032267#和一个#t 4032268#！我马上给你做个椅子。");

            gainItem(4032267, -1);
            gainItem(4032268, -1);
            gainItem(3010060, 1);
            forceCompleteQuest();
            forceCompleteQuest(20000);
            forceCompleteQuest(20001);
            forceCompleteQuest(20002);
            forceCompleteQuest(20015);
            gainExp(95);
            guideHint(10);

            await SayNext("来这是给你的 #t3010060#. 你怎么看?? 漂亮吧呵呵^^ 你可以 #b把椅子放到快捷键上面并使用它让HP恢复更快。#k. 椅子在 #b装饰栏里面#k 打开你的道具栏, 所以请确认是不是收到椅子了 #b#p1102008##k. 好了，请按照箭头指示走你会看到另一个人。 \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i3010060# 1 #t3010060# \r\n#fUI/UIWindow.img/QuestIcon/8/0# 95 经验值");
        }
        // Quest: 20015 
        public async Task q20015s()
        {
            await SayNext("你知道吗？冒险岛世界看起来平和, 但黑暗势力已经渗透了一些地区。 黑魔法师和想复活他的人正在侵略冒险岛世界。");
            await SayNext("当我们的敌人越来越强大时，我们不能只是坐在这里什么都不做。我们自己的恐惧只会回来困扰我们。");
            if (await SayAcceptDecline("但我不会太担心。 有很多像你一样坚定的人会守护冒险岛世界，如果你有足够的勇气自愿成为骑士团的一员，我知道我可以指望你。"))
            {
                forceCompleteQuest(20015);
                await SaySpeech([
                        "嘻嘻，我就知道你会这么说。但你知道，在你为冒险岛世界奋斗之前，你还有很长的路要走",
                        "南哈特, 我身边的谋士, 将会帮你成为一名骑士. 我期待着你的进步。我指望你了!"
                    ]);
            }
        }
        // Quest: 20016 
        public async Task q20016s()
        {
            await SaySpeech([
                "嗨, #h0#. 来迎来到 #p1101000# 骑士团. 我的名字是 #p1101002# 而我目前作为年轻慈禧的战术家。哈哈！",
                "我敢肯定，你有很多的问题，因为一切都发生得太快。我会解释这一切，一个接一个，从那里你是你在这里做什么。",
                "这个岛叫做耶雷弗。多亏了女皇的魔法，这座岛通常像空中的小船一样漂浮在空中，在冒险岛世界周围巡逻。不过，现在我们停在这里是有原因的。",
                "这位年轻的女皇是冒险岛世界的统治者。什么？这是你听说过她的第一次？啊，是的。嗯，她是冒险岛世界的统治者，但她不喜欢来控制它。她从远处观看，以确保一切都很好。好吧，至少这是她一贯的作用。",
                "但现在不是这样。我们已经在整个冒险岛世界找到了预示着黑魔法师复活的迹象。我们不能让黑魔法师回来恐吓冒险岛世界，就像他过去一样！",
                "但那是很久以前的事了，今天的人们还没意识到黑魔法师有多可怕。我们都被我们今天所享受的和平的冒险岛世界宠坏了，也忘记了冒险岛世界曾经是多么的混乱和可怕。如果我们不做点什么，黑暗魔法师将再次统治冒险岛世界！",
                "这就是为什么年轻的女皇决定自己动手。她正在组建一个勇敢的法师骑士的头衔，以一劳永逸地打败黑魔法师。你知道你需要做什么，对吗？我相信你有个主意，因为你，你自己，报名成为一名骑士。",
                "我们必须变得更强，这样如果黑魔法师复活我们就能击败他。我们的首要目标是防止他破坏冒险岛世界，而你将在其中扮演一个重要的角色。"
            ]);
            if (await SayAcceptDecline("我的解释到此结束。我回答了你所有的问题吗？ \r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#fUI/UIWindow.img/QuestIcon/8/0# 380 exp"))
            {
                if (!isQuestStarted(20016))
                {
                    forceStartQuest();
                    gainExp(380);
                }

                await SaySpeech([
                    "我很高兴你清楚我们目前的状况但你知道在你目前的水平上你连面对Black Mage的手下都不够坚强更别说面对黑魔法师本人了事实上连他手下的手下都没有你将如何以你目前的水平保护冒险岛世界？",
                    "虽然你已经被接受为骑士，但你还不能被承认为骑士。你不是一个正式的骑士，因为你甚至不是一个受训的骑士。如果你保持目前的水平，你将只不过是一个勤杂工。 #p1101000# 骑士.",
                    "但从一开始就没人是个强壮的骑士。女皇不希望有人强大。她需要一个有勇气的人，经过严格的训练，她能把他培养成一个强壮的骑士。所以，你应该先成为一名受过训练的骑士。我们会讨论你的任务，当你到达这一点。",
                    "走左边的入口到达训练森林。在那里，你会发现#p1102000#，培训老师，谁将教你如何变得更强。我可不想看到你漫无目的的四处游荡直到你到达LV。10，你听到了吗？",
                ]);

                forceCompleteQuest();
            }
        }
        // Quest: 20017 
        public async Task q20017s()
        {
            await SaySpeech([
                "嗯？是#p1101002#派你过来的吗？啊哈！你是这次新来的吗？很高兴认识你。很高兴认识你！我的名字是#p1102000#。是专门教你这种贵族的骑士训练师。嗯...你怎么这么看着我...啊，你好像是第一次见到菲约吧。",
                "我们种族被称为菲约。你和女王旁边的#p1101001#说过话吗？菲约是和#p1101001#一样的种族。虽然派系有些不同...差不多。我们只生活在圣地，虽然会有些陌生，但很快就会熟悉的。",
                "啊，你或许知道吧？在这圣地根本不存在怪物。拥有邪恶力量的存在是无法涉足圣地的。不过不要担心。训练是以#p1101001#所创造的幻想生物绢毛鸟作为对象来进行的。那么开始训练吗？",
                ]);
            if (await SayAcceptDecline("运好气了吧！那么...看你的实力，你应该可以捕猎比中等稍强一点的绢毛鸟。捕猎在#m130010100#的15只#o0100122#该足够了吧？怎么样？能搞定#o0100122#吗？"))
            {
                guideHint(12);
                forceStartQuest(20020);
                forceCompleteQuest(20100);
                forceStartQuest();
            }
        }
        // Quest: 20020 
        public async Task q20020s()
        {
            await SaySpeech([
                "我看你已经达到10级了，你已经很努力了。我想现在是时候让你脱颖而出，成为一名贵族，正式成为训练中的骑士了。不过，在这之前，我想问你一件事。你决定要成为哪个骑士了吗?",
                "没有一条路可以成为骑士。事实上，有五个是为你准备的。这取决于你选择你想走哪条路，但这绝对应该是你不会后悔的事情。所以。。。我愿意向你展示你成为骑士后的样子.",
                ]);
            var o = await AskMenu("你怎么认为？你有兴趣把自己看作骑士团的领袖吗？如果你已经决定了你想成为什么样的骑士，那么你就不必去看它了…\r\n\r\n#b#L0#让我看看我作为骑士领袖的样子.#l ..#b#L1#不，我没事.");
            if (o == 0)
            {
                if (await AskYesNo("你想现在亲自看看吗？很快就会有一个短片出来。为你即将见证的事情做好准备."))
                {
                    forceStartQuest();
                    forceCompleteQuest();
                    warp(913040100, 0);
                }
            }
        }
        // Quest: 20100 
        public async Task q20100s()
        {
            if (await SayAcceptDecline("啊，你回来了。我看得出来你现在已经10级了。看来你对成为骑士有一线希望基本训练现在结束了，是你做决定的时候了。"))
            {
                forceStartQuest();
                forceCompleteQuest();

                await SayOK("现在往左看。骑士的首领会等着你有五条路供你选择。你只需要从中选择一个。他们五个都会带你找到一条骑士之路所以我建议你注意每条路能提供什么选一条你最愿意走的路");
            }
        }
        // Quest: 20101 
        public Task q20101e() => Cygnus1stJob(Job.DAWNWARRIOR1);
        // Quest: 20102 
        public Task q20102e() => Cygnus1stJob(Job.BLAZEWIZARD1);
        // Quest: 20103 
        public Task q20103e() => Cygnus1stJob(Job.WINDARCHER1);
        // Quest: 20104 
        public Task q20104e() => Cygnus1stJob(Job.NIGHTWALKER1);
        // Quest: 20105 
        public Task q20105e() => Cygnus1stJob(Job.THUNDERBREAKER1);
        async Task Cygnus1stJob(Job nextJob)
        {
            Dictionary<Job, ItemQuantity[]> initialItems = new()
            {
                { Job.DAWNWARRIOR1, [ new (1302077, 1)] },
                { Job.BLAZEWIZARD1, [ new (1372043, 1)] },
                { Job.WINDARCHER1, [ new (1452051, 1), new(2060000, 200)] },
                { Job.NIGHTWALKER1, [ new (1472061, 1), new(2070000, 200)] },
                { Job.THUNDERBREAKER1, [ new (2060000, 1)] },
            };

            if (await AskYesNo($"你决定好了嘛 ? 这会是你最后的决定唷, 所以想清楚你要做什么.你想要成为#b初级骑士 - {c.CurrentCulture.GetJobName(nextJob)}#k吗?"))
            {
                if (getJob() != Job.NOBLESSE)
                {
                    return;
                }

                var jobType = nextJob.GetJobNiche();
                if (!canGetFirstJob(jobType))
                {
                    await SayOK($"请先将等级提升到 #b10级, {getFirstJobStatRequirement(jobType)}#k");
                    return;
                }

                var items = initialItems[nextJob];
                var itemDisplay = string.Join("\r\n", items.Select(x => $"#b#v{x.ItemId}##t{x.ItemId}##k * #r{x.Quantity}#k"));
                if (!CanHoldAll(items))
                {
                    await SayOK($"请先给背包腾出一定量的空间用于接收初始装备物资。\r\n\r\n\r\n{itemDisplay}");
                    return;
                }

                changeJob(nextJob);
                getPlayer().resetStats();
                foreach (var item in items)
                {
                    gainItem(item.ItemId, item.Quantity, true);
                }
                forceCompleteQuest();

                await SaySpeech([
                    $"从这一刻起，女皇任命你为#b初级骑士#k！\r\n带上我为你准备初始物资开始历练吧！\r\n\r\n{itemDisplay}",
                    "我还扩大了你的背包空间.",
                    "打开技能栏，看看获得的新技能.",
                    "从现在开始，你死亡的时候会损失一部分经验值.",
                    "现在。。。我要你出去向全世界展示皇家的骑士们是如何成长的."
                    ]);
            }
            else
            {
                await SayNext("这个决定..非常重要.");
            }
        }

        // Quest: 20200 
        public async Task q20200s()
        {
            if (await SayAcceptDecline("#h0#? 哇，自从我上次见到你以来，你的水平已经飞涨了。你看起来也完成了很多任务。。。你现在似乎比我上次见到你时更愿意继续前进。你怎么认为？你有兴趣参加夜校考试吗？是时候让你从骑士的训练中成长为一个真正的骑士了，对吧？"))
            {
                startQuest();
                completeQuest();

                await SayOK("如果你想参加骑士考试，请来参加。每个首席骑士都会测试你的能力，如果你达到他们的标准，那么你将正式成为一名骑士。");
            }
            else
            {
                await SayNext("你觉得作为一个实习生你还有任务要做吗？我赞扬你的耐心，但这太过分了。皇家骑士迫切需要新的，更强大的骑士。");
            }
        }
        // Quest: 20201 
        public Task q20201e() => Cygnus2ndJob(Job.DAWNWARRIOR2);
        // Quest: 20202 
        public Task q20202e() => Cygnus2ndJob(Job.BLAZEWIZARD2);
        // Quest: 20203 
        public Task q20203e() => Cygnus2ndJob(Job.WINDARCHER2);
        // Quest: 20204 
        public Task q20204e() => Cygnus2ndJob(Job.NIGHTWALKER2);
        // Quest: 20205 
        public Task q20205e() => Cygnus2ndJob(Job.THUNDERBREAKER2);

        public async Task Cygnus2ndJob(Job nextJob)
        {
            if (await AskYesNo($"既然你带来了所有的#b考试的证物#k，那我现在相信你有资格成为#b正式骑士 - {c.CurrentCulture.GetJobName(nextJob)}#k，你想成为其中的一员吗？"))
            {
                if (getPlayer().getRemainingSp() > ((getPlayer().getLevel() - getJob().MaxLevel) * 3))
                {
                    await SayNext("你还有技能点没有使用完，所以你还不能成为正式的骑士！在一转技能上使用更多的SP.");
                    return;
                }

                changeJob(nextJob);
                CompleteQuestN();
            }
        }
        // Quest: 20311 
        public Task q20311s() => Cygnus3rdJob(Job.DAWNWARRIOR3);
        // Quest: 20312 
        public Task q20312s() => Cygnus3rdJob(Job.BLAZEWIZARD3);
        // Quest: 20313 
        public Task q20313s() => Cygnus3rdJob(Job.WINDARCHER3);
        // Quest: 20314 
        public Task q20314s() => Cygnus3rdJob(Job.NIGHTWALKER3);
        // Quest: 20315 
        public Task q20315s() => Cygnus3rdJob(Job.THUNDERBREAKER3);

        public async Task Cygnus3rdJob(Job nextJob)
        {
            await SayNext("你所带回来的宝石是神兽的眼泪，它拥有非常强大的力量。如果被黑魔法师给得手了，那我们全部都可能要倒大楣了...");
            if (await AskYesNo($"女皇为了报答你的努力，将任命你为皇家骑士团的#b{c.CurrentCulture.GetJobName(nextJob)}#k，你准备好了嘛?"))
            {
                var nPSP = (getPlayer().getLevel() - getJob().MaxLevel) * 3;
                if (getPlayer().getRemainingSp() > nPSP)
                {
                    await SayNext("请检查你的技能点数是否已经加完。");
                    return;
                }

                changeJob(nextJob);  //更改职业
                forceCompleteQuest();
            }
            else
            {
                await SayNext("我猜你还没准备好.");
            }
        }
        // Quest: 20400 
        public async Task q20400s()
        {
            await SayNext("不久前，我们收到了#b高级骑士#p1103000##k的求救信号, 目前驻扎在#r圣地#k. 你的工作是找到他,首先去和#b#p1101002##k联系，接受关于你任务的进一步指示.");

            forceCompleteQuest();
        }
        // Quest: 20401 
        public async Task q20401s()
        {
            await SayNext("上一次看到#b高级骑士#p1103000##k时,他正在调查最近在废矿区僵尸激增的原因。你应该亲自去看看是否能找到任何可能发生的线索.");

            forceCompleteQuest();
        }
        // Quest: 20405 
        public async Task q20405s()
        {
            await SaySpeech([
                new SpeechText ("墙上有一张纸条：“诅咒的源头仍然不见了，但我想这里发现了一个奇怪的装置，我想是他们用过的。'", 3),
                new SpeechText ("'这台机器被送到雷夫克进行雪崩，我现在得继续我的任务。愿女皇保佑我.'", 3)
                ]);
            forceCompleteQuest();
        }
        // Quest: 20406 
        public async Task q20406s()
        {
            await SayNext("是这样吗？有句话是说，#p1103000#打算继续他的旅程？不可能，在那之前还有进一步的指示要他详细说明任务的进展。#如果洞穴里真的没有什么东西了，请返回洞穴并再次报告.");
            forceCompleteQuest();
        }
        // Quest: 20408 
        public async Task q20408s()
        {
            await SaySpeech([
                "#h0#... 首先，谢谢你的出色工作。如果不是你，我。。。我不可能免受黑巫婆的诅咒。非常感谢你.",
                "如果没有别的，这一连串的小事故反而让事件更清晰了，那就是你付出了无数小时的努力来改善自己，为皇家骑士做出贡献."
                ]);
            if (await SayAcceptDecline("为了庆祝你的努力和成就。。。我想授予你一个新的头衔，并再次祝福你。你会吗。。。接受这个?"))
            {
                if (getJobId() % 10 == 1)
                {
                    changeJobById(getJobId() + 1);
                }

                forceStartQuest();
                forceCompleteQuest();

                await SayOK("#h0#. 为了勇敢地与黑魔法师战斗，从现在起，我将任命你为皇家骑士团的新首席骑士。请明智地运用你的权力和权威来帮助保护冒险岛世界的公民.");
            }
        }
        //// Quest: 20500 
        //public Task q20500s()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20502 
        //public Task q20502s()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20502 
        //public Task q20502e()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20506 
        //public Task q20506e()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20507 
        //public Task q20507s()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        //// Quest: 20509 
        //public Task q20509e()
        //{
        //    // TODO

        //    return Task.CompletedTask;
        //}
        // Quest: 20520 
        public async Task q20520s()
        {
            await SayNext("哇，你已经到了50级了，为什么你还那样到处走？我的意思是，你已经达到50级了，但你仍然用自己的脚走路。对你这样的骑士来说这是不寻常的行为.");
            if (await SayAcceptDecline("好吧，我想这取决于你，但这样做，你也有可能损害女皇的尊严和荣誉。这就是为什么我在这里给你一个有用的指针。它叫“越野车”，你当然对这个感兴趣，对吧?"))
            {
                forceStartQuest();
                forceCompleteQuest();
                await SayOK("这里有一座只有皇家骑士才能享受的特殊坐骑. 如果您感兴趣, 请访问 #b圣地#k. 我会给你提供更多的信息.");
            }
        }
        // Quest: 20522 
        public async Task q20522s()
        {
            await SaySpeech([
                "骑士的骑术和普通人的骑术有点不同。这是通过一种在这个岛上可以找到的咪咪种族的生物发生的；他们被称为#b咪咪亚纳斯 #k。骑士们骑的不是怪物，而是咪咪亚纳。有一件事你永远不应该忘记.",
                "别把这看成是一种坐骑或交通工具。这些坐骑可以是你的朋友，你的同志，你的同事。。。以上都是。即使是一个足够亲密的朋友也可以托付你的生命！这就是为什么圣地骑士会自己种坐骑."
                ]);
            if (await SayAcceptDecline("看，这是一个米米安娜蛋。你准备好养一只小咪咪了吗？让它作为你余生的旅行伴侣?"))
            {
                if (!haveItem(4220137) && !canHold(4220137))
                {
                    await SayOK("在你的背包栏上空出一个位置 ，这样我就可以给你米米亚娜蛋了.");
                    return;
                }

                forceStartQuest();
                if (!haveItem(4220137))
                {
                    gainItem(4220137);
                }
                await SayOK("米米安娜蛋可以通过 #b分享你的日常经验#k. 米米安娜长大后，请来找我.");
            }
        }
        // Quest: 20522 
        public async Task q20522e()
        {
            await SayNext("嘿，在那儿！米米安娜的蛋怎么样了");

            if (!haveItem(4220137))
            {
                await SayOK("我明白了，你丢了你的蛋。。。当你抚养一个小咪咪的时候你需要更加小心!");
                return;
            }

            forceCompleteQuest();
            gainItem(4220137, -1);
            gainExp(37600);
            await SayOK("哦，你能唤醒米米安娜蛋吗？太神奇了。。。大多数骑士都无法在这么短的时间内唤醒它.");
        }
        // Quest: 20526 
        public async Task q20526s()
        {
            await SaySpeech([
                "你失去了你的咪咪安娜？！天哪，你一定要为他们上心啊，因为他们是女皇给我们的礼物！你必须再次接受教育：骑士的骑术和普通人的骑术有点不同。这是通过一种在这个岛上可以找到的咪咪种族的生物发生的；他们被称为#b咪咪安娜#k。骑士骑咪咪安娜而不是骑怪物。这件事你永远不应该忘记.",
                "别把这看成是一种坐骑或交通工具。这些坐骑可以是你的朋友，你的同志，你的同事。。。以上都是。即使是一个足够亲密的朋友也可以托付你的生命！这就是为什么圣地骑士会自己种坐骑."
                ]);

            if (await SayAcceptDecline("看，这是一个米米安娜蛋。你准备好养一只小咪咪了吗？让它作为你余生的旅行伴侣?"))
            {
                if (!haveItem(4220137) && !canHold(4220137))
                {
                    await SayOK("在你的背包里空出一个位置，这样我就可以给你米米亚娜蛋了.");
                    return;
                }

                forceStartQuest();

                if (!haveItem(4220137))
                {
                    gainItem(4220137);
                }
                await SayOK("米米安娜的蛋可以通过#b分享你的日常经验来养大#k。 等米米安娜完全长大后, 请务必来找我. 还有一事, 我和他谈过 #p2060005# 事先为您取回 #b#t4032117##k. 当然了价格不变: #r10,000,000 金币#k.");
            }
        }
        // Quest: 20526 
        public async Task q20526e()
        {
            await SayNext("嘿，在那儿！米米安娜的蛋怎么样了");

            if (!haveItem(4220137))
            {
                await SayOK("我明白了，你丢了你的蛋。。。当你抚养一个小咪咪的时候你需要更加小心!");
                return;
            }
            if (!canHold(1902005))
            {
                await SayOK("请在你的装备栏上为你的咪咪空出一个空间!");
                return;
            }

            forceCompleteQuest();
            gainItem(1902005, 1);
            gainItem(4220137, -1);
            gainMeso(-10000000);
            await SayOK("哦，你能唤醒米米安娜蛋吗？太神奇了。。。大多数骑士都无法在这么短的时间内唤醒它.");
        }
        // Quest: 20527 
        public async Task q20527s()
        {
            var mount = getPlayer().getMount();

            if (mount != null && mount.getLevel() >= 3)
            {
                forceCompleteQuest();
                await SayNext("好吧，我会教你如何训练咪咪，咪咪的下一步。当你准备好了，再和我谈谈.");
            }
            else
            {
                await SayNext("看来你的咪咪还没到3公里。请多训练一点再设法提高它.");
            }
        }
        // Quest: 20600 
        public async Task q20600s()
        {
            if (await SayAcceptDecline("#h0#，达到100级之后，你是不是就疏于修炼了呢？虽然你确实变强了，但修炼是不能停止的。你必须以骑士团长们作为榜样。他们为了对付黑魔法师，一刻都没有停止修炼。"))
            {
                forceStartQuest();
            }
        }
        // Quest: 20610 
        public async Task q20610s()
        {
            if (await SayAcceptDecline("你在这段时间学了很多技能吗？应该不少吧...现在你想学习#b新技能#k吗？"))
            {
                forceStartQuest();
            }
            else
            {
                await SayOK("你不是没有野心，而是没有上进心。这可不太好。");
            }
        }
        // Quest: 20700 
        public async Task q20700s()
        {
            await SayNext("你终于在训练中成为了一名骑士。我想马上给你一个任务，但你看起来离自己能完成任务还有好几英里远。你确定你能像这样去金银岛吗?");
            if (await SayAcceptDecline("去金银岛由你决定，但是一个在训练中不能在战斗中照顾自己的骑士很可能会损害女皇无可挑剔的名声。作为这个岛上的首席战术家，我不能让这种事发生，周期。我要你继续训练直到时机成熟."))
            {
                forceCompleteQuest();

                await SaySpeech([
                    "#p1102000#, 训练教练，将帮助你训练成为一个有用的骑士。一旦你达到13级，我会给你分配一两个任务。所以在那之前，继续训练.",
                        "哦，你知道如果你和 #p1101001# 交谈, 她会给你祝福吗？祝福对你的旅途一定有帮助."
                    ]);
            }
        }
        // Quest: 20710 
        public async Task q20710s()
        {
            if (await SayAcceptDecline("...没想到你竟然是骑士团成员。没办法，总得有人帮着调查...我跟你说明一下这次的事情。"))
            {
                forceStartQuest();

                await SaySpeech([
                    "根据不久前得到的情报，#b#m103000000#地铁里#k扔满了奇怪的人偶。这是冬青说的，应该不会有错。那些人偶非常可疑...请你去把#b#t4032136##k拿回来。",
                    "冬青把地铁里的人偶已经全部扔进了垃圾桶，你去#b垃圾桶#k里寻找，应该可以找到人偶。祝你好运。"
                    ]);
            }
            else
            {
                await SayOK("什么？你拒绝了？好吧，拒绝就拒绝吧。我会如实向#p1101002#报告的。");
            }
        }
        // Quest: 20720 
        public async Task q20720s()
        {
            if (await SayAcceptDecline("这段时间升级还顺利吗？现在你也许正在#m103000000#执行组队任务。升级虽然是好事，但有个骑士团的任务需要交给你，因为我收到了新的情报。"))
            {
                forceStartQuest();
            }
        }

    }
}