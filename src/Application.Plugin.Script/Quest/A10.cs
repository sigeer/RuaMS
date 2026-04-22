namespace Application.Plugin.Script.Quest
{
    // 职业
    internal partial class QuestScript
    {
        // Quest: 1054 
        public Task q1054s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 29505 
        public Task q29505s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 29505 
        public Task q29505e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 29506 
        public Task q29506s()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 29506 
        public Task q29506e()
        {
            // TODO

            return Task.CompletedTask;
        }
        // Quest: 6030 
        public async Task q6030e()
        {
            await SaySpeech([
                "我将教你有关炼金术的基础知识。",
                "虽然科学很好地展示了构成物品的元素的思考方面，但单凭科学远远不足以设计出一个物品。",
                "事实上，要能够让‘碎片’变成一个整体，应该如何做？铁匠的古老方法往往会使物品的潜在潜能减弱。",
                "炼金术可以胜任这项任务。干净而迅速，#r它将形成物品的部分合并起来，几乎没有任何副作用#k，如果做得正确，几乎不会有任何废料，最大程度地利用了这个过程。要掌握它需要一段时间，但一旦掌握，一切都会井井有条。",
                "还要记住：#b交换#k的极限，即炼金术基础领域，材料的总量不会改变，没有任何物品可以从虚无中创造出来。明白了吗？"
                ]);
            gainMeso(-10000);

            forceCompleteQuest();
        }
        // Quest: 6031 
        public async Task q6031e()
        {
            await SaySpeech([
                "我将教你有关科学理论的基础知识。",
                "科学阶段是炼金术无法满足要求的地方。所有物品都有分子构成。物品的#r排列方式和每个内在物质单位#k定义了物品将具有的许多属性。",
                "这也适用于#r制造者#k的情况。一个人必须能够研究正在用来形成物品的每个组件的痕迹，才能判断实验是否会最终成功或失败。",
                "记住这一点：科学的主要视角，使其流畅运转的那一台引擎，无论是什么情况，都是#b理解产生结果的过程#k，而不是随意地尝试。",
                "这清楚了吗？很好，那么课程结束。下课。"
                ]);
            gainMeso(-10000);

            forceCompleteQuest();
        }
        // Quest: 6032 
        public async Task q6032e()
        {
            await SaySpeech([
                "所以你来参加我的课程了，是吧？好的，我会快点。",
                "我将教你#b制造者#k方法的实际应用。你只需要想好要制作的物品，收集所有配方中的原料，并以#r科学炼金术的方式#k混合它们。简单，不是吗？",
                "以制作#b重量耳环#k为例。有一个相当特定的#r延展性理论#k来生成它，就像其他'独特'物品一样，名字围绕着我们正在处理的东西的#r主要物理力量#k：在这种情况下，是#b重量耳环的延展性引力理论#k（因为它是'重量耳环'，明白了吗？）",
                "好的，现在你需要交给我一笔费用，就是10,000金币，作为这些信息的费用。收取的费用将用于获取你学习#b制造者#k这门艺术所需的材料。",
                ]);

            gainMeso(-10000);

            forceCompleteQuest();
        }
        // Quest: 6033 
        public async Task q6033e()
        {
            await SayNext("嗯，你带来了#b#t4260003##k？好的，让我们来看看它。");
            if (getQuestProgressInt(6033) == 1 && haveItem(4260003, 1))
            {
                await SayNext("你确实制作了一块精美的怪物水晶，我看到了。你通过了！现在，我将教你制造者技能的下一步。记得保留怪物水晶，这是你的作品。");
                forceCompleteQuest();

                var skillid = (int)Math.Floor(getPlayer().getJob().getId() / 1000.0) * 10000000 + 1007;
                teachSkill(skillid, 2, 3, -1);
                gainExp(230000);
            }
            else
            {
                await SayNext("嘿，怎么了？我明明告诉过你要制作一块怪物水晶才能通过我的测试，是吧？在测试开始前购买或制作都不算数。去给我制作一块#b#t4260003##k。");
                return;

            }
        }
        // Quest: 6036 
        public async Task q6036e()
        {
            await SayNext("又来烦我了？有什么事？");
            if (haveItem(4031980, 1))
            {
                await SayNext("你制作了一件#b#t4031980##k？！怎么可能，你是怎么做到的？？...好吧，我猜没办法了。学生超越了老师！年轻人确实能让人的感知能力发生奇迹。\r\n\r\n你现在已经准备好迈向制造者技能的最后一步，将其完美地体现出来！");

                forceCompleteQuest();

                gainItem(4031980, -1);
                var skillid = (int)Math.Floor(getPlayer().getJob().getId() / 1000.0) * 10000000 + 1007;
                teachSkill(skillid, 3, 3, -1);
                gainExp(300000);
            }
            else
            {
                await SayNext("...请让开，如果我每时每刻都被打扰，我无法完成这项工作。");
            }
            
        }
        // Quest: 6700 
        public Task q6700e()
        {
            // TODO
            return Task.CompletedTask;
        }

    }
}