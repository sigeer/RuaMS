using Application.Core.scripting.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 1209000 
        public async Task talkHelena()
        {
            await SaySpeech([
                "醒了？战神？伤口还好吧？……什么？现在的状况？",
                "避难准备都做好了，所有的人都上了方舟。避难船飞行的时候就只有听天由命了，没啥可担心的。准备得差不多就该向金银岛出发了。",
                "战神的同伴们？他们……已经去找黑魔法师了。在我们避难的时候，他们打算阻止黑魔法师的进攻……什么？你也要去找黑魔法师？不行！你伤得太重，跟我们一起吧！"
                ]);
            setQuestProgress(21000, 21002, 1);
            showIntro("Effect/Direction1.img/aranTutorial/Trio");
        }

        // Npc: 1202000 
        public async Task awake()
        {
            if (getPlayer().getMapId() == 140090000)
            {
                if (!containsAreaInfo(21019, "helper=clear"))
                {
                    await SaySpeech([
                        new SpeechText("你终于醒来了！", 8),
                        new SpeechText("……你是？", 2),
                        new SpeechText("我一直在等你，等着你这个和黑魔法师战斗的英雄醒来！", 8),
                        new SpeechText("……你在说什么？你到底是谁？", 2),
                        new SpeechText("等等……我是谁？我怎么什么都想不起来……啊……！头好疼！", 2),
                        ]);
                    showIntro("Effect/Direction1.img/aranTutorial/face");
                    showIntro("Effect/Direction1.img/aranTutorial/ClickLilin");
                    updateAreaInfo(21019, "helper=clear");
                }
                else
                {
                    await SaySpeech([
                        new SpeechText("还好吗？", 8),
                        new SpeechText("我……我什么都记不起来……这是哪里？你又是谁？", 2),
                        new SpeechText("镇静一点。黑魔法师的诅咒让你失去了记忆……不过你用不着担心。你想知道的事情，我都会一一告诉你。", 8),
                        new SpeechText("你是我们的英雄。数百年前，你勇敢地和黑魔法师战斗，并拯救了冒险岛世界。不过，在最后时刻你中了黑魔法师的诅咒，被封冻在冰块里沉睡了好久好久。所以，记忆也渐渐消失了。", 8),
                        new SpeechText("这个地方叫做里恩岛。黑魔法师把你封冻在了这里。在黑魔法师的诅咒下，不论四季变化，这里永远都是冰封雪飘。我们是在冰窟的最深处发现你的。", 8),
                        new SpeechText("我叫#p1201000#，属于里恩一族。里恩一族从很久以前就遵照预言在这里等待着英雄的归来。然后……我们终于发现了你。就在这个地方……", 8),
                        new SpeechText("我是不是一次说了太多？理解起来有些困难？没关系，慢慢你就会明白……#b咱们赶紧回村子里吧#k。回村子的路上，我再慢慢給你解释。", 8),
                        ]);
                    spawnGuide();
                    warp(140090100, 0);
                }
            }
            else
            {
                var option = await AskMenu(
                    "你还有什么疑问吗？如果有的话，我会尽量解释得更清楚。", [
                        "我是谁？",
                        "我在哪里？",
                        "你是谁？",
                        "告诉我我该做什么。",
                        "告诉我关于我的物品栏。",
                        "我如何提升我的技能？",
                        "我想知道如何装备物品。",
                        "我如何使用快捷栏？",
                        "我如何打开可破坏的容器？",
                        "我想坐在椅子上，但我忘了怎么做。"
                        ]);
                switch (option)
                {
                    case 0:
                        await SayNext("你是数百年前拯救冒险岛世界免受黑魔法师侵害的英雄之一。由于黑魔法师的诅咒，你失去了记忆。");
                        break;
                    case 1:
                        await SayNext("这个岛叫做里恩，这就是黑魔法师的诅咒让你沉睡的地方。这是一个被冰雪覆盖的小岛，大部分居民是企鹅。");
                        break;
                    case 2:
                        await SayNext("我是#p1201000#，是里恩一族成员，我一直在等待你的归来，正如预言所言。我将会是你的向导。");
                        break;
                    case 3:
                        await SayNext("我们别浪费时间了，直接去镇上吧。等到了那里我会告诉你详细情况。");
                        break;
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                        guideHint(option + 10);
                        break;
                    default:
                        break;
                }

            }
        }


    }
}
