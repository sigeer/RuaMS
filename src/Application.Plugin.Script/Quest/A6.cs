using Application.Core.Client;
using Application.Core.Game.Maps;
using Application.Core.scripting.Infrastructure;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Shared.Constants;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Skill;
using Application.Utility.Configs;
using scripting.map;
using System.Reflection.Metadata;

namespace Application.Plugin.Script.Quest
{
    // 战神
    internal partial class QuestScript
    {
        // Quest: 21000 
        public async Task q21000s()
        {
            if (await SayAcceptDecline("糟糕！有个孩子被留在森林里了！我们不能丢下孩子就这么逃走！战神……请你救救孩子吧！你伤得这么重，还要你去战斗，我们心里也很过意不去……但只有你能够救那个孩子啊！"))
            {
                await forceStartQuest();
                await SaySpeech([
                    "#b孩子可能在森林的深处#k！必须在黑魔法师找到我们之前，启动方舟，所以必须尽快救出孩子才行！",
                    "关键是不要慌张，战神。如果你要查看任务状态，按#bQ键#k就能在任务栏中查看。",
                    "拜托了，战神！救救孩子吧！我们不能再有人因为黑魔法师而牺牲了！"
                    ]);
                await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            }
            else
            {
                await SayNext("不行，战神……要是抛下孩子我们自己逃掉……就算能活下去也没什么意义！我知道这个要求对你而言很勉强，不过还是请你再考虑考虑。");
            }
        }
        // Quest: 21001 
        public async Task q21001s()
        {
            if (await SayAcceptDecline("呃呃……吓死我了……快，快带到赫丽娜那边去！"))
            {
                await gainItem(4001271, 1);
                await forceStartQuest();
                await warp(914000300, 0);
            }
            else
            {
                await SayNext("啊！战神大人拒绝了！");
            }
        }
        // Quest: 21001 
        public async Task q21001e()
        {
            if (await AskYesNo("呵呵，平安回来了？孩子呢？孩子也带回来了吗？"))
            {
                await SaySpeech([
                    new SpeechText("太好了……真是太好了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("赶快上船！已经没时间了！", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText("啊，没错。现在不是感伤的时候。黑魔法师的气息越来越近！似乎他们已经察觉方舟的位置，得赶紧启航，不然就来不及了！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("立刻出发！", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText("战神！请你也上船吧！我们理解你渴望战斗的心情……不过，现在已经晚了！战斗就交给你的那些同伴吧，和我们一起去金银岛吧！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("不行！", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText("赫丽娜，你先出发去金银岛。一定要活着，我们一定会再见的。我要和同伴们一起同黑魔法师战斗！", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    ]);

                await gainItem(4001271, -1);
                await removeEquipFromSlot(-11);
                await forceCompleteQuest();

                await warp(914090010, 0); // Initialize Aran Tutorial Scenes
            }
            else
            {
                await SayNext("孩子呢？孩子救出来了的话，就赶紧让我们看看。");
            }

        }
        // Quest: 21010 
        public async Task q21010s()
        {
            await SaySpeech([
                "咦？什么人在岛上……？哎哟，这不是#p1201000#吗？#p1201000#来这里是为了……这人是#p1201000#的朋友吗？啊？这说这个人是英雄？",
                "     #i4001170#",
                "这位原来就是#p1201000#一族数百年间苦苦守候的英雄啊！啊，乍一看倒是和普通人没什么两样……",
                ]);
            if (await SayAcceptDecline("但是可能是因为你被黑魔法师的诅咒困在冰里几百年时间，所以体力全都消耗完了。#b我给你一个体力恢复药水，请快点喝下去吧#k。"))
            {
                if (getPlayer().HP >= 50)
                {
                    await getPlayer().UpdateHP(25);
                }

                await gainItem(2000022, 1);
                await forceStartQuest();

                await SaySpeech([
                    new SpeechText("请先喝掉药水，然后再慢慢说！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("#b（这药水怎么喝？……不记得了……）", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                ]);
                await guideHint(14);
            }
            else
            {
                await SayNext("哦，不需要拒绝我的提议。这没什么大不了的。只是一种药水。好吧,如果你改变主意了就告诉我.");
            }
        }
        // Quest: 21010 
        public async Task q21010e()
        {
            if (getPlayer().HP < 50)
            {
                await SayNext("你还没喝那药水呢.");
                return;
            }

            await SaySpeech([
                "我们一直试图在冰层深处寻找传说中的英雄，不过从没想过真能找到你！预言果然没有错！#p1201000#做出了正确的选择！既然英雄重新回来了，我们就没有必要再惧怕黑魔法师了！ ",
                "哎哟，我怎么抓着您聊了这么久？实在是太高兴了……其他的企鹅估计也会像我这样的。虽然知道你很忙，不过在会存在的路上，#b还是尽量和其他的企鹅搭搭话吧#k。有大英雄和他们说话，他们肯定会惊讶得要死！\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0# \r\n#i2000022# #t2000022# 5个\r\n#i2000023# #t2000023# 5个\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 16 exp",
                "这位原来就是#p1201000#一族数百年间苦苦守候的英雄啊！啊，乍一看倒是和普通人没什么两样……",
                ]);

            await gainExp(16);
            await gainItem(2000022, 3);
            await gainItem(2000023, 3);
            await forceCompleteQuest();

            await SayNext("你升级了吗？不知道你有没有得到技能点数？在冒险岛世界，每升1级就能获得技能点数3。按#bK键#k，打开技能栏就可确认。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face);
            await SayNext("#b（对我这么亲切，我却什么都想不起来。我真的是英雄吗？还是先查看一下技能吧……怎么查看技能呀？）", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
            await guideHint(15);
        }
        // Quest: 21011 
        public async Task q21011e()
        {
            await SaySpeech([
                "和#p1201000#在一起的，难道……难道就是传说中的英雄？#p1201000#！别不耐烦地点头，给我们介绍介绍呀！这位就是传说中的英雄吗？！",
                "   #i4001171#",
                "……真对不起，太激动了，忍不住嗓门大了些。呜呜～真是令人激动……唉，眼泪都快出来了……#p1201000#这回可开心了。",
                "等等……英雄大人怎么能没有武器呢？我听说每个英雄都有自己的独特武器……啊，估计是和黑魔法师战斗的时候遗失了。"
                ]);
            if (await AskYesNo("虽然寒碜了点，不过#b先拿这把剑用着吧#k。算是送给英雄的礼物。英雄如果没有武器，岂不是会有些奇怪？\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v1302000# #t1302000# 1 个\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 35 exp"))
            {
                if (canHold(1302000))
                {
                    await gainItem(1302000, 1);
                    await gainExp(35);
                    await forceCompleteQuest();

                    await SayNext("#b（看自己这技能水平没一点英雄的样子……这把剑感觉也很陌生。以前的我是用剑的吗？这把剑怎么用呢？）", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd );
                    await guideHint(16);
                }
                else
                {
                    await Popup("你的背包已经满了");
                }
            }

        }
        // Quest: 21012 
        public async Task q21012s()
        {
            await SaySpeech([
                "英雄！你好！啊？你难道不知道自己是英雄吗？前面3个人都喊那么大声了，我还能听不见吗？整个岛都知道英雄苏醒的事情了。",
                "咦，你怎么好像不开心的样子？有什么问题吗？啊？不知道自己到底是不是英雄？你失忆了吗？怎么会……看样子是被封冻在冰里数百年来的后遗症。",
                ]);
            if (await SayAcceptDecline("嗯，既然你是英雄，挥挥剑也许就会想起什么来呢？试着去#b打猎怪兽#k，怎么样？"))
            {
                await forceStartQuest();

                await SaySpeech([
                    "对了，这附近有许多#r#o9300383##k，请击退 #r3只#k试试，说不定你就能想起什么了。",
                   "哦，你应该还没有忘记使用技能的方法吧？#b将技能拖到快捷栏上，以方便使用#k。除了技能以外，消耗道具也可以拖到这里来方便使用。",
                ]);
                await guideHint(17);
            }
            else
            {
                await SayOK("嗯……说不定这方法能够让你恢复记忆～不论怎样，还是值得一试的。");
            }

        }
        // Quest: 21012 
        public async Task q21012e()
        {
            await SayOK("嗯...看您的表情，似乎什么都没有想起来...可是请不要担心。总有一天会好起来的。来，请您喝下这些药水打起精神来!\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v2000022# 10 #t2000022#\r\n#v2000023# 10 #t2000023#\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 57 exp");
            if (canHold(2000022) && canHold(2000023))
            {
                await forceCompleteQuest();
                await gainExp(57);
                await gainItem(2000022, 10);
                await gainItem(2000023, 10);

                await SayOK("#b（就算我真的是英雄……一个什么能力都没有的英雄又有什么用呢？）", 3);
            }
            else
            {
                await Popup("背包满了");
                
            }
        }
        // Quest: 21013 
        public async Task q21013s()
        {
            await AskMenu("英、英雄大人………我一直都很想见你。\r\n#b#L2#（做腼腆状。）#l");
            if (await SayAcceptDecline("我从很久以前就想送英雄大人一件礼物……既然今天遇见了英雄，不知英雄能否赏脸收下我这份薄礼？"))
            {
                await forceStartQuest();

                await SayNext("礼物的各个部分都装在附近的一个盒子里。对不起，麻烦你了，你能不能把盒子弄坏，给我拿个 #b#t4032309##k 和 #b#t4032310##k? 我马上给你组装好.", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face);
                await guideHint(18);
            }
        }
        // Quest: 21013 
        public async Task q21013e()
        {
            if (await AskYesNo("材料都拿来了吗？请稍等。这么混合一样……\r\n\r\n#fUI/UIWindow.img/QuestIcon/4/0#\r\n#v3010062# #t3010062# 1个\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 95 exp"))
            {
                await forceCompleteQuest();
                await gainExp(95);
                await gainItem(4032309, -1);
                await gainItem(4032310, -1);
                await gainItem(3010062, 1);

                await SayNext("好了，椅子做好了！嘿嘿！就算是英雄肯定也会有需要歇歇的时候，所以我一直想送你一把椅子。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face);
                await SayNext("我想就算是英雄也不能永远活力充沛，肯定也有疲劳、困倦的时候。但真正的英雄是能够克服万难取得最后胜利的。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face);

                await guideHint(19);
            }
        }
        // Quest: 21015 
        public async Task q21015s()
        {
            await SaySpeech([
                "好了，说明到这里就告一段落，我们要进入下一阶段了。下一阶段是什么？刚才我已经说过了。就是不断地磨练自己，直到你拥有足以战胜黑魔法师的实力。",
                "虽然在几百年前你确实是英雄，但这毕竟是很久以前的事情了。就算没有黑魔法师的诅咒，在冰块里封冻了那么久，身体筋骨什么的也没那么灵活了吧？首先要做些准备活动。想知道是怎么样的准备活动？",
                ]);
            if (await SayAcceptDecline("体力是革命的本钱。英雄也要从基础体力开始训练！……那句话你也知道吧？当然要从#b基础体力锻炼#k开始练起……啊，你可能不记得了。不过也没关系。尝试一下你就明白了。现在就开始基础体力锻炼吧？"))
            {
                await forceStartQuest();

                await SayNext("在这个几乎全是企鹅的岛上，也有几只怪兽。去村子右边的#b#m140020000##k，就能看到许多#o0100131#。请消灭#r10只#o0100131##k。我们这些笨拙的企鹅用喙都能抓到的#o0100131#，你总不能还抓不到吧？");
                await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            else
            {
                await SayNext("别觉得自己原来是英雄就了不起了。冰冻三尺非一日之寒。要想变得足够厉害，就赶紧开始磨练自己吧。");
            }
        }
        // Quest: 21016 
        public async Task q21016s()
        {
            if (await SayAcceptDecline("开始基础体力锻炼吧？准备好了？再确认一下剑是否装备好了？技能和药水是否已经拖到了快捷栏中？"))
            {
                await forceStartQuest();
                await SayNext("很好。先从比#o0100131#稍微厉害一点的怪兽#r#o0100132##k，开始狩猎吧。去#b#m140020100##k抓获#r15只#k左右就行，这将对你的体能提高大有帮助。体力就是冒险的根本！赶紧出发吧！");
                await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            else
            {
                await SayNext("还没做好打猎#o0100132#的准备吗？最好在出发前做好万全的准备。别因为准备不充分而中途挂掉。");
            }

        }
        // Quest: 21017 
        public async Task q21017s()
        {
            await SaySpeech([
                    new SpeechText("现在，身体筋骨差不多舒展开了吧？这种时候还要进一步加强训练强度才能保证锻炼出过硬的基础体力。来吧，继续基础体力的锻炼吧。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("这次去#b#m140020200##k消灭#r#o0100133##k试试看。大概消灭#r20只#k就行，将会对你的体力增长大有帮助。快去快去……咦？你有什么要说的吗？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("……为什么要消灭的怪兽数量越来越多了？", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("是要越来越多。难道说20只还不够吗？要不消灭100只试试？哦，这还不够，索性不如像林中之城的谁那样，一口气消灭999只怪兽试试……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("不，不用了，这样就够了。", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await SayAcceptDecline("哎呦哎呦，用不着这么谦虚。我充★分★理解英雄大人渴望赶紧变得厉害起来的心情。真不愧是英雄大人……"))
            {
                await forceStartQuest();
                await SaySpeech([
                    new SpeechText("#b（再这么说下去，搞不好真得让我去消灭999只怪兽了，赶紧接受任务得了。）", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("那就拜托你消灭20只#o0100133#。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                ]);

                await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow3");
            }
            else
            {
                await SayNext("#b（怀着沉重的心情，没有说出拒绝的话。现在临阵脱逃也不可能……镇定一下情绪，再说两句。）", NpcTalkSpeaker.PlayerRight);
            }
        }
        // Quest: 21018 
        public async Task q21018s()
        {
            await SayNext("来，让我测试一下你至今为止的基础体力训练成果。测试方法很简单。这座岛上有一种最强悍凶猛的怪兽，叫#o0100134#，你只要击退它就可以！要是能击退#r50只#k就最好了……");
            if (await SayAcceptDecline("不过#o0100134#的数量本来就不多，杀掉那么多恐怕不利生态平衡的保持，你消灭5只就差不多了。你看，这训练与自然环境之间是多么地和谐啊！真是完美啊..."))
            {
                await forceStartQuest();
                await SayNext("#o0100134#在岛的较深处。村子左边的路一直走，就能看到#b#m140010200##k，请去那里消灭#r5只#o0100134##k。");
                await showInfo("Effect/OnUserEff.img/guideEffect/aranTutorial/tutorialArrow1");
            }
            else
            {
                await SayNext("哎哟，难道说你觉得5只太少了？如果你愿意加强锻炼，多消灭一些也没关系的。既然是英雄大人的心愿，我就睁一只眼闭一只眼吧，虽然很可惜那些怪兽...");
            }
        }
        // Quest: 21100 
        public async Task q21100s()
        {
            await SaySpeech([
                    new SpeechText("与黑魔法师战斗的英雄们……有关他们的信息几乎什么都没留下。即使在预言书中也只记载了5位英雄，也没有任何有关他们外貌的描述。你还能记起来些什么吗？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("一点都想不起来了……", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("果然，黑魔法师的沮咒果然很厉害。不过，作为英雄的你肯定和过去应该还会存在某个联系点的。会是什么呢？武器和衣服是不是在战斗中都遗失了呢……啊，对了，应该是#b武器#k！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("武器？", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("以前，我们在冰窟中挖掘英雄的时候，发现过一个巨大的武器。我们猜测可能是英雄使用的武器，所以就放在了村子中央。你来来去去的时候没看到吗？#b#p1201001##k……\r\r#i4032372#\r\r大概长这样……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("确实，那个#p1201001#在村子里，看起来是有些奇怪。", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await SayAcceptDecline("没错，就是那个东西。据说英雄的武器是会挑选主人。如果你就是使用#p1201001#的英雄，那么在抓住#p1201001#的刹那，武器应该会有反应的。快去点击#b#p1201001#试试#k。"))
            {
                await forceStartQuest();
                await SayOK("如果#p1201001#有反应，就说明你是使用过#p1201001#的英雄，是#b战神#k。", 8);
                await showIntro("Effect/Direction1.img/aranTutorial/ClickPoleArm");
            }
            else
            {
                await SayNext("你还在犹豫什么？就算#p1201001#没有反应，我也没什么好失望的。快去抓住#p1201001#试试吧。需要在武器合适的地方#b点击#k才能。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face);
            }
        }
        // Quest: 21101 
        public async Task q21101s()
        {
            await SaySpeech([
                    new SpeechText("#b(抚摸着#p1201001#，本来冰凉的战斧上却传来了温暖的感觉，令我似乎想起了些过去的事情。)#k", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText("#b(...使用战斧的英雄曾经是一位拥有强劲力量和充沛体力，能够自由使用多种技能进行近身战的战士...)#k", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText("#b(...虽然拥有很高的STR，但还有一些DEX，所以并非全靠力气吃饭...)#k", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                ]);

            if (await SayAcceptDecline("#b(这是我自己的记忆吗？还是对同伴英雄的记忆呢？...还得再摸一次#p1201001#试试看。)#k"))
            {
                await forceStartQuest();

                await SayNext("#b(我就是这柄战斧曾经的主人？如果确信无疑，请再次抚摸#p1201001#。)#k");
            }
            else
            {
                await SayNext("#b(请慎重考虑清楚。)#k");
            }
        }
        // Quest: 21101 
        public async Task q21101e()
        {
            if (getPlayer().getJob() == Job.LEGEND)
            {
                await changeJobById(JobId.ARAN1);
                await resetStats();

                if (YamlConfig.config.server.USE_FULL_ARAN_SKILLSET)
                {
                    await teachSkill(21000000, 0, 10, -1);   //combo ability
                    await teachSkill(21001003, 0, 20, -1);   //polearm booster
                }

                await completeQuest();

                //getPlayer().changeSkillLevel(SkillFactory.getSkill(20009000), 0, -1);
                //getPlayer().changeSkillLevel(SkillFactory.getSkill(20009000), 1, 0);
                //showInfo("You have acquired the Pig's Weakness skill.");
                await SayOK("#b(你可能开始记起某些事了……)#k", 3);
            }
        }
        // Quest: 21200 
        public async Task q21200s()
        {
            if (await SayAcceptDecline("修炼进展得如何？哟，等级升得这么高了？难怪人们都说济州岛是养马的天堂，金银岛是升级的天堂...对了，现在还不是说闲话的时候。能否麻烦你回岛上来一趟？"))
            {
                await startQuest();

                await SayOK("#b保管在#m140000000##k的你的#b#p1201001##k突然出现了奇怪的反应。据说长矛在呼唤自己主人的时候才会发出那样的反应。#b也许有什么事情要转达给你？#k请速回岛上一趟吧。");
            }
        }
        // Quest: 21200 
        public async Task q21200e()
        {
            await SaySpeech([
                    new SpeechText("嗡嗡嗡嗡嗡……", NpcTalkSpeaker.ExtraNpc, 1201001),
                    new SpeechText("#b（以前没见过他啊？怎么看起来不太像人类？）", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("喂！战神！还听不见我的声音吗？到底听不听得见？唉，烦死了！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（咦？这是谁的声音？怎么听起来像个凶巴巴的少年……）", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("唉……哪有这样的主人啊？丢开武器在冰窟里睡了几百年，现在连话都听不懂了……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("你是谁啊？", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("啊，战神？现在听到我的声音了？是我啊，不记得我了？我就是武器#b长矛 #p1201002##k啊？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（……#p1201002#？#p1201001#会说话？）", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("不至于吧？这么吃惊？再怎么失忆，总不能连我都忘了吧？太不够意思了！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("不好意思，真的一点都想不起来。", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await AskYesNo("说声不好意思就能算了？！几百年来就我一个人孤苦伶仃地，有多寂寞你知道吗？不管怎样，你快点给我想起来！"))
            {
                await completeQuest();

                await SayNext("#b（一口一个自己是#p1201001#、#p1201002#的，还越说越生气了。再这么说下去也不会有啥进展，还是先走到 #p1201000#跟前，好好商量商量。）", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd);
            }

        }
        // Quest: 21201 
        public async Task q21201e()
        {
            await SaySpeech([
                    new SpeechText("曾经是谁说要让我成为击退黑魔法师的传世武器的？结果中了诅咒，自顾自地沉睡了几百年，把我丢在一边不管不顾。……什么？想不起来了？一句想不起来了就想了事？当初苦苦求我，拜托我的时候怎么说的来着……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("说要向#p1203000#证明自己的实力，请他给我一个机会。", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("没错！当初为了得到我，你低三下四地苦苦哀求。要知道像我这样优秀的武器，你上哪里去找啊？能够和黑魔法师相抗衡的最强的巨大的战斧就是我了！结果你却把我扔在冰窟里，一放就是几百年……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("倒也没有苦苦哀求。", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("什么？没有苦苦哀求？是谁哭着闹着要得到我，甚至不惜双膝下跪苦苦哀求？要不#p1203000#怎么会答应……等等？战神！你难道……难道已经想起来了？！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("有一点点……", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("……真不愧是战神啊……呜，呜呜！不，我一点都没有感动！……虽然中了黑魔法师的诅咒，能力尽失，连拿起我的力气都没有了……即便如此，你居然还能想起我，真不愧是我的主人啊！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                ]);
            if (await SayAcceptDecline("就算你失去记忆也还是我的主人。那经过极端训练的身体依然能够记得当初的技能，虽然在冰窟中沉睡了数百年，但这一点我还是很清楚的。好吧，我帮你唤醒你的能力！"))
            {
                if (!isQuestCompleted(21201))
                {
                    await changeJobById(JobId.ARAN2);

                    if (YamlConfig.config.server.USE_FULL_ARAN_SKILLSET)
                    {
                        await teachSkill(Aran.POLEARM_MASTERY, 0, 20, -1);   //polearm mastery
                        await teachSkill(Aran.FINAL_CHARGE, 0, 30, -1);   //final charge
                        await teachSkill(Aran.COMBO_SMASH, 0, 20, -1);   //combo smash
                        await teachSkill(Aran.COMBO_DRAIN, 0, 20, -1);   //combo drain
                    }

                    await completeQuest();

                    await SayNext("你的等级还没有以前那么高，没法帮你唤醒所有的能力。不过，先帮你唤醒一部分的能力，这样将来升级也会更快一些。快点去训练吧，这样你才能恢复成以前的自己。");
                }
            }
        }
        // Quest: 21202 
        public async Task q21202s()
        {
            await SaySpeech([
                    new SpeechText("呵呵……年轻人来这么偏僻的地方干嘛？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("我想要最厉害的长矛！", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("最厉害的长矛？那种东西在小村子里怎么有卖的……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("我知道你就是冒险岛世界里最厉害的铁匠！我想要你做的武器！", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await SayAcceptDecline("我这个老头子太老了，哪还能做什么优秀的武器啊。倒是有一支很久以前做的长矛……不过却不能给你。那个家伙太锋利，弄不好连主人都会被伤到。这种武器你还想要吗？"))
            {
                await startQuest();

                await SayOK("呵呵……既然你这么说，我这个老头子就试一试你。你去旁边的#b修炼场#k，打败那些#r#o9001012##k，取回#b#t4032311#30个#k给我。", 8);
            }
        }
        // Quest: 21202 
        public async Task q21202e()
        {
            await SaySpeech([
                    new SpeechText("哎呦～#t4032311#都取回来了？你………比我想象的还要厉害一些嘛。不过，对于随时都可能伤到自己的危险武器，你那种毫不畏惧的爽朗豪放的心态实在是……好吧，#p1201001#就给你了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                    new SpeechText("#b（过了好一会儿，#p1203000#才郑重地将裏在布里的#p1201001#交给我。）", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd),
                ]);
            if (await AskYesNo("这就是专门为你而做的长矛，名叫#p1201002#……以后就拜托了。"))
            {
                await completeQuest();
                await removeAll(4032311);
            }

        }
        // Quest: 21300 
        public async Task q21300s()
        {
            await SayNext("修炼得如何？嗯...70级了...虽然还不够，不过比起当初把你刚从冰川里挖出来的那个状态要强百倍了。像这样下去，很快你就能恢复从前的实力了。");
            if (await SayAcceptDecline("在这之前，请回来#m140000000#一趟。#b你的战斧又出现了奇怪的反应。似乎有什么事情要跟你说的样子。#k说不定能进一步唤醒你的能力呢，赶紧回来看一眼吧。"))
            {
                await forceStartQuest();

                await SayOK("不管怎样，拥有意识的武器还是很厉害的，某个方面来看，那家伙有种很神圣的感觉。如果不听它的，它就会呜呜哭...啊，这种话可一定要对战斧保密。我可不想让它吵得更凶。");
            }
            else
            {
                await SayOK("你知道吗？不要以为先升级升到70级，以后再转职也可以。到时候辛辛苦苦累计的SP值没法用于3转技能，你就傻眼了。当然，我也不是说#p1201001#非要给你转职不可...只是提前说明一下，供你参考。");
            }
        }
        // Quest: 21301 
        public async Task q21301e()
        {
            if (await AskYesNo("抓到#o9001013#了吗？呵呵呵……真不愧是我的主人！很好，把你找到的#t4032312#给我吧……啊……？怎么不说话？难道……你没找到？"))
            {
                await SaySpeech([
                    new SpeechText("什么？！没找到#t4032312#？怎么会这样？！你忘了吗？啊，怎么……再怎么被黑魔法师诅咒，再怎么被封冻几百年也不能让你变得这么笨啊……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("不行。我不能绝望。主人糊里糊涂，我可不能糊涂……深呼吸……深呼吸……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("再去找找，反正小偷已经逃走。看来必须重新做#t4032312#了。以前也做过一次，你知道需要哪些材料吧？快去搜集材料……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("   #i4001173#", 0),
                    new SpeechText("……彻底没希望了。啊啊啊！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（#p1201002#正在气头上。先撤再说。说不定#p1201000#能给我什么帮助。）", NpcTalkSpeaker.PlayerRight),
                ]);

                await completeQuest();
            }
        }
        // Quest: 21302 
        public async Task q21302e()
        {
            await SayNext("啊！这、这是……你终于想起制作红珠玉的方法了？啊啊……这就是为什么你再笨再健忘，我也依然对你不离不弃……哎呀，现在不是说这些的时候！快把宝石给我！"); //Giant Polearm
            if (await AskYesNo("好了，红珠玉的力里应该可以恢复了，你的能力也需要再唤醒一些。现在你的等级升高了不少，可以被唤醒的能力也更多了！"))
            {
                if (!isQuestCompleted(21302))
                {
                    if (haveItem(4032312, 1))
                    {
                        await gainItem(4032312, -1);
                    }

                    await changeJobById(JobId.ARAN3);
                    if (YamlConfig.config.server.USE_FULL_ARAN_SKILLSET)
                    {
                        await teachSkill(21110002, 0, 20, -1);   //full swing
                    }

                    await completeQuest();
                }

                await SayNext("赶紧恢复以前的能力吧，带上我一起去冒险……");
            }
        }
        // Quest: 21303 
        public async Task q21303s()
        {
            await SaySpeech([
                    new SpeechText("呜呜～#p1203001#很难过。#p1203001#很生气。#p1203001#很想哭……呜呜呜呜～", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("怎、怎么了？", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("#p1203001#做好了宝石。#b像苹果一样的红宝石#k。结果#r小偷#k却把宝石给偷走了。#p1203001#宝石没了。#p1203001#好难过……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("你说小偷偷走了红宝石？", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await SayAcceptDecline("对。必须找回#p1203001#宝石。你要是能帮我找回#p1203001#宝石，我会好好答谢你的。要是帮我抓到小偷，我也会答谢你的。"))
            {
                await forceStartQuest();

                await SayOK("小偷往那个方向去了。那个方向是……吃饭的手是右手，不吃饭的手是左手……#b左边#k！往左边去就能抓到小偷。");
            }
        }
        // Quest: 21400 
        public async Task q21400s()
        {
            if (await SayAcceptDecline("修炼进行得顺利吗？对不起，我知道你很忙，但是请你马上回#m140000000#一趟。#p1201002#又出现了异常的反应...对不起。这次的反应和过去的完全不同。感觉更深、更黑暗..."))
            {
                await startQuest();

                await SayOK("有种不祥的预感...请速速回去。虽然我从来没有见过#p1201002#，也没听过它的声音...不过我可以感觉到它的痛苦。只有#b#p1201002#的主人，你才能解决它的问题#k！");
            }
            else
            {
                await SayOK("我不是在开玩笑！真的很奇怪...#p1201002#肯定是出了什么事了！");
            }
        }
        // Quest: 21401 
        public async Task q21401s()
        {
            await SaySpeech([
                "...问我怎么会变成这样？...本来我不太想说的...不，我当然瞒不过主人您了...",
                "...你被封印在冰川里的几百年时间...我也呆在冰川里，没有主人，让我感到非常的孤独...因此心中出现了黑暗。",
                "不过，你重新苏醒后，我心中的黑暗也跟着完全消失了。既然主人回来了，心里也没有什么可难过的了。本以为这样就没事了...没想到这只是我的错觉。"
                ]);
            if (await SayAcceptDecline("拜托你了，战神...请阻止我。能阻止我的暴走的，只有主人你。我已经再也无法抑制了！请你一定要把暴走的我打倒！"))
            {
                var em = GetSoloQuestEventManager(21401);
                var r = await em.StartInstance(getPlayer());

                if (r == Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await startQuest();
                }
                else
                {
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
            }
        }
        // Quest: 21401 
        public async Task q21401e()
        {
            await SayNext("谢谢你，战神。多亏了你，才阻止了我的暴走。真是万幸……以主人的实力，这点小事当然不在话下了！");
            if (await AskYesNo("现在来看，你的等级已经很高了。既然能够打倒暴走状态下的我……那么唤醒你过去全部的力量也应该是可以的了。"))
            {
                if (!isQuestCompleted(21401))
                {
                    if (!canHold(2280003, 1))
                    {
                        await SayOK("你的#b物品栏#k已满. 请留出多余的空间来获取物品.");
                        return;
                    }

                    await gainItem(2280003, 1);
                    await changeJobById(JobId.ARAN4);

                    await completeQuest();
                }
                await SayNext("沉睡的技能全都唤醒了……毕竟好久没用了，还需要熟悉熟悉。不过，应该进步会很快的。");
            }
        }
        // Quest: 21600 
        public async Task q21600s()
        {
            if (await SayAcceptDecline("英雄，你好！我是看管雪橇哈士奇犬的#p1202007#。不好意思打扰你了，只是能够帮助我的只有英雄你一人了...如果你不是太忙的话，能否听听我的苦衷？"))
            {
                await forceStartQuest();

                await SaySpeech([
                    "就是不久前的事情。我像平时一样照看可爱的哈士奇犬们，却发现有个奇怪的家伙夹在他们中间。毛色光泽都很不一样，牙齿也锐利很多……怎么看都不像一只哈士奇犬。",
                    "我开始还以为是只变种的哈士奇犬。后来一查才发现那家伙不是哈士奇，而是只#b狼#k！里恩岛上根本没有狼，也不知道是从哪里混进来的……很奇怪不是吗？",
                    "我也知道不能把狗和狼一起养，但这小狼崽才刚刚出生，丢掉又太不近人情了。再加上小狼崽的身体还很弱。所以，我打算把这只小狼崽养到他能自食其力的大小。",
                    "虽然我很精通犬类的饲养，但如何养狼却是一窍不通。所以必须找人帮忙。#b#m230000000#的某个地方#k住着一个叫#b#p2060000##k的人，懂得饲养狼的办法。所以想请英雄去见见她，请求她的帮助。谢谢你了。",
                    "得到#p2060000#的同意后，她应该会给你一个东西，你把那个东西带回来给我就行。我的家就在里恩村旁边，#b#m140020100##k附近。"
                    ]);
            }
        }
        // Quest: 21604 
        public async Task q21604s()
        {
            await SaySpeech([
                    "啊呀，你带着的不是狼吗？我已经好久都没见过带狼的人了。不过，带着狼却不#b骑乘#k，难道你还不会骑乘之术吗？",
                    "所谓骑乘，就是一种骑在狼背上快速行进，并能和狼之间实现良好沟通的技术。我曾经骑过#o5130104#和#o5140000#，当时我可帅呢！",
                ]);
            if (await SayAcceptDecline("你想学习骑狼吗？如果你想学的话，我#p2020007#可以帮助你。"))
            {
                await forceStartQuest();

                await SaySpeech(["要想骑乘，没有任何准备，直接骑在狼背上是很困难的。要先弄个#b#t1912011##k，这样才能让狼不觉得难受。我会做狼鞍，你去找材料就好。",
                "制作#t1912011#的材料是#b#t4000048##k。大概#b50张#k就够了。等你把材料都找齐了，我就把骑乘的技巧和#t1912011#一起传授给你。赶紧去找材料吧。我也很期待啊。"]);

            }
        }
        // Quest: 21613 
        public async Task q21613s()
        {
            await SaySpeech([
                    new SpeechText("我们是一群狼，在寻找我们丢失的孩子。我听说你在照顾我们的孩子。我们很感激你的好意，但现在是时候把孩子还给我们了.", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("狼人也是我的朋友，我不能就这样放弃一个朋友", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                ]);
            if (await SayAcceptDecline("我们明白，但我们不会丢下我们的小狗离开。这样吧，我们来考验你，看看你是否有资格养一只狼。#r狼的考试#k"))
            {
                var em = GetSoloQuestEventManager(21613);
                var r = await em.StartInstance(getPlayer());

                if (r == Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await startQuest();
                }
                else
                {
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
            }
        }
        // Quest: 21618 
        public async Task q21618s()
        {
            await SaySpeech([
                    new SpeechText("哦，这只你的狼朋友。。。你看，我感觉到他皮毛背后隐藏着某种力量。等等，主人，如果我醒来，那是隐藏的力量?", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("等等，你能做到吗?", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                ]);
            if (await SayAcceptDecline("很惊讶吧？冰河中冰冻的时间是否也妨碍了你的感官？当然，为什么！准备好了告诉我!", 9))
            {
                await forceStartQuest();
            }
        }
        // Quest: 21618 
        public async Task q21618e()
        {
            if (!haveItemWithId(1902017, false))
            {
                await SayNext("在开始进化之前，你必须先解开狼的尾巴.");
                return;
            }

            await SayNext("让开，瞧瞧，玛哈的威力!!");

            await forceCompleteQuest();

            await gainItem(1902017, -1);
            await gainItem(1902018, 1);
        }
        // Quest: 21700 
        public async Task q21700s()
        {
            await SaySpeech([
                    new SpeechText("你似乎在回想什么。这个长矛果然认出了你。那么你肯定就是#b使用长矛的英雄，战神#k了。你想起什么其他的了吗？有关长矛的技能之类……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（说技能倒是想起来了几个。）#k", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("虽然数量不多，不过也已经很不容易了。现在让我们集中精力来恢复过去的技能吧。虽然你失忆了，但毕竟是以前曾经烂熟于心的东西，要恢复起来应该很快。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("怎么恢复过去的技能？", NpcTalkSpeaker.PlayerRight)
                ]);
            if (await SayAcceptDecline("那个......办法只有一个。就是修炼！修炼！修炼！只有不停地修炼才能找回曾经忘却的身体感觉！"))
            {
                if (!isQuestStarted(21700) && !isQuestCompleted(21700))
                {
                    await gainItem(1442000, 1);
                    await forceStartQuest();
                }

                await SayNext("武器要是能使得更熟练就好了。送你一支#b长矛#k，希望你在修炼的时候能够进步得更快。带着这支长矛……");
            }
            else
            {
                await SayNext("什么？你不愿意？打算一个人修炼？有人指导的效果可比自己慢慢摸索的效果好很多哦。再说，你也该学习学习如何与人打交道了。");
            }
            
        }
        // Quest: 21703 
        public async Task q21703s()
        {
            await SaySpeech([
                    new SpeechText("……现在你的能力是什么程度，我大概了解了……呵呵……没想到我这把老骨头还能有今天……真是感动得要流眼泪……不，是鼻涕……", 0),
                    new SpeechText("#b（……也没怎么修炼嘛……？）#k", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("好，现在让我们开始第三阶段，也是最后一阶段的锻炼。这次修炼的对象是……#r#o9300343##k！猪猪！你了解他们吗?", 0),
                    new SpeechText("一点点……", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("他们是天生的战士！从出生的那一刻起，对食物就充满了无穷的愤怒，凡是他们经过的地方都不会留下任何食物。很可怕吧？", 0),
                    new SpeechText("#b（他不是在开玩笑吧？）#k", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await SayAcceptDecline("来，快点#b再次进入修炼场#k，去和那些天生的战士们——#o9300343#战斗吧，打倒#r30只#k后，你的能力将会有一个质的飞跃。全力以赴地去战斗吧！超越我这个教练！"))
            {
                await forceStartQuest();
                await SayOK("快走吧！去打倒那些#o9300343#!");
            }
        }
        // Quest: 21703 
        public async Task q21703e()
        {
            await SaySpeech([
                    new SpeechText("啊，你打败了所有30个#o9300343#后又回来了。我知道你有这个能力……尽管你没有记忆，能力也很少，但我还是能看出你与众不同！怎么会这样？因为你显然带着长柄武器！", 0),
                    new SpeechText("#b（他在开玩笑吗？）#k", NpcTalkSpeaker.PlayerRight),
                ]);
            if (await AskYesNo("我没什么可教你的了，因为你的技术已经超越了我。现在就走吧！别回头！老头很高兴能成为你的导师。"))
            {
                if (isQuestStarted(21703))
                {
                    await forceCompleteQuest();
                    await teachSkill(21000000, (sbyte)getPlayer().getSkillLevel(21000000), 10, -1);   // Combo Ability Skill
                    await gainExp(2800);
                }

                await SaySpeech([
                    new SpeechText("（你还记得#b连击#k！一开始你对这项训练持怀疑态度，但是，它非常有效！）", NpcTalkSpeaker.PlayerRight),
                    new ("现在向#p1201000#汇报。我知道当她看到你所取得的进步时，她一定会很高兴！", 0)
                    ]);
            }
        }
        // Quest: 21704 
        public async Task q21704s()
        {
            await SaySpeech([
                    new SpeechText("修炼进行的不错吧？#p1202006#个性很强……他对战神的技能确实很有研究，对你应该能帮上不少忙。", 0),
                    new ("#b（告诉他自己回想起来连击能力这个技能。）#k", NpcTalkSpeaker.PlayerRight),
                    new ("是吗！看来除了#p1202006#的训练方式之外，你自己仍然记的从前的那些技能也很关键啊……看来只是在这里冰冻的太久，需要时间恢复而已。#b继续加油训练吧，争取早日恢复所有的技能！#k\r\n\r\n#fUI/UIWindow.img/QuestIcon/8/0# 500 exp", 0)
            ]);

            await forceCompleteQuest();
            await gainExp(500);
        }
        // Quest: 21712 
        public async Task q21712s()
        {
            await SayNext("#t4032315#总是会#b发出奇怪的声音#k。当然，你是听不见的。因为那是只有#o1210102#才能听到的声音。在这种声音的影响下，#o1210102#的性格似乎发生了变化。");
            if (await SayAcceptDecline("性格变得怪异的#o1210102#开始和没有发生变化的#o1210102#战斗，这导致射手村附近的#o1210102#全部变得残暴起来了。#b使得最近怪物们发生了变化的根源就是这个人偶！#k你明白了吧？"))
            {
                await forceStartQuest();
                await SaySpeech([
                        new SpeechText("怎么会发生这种事情呢......这种人偶不可能是自然形成的，一定是有人故意而为......看来要对#o1210102#的状态观察一段时间了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                        new SpeechText("#b（射手村周边的#o1210102#发生变化的原因找到了。现在把搜集到的信息告诉#p1002104#吧。）#k", NpcTalkSpeaker.PlayerRight),
                    ]);
            }
            else
            {
                await SayNext("你还是不明白怎么回事？如果你再跟我说一次，我再跟你解释一遍。");
            }
        }
        // Quest: 21716 
        public async Task q21716s()
        {
            await SaySpeech([
                    new SpeechText("#p1032112#说了什么？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（#p1032112#告诉我，不久前，有个奇怪的孩子来到这里，那个孩子手上好像拿着人偶。好像从那之后魔法森林中就出现了奇怪的怪物......）#k", NpcTalkSpeaker.PlayerRight),
                    ]);
            if (await SayAcceptDecline("吼...抱着人偶的小孩...不得不叫人怀疑。是有人故意把怪物变成为凶暴的证据啊..."))
            {
                await forceStartQuest();
                await SaySpeech([
                    new SpeechText("魔法森林的和平已经被打破......这种恶行绝对不能饶恕......看来我得提醒村民们最近一定要多加小心。", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("#b（不过话说回来，村庄的人们真的有办法应对残暴的绿蘑菇吗？应该会很辛苦的说...既然找出引起绿蘑菇暴走的原因，现在把搜集到的情报告诉#p1002104#吧。）#k", NpcTalkSpeaker.PlayerRight),
                    ]);
            }
        }
        // Quest: 21719 
        public async Task q21719s()
        {
            await SaySpeech([
                    new SpeechText("莫非你是前不久在#m101000000#的那个人？终于找到你了！我找你找得好辛苦，知道吗？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("你是谁？", NpcTalkSpeaker.PlayerRight),
                    ]);
            if (await SayAcceptDecline("我？你想知道的话就来我的洞窟吧。我想好好招待你一番。点击接受按钮就能立刻移动到我家。我在那里等你。"))
            {
                await forceCompleteQuest();
                await warp(910510200, 0);
            }
        }
        // Quest: 21720 
        public async Task q21720e()
        {
            await SaySpeech([
                    new SpeechText("有什么事吗？你不是一直在金银岛上修炼吗？真相叔叔还带口信说你帮了大忙了。……什么？黑色之翼？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（讲述有关人偶师、黑色之翼，以及黑色之翼的目的事情。）#k", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("原来是这样……叫黑色之翼啊。居然还有这么一帮人……明知很危险也要在冒险岛世界里复活黑魔法师，太不像话了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("是，是啊……\r\r#b（他的语气突然变得很决断，好可怕。）#k", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("预言里只说到英雄会苏醒，与黑魔法师战斗。但我还一直半信半疑，这下我才明白黑魔法师是真的存在的。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("是不是很可怕？", NpcTalkSpeaker.PlayerRight),
                    ]);
            if (await AskYesNo("有什么可怕的？管他是什么黑魔法师还是什么别的，你都会将他们一一打倒的，还有什么好担心的呢？我们只会觉得斗志更加高涨。啊，对了，我发现了这个#b技能#k……看一眼吗？"))
            {
                if (getQuestStatus(21720) == 1)
                {
                    await forceCompleteQuest();
                    await teachSkill(21001003, (sbyte)getPlayer().getSkillLevel(21001003), 20, -1);
                    await gainExp(3900);
                }

                await SaySpeech([
                        new SpeechText("#b（你还记得快速矛的技能！）#k", NpcTalkSpeaker.PlayerRight),
                        new SpeechText("这个技能是在一个古老的神秘书籍中发现的。我有预感这可能是你过去用过的一种技能，我想我是对的。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText("你正在渐渐地变得强大起来。我会让你强大起来而倾尽全力帮助你的。有什么好害怕的呢？千万不能退缩。我们为了打败黑魔法师不是已经等待了数百年了吗？这次一定会取得胜利的！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                        new SpeechText("呐，为了这个目标必须继续修炼！修炼明白吗？前往金银岛继续修炼吧。一定要练到能打败黑魔法师的程度才行！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    ]);
            }
        }
        // Quest: 21729 
        public async Task q21729s()
        {
            await SayNext("好，你不应该回#b特鲁#k那儿获取下一步的信息。……哦，等等！我想起了一些事情。看到那边的#r#p1061006##k了吗？那座石像的来源不明，上面写着一些东西，可能是洞穴的暗号？#r在那里获取暗号#k，可能会对你这一趟有帮助。");

            await forceStartQuest();
        }
        // Quest: 21733 
        public async Task q21733s()
        {
            await AskMenu("喂，你在哪？有一件急事！\r\n#b#L0#(喂……？#p1002104#以前不是叫我英雄的吗……)#l\r\n#k");
            if (await SayAcceptDecline("我有很重要的情报！赶紧到#b#m104000004##k来！"))
            {
                await forceStartQuest();
            }
            else
            {
                await SayOK("你说什么呢？很着急！别废话赶紧来！");
            }
        }
        // Quest: 21733 
        public async Task q21733e()
        {
            await SaySpeech([
                    new SpeechText("啊……没想到还会碰上这种事情。怎么都没想到人偶师还会潜伏到这里来。平时大概是疏于修炼了，完全被对方给算计了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("对不起，都是因为我……", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("啊？英雄大人不必内疚。你也不知道那家伙会来啊。不必道歉。不过，这也暴露出了他们的弱点。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("弱点？", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("人偶师讨厌的那个文件。如果那个文件是假的，人偶师是不会这么兴师动众，带着一群人跑来折腾的。那个文件充分证明了黑色之翼的目标其实是金银岛封印石。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("话虽这么说，但我的位置也暴露了。", NpcTalkSpeaker.PlayerRight),
                    ]);
            if (await AskYesNo("别担心。这次我为了拿利琳寄过来的文件才出去的，没想到中了敌人的招。平时我不会这么不小心的。好歹也是个情报商人，总会为自己准备一条退路的。现在关键的是#b精准矛#k这个技能你知道吗？"))
            {
                await gainExp(8000);
                await teachSkill(21100000, 0, 20, -1); // polearm mastery

                await forceCompleteQuest();

                await SayOK("黑色之翼再怎么兴风作浪也没法阻止你日益强大起来。继续努力，直到击败黑魔法师为止。我也会尽最大努力为你收集信息的。");
            }
        }
        // Quest: 21734 
        public async Task q21734s()
        {
            await SaySpeech([
                "很忙吗，英雄大人？前不久我使尽各种手段在金银岛上四处探查，终于找到了一个有意思的情报。是关于人偶师的……",
                "你知道吗？自从你教训了#o9300346#之后，火独眼兽洞穴里的入口不就不能用了吗？#o9300346#那个家伙，好像又在别的地方建立了根据地。",
                "在#m105040300#的#b#m105040200##k，有人看见#o9300346#走进了一个#b小木屋#k。情报很可靠。快去那边击退#r#o9300346##k吧。"
                ]);

            await forceStartQuest();
        }
        // Quest: 21734 
        public async Task q21734e()
        {
            await SaySpeech([
                new SpeechText("看样子，你应该已经打败人偶师了……怎么不高兴的样子？发生什么事了？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("没发现任何有关金银岛封印石的情报。", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("啊哈！原来是为这事。呵呵呵……完全不用担心。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    ]);
            await forceCompleteQuest();

            await gainExp(12500);
            await teachSkill(21100005, 0, 20, -1); // combo drain
        }
        // Quest: 21735 
        public async Task q21735s()
        {
            await SaySpeech([
                new SpeechText("#t4032323#我已经找到了。你看，呵呵呵。\r\r\r\r#i4032323#", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("!!\r\r……你怎么找到的？！", NpcTalkSpeaker.PlayerRight),
                    ]);
            if (await SayAcceptDecline("上次被人偶师攻击后，我动员了所有的情报网搜遍了整个金银岛。我怎么可能坐以待毙？一定要抢在他们前面找到他们想要的东西……也算是报了上次一箭之仇。"))
            {
                if (!canHold(4032323, 1))
                {
                    await SayNext("背包中的其他栏至少需要一个空位来接受任务。");
                    
                    return;
                }

                if (!haveItem(4032323, 1))
                {
                    await gainItem(4032323, 1);
                }
                await forceStartQuest();
            }
        }
        // Quest: 21735 
        public async Task q21735e()
        {
            if (haveItem(4032323, 1))
            {
                await SayNext("黑色之翼的动向，我已经从真相叔叔那里听说了。听说前不久还被他们袭击了一次......你还好吧？咦？这个......这就是金银岛封印石吗？没想到真相叔叔果然比那些家伙们早一步找到金银岛封印石。不知道这颗宝石到底有什么用......只知道这个东西肯定和黑魔法师有关。");
                if (await AskYesNo("不知道这颗宝石到底有什么用……只知道这个东西肯定和黑魔法师有关。既然那些家伙在找这个东西，我们一定要保护好这个东西。看来不论发生什么，你都要不断地变得更强，才行。"))
                {
                    await gainItem(4032323, -1);
                    await gainExp(6037);
                    await forceCompleteQuest();
                }
            }
        }
        // Quest: 21736 
        public async Task q21736s()
        {
            await SaySpeech([
                "好久不见了，英雄。这段时间等级上升很快嘛？看来你确实很拼命地在修炼啊。很勤奋。有点英雄的架势了。#p1201000#也会为你开心的吧？",
                "对了，现在不是说这些的时候。我觉得光在金银岛搜集情报似乎情报面太窄，为了拓宽情报面，我已经开始在神秘岛大陆展开搜索。一开始就选择了#b#m200000000##k，没想到那里果然有问题。",
                "在神秘岛大陆的#m200000000#，好像正在发生着什么非比寻常的事。虽然不同于人偶师出现的时候，但总感觉这种奇怪的氛围一定和黑色之翼有关。怎么样，好久没遇到过这么大的事件了。会不会很激动呢？",
                ]);
            if (await SayAcceptDecline("那么你准备好了吗？如果你接受的话，到#m200000000#，找到#b妖精#p2012012##k，向他询问发生在#b#m200000000#的奇怪事情……#k是怎么回事就行了。"))
            {
                await forceStartQuest();
            }
        }
        // Quest: 21738 
        public async Task q21738s()
        {
            await SaySpeech([
                 new SpeechText("你有什么事? 虽然我并不欢迎不速之客......但你的身上却散发一种非比寻常的气息......看来我得听听你的事情了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#b（讲述关于#o9300347#的事情。）", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("#o9300347#? 虽然这的确是个严峻的问题......不过到目前为止应该对#m200000000#还造不成影响。等等，你刚才说#o9300347#在哪儿?", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("在#m200060001#。", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("#m200060001#? #o9300347#居然在那里, 那么你是说有人想要入侵#m920030001#? 到底为什么呢? 是谁? ", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("#m920030001#?", NpcTalkSpeaker.PlayerRight),
                    new SpeechText("......你到底是什么人竟然来问这样的问题? 你先稍等会儿。我要先卜一卦看你是不是值得信任。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(".............", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText(".........................", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    new SpeechText("你, 你......不, 你......完全不同于普通人类。那悠久的岁月, 那可怕的宇宙, 然而你有着再次战胜它们的伟大命运......你到底是谁?", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                    ]);
            if (!IsQuestNotStarted(21738))
            {
                await forceStartQuest();
            }
            await SayNext("......不管是谁都好。占卦已经让我把一切都告诉你了。关于封印的庭院的一切......", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face);
        }
        // Quest: 21739 
        public async Task q21739e()
        {
            await SayNext("那么，你打败巨人了吗？哦，黑色之翼卧底？他抢走了天空之城的封印石？！哦，不。太可怕了！我们需要尽快想个对策！把情况告诉明珠港的特鲁。");
            await forceCompleteQuest();
            await gainExp(29500);
        }
        // Quest: 21740 
        public async Task q21740s()
        {
            await SayNext("你回来了，英雄。在天空之城的事情办得怎么样？确实是和黑色之翼有关吧？为什么表情这么凝重？说来听听。");
            await forceStartQuest();
        }
        // Quest: 21740 
        public async Task q21740e()
        {
            await SaySpeech([
                new SpeechText("啊，很久不见了。战神。这段事件修炼得还好吧？正好我发现了新的技能想叫你回来呢......你回来的正是时候！", 0),
                        new SpeechText("#b（对利琳讲述有关天空之城封印石的事情。）#k", NpcTalkSpeaker.PlayerRight)
                ]);
            await forceCompleteQuest();
            await teachSkill(21100004, 0, 20, -1); // combo smash
        }
        // Quest: 21741 
        public async Task q21741s()
        {
            await SayNext("这段时间升级很快嘛，英雄大人？我终于又发现了和黑色之翼有关的有趣事情了。这一次咱们早点......#b武陵#k这个村子你知道吗？看来你得去一趟那里。");
            if (await SayAcceptDecline("武陵的#b陈道人#k好像已经和黑色之翼相接触。虽然不知道事情是怎么变成这样的，但信息应该准确无误。"))
            {
                await SayNext("你如果准备好的话，#b就请马上去武陵。#k你去查出黑色之翼为什么会和陈道人接触，以及它们之间到底有着怎样的交易。");
                await forceStartQuest();
            }

        }
        // Quest: 21742 
        public async Task q21742s()
        {
            await SaySpeech([
                new SpeechText("虽说也不是什么着急的活儿，不过你这么问总让人觉得有些不爽。我是不是应该让你下次再来找我呢？反正没什么事情，就请让我清净一点行吗？", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("我听说你见过黑色之翼的武士......", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText("啊，你是说一身漆黑，眉宇间皱纹很深的那个男人吗？是见过。不但见过而且他有东西放在我这里，让我转交给武公老头子。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("东西？", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    new SpeechText("嗯，好大一个#b画轴#k塞给我，让我一定要转交。他一脸杀气的，好像我不转交的话，他还会来找我似的。哎呦，真是吓死人了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("然后呢，画轴转交出去了吗？", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd),
                    ]);
            if (await SayAcceptDecline("没有，那个......其实出了点问题......你愿意听我说吗？"))
            {
                await SaySpeech([
                new SpeechText("是这样的，我正在做一种新药水，当时正好在煮草药，结果没想到画轴一下子掉了进去。我虽然以最快速度把它捞了出来，不过画轴浸水后，上面的字都消失了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    new SpeechText("于是我就发愁了，这怎么转交给武公老头子啊。于是我决定先把画轴修复好，再转交给他。正好你能帮我一个忙。画轴上面的字是武陵最有名的画师#b津津#k写的，你去的话，他一定会帮你修复画轴的。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                    ]);
                if (!haveItem(4220151, 1))
                {
                    if (!canHold(4220151, 1))
                    {
                        await SayOK("Please free a room on your ETC inventory.", 9);
                        return;
                    }

                    await gainItem(4220151, 1);
                }

                await forceStartQuest();
            }
        }
        // Quest: 21742 
        public async Task q21742e()
        {
            await SayNext("怎么样？画轴修复好了吗？要不看看这上面都写了些什么？");
            await gainItem(4032342, -8);
            await gainItem(4220151, -1);
            await gainExp(10000);

            await forceCompleteQuest();
        }
        // Quest: 21746 
        public async Task q21746s()
        {
            await SayNext("封印石......那是很久很久以前，由武陵看管的东西......难道说觊觎它的人又出现了......");
            var mapobj = await getWarpMap(925040001);
            if (mapobj.countPlayers() == 0)
            {
                await mapobj.resetPQ(1);

                await warp(925040001, 0);
                await forceStartQuest();
            }
            else
            {
                await SayOK("有人已经在挑战，请等待他挑战结束。");
            }
        }
        // Quest: 21747 
        public async Task q21747s()
        {
            if (await SayAcceptDecline("没想到在数百年的岁月之后，英雄的后裔又重新出现了......也不知道对冒险岛世界师傅还是祸......怎样都无所谓了。好吧......我告诉你有关武陵封印石的事情。"))
            {
                await SayNext("武陵的封印石所在的地方叫做封印的寺院。那里的入口被隐藏在武陵寺院内。你去仔细观察武陵寺院入口处熊猫提着的灯盏。如果能从中找出#b刻有入口字样的灯盏#k，就可以进入封印的寺院了。暗号是#b道可道非常道。#k");
                await forceStartQuest();
            }

        }
        // Quest: 21747 
        public async Task q21747e()
        {
            await SayNext("成功打败了影子武士吗？表情怎么这么凝重......难道说你失败了......");
            await SayNext("原来是这样，武陵的封印石最终还是被抢走了......很遗憾，不过也没办法。我现在也不明白英雄们为什么要把封印石交给武陵。");
            await gainExp(16000);
            await forceCompleteQuest();
        }
        // Quest: 21748 
        public async Task q21748e()
        {
            await SayNext("战神，你平安回来了！在武陵的任务完成的如何了？#r影子武士#k偷袭了武陵并再次偷走了封印石？那真不幸。至少你没有受伤，我很高兴。");
            await SayNext("我研究了一些新的技能，试图帮你找回记忆。好消息的是，我想起了其中一个：#r战神突进#k！有了它，你将能够击退前面的敌人。对你的能力来说是一个很好的提升，对吧？");
            await gainExp(20000);
            await teachSkill(21100002, 0, 30, -1); // final charge

            await forceCompleteQuest();
        }
        // Quest: 21749 
        public async Task q21749s()
        {
            await SayNext("到目前为止，在#r射手村#k和#r艾琳森林#k这两个地区发现了两个封印石。。。事情似乎开始失控了。");
            await SayNext("战神，你现在要做的就是再次通过#b通往艾琳森林的时光之门#k。这次你一定要找回#r艾琳森林的封印石#k。通过我的情报了解到，#b#p2131002##k有关于那个封印石的线索，#r找到她#k。请一定要做到，我们的世界比以前更需要你的帮助！");
            await forceCompleteQuest();
            await gainExp(500);
        }
        // Quest: 21750 
        public async Task q21750e()
        {
            await SayNext("战神，你终于回来了！！！你最近怎么样？这么久你去哪了？我们有很多事情需要帮忙。。。");
            await forceCompleteQuest();
        }
        // Quest: 21753 
        public async Task q21753s()
        {
            await SayNext("战神，我发现了一些令人不安的消息。。。你说你来自东部森林区，对吗？我们追踪并研究了用来支撑进入未来之门的魔法。结果发现那是一种#r时间之门#k。你用的衣服。。。以前从没有人见过。那一定意味着，你一定来自未来。");
            await SayNext("现在关于这个问题：在你的时间轴上似乎丢失了封印石。。。它是一个强大的神器，可以阻止黑魔法师的军队围攻我们的世界。。如果那个封印石消失了，再没有什么能阻止他了。因为这是一件非常重要的事情，所以要从未来找到我的自我。明白了，把我从未来带走吧！");
            await forceStartQuest();
        }
        // Quest: 21754 
        public async Task q21754s()
        {
            if (!canHold(4032328, 1))
            {
                await SayNext("嗯，你需要给信空出一个背包位置。");
                return;
            }

            await SayNext("给，拿着这个。把它交给#r#p1002104##k，里面有一封维护世界和平的信。不要把这个秘密告诉别人。");

            await forceStartQuest();

            await gainItem(4032328, 1);
        }
        // Quest: 21757 
        public async Task q21757e()
        {
            await SayNext("哦，一封写给伦普雷斯的信？贝洛斯家？！");
            await forceCompleteQuest();
            await gainExp(1000);
            await gainItem(4032330, -1);
        }
        // Quest: 21766 
        public async Task q21766s()
        {
            await SayNext("嘿！你能帮我个忙吗？最近 #p20000# 有点奇怪。。。");
            await SayNext("就在最近，他还常常愁眉苦脸地抱怨自己的关节炎，但他突然变得满面笑容!!");
            await SayNext("我觉得那个木箱后面有个秘密。你能偷偷地看一下旁边的木箱吗 #p20000#?");
            await SayNext("你知道 #p20000# 在哪, 是吗？他在右边。一直往前走，直到你看到维京在哪里，然后穿过那条悬挂的鲨鱼和章鱼，你就会看到约翰。盒子应该就在他旁边.");
            await forceStartQuest();
        }
        // Quest: 21766 
        public async Task q21766e()
        {
            await forceCompleteQuest();
            await gainExp(200);
        }
        // Quest: 21767 
        public async Task q21767s()
        {
            if (haveItem(4032423, 1))
            {
                await forceStartQuest();
                return;
            }

            if (!canHold(4032423, 1))
            {
                await SayNext("请确认你的背包里是否还有空位.");
                return;
            }

            await SayNext("#b嗯，盒子里有一种药材。这可能是什么？你最好把这个带给约翰，问他是什么。#k");
            await gainItem(4032423, 1);
            await forceStartQuest();
        }

    }
}