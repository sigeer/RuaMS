using Application.Core.Channel.DataProviders;
using Application.Shared.Constants;
using Application.Shared.Constants.Job;

namespace Application.Plugin.Script.Quest
{
    /// <summary>
    /// 战神/海盗/反抗者等新手与转职任务线的缺失实现。
    /// 对应 JS scripts/quest/{22500,22501,22502,22503,22504,22507,23011,2560,2561,2568,2570,2573,20514}.js。
    /// 这些任务在 Quest.wz 中未声明 startscript/endscript，当前运行期不会分发。
    /// </summary>
    internal partial class QuestScript
    {
        #region 伊班与小龙（22500 ~ 22507）

        // Quest: 22500
        public async Task q22500s()
        {
            await SayNext("我终于来了！*吸气*啊，这一定是我呼吸的空气。那，那一定是太阳！还有，一棵树！还有，一株植物！还有，一朵花！哇哈哈哈！太不可思议了！这比我被困在蛋壳里时想象的世界要好得多。和你……你是我的主人吗？嗯，我对你的印象不太一样。");
            await SayNext("天哪，它说话了！");
            await SayNext("我的主人很奇怪。我想我现在无能为力了，因为任务已经完成了。*很高兴见到你。我们会经常见面的。");
            await SayNext("嗯？你什么意思？我们会经常见面吗？什么任务?");
            await SayNext("你什么意思我什么意思？！你把我从蛋壳里吵醒了。你是我的主人！所以你当然有责任照顾我，训练我，帮助我成为一条强壮的龙。显然!");
            await SayNext("天那！一条龙？你是龙？！我无法理解……为什么我是你的主人？你在开玩笑吗?");
            await SayNext("你在说什么？你的灵魂和我的灵魂达成了协议！我们现在差不多是同一个人了。我真的要解释吗？结果，你成了我的主人。我们受契约的约束。你不能改变主意……契约不能被破坏。");
            await SayNext("等等，等等，等等。让我直说吧。你是说除了帮助你我别无选择?");
            await SayNext("是啊！嘿……！脸怎么了？你……不想当我的主人?");
            await SayNext("不……不是那个……我只是不知道我是否准备好养宠物。");
            await SayNext("一……一……一只宠物？！你刚才是不是叫我宠物？！怎么敢……为什么，我是一条龙！世界上最强壮的!");
            await SayNext("#b(你怀疑地盯着他看。他看起来像只蜥蜴。一个瘦小的小家伙。)#k");

            if (!await SayAcceptDecline("你为什么那样看着我？！小心点！看看我能用我的力量做些什么。准备好了吗?"))
            {
                await SayNext("你不相信我？grrrr，你让我生气了!");
                return;
            }

            if (!isQuestStarted(22500))
            {
                await forceStartQuest();
            }

            await SayNext("让我去杀死 #r#o1210100##ks! 现在就去! 我会告诉你龙能多快打败 #o1210100#s! 冲啊!");
            await SayNext("等一下！你加了你的能力值吗? 我受主人的影响很大#b智力 和 运气#k! 如果你真的想看看我能做什么, 分配你的能力点 和 #b装备你的魔术师装备#k 在你使用技能之前!");
            await sendImage("UI/tutorial/evan/11/0");
        }

        // Quest: 22501
        public async Task q22501s()
        {
            await SayNext("哟，主人。现在我已经向你展示了我的能力，轮到你了。向我证明……你能找到食物！我快饿死了。你现在可以用我的力量，所以你必须照顾我。");
            await SayNext("呃，我还是不明白怎么回事，但我不能让像你这样可怜的小动物饿死，对吧？食物，你说？你想吃什么?");
            await SayNext("嗨，我几分钟前才出生。我怎么知道我吃了什么？我只知道我是一条龙……我是你的龙。你是我的主人。你得好好对待我!");

            if (await SayAcceptDecline("我想我们应该一起学习。但我饿了。师父，我要吃的。记住，我是个宝宝！我马上就要哭了!"))
            {
                await forceStartQuest();
                await SayOK("#b(#p1013000# 小龙看起来非常饿。你必须喂他。也许你父亲可以给你一些关于龙吃什么的建议。)#k");
            }
            else
            {
                await SayNext("*gasp* 你怎么能拒绝喂你的龙呢？你这是虐待儿童! ");
            }
        }

        // Quest: 22502
        public async Task q22502s()
        {
            if (!await SayAcceptDecline("蜥蜴不会喜欢 #b#t4032452##k, 像牛一样? 附近有很多 #b干草堆#k , 所以试着喂它。"))
            {
                await SayNext("嗯，除非你努力，否则你永远不会知道。不管你信不信，那只蜥蜴大到可以爬上枫树。它可能吃干草。");
                return;
            }

            await forceStartQuest();
            await sendImage("UI/tutorial/evan/12/0");
        }

        // Quest: 22503
        public async Task q22503s()
        {
            await SayNext("不，不，不。这不是我需要的。我需要一些更有营养的东西，主人!");
            await SayNext("嗯……所以你不是草食动物。你可能是食肉动物。你毕竟是条龙。弄一些 #t4032453# 怎么样?");

            if (await SayAcceptDecline("什么是……#t4032453#? 从没听说过，但如果好吃，我接受！给我点好吃的。除了植物！!"))
            {
                await forceStartQuest();
                await SayNext("#b(尝试是给 #p1013000# 一些 #t4032453#. 你得去农场弄一些 #o1210100#s. 十个就够了……)#k");
            }
            else
            {
                await SayNext("你怎么能这样，要饿死我啊。我还只是个宝宝。这是不对的!");
            }
        }

        // Quest: 22504
        public async Task q22504s()
        {
            await SayNext("呃。这样还不行。我还需要别的东西。没有植物。没有肉。什么，你不知道？但你是主人，你也比我大。你一定知道什么对我有好处!");
            await SayNext("但……我不觉得。这和年龄无关……");

            if (await SayAcceptDecline("既然你年纪大了，你在这个世界上也必须更有经验。你知道的比我多是有道理的。哦，很好。我会问比你大的人，师父!"))
            {
                await forceStartQuest();
                await SayNext("#b(你已经问过父亲一次了，貌似你没有更好的办法。是时候再去找他一次了!)#k");
            }
            else
            {
                await SayNext("我自己想办法找到答案是没有用的。我最好去找 #b比主人更老更聪明的人#k!");
            }
        }

        // Quest: 22507
        public async Task q22507s()
        {
            await SayNext("我就知道！我知道我们是有联系的，师父！当你变得更强，我也会变得更强。当我变得更强壮时，你可以利用我的力量！这是我们的约定。我知道我选了一个好主人！");
            await SayNext("我明白了。我们到底是怎么完成这个任务的？");
            await SayNext("我不知道。我只是个鸡蛋。我真的不记得了……虽然我隐约记得你，师父，在一片雾蒙蒙的森林里朝我走来。我记得你见到我时的惊讶。我也在呼唤你作为报答。");
            await SayNext("(等等！听起来就像你做的一个梦……你们两个是在梦里认识的吗？你在梦中看到的巨龙可能是……#p1013000#?)");
            await SayNext("师父，你和我在精神上是一体的。我一见到你就知道了。所以我想和你订个协议。没有其他人。当然，你得给我说好的价格。");
            await SayNext("我要为此付出代价？?");
            await SayNext("你不记得了吗？当你认出我并触摸我的时候？这是我设定的一个条件。你一碰我的蛋，我和你就在精神上合二为一。");
            await SayNext("一个在……精神上?");
            await SayNext("对！精神契约！你我有各自的身体，但我们有一个共同的精神。这就是为什么当我变得更强时你会变得更强，反之亦然！太棒了，对吧？至少，我想是的。");
            await SayNext("我不知道你在说什么，但听起来很重要。");
            await SayNext("这当然是大事，傻主人！你再也不用担心怪物了。你现在有我来保护你！去测试我吧。事实上，我们现在就得走了！");
            await SayNext("但这里很平静。周围没有危险的怪物。");
            await SayNext("天那？！那不好玩！师父，你不喜欢冒险吗？代表你的种族与怪物战斗，战胜邪恶，拯救无辜，诸如此类？你不喜欢这些吗?");
            await SayNext("这不是我五年计划的一部分。我只是开玩笑，但说真的，我是农民的孩子……");

            if (!await SayAcceptDecline("呃，让我告诉你吧。龙师不可能过上平静的生活。我有很多机会证明我的能力。相信我，我们的旅程将是一次大冒险。答应我你会和我在一起，好吗?\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 810 exp"))
            {
                await SayNext("呃，你开玩笑吧？告诉我你的手指滑了！去接受这个任务吧。");
                return;
            }

            if (!isQuestCompleted(22507))
            {
                await forceCompleteQuest();
                await gainExp(810);
            }

            await SayNext("呵呵，那好吧，主人。我们开始吧!");
            await SayNext("(你有点迷茫，但你现在和大龙米尔一起旅行。也许你会像他说的那样一起去冒险。)");
            await SayNext("(你还有任务没完成。你父亲需要和你谈谈，现在就去看看他。)");
        }

        #endregion

        // Quest: 23011
        /// <summary>战斗法师转职。</summary>
        public async Task q23011e()
        {
            if (!await AskYesNo("所以你最终决定成为一名战斗法师，嗯？好吧，你还是可以改变主意的。停止我们的对话，放弃这个任务，和另一个教师谈谈。所以，你确定你想成为一名战斗法师？我不想教你除非你百分之百确定..."))
            {
                await SayNext("做决定前要仔细考虑。");
                return;
            }

            if (!isQuestCompleted(23011))
            {
                await gainItem(1382100, 1);
                await gainItem(1142242, 1);
                await forceCompleteQuest();
                await changeJobById(3200);
                await showItemGain(1382100, 1);
                await showItemGain(1142242, 1);
            }

            await SayNext("好吧，好吧。欢迎加入反抗军，孩子。从现在起，你将扮演一个战斗法师的角色，一个凶猛的魔术师随时准备带领你的队伍进入战斗。");
            await SayNext("但别到处宣扬你是个战斗法师，嗯？不需要引诱黑法师来追你。从现在起，我将是你的老师。如果有人问，你就说是一个普通学生来拜访我，而不是作为抵抗军的一员。我时不时地给你上特别的课。你最好不要在课堂上睡着，听到了吗？");
        }

        #region 诺特勒斯号 / 海盗转职（2560 ~ 2573）

        // Quest: 2560
        public async Task q2560s()
        {
            await SayNext("吱吱吱吱吱！吱！吱！");
            await SayNext("肚子吃饱了，但精神还是不清醒……到底是怎么回事？一睁开眼睛看到的是猴子，不知道这是什么地方……船怎么样了呢？你知道怎么回事吗？");

            if (await SayAcceptDecline("吱吱，吱吱！ (猴子点着头。它真的知道情况吗？仔细问问猴子吧！)"))
            {
                await forceStartQuest();
            }
            else
            {
                await SayNext("吱吱，吱吱！(猴子看起来很不满意。)");
            }
        }

        // Quest: 2561
        public async Task q2561s()
        {
            await SayNext("吱吱～吱，吱！吱吱吱！吱吱～！吱！吱？！");
            await SayNext("我明明是想到冒险岛世界去当一个冒险家的……到底是怎么回事？");
            await SayNext("吱吱吱吱～吱！吱吱！吱吱吱吱吱！");
            await SayNext("和船长说话的时候看了看守卫，好像是……没错！蝙蝠怪！蝙蝠怪出现了！然后我……就那样掉下了船？那怎么会没事呢？虽然我#b知道游泳的方法#k，但是在这个过程中，我不可能神智清醒啊……难道，我是个游泳神童吗？！");
            await SayNext("吱吱吱吱吱！吱！吱！(有只小猴子好像很不满意地在那里上窜下跳。看上去好像还很小。我醒来之后看到的第一个面孔，好像就是它……)");
            await SayNext("嗯……？从刚才开始，你就一直想跟我说话吗？嗯？手上拿着的是……(猴子从怀里拿出一个苹果。看上去很好吃的熟透的苹果。但是……给我苹果干什么？)\r\n\r\n#i2010000#");

            if (await SayAcceptDecline("吱吱吱吱吱～咕嘟！(猴子一脸郁闷的表情，把苹果递给我，做出吃的动作。难道……它知道我体力很虚弱，让我吃掉它！这家伙，心地还挺善良的！)"))
            {
                if (!isQuestStarted(2561))
                {
                    await gainItem(2010000, 1, true);
                    await forceStartQuest();
                }
                await SayNext("(获得了看上去很好吃的苹果。吃一口看看吧。按#bI#k键，可以打开背包吗～？)");
                await ShowEffect("UI/tutorial.img/28");
            }
            else
            {
                await SayNext("问题是，我不喜欢苹果……对不起，不用了，谢谢。");
            }
        }

        // Quest: 2568
        public async Task q2568s()
        {
            if (!await SayAcceptDecline("你来啦。在你去做事的时候，我已经把点火装置装到了大炮上。好了，事不宜迟！我们马上出发吧！"))
                return;

            await forceStartQuest();
            await warp(912060200, 0);
        }

        // Quest: 2568
        public async Task q2568e()
        {
            await SayNext("");
        }

        // Quest: 2570
        /// <summary>炮手转职（凯琳）。</summary>
        public async Task q2570e()
        {
            await SayNext("见到你很高兴，#h0#。听说你帮助司徒诺回到了这里。看你好像受了很重的伤，这么快就好了吗？身体真够结实的……现在我理解司徒诺为什么对你那么感兴趣了。我叫凯琳，是海盗船诺特勒斯号的船长，同时也是海盗的转职教官。");
            await SayNext("司徒诺想让你成为#b火炮手#k，你应该已经听说了吧？我也非常欢迎你这样的人才加入我们海盗。但是我担心你愿不愿意成为海盗。我得先给你讲讲海盗的故事。");
            await SayNext("为了对很久以前威胁整个冒险岛世界的黑魔法师进行调查，我组建了海盗团。诺特勒斯号的海盗们现在正在冒险岛世界的各个地方调查黑魔法师的痕迹。");
            await SayNext("如果你能成为海盗，就必须帮助我们调查黑魔法师。当然，这不是义务，只是我的建议。我是海盗们的转职教官，不是海盗们的主人。因此不是命令，只是建议。");
            await SayNext("但是，我知道你会帮助我们对抗黑魔法师。你眼中闪烁着所有英雄都会有的光芒。不管怎样，相信你一定愿意帮助我们。不是为了奖励，而是出于善意……呵呵，序论好像太长了。");

            if (!await SayAcceptDecline("现在，你现在就决定吧。你想加入海盗队吗？如果你成为一名炮手，我会很高兴的。"))
            {
                if (isQuestCompleted(2570))
                {
                    await SayNext("Oh. So...you want to be something else? I understand...but Cutter might not...");
                }
                return;
            }

            if (!isQuestCompleted(2570))
            {
                await gainItem(1532000, 1);
                await gainItem(1002610, 1);
                await gainItem(1052095, 1);
                await changeJobById(501);
                await forceCompleteQuest();
                await forceCompleteQuest(29900);
                await teachSkill(109, 1, 1, -1);
                await teachSkill(110, 0, -1, -1);
                await teachSkill(111, 1, 1, -1);
                await showItemGain(1532000, 1);
                await showItemGain(1002610, 1);
                await showItemGain(1052095, 1);
            }

            await SayNext("好了，现在你也是海盗中的一员了。现在你有了很多海盗技能，打开技能窗看看吧。我给了你一些SP，你可以用它提升技能。等级提高之后，你可以使用更多的技能。希望你努力修炼。");
            await SayNext("光是技能还不行。请你打开属性窗，按照海盗的需要对自己的属性进行分配。火炮手必须拿着沉重的火炮，因此必须以力量为中心。不知道的话，可以使用#b自动分配#k。");
            await SayNext("此外我再送你一件礼物。我帮你增加了其他栏的空格数。如果是需要的东西，可以放进背包带在身上。");
            await SayNext("'啊，还有一件事不能忘记。你成为了海盗，在战斗的时候必须注意管理体力。死亡的话，之前技能的经验值会受到损失。好不容易获得的经验值，如果损失的话，岂不是太可惜了。");
            await SayNext("好了！我能教你的就是这些。我给了你几件适合你使用的武器，你去一边旅行，一边锻炼自己吧。如果遇到了黑魔法师的部下，一定要除掉他们！明白了吗？");
        }

        // Quest: 2573
        public async Task q2573s()
        {
            await SayNext("你好？你不觉得今天是旅游的好天气吗？我看你很陌生，是新开始冒险的冒险家吧。我是开往冒险岛的这条船的船长，我叫斯奇普。见到你很高兴。");

            if (await SayAcceptDecline("还不是出航时间。现在正在做出航准备，你可以在船上看看四处看看，顺便等一下。"))
            {
                await warp(3000000, 0);
                await forceCompleteQuest();
                await SayNext("哦～！已经做好出发准备了吗？速度真快。感觉会是个好的开始～这次航行一定会很愉快。好了，我们出发吧。");
            }
            else
            {
                await SayNext("嘿，别紧张！有时候你只是需要等待。");
            }
        }

        #endregion

        // Quest: 20514
        /// <summary>
        /// 经验加成任务：按「上次记录到现在的经验差」写入任务进度。
        /// JS 用 <c>raiseOpen()</c> 作为入口（不是 start/end），由任务数据决定触发时机。
        /// </summary>
        public async Task q20514Open()
        {
            var chr = getPlayer();
            var questStatus = chr.getQuestStatus(getQuest());

            if (questStatus == 0)
            {
                await setQuestProgress(20515, 0, chr.getLevel());
                await setQuestProgress(20515, 1, (int)chr.getExp());
            }
            else if (questStatus == 1)
            {
                var diffExp = chr.getExp() - getQuestProgressInt(20515, 1);
                var initLevel = getQuestProgressInt(20515, 0);

                for (var i = initLevel; i < chr.getLevel(); i++)
                {
                    diffExp += ExpTable.getExpNeededForLevel(i);
                }

                if (diffExp > 0)
                {
                    var consItem = ItemInformationProvider.getInstance().getQuestConsumablesInfo(4220137);
                    if (consItem != null)
                    {
                        var exp = consItem.exp;
                        var grade = consItem.grade;
                        await setQuestProgress(20514, 0, (int)Math.Min(diffExp, (long)exp * grade));
                    }
                }
            }
        }
    }
}
