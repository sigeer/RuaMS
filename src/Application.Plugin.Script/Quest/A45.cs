namespace Application.Plugin.Script.Quest
{

    // Masteria
    internal partial class QuestScript
    {
        // Quest: 8219 
        public async Task q8219s()
        {
            if (await SayAcceptDecline("现在是时候了，孩子。我们已经准备好进一步研究最近发生的所有这些奇怪事件的原因。我还必须介绍你认识我的兄弟，杰克。"))
            {
                await SayOK("他目前正在深红之林山脉漫游，经过邪恶的幻影森林，前往深红之城堡的道路。你的下一个目的地就在那里，祝你旅途平安。");
                forceStartQuest();
            }
            else
            {
                await SayOK("好的，那么。再见了。");
            }
        }
        // Quest: 8219 
        public async Task q8219e()
        {
            await SayNext("你是谁？哦，你是替我兄弟约翰而来的？太好了。");
            await SayOK("看起来你帮助了城市里的一些人办事，对吧？我会好好评价你的。看看这个：这是我在足够的探索后自己制作的幻影森林地图。拿着它，你将获得其他时代未曾发现的路径通行权。记住要#r永远不要丢失它#k，否则你将再也得不到它！\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i3992040# #t3992040#\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 175000 经验值");
            if (canHold(3992040, 1))
            {
                forceCompleteQuest();
                gainItem(3992040, 1);
                gainExp(175000);
            }
            else
            {
                await SayOK("嘿，你的消耗栏没有足够的空间来存放我要给你的物品。解决这个小问题然后再和我交谈。");
            }
        }
        // Quest: 8221 
        public async Task q8221s()
        {
            if (await SayAcceptDecline("是时候了！我们需要为你制作一种安全前往射手村山谷之巅的方式，否则我们所做的一切都将毫无意义。你必须得到 #b#t3992039##k。你准备好了吗？"))
            {
                await SayOK("好的，我需要你先准备这些物品：#b10 #t4010006##k，#b4 #t4032005##k 和 #b1 #t4004000##k。去吧！");
                forceStartQuest();
            }
            else
            {
                await SayOK("好的，那就这样吧。再见。");
            }
            
        }
        // Quest: 8223 
        public async Task q8223s()
        {
            if (await SayAcceptDecline("哦，杰克派你来的？好时机，我正与杰克和其他人计划突袭堡垒，夺回属于我们的权利。你看起来准备好和我们并肩作战了，对吧？"))
            {
                await SayOK("太棒了！你现在的任务是削弱敌人的力量，瓦解他们的防御。击败75个风刃袭击者、火焰使者和夜影使者，然后回到我这里汇报。");
                forceStartQuest();
            }
            else
            {
                await SayOK("好的，那就这样吧。再见。");
            }
        }
        // Quest: 8224 
        public async Task q8224s()
        {
            if (await SayAcceptDecline("嘿，旅行者，过来！我是塔格林，乌鸦忍者团的领袖。我们是目前归属于新叶城郡的雇佣兵。我们的工作是追捕最近在这里潜伏的那些怪物。你有兴趣为我们做个小任务吗？当然，对双方来说，报酬都会很丰厚。"))
            {
                forceStartQuest();

                await SayOK("好的。我需要你去森林里猎杀#b那些假树#k，并收集它们的掉落物50个作为你完成任务的证明。");
            }
            else
            {
                await SayOK("好的，那就这样吧。希望能再见到你。");
            }
        }
        // Quest: 8225 
        public async Task q8225s()
        {
            if (await SayAcceptDecline("嘿，伙伴。现在你成为了渡鸦之爪团队的一员，我有一个任务给你。你准备好了吗？"))
            {
                forceStartQuest();

                await SayOK("非常好。为了证明你在我们队伍中的价值，你必须先通过一个小挑战：你必须能够在这里移动得非常出色，了解这片森林所隐藏的所有秘密。追踪一下#b射手村的地图#k，然后来找我谈谈。我会评估你是否值得加入我们。");
            }
            else
            {
                await SayOK("好的，那就这样吧。希望能再见到你。");
            }
        }
        // Quest: 8226 
        public async Task q8226s()
        {
            if (await SayAcceptDecline("既然你已经加入我们的团队，听听我要说的话。我们，射手村的乌鸦忍者团，被雇佣来解决许多问题，为雇主解决各种大陆上的问题。我即将谈论你的任务，你准备好了吗？"))
            {
                forceStartQuest();

                await SayOK("你的下一个任务是：击败在这片森林中徘徊的长者幽灵。他们是一群强大的敌人，所以要保持警惕。我需要你带来100个#t4032010#作为你任务完成的证明。");
            }
            else
            {
                await SayOK("好的，那就这样吧。希望能再见到你。");
            }
        }
        // Quest: 8227 
        public async Task q8227s()
        {
            if (await SayAcceptDecline("嘿，伙计！好时机。我从要塞官员那里偷到了这份文件，但它的信息被加密了。我无法使用它，所以需要你把它带给约翰，看看他能否解密它？"))
            {
                if (canHold(4032032, 1))
                {
                    gainItem(4032032, 1);
                    await SayOK("很好，这件事就拜托你了。");
                    forceStartQuest();
                }
                else
                {
                    await SayOK("嘿，你的其他栏没有空位了。");
                }
            }
            else
            {
                await SayOK("来吧，城市真的需要你在这件事上合作！");
            }
        }
        // Quest: 8227 
        public async Task q8227e()
        {
            if (haveItem(4032032, 1))
            {
                gainItem(4032032, -1);
                forceCompleteQuest();

                await SayOK("哦，你从要塞带来了一封信？很棒！让我看看我能否立刻解密它。");
            }
            else
            {
                await SayOK("杰克说你没有带来加密的信？来吧，孩子，我们需要它来解读敌人的下一步行动！");
            }
        }
        // Quest: 8228 
        public async Task q8228s()
        {
            if (await SayAcceptDecline("嗯，这不太好。我似乎无法让这些超级雕纹起作用，该死。...啊，是外来者！他可能知道这张纸上写的是什么语言。让埃尔帕姆试着读一下，也许他知道些什么。"))
            {
                if (canHold(4032032, 1))
                {
                    gainItem(4032032, 1);
                    forceStartQuest();

                    await SayOK("好的，我就靠你了。");
                }
                else
                {
                    await SayOK("嘿。你的其他栏没有空位。");
                }
            }
            else
            {
                await SayOK("来吧，城市真的需要你在这件事上合作！");
            }
        }
        // Quest: 8228 
        public async Task q8228e()
        {
            if (haveItem(4032032, 1))
            {
                gainItem(4032032, -1);
                forceCompleteQuest();

                await SayOK("你好，这个世界的本地人。所以你有需要翻译的消息？我们Versal的人民以精通许多外语而闻名，这个也许是我们所熟悉的。请稍等...");
            }
            else
            {
                await SayOK("恐怕你并没有携带你声称带着的信件。");
            }
        }
        // Quest: 8229 
        public async Task q8229s()
        {
            if (await SayAcceptDecline("我知道我们可以依靠外来者处理这个问题！现在我们已经由他翻译的信件，把它交给杰克，他知道该怎么做。"))
            {
                if (haveItem(4032018, 1))
                {
                    forceStartQuest();
                }
                else if (canHold(4032018, 1))
                {
                    gainItem(4032018, 1);
                    forceStartQuest();
                }
                else
                {
                    await SayOK("喂，你需要在你的其他栏中有一个空位才能得到这份信件。");
                }
            }
            else
            {
                await SayOK("来吧，城市真的需要你在这件事上合作！");
            }
        }
        // Quest: 8229 
        public async Task q8229e()
        {
            if (haveItem(4032018, 1))
            {
                gainItem(4032018, -1);
                gainExp(50000);

                forceCompleteQuest();
                await SayOK("哦，你带来了。干得好，现在对策过程将会更加容易。\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 50000 经验值");
            }
            else
            {
                await SayOK("怎么了？为什么你还没有取回翻译好的消息？请把信的内容带给我，让我尽快制定对策。");
            }
        }
        // Quest: 8230 
        public async Task q8230s()
        {
            if (await SayAcceptDecline("嘿，旅行者！我需要你的帮助。我看到一个巨大的威胁即将危及射手村的居民。这些在这里四处游荡的生物突然出现…这肯定不是好事。你愿意听听我要说什么吗？"))
            {
                forceStartQuest();

                await SayOK("事情是这样的：扭曲大师，目前控制着红树林要塞的重要人物，计划对射手村发动一次大规模袭击，可能会在接下来的几天内发生。我不能就这样呆在这里观察，而他们准备这次袭击。但是，我不能就这样离开这个位置，我必须不惜一切代价留意他们的动向。这就是你的任务：去找到卢坎，过去红树林要塞的骑士，他目前正在树林中徘徊，从他那里接受进一步的指令，他知道该怎么做。");

            }
            else
            {
                await SayOK("好的，那就这样吧。希望能再见到你。");
            }
        }
        // Quest: 8230 
        public async Task q8230e()
        {
            if (haveItem(3992041))
            {
                await SayOK("啊，你完成了我交给你的任务。干得好，现在那些家伙正忙着从这次进攻中恢复过来。现在，请记住：#b那把钥匙必须用来进入#k要塞内部圣所。如果你想进入那里，一定要随身携带这把钥匙。");
                forceCompleteQuest();
            }
            else if (getQuestStatus(8223) == 2)
            {
                await SayOK("你完成了任务但是丢失了钥匙？那可不好，你需要这把钥匙才能进入要塞内部的房间。去找卢坎问问接下来应该做什么，我们需要你进入要塞内部。");
            }
            else
            {
                await SayOK("城里的人指望着你。请快点。");
            }
        }

        public async Task q8231_8238(string target, int requiredItem, int count)
        {
            if (await SayAcceptDecline("嘿，游客！我需要你的帮助。新叶城的居民面临新的威胁。我正在招募任何人，这次的目标是 #r" + target + "#k。你愿意加入吗？"))
            {
                forceStartQuest();

                var reqs = $"#r{count} 个 #t{requiredItem}##k";
                await SayOK("非常好。尽快给我 #r" + reqs + "#k，新叶城正在指望你。");

            }
        }

        // Quest: 8231 
        public Task q8231s() => q8231_8238("小矮人", 4032031, 30);
        // Quest: 8232 
        public Task q8232s() => q8231_8238("小矮人", 4032031, 30);

        // Quest: 8233 
        public Task q8233s() => q8231_8238("长者幽灵", 4032011, 30);

        // Quest: 8234 
        public Task q8234s() => q8231_8238("长者幽灵", 4032011, 30);

        // Quest: 8235 
        public Task q8235s() => q8231_8238("无头骑士", 4031903, 1);

        // Quest: 8236 
        public Task q8236s() => q8231_8238("无头骑士", 4031903, 1);

        // Quest: 8237 
        public Task q8237s() => q8231_8238("大脚怪", 4032013, 1);

        // Quest: 8238 
        public Task q8238s() => q8231_8238("大脚怪", 4032013, 1);

    }
}