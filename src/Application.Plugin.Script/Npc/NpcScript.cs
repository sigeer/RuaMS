using Application.Core.Client;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Infrastructure;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;
using Application.Shared.Constants;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Map;
using Application.Shared.GameProps;
using Application.Shared.MapObjects;
using Application.Utility;
using Application.Utility.Exceptions;
using Google.Protobuf.WellKnownTypes;
using Humanizer;
using Microsoft.VisualBasic;
using scripting.npc;
using server.life;
using System.Drawing;
using System.Numerics;
using tools;
using static System.Collections.Specialized.BitVector32;


namespace Application.Plugin.Script
{
    // 未包含的NPC: 1002005, 1012009, 1012118, 1013001, 1013002, 1013104, 1013200, 1022005, 1022101_old,
    // 1022104, 1032001_nextLevel, 1032006, 1032113, 1052014, 1052017, 1052113, 1061008, 1091004, 1092015,
    // 1094000, 1095001, 1096001, 1096003, 1096005, 1096010, 11000, 1100000, 1103000, 1104201, 1104202,
    // 1104203, 1104204, 1104205, 1104206, 1104207, 1200000, 1209001, 1209002, 1209003, 1209004, 1209005,
    // 1300001, 1300006, 2010006, 2020004, 2030006_old, 2030013_old, 2040047_old, 2041008, 2041017, 2041024,
    // 2041029, 2042000_New, 2050004, 2060008, 2070000, 2080005, 2082014, 2090000, 2091005_old, 2093003, 2100,
    // 2100000, 2100002, 2100003, 2101, 2101000, 2101001, 2101002, 2101004, 2101005, 2101006, 2101007, 2101008,
    // 2101009, 2101010, 2110000, 2110002, 2111001, 2111004, 2111005, 2111007, 2111008, 2111009, 2111016, 2120003,
    // 2131000, 2131001, 2131002, 2131003, 2131004, 2131005, 2131006, 2131007, 2132000, 2132001, 2132002, 2132003,
    // 9000017, 9000019, 9000021_old, 9010022_old, 9030000, 9030100, 9040004, 9040008, 9120009, 9120023, 9201079, 9201081,
    // 9209000_old, 9220005_old, 9250045, 9270031, 9270042, 9270054,
    // changeName, commands, cpqchallenge2, credits, gachapon, gachaponold, gachaponRemote, MagatiaPassword, mapleTV, MaybeItsGrendel_end,
    // mc_enter, mc_enter1, mc_move, mc_roomout, PupeteerPassword, rank_user, rebirth, scroll_generator, ThiefPassword, unidentifiedNpc, waterOfLife

    internal partial class NpcScript : NPCConversationManager
    {
        NPC? _npcObj;
        public NpcScript(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj?.getObjectId() ?? -1, null)
        {
            _npcObj = npcObj;
        }

        // Npc: 2003 
        public async Task begin5()
        {
            // TODO
            switch (await SayOption("现在...问我任何你可能对旅行有的问题！", [
                "我该怎么移动？",
                "我怎么打倒怪物？",
                "我怎么捡起物品？",
                "我死后会发生什么？",
                "我死后会发生什么？",
                "告诉我更多关于这个岛！",
                "我应该怎么做才能成为一个战士？",
                "我应该怎么做才能成为一个弓箭手？",
                "我应该怎么做才能成为一个魔法师？",
                "我应该怎么做才能成为一个盗贼？",
                "我怎么提升角色属性？(S)",
                "我怎么查看刚刚捡起的物品？",
                "我怎么穿上物品？",
                "我怎么查看我穿着的物品？",
                "我怎么查看我穿着的物品？",
                "我怎么去维多利亚岛？",
                "什么是金币？"
                ]))
            {
                case 0:
                    await SaySpeech([
                        "好的，这是你移动的方法。使用#bleft, right arrow#k在平地和斜坡上移动，并按#bAlt#k跳跃。一些特定的鞋子可以提高你的速度和跳跃能力。",
                        "为了攻击怪物，你需要装备武器。装备后，按下#bCtrl#k来使用武器。在正确的时机，你就能轻松地击败怪物。"
                        ]);
                    break;
                case 1:
                    await SaySpeech([
                        "这里是如何击败怪物的方法。每个怪物都有自己的生命值，你可以用武器或法术来攻击它们。当然，它们越强大，就越难击败。",
                        "一旦你进行职业转职，你将获得不同类型的技能，你可以将它们分配到快捷键上以便更容易地使用。如果是攻击技能，你不需要按下Ctrl键来攻击，只需按下分配为快捷键的按钮。"
                        ]);
                    break;
                case 2:
                    await SaySpeech([
                        "这就是你收集物品的方法。一旦你打倒一个怪物，一个物品就会掉到地上。当这种情况发生时，站在物品前面，按#bZ#k或#b小键盘上的0#k来获取物品。",
                        "请记住，如果你的物品栏已满，你将无法获得更多物品。因此，如果你有一件不需要的物品，就卖掉它，这样你就可以从中获利。当你进行职业转职后，物品栏可能会扩展。"
                        ]);
                    break;
                case 3:
                    await SaySpeech([
                        "当你死亡时，你会变成一个幽灵。当你的生命值降到0时，会在那个地方出现一块墓碑，你将无法移动，但仍然可以进行聊天。",
                        "如果你只是一个新手，死亡并不会让你失去太多。但一旦你有了职业，情况就不一样了。当你死亡时，你会失去一部分经验，所以一定要尽量避免危险和死亡。"
                        ]);
                    break;
                case 4:
                    await SaySpeech([
                        "想知道什么时候能转职？哈哈~！你真性急。每个职业都有固有的转职条件。一般8~10级你就可以选择职业。努力啊！",
                        "等级并不是唯一决定进步的因素。你还需要根据职业提升特定能力的等级。例如，要成为一名战士，你的力量属性必须超过35，你知道我在说什么吗？确保提升与你的职业直接相关的能力。"
                        ]);
                    break;
                case 5:
                    await SaySpeech([
                        "想知道这个岛的情况？这里是叫彩虹岛的空中浮动岛。从远古就在天空上飞行了，因此这里很少出现凶猛的怪物。所以是相对安全的岛，是新手练习的好地方。",
                        "但是，如果你想成为一个强大的玩家，最好不要考虑在这里呆太久。你也不会找到工作。这个岛屿下面有一个叫做金银岛的巨大岛屿。那个地方比这里大得多，甚至有些荒谬。"
                        ]);
                    break;
                case 6:
                    await SaySpeech([
                        "你想当#b战士#k吗？嗯。。。那么你必须要去金银岛。金银岛北部有战士之村，叫#r勇士部落#k。去那里找#b武术教练#k后收下他的培训，你就会当战士。但要当战士你的等级必需达到10级.",
                        "为了攻击怪物，你需要装备武器。装备后，按下#bCtrl#k来使用武器。在正确的时机，你就能轻松地击败怪物。"
                        ]);
                    break;
                case 7:
                    await SaySpeech([
                        "你想当弓箭手吗？在金银岛你会当弓箭手。在金银岛南部有弓箭手的村落，射手村。在那里赫丽娜会告诉你当弓箭手的方法。但关键是要当弓箭手你的等级应该是10级。",
                        ]);
                    break;
                case 8:
                    await SaySpeech([
                        "你想当魔法师是吗？那你要去金银岛东部的魔法密林。在那里你会见到很多魔法师。而且在那里你要见汉斯。他就会让你当魔法师。",
                        "哦，顺便说一下，与其他职业不同，要成为一个魔法师，你只需要达到8级。提前进行职业转职所带来的好处也伴随着成为一名真正强大的法师所需付出的艰辛。在选择你的道路之前，请仔细考虑。"
                        ]);
                    break;
                case 9:
                    await SaySpeech([
                        "你你想当飞侠吗？那你要去金银岛西部的废弃都市。废都的达克鲁就会告诉你当飞侠的办法。关键的是为了当飞侠，你的等级应该是10级。",
                        ]);
                    break;
                case 10:
                    await SaySpeech([
                        "你想知道如何提升你角色的能力属性吗？首先按下#bS#k来查看能力窗口。每次升级时，你会获得5个能力点，也就是AP。将这些AP分配到你选择的能力上。就是这么简单。",
                        "将鼠标光标放在所有技能上方，以获得简要解释。例如，战士的STR，弓箭手的DEX，魔法师的INT，以及盗贼的LUK。但这并不是你需要了解的全部，所以你需要仔细思考如何通过分配点数来强调你角色的优势。"
                        ]);
                    break;
                case 11:
                    await SaySpeech([
                        "你想知道如何查看你捡起的物品，是吗？当你打败一个怪物时，它会掉落一个物品在地上，你可以按#bZ#k来捡起这个物品。这个物品将被存储在你的物品库存中，你可以通过简单地按#bI#k来查看它。",
                        ]);
                    break;
                case 12:
                    await SaySpeech([
                        "你想知道如何穿戴物品，对吧？按下#bI#k来查看你的物品库存。将鼠标光标放在物品上，双击它就可以穿在你的角色身上。如果你发现自己无法穿戴该物品，很可能是你的角色不符合等级和属性要求。你也可以通过打开装备库存(#bE#k)并将物品拖入其中来穿戴物品。要脱下物品，双击装备库存中的物品。",
                        ]);
                    break;
                case 13:
                    await SaySpeech([
                        "你想要检查已装备的物品，对吧？按下#bE#k打开装备栏，你会看到你当前穿着的物品。要脱下一个物品，双击该物品。该物品将被发送到物品栏。",
                        ]);
                    break;
                case 14:
                    await SaySpeech([
                        "获得职业后获得的特殊“能力”被称为技能。你将获得专门针对该职业的技能。你现在还没有达到那个阶段，所以还没有任何技能，但记住，要查看你的技能，请按#bK#k打开技能书。这将在日后帮助你。",
                        ]);
                    break;
                case 15:
                    await SaySpeech([
                        "你如何到达金银岛？在这个岛的东部有一个叫做南港的港口。在那里，你会找到一艘在空中飞行的船。船前站着船长。问他关于这件事。",
                        "哦，是的！在我离开之前，还有一条信息要告诉你。如果你不确定自己在哪里，记得按下#bW#k。世界地图会弹出来，显示你所在的位置。有了这个，你就不用担心迷路了。"
                        ]);
                    break;
                case 16:
                    await SaySpeech([
                        "是冒险岛中使用的货币。你可以通过金币购买物品。要赚取金币，你可以击败怪物，将物品出售给商店，或者完成任务...",
                        ]);
                    break;
                default:
                    await begin5();
                    break;
            }
            await begin5();
        }


        // Npc: 2007 
        public async Task tutorialSkip()
        {
            if (await SayYesNo("您想要跳过教程，直接前往明珠港吗？"))
            {
                warp(104000000, 0);
            }
            else
            {
                await SayNext("旅行愉快。");
            }
        }


        // Npc: 2104 
        public async Task HL_LADDER()
        {
            await SayNext("好的，那么我们出发吧。冒险旅途开始了！");
            warp(1);
        }



        // Npc: 12101 
        public async Task rein()
        {
            await SaySpeech([
                "这是位于冒险岛东北部的名为#b彩虹岛#k的小村子。你知道这里可是新手的天堂哦，因为这个地方周围只有一些弱小的怪物~。",
                "如果你想变得更强大，那就去#b南港#k吧，那里有一个港口。你可以乘坐一搜巨大的游轮前往一个叫做#b金银岛#k的地方。与这个小岛相比，它的面积可是一望无际。",
                "在金银岛上，你可以游历并选择你的职业。我听说有一个荒凉的村落，有许多战士住在那里。还有悬崖、高地……那会是什么样的地方？"
                ]);
        }


        // Npc: 22000 
        public async Task begin7()
        {
            if (await SayYesNo("要去金银岛吗？只需要支付 #e150 金币#n，我会把你送到 #b明珠港#k。#r但是#k一旦离开这里，你就不能再回来了，要出发吗？"))
            {
                if (haveItem(4031801))
                {
                    await SayNext("这是#t4031801#？那你可以免费搭乘这次航班。");
                    await SayNext("事不宜迟，我们出发吧！");

                    // ！！！ 判断物品是否存在之后又等待了对话，虽然此时玩家无法操作物品，但是可能有其他后台操作（比如道具过期）恰好在等待期间移除了道具。需要重新判断
                    if (!haveItem(4031801))
                    {
                        await SayNext("推荐信呢？");
                        return;
                    }
                    else
                    {
                        gainItem(4031801, -1);
                        warp(104000000, 0);
                    }
                }
                else if (getLevel() > 6)
                {
                    await SayNext("事不宜迟，我们出发吧！");
                    if (getMeso() < 150)
                    {
                        await SayNext("你的金币不足以支付这次航行");
                        return;
                    }
                    else
                    {
                        gainMeso(-150);
                        warp(104000000, 0);
                    }
                }
                else
                {
                    await SayOK("你的等级太低了，请7级后再来。");
                    return;
                }
            }
        }


        // Npc: 1002000 
        public async Task rithTeleport()
        {

            await SayNext("你想去其他城镇吗？只要有一点钱，我就可以安排。虽然有点贵，但我给新手提供90%的特别折扣。");
            switch (await SayOption("如果这是你第一次来到这个地方，可能会对这里感到困惑是可以理解的。如果你对这个地方有任何问题，尽管问吧。",
                [
                "金银岛上有哪些城镇？",
                "请带我去别的地方。"
                ]))
            {
                case 0:
                    {
                        int[] imaps = [104000000, 102000000, 101000000, 100000000, 103000000, 120000000, 105040300];
                        switch (await SayOption("金银岛中存在7大主城区域. 你想了解哪一个？", imaps.Select(x => $"#m{x}#")))
                        {
                            case 0:
                                await SaySpeech([
                                    "你所在的城镇是明珠港！好的，我会为你详细介绍一下#b明珠港#k。明珠港是你乘坐维多利亚号登陆金银岛的地方。很多刚从彩虹岛来到这里的新手都会在这里开始他们的旅程。这个小镇为新手提供了许多便利和指引，帮助他们熟悉游戏世界，并踏上冒险的旅途。在这里，你可以接取任务、学习技能、购买装备，为接下来的旅程做好准备",
                                    "这是一个宁静的小镇，背后是宽阔的水域，这要归功于港口位于岛屿的西端。这里的大多数人都是或者曾经是渔民，因此他们可能看起来有些令人畏惧，但如果你主动与他们交谈，他们会变得非常友好。这些渔民们经历了海上的风雨，有着丰富的故事和经验，他们乐于与新来的人分享自己的生活和见闻。在明珠港，你可以感受到一种淳朴而热情的氛围，这里的人们都乐于助人，愿意为需要帮助的人伸出援手。",
                                    "镇子周围是一片广袤而美丽的草原。这里的怪物大多体型小巧且性格温顺，对于初来乍到的新手冒险家来说，是绝佳的练级场所。如果你还未决定自己的职业方向，那么这片草原将是你快速提升等级、熟悉游戏机制的好地方。在这里，你可以轻松击败怪物，积累经验值，同时还有机会获得丰厚的奖励，为你的冒险之旅打下坚实的基础。"
                                    ]);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case 1:
                    var selStr = getJobId() == 0 ? "所有新手都可以享受90%的特别折扣。好的，你想去哪里？" : "看来你不是新手, 嗯? 那我只能收你全价了. 你想去哪儿?";
                    int[] maps = [102000000, 100000000, 101000000, 103000000, 120000000];
                    int[] cost = [1000, 1000, 800, 1000, 800];

                    var option = await SayOption(selStr, maps.Select(x => $"#m{x}#"));

                    var realCost = DiscountForNovice(cost[option]);
                    if (await SayYesNo("我猜你不需要在这里。你真的想要移动到 #b#m" + maps[option] + "##k 吗？好吧，这将花费你 #b" + realCost + " 金币#k。你觉得怎么样？"))
                    {
                        if (getMeso() < realCost)
                        {
                            await SayOK("你的金币不够。以你的能力来说，不该只挣这么点！");
                        }
                        else
                        {
                            gainMeso(-realCost);
                            warp(maps[option]);
                        }
                    }
                    break;
                default:
                    break;
            }
        }


        // Npc: 1002002, 2010005, 2040048 
        public async Task florina2()
        {
            var cost = 1500;
            var ticket = 4031134;
            var option = await SayOption(
                $"你听说过位于立石港附近，能够欣赏到壮观海景的海滩#b黄金海滩#k吗？我可以带你去那里，只需#b{cost}金币#k，或者如果你有#b#t{ticket}##k的话，那就可以免费。",
                [$"我支付{cost}金币。", $"我有#t{ticket}#。", $"什么是#t{ticket}#？"]);
            switch (option)
            {
                case 0:
                    if (getMeso() < 1500)
                    {
                        await SayNext("我觉得你缺少金币。有很多方法可以赚钱，比如...卖掉你的盔甲...打败怪物...做任务...你知道我在说什么。");
                        return;
                    }
                    else
                    {
                        gainMeso(-1500);
                        getPlayer().saveLocation("FLORINA");
                        warp(110000000, "st00");
                    }
                    break;
                case 1:
                    if (await SayYesNo($"所以你有一张#b#t{ticket}##k吗？你可以随时用它前往黄金海滩。好的，但要注意可能会遇到一些怪物。好的，你现在想前往黄金海滩吗？"))
                    {
                        if (!haveItem(ticket))
                        {
                            await SayNext($"嗯，你的 #b#t{ticket}##k 到底在哪里？你确定你有吗？请再检查一遍。");
                            return;
                        }
                        getPlayer().saveLocation("FLORINA");
                        warp(110000000, "st00");
                    }
                    else
                    {
                        await SayNext("你一定有一些事情要处理。你一定因为旅行和打猎而感到疲倦。去休息一下，如果你改变主意了，再来找我谈谈吧。");
                    }
                    break;
                case 2:
                    await SayNext($"你一定对#b#t{ticket}##k很感兴趣。哈哈，这很可以理解。#t{ticket}#是一种物品，只要你拥有它，就可以免费前往黄金海滩。这是一种非常稀有的物品，甚至我们也不得不购买，但不幸的是，我在几周前在我珍贵的暑假期间丢失了我的。");
                    break;
                default:
                    break;
            }
        }


        // Npc: 1002003 
        public async Task friend00()
        {
            if (await SayYesNo("我希望我能赚到像昨天一样多的钱...嗨，你好！你不想扩展你的好友列表吗？你看起来像是有很多朋友的人...那么，你觉得呢？只要有一些钱，我就可以帮你实现。不过要记住，这只适用于一个角色，不会影响你账号上的其他角色。你想扩展你的好友列表吗？"))
            {
                if (await SayYesNo("好的，不错的选择！其实并不是很贵。#b24万金币，我就可以给你的好友列表增加5个名额#k。不，我不会单独出售它们。一旦你购买了，它就会永久地出现在你的好友列表上。所以如果你是那些需要更多空间的人，那么你最好去做。你觉得呢？你会花24万金币吗？"))
                {
                    var capacity = getPlayer().getBuddylist().Capacity;
                    if (capacity >= 50 || getMeso() < 240000)
                    {
                        await SayNext("嘿... 你确定你有 #b240,000金币#k 吗？如果是的话，那么检查一下你是否已经将好友列表扩展到了最大。即使你付了钱，你的好友列表上限也只能有 #b50#k 个。");
                    }
                    else
                    {
                        var newcapacity = capacity + 5;
                        gainMeso(-240000);
                        getPlayer().setBuddyCapacity(newcapacity);
                        await SayOK("好的！你的好友列表现在应该有5个额外的槽位。自己去检查一下。如果你还需要更多的好友列表空间，你知道该找谁。当然，这并不是免费的……好了，再见……");
                    }
                }
                else
                {
                    await SayNext("我明白了...我觉得你的朋友可能没有我想象的那么多。如果不是这样，你现在手头上就没有24万金币？不管怎样，如果你改变主意了，回来找我，我们可以谈生意。当然，前提是你得解决一些财务问题... 嘿嘿...");
                }
            }
            else
            {
                await SayNext("我明白了...你的朋友似乎没有我想象的那么多。哈哈哈，开玩笑的！无论如何，如果你改变主意了，随时可以回来找我谈生意。如果你交了很多朋友，你就知道了... 嘿嘿...");
            }
        }


        // Npc: 1002004, 1032005 
        public async Task mTaxi()
        {
            await SayNext("你好！这辆出租车只对VIP客户开放。与普通出租车只能带你去不同的城镇不同，我们提供更好的服务，值得VIP级别的待遇。价格有点高，但是……只需10,000金币，我们就会安全地带你去#b蚁穴#k。");
            var cost = DiscountForNovice(1000);
            if (await SayYesNo(getJobId() == 0 ? $"我们为新手提供 90% 的特别折扣。 蚂蚁广场位于维多利亚大陆中心的地穴深处, 那里是24小时移动商店的所在地。 你想去那里并花费 #b1,000 金币#k 吗?" : "正常费用适用于所有非初学者。 蚂蚁广场位于维多利亚大陆中心的地穴深处, 那里是24小时移动商店的所在地。 你想去那里并花费 #b10,000 金币#k 吗?"))
            {
                if (getMeso() < cost)
                {
                    await SayNext("看来你没有足够的金币. 抱歉，没有它你将无法使用它。");
                }
                else
                {
                    gainMeso(-cost);
                    warp(105070001);
                }
            }
            else
            {
                await SayOK("这个城镇也有很多值得一看的地方。如果你觉得有必要去蚂蚁广场，就来找我们吧。");
            }
        }


        // Npc: 1002006 
        public async Task bookPrize()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1002100 
        public async Task jane()
        {
            if (isQuestCompleted(2013))
            {
                await SayNext("是你啊...多亏了你，我才能完成了很多事情。最近我一直在制作各种物品。如果你需要什么，告诉我一声。");

                (int item, int price, string desc)[] items = [(2000002, 310, "300 HP."), (2022003, 1060, "1000 HP."), (2022000, 1600, "800 MP."), (2001000, 3120, "1000 HP and MP.")];
                var option = await SayOption("你想购买哪些药水?#b", items.Select(x => "#i" + x.item + "# (价格 : " + x.price + " 金币)"));
                var inputNumber = await SayInputNumber("你想买 #b#t" + items[option].item + "##k? #t" + items[option].item + "# 允许您恢复 " + items[option].desc + " 你想买多少个?", 1, 1, 100);
                var totalCost = inputNumber * items[option].price;
                if (await SayYesNo($"你将购买这些 #r{inputNumber}#k #b#t{items[option].item}#(s)#k 吗？#t{items[option].item}# 一个需要 {items[option].price} 金币，所以总共需要 #r{totalCost}#k 金币。"))
                {
                    if (getMeso() < totalCost)
                    {
                        await SayNext("你是否缺少金币？请检查一下你的消耗物品栏中是否有空位，并且你是否携带了至少 #r" + totalCost + "#k 金币。");
                    }
                    else
                    {
                        if (canHold(items[option].item, inputNumber))
                        {
                            gainMeso(totalCost);
                            gainItem(items[option].item, inputNumber);
                            await SayNext("谢谢你的光临。这里的东西总是可以购买，如果你需要什么，请再来。");
                        }
                        else
                        {
                            await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.USE))));
                        }
                    }
                }
                else
                {
                    await SayNext("我还有你之前给我的很多材料。物品都在那里，所以你可以慢慢挑选。");
                }
            }
            else
            {
                if (isQuestCompleted(2010))
                {
                    await SayNext("你似乎不够强大，无法购买我的药水……");
                }
                else
                {
                    await SayOK("我的梦想是到处旅行，就像你一样。然而，我的父亲不允许我这样做，因为他认为这太危险了。不过，如果我能向他证明我并不是他所认为的软弱女孩，他也许会同意...");
                }
                return;
            }
        }



        // Npc: 1012008, 2040014 
        public Task minigame00()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1012115 
        public async Task blackShadowHene1()
        {
            var status = getQuestStatus(20706);

            if (status == 0)
            {
                await SayNext("看起来这个地区没有什么可疑的东西。");
            }
            else if (status == 1)
            {
                forceCompleteQuest(20706);
                await SayNext("你已经发现了影子！最好向#p1103001#报告。");
            }
            else if (status == 2)
            {
                await SayNext("影子已经被发现了。最好向#p1103001#报告一下。");
            }
        }


        // Npc: 1012116 
        public async Task blackShadowHene2()
        {
            await SayNext("看起来这个地区没有什么可疑的东西。");
        }


        // Npc: 1022101 
        public async Task go_xmas()
        {
            if (await SayYesNo("圣诞老人告诉我来到这里，只是他没有告诉我什么时候……我希望我来的时间对了！哦！顺便说一下，我是鲁尼，我可以带你去#b快乐村#k。你准备好了吗？"))
            {
                getPlayer().SaveLocation(SavedLocationType.HAPPYVILLE);
                warp(209000000, 0);
            }
        }


        // Npc: 1022103 
        public Task s4strike_statue()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1032003 
        public async Task herb_in()
        {
            if (getLevel() < 25)
            {
                await SayOK("你必须达到更高的等级才能进入忍苦树林。");
            }
            else
            {
                if (await SayYesNo($"嗨，我是#p{npc}#。我可以让你进入忍苦树林，只需要支付一小笔费用。你想用 #b5000#k 金币进入吗？"))
                {
                    if (getMeso() < 5000)
                    {
                        await SayOK(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                    else
                    {
                        if (isQuestStarted(2050))
                        {
                            warp(101000100, 0);
                        }
                        else if (isQuestStarted(2051))
                        {
                            warp(101000102, 0);
                        }
                        else if (getLevel() >= 25 && getLevel() < 50)
                        {
                            warp(101000100, 0);
                        }
                        else if (getLevel() >= 50)
                        {
                            warp(101000102, 0);
                        }
                        gainMeso(-5000);
                    }
                }
                else
                {
                    await SayOK("好的，下次见。");
                }
            }
        }


        // Npc: 1032004 
        public async Task herb_out()
        {
            if (await SayYesNo("你想回到#m101000000#吗？"))
            {
                warp(101000000, 0);
            }
        }


        // Npc: 1032007, 2012000, 2040000, 2082000, 2102002 
        public async Task sell_ticket()
        {
            ContiMoveBase? currentContiMove;

            if (npc == 2012000)
            {
                // 天空之城

                ContiMoveBase[] list = c.CurrentServer.ContiMoves.Values.Where(x => x.StationAMap.Id == getMapId()).ToArray();
                var option = await SayOption("您好，我是天空之城售票员，我负责销售开往各地船票。你想购买去那里的船票呢？",
                    list.Select(x => x.GetDestinationMapName(getPlayer())));

                currentContiMove = list[option];
            }
            else
            {
                currentContiMove = GetContiMove();
            }

            if (currentContiMove == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var target = currentContiMove.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var p = currentContiMove.GetTicket(getPlayer());
            if (p == null)
            {
                await SayOK("无法与我对话");
                return;
            }
            if (await SayYesNo($"你好，我负责出售前往{target}的船票。前往{target}的船每{TimeSpan.FromMilliseconds(currentContiMove.GetTransportationTime(currentContiMove.RideTime)).Humanize()}出发一次，票价为#b{p.Value.TicketPrice}金币#k。你确定要购买#b#t${p.Value.TicketItemId}##k吗？"))
            {
                if (getMeso() >= p.Value.TicketPrice && canHold(p.Value.TicketItemId))
                {
                    gainItem(p.Value.TicketItemId, 1);
                    gainMeso(-p.Value.TicketPrice);
                }
                else
                {
                    await SayOK("你确定你有 #b" + p.Value.TicketPrice + " 金币#k 吗？如果是的话，请检查你的其它物品栏，看看是否已经满了。");
                }
            }
            else
            {
                await SayNext("你一定是有一些事情要在这里处理，对吧？");
            }
        }


        // Id 1032008, 2012001, 2012013, 2012021, 2012025, 2041000, 2082001, 2102000
        public async Task get_ticket()
        {
            var currentContiMove = GetContiMove();
            if (currentContiMove == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var target = currentContiMove.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var p = currentContiMove.GetTicket(getPlayer());
            if (p == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var next = DateTimeOffset.FromUnixTimeMilliseconds(currentContiMove.ArriveAt).ToLocalTime().Humanize();

            if (haveItem(p.Value.TicketItemId))
            {
                if (currentContiMove.CanEnter)
                {
                    if (await SayYesNo($"你想去{target}吗？"))
                    {
                        if (currentContiMove.Enter(getPlayer()))
                        {
                            gainItem(p.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayOK($"飞往{target}的船只已经启程，请耐心等待下一班。下一班将在 ${next}抵达。");
                        }
                        return;
                    }
                    else
                    {
                        await SayOK("好的，如果你改变主意，就跟我说话！");
                    }
                }
                else
                {
                    await SayOK($"飞往${target}的船只已经启程，请耐心等待下一班。下一班将在 ${next}抵达。");
                }
            }
            else
            {
                await SayOK($"确保你有一张飞往{target}的船票才能乘坐这艘船。检查你的物品栏。");
            }
        }


        // Id 1032009, 2012002, 2012022, 2012024, 2041001, 2082002, 2102001
        public async Task goOutWaitingRoom()
        {
            if (await SayYesNo("你想离开吗？"))
            {
                await SayNext("好的，下次见。保重。");
                WarpReturn();
            }

        }



        // Npc: 1032109 
        public async Task blackShadowEli1()
        {
            if (!isQuestStarted(20718))
            {
                // thanks Stray, Ari
                return;
            }

            await SayOK("一个神秘的黑色身影出现并召唤了许多愤怒的怪物！");
            var player = getPlayer();
            var map = player.getMap();
            var mobId = 2220100;
            for (var i = 0; i < 10; i++)
            {
                map.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), new Point(117, 183));
            }
            for (var i = 0; i < 10; i++)
            {
                map.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), new Point(4, 183));
            }
            for (var i = 0; i < 10; i++)
            {
                map.spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(mobId), new Point(-109, 183));
            }

            completeQuest(20718, 1103003);
            gainExp((int)(4000 * getPlayer().getExpRate()));

        }


        // Npc: 1032110 
        public async Task blackShadowEli2()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1032111 
        public async Task giveSap()
        {
            if (isQuestStarted(20716))
            {
                if (!hasItem(4032142))
                {
                    if (canHold(4032142))
                    {
                        gainItem(4032142, 1);
                        await SayOK("你装瓶了一些清澈的树液。#i4032142#");
                    }
                    else
                    {
                        await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
                    }
                }
                else
                {
                    await SayOK("这个小树桩上不断流出的树液。");
                }
            }
            else
            {
                await SayOK("这个小树桩上不断流出的树液。");
            }
        }


        // Npc: 1040000 
        public async Task summonMobInLuke()
        {
            if (isQuestStarted(28177) && !haveItem(4032479))
            {
                if (canHold(4032479))
                {
                    gainItem(4032479, 1);
                    await SayOK("哼，你在找我吗？是Stan长官派你来的，对吧？但是嘿，我不是你要找的嫌疑人。如果我有证据呢？拿着这个，把它还给 #b#p1012003##k。");
                }
                else
                {
                    await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
                }
            }
            else
            {
                await SayOK("呼呼呼...");
            }

        }

        // Npc: 1043000 
        public Task bush1()
        {
            int[] prizes = [1040052, 1040054, 1040130, 1041143, 1042013, 1042022, 1042034, 1042084, 1042098, 1042117, 1702002, 1702015];
            int[] chances = [10, 10, 10, 15, 10, 10, 10, 10, 10, 10, 5, 5];
            var totalodds = 0;
            var choice = 0;
            for (var i = 0; i < chances.Length; i++)
            {
                var itemGender = (int)(Math.Floor(prizes[i] / 1000.0) % 10);
                if ((getPlayer().getGender() != itemGender) && (itemGender != 2))
                {
                    chances[i] = 0;
                }
            }
            for (var i = 0; i < chances.Length; i++)
            {
                totalodds += chances[i];
            }
            var randomPick = Random.Shared.Next(totalodds) + 1;
            for (var i = 0; i < chances.Length; i++)
            {
                randomPick -= chances[i];
                if (randomPick <= 0)
                {
                    choice = i;
                    randomPick = totalodds + 100;
                }
            }
            if (isQuestStarted(2050))
            {
                gainItem(4031020, 1);
            }
            gainItem(prizes[choice], 1);
            warp(101000000, 0);
            return Task.CompletedTask;
        }


        // Npc: 1043001 
        public Task bush2()
        {
            int[] prizes = [1060041, 1060048, 1060116, 1061113, 1061130, 1061139, 1062009, 1062017, 1062024, 1062056, 1062061, 1702045, 1702114];
            int[] chances = [10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 5, 5];
            var totalodds = 0;
            var choice = 0;
            for (var i = 0; i < chances.Length; i++)
            {
                var itemGender = (int)(Math.Floor(prizes[i] / 1000.0) % 10);
                if ((getPlayer().getGender() != itemGender) && (itemGender != 2))
                {
                    chances[i] = 0;
                }
            }
            for (var i = 0; i < chances.Length; i++)
            {
                totalodds += chances[i];
            }
            var randomPick = Random.Shared.Next(totalodds) + 1;
            for (var i = 0; i < chances.Length; i++)
            {
                randomPick -= chances[i];
                if (randomPick <= 0)
                {
                    choice = i;
                    randomPick = totalodds + 100;
                }
            }
            if (isQuestStarted(2051))
            {
                gainItem(4031032, 1);
            }
            gainItem(prizes[choice], 1);
            warp(101000000, 0);
            return Task.CompletedTask;
        }




        // Npc: 1052006 
        public async Task subway_ticket()
        {
            await SayNext("嗨，我是售票员。");
            var zones = 0;
            var cost = 1000;

            if (isQuestStarted(2055) || isQuestCompleted(2055))
            {
                zones++;
            }
            if (isQuestStarted(2056) || isQuestCompleted(2056))
            {
                zones++;
            }
            if (isQuestStarted(2057) || isQuestCompleted(2057))
            {
                zones++;
            }

            if (zones == 0)
            {
                await SayOK(GetDefault0());
                return;
            }

            var selStr = "Which ticket would you like?#b";
            for (var i = 0; i < zones; i++)
            {
                selStr += "\r\n#L" + i + "#Construction site B" + (i + 1) + " (" + cost + " mesos)#l";
            }
            var option = await SayOption(selStr);
            if (getMeso() < cost)
            {
                await SayOK(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
            }
            else
            {
                gainMeso(-cost);
                gainItem(4031036 + option, 1);
            }
        }


        // Npc: 1052007 
        public async Task subway_in()
        {
            var option = await SayOption("选择你的目的地。\r\n#L0##b废都广场#l\r\n#L1#进入建筑工地#l\r\n#L2#新叶城#l");
            switch (option)
            {
                case 0:
                    var em0 = GetEventManager<PrivateContiMove>("KerningTrain");
                    var r = em0.StartInstance(getPlayer());
                    await SayOK(em0.HandleCreateInstanceResult(r, c));
                    break;
                case 1:
                    if (haveItem(4031036) || haveItem(4031037) || haveItem(4031038))
                    {
                        var text = "Here's the ticket reader. You will be brought in immediately. Which ticket you would like to use?#b";
                        for (var i = 0; i < 3; i++)
                        {
                            if (haveItem(4031036 + i))
                            {
                                text += "\r\n#b#L" + (i + 1) + "##t" + (4031036 + i) + "#";
                            }
                        }
                        var selectTicket = await SayOption(text);
                        gainItem(4031035 + selectTicket, -1);
                        warp(103000897 + (selectTicket * 3), "st00");  // thanks IxianMace for noticing a few scripts having misplaced warp SP's
                        return;
                    }
                    else
                    {
                        await SayOK("看起来你好像没有票！");
                    }
                    break;
                case 2:
                    await ContiMove_NLC();
                    break;
                default:
                    break;
            }
        }


        // Npc: 1052008 
        public Task subway_get1()
        {
            int[] prizes = [4020000, 4020001, 4020002, 4020003, 4020004];
            if (isQuestStarted(2055))
            {
                gainItem(4031039, 1);
            }
            else
            {
                gainItem(Randomizer.Select(prizes), 1);
            }
            warp(103000100, 0);
            return Task.CompletedTask;
        }


        // Npc: 1052009 
        public Task subway_get2()
        {
            int[] prizes = [4020005, 4020006, 4020007, 4020008, 4010000];
            if (isQuestStarted(2056))
            {
                gainItem(4031040, 1);
            }
            else
            {
                gainItem(Randomizer.Select(prizes), 1);
            }
            warp(103000100, 0);
            return Task.CompletedTask;
        }


        // Npc: 1052010 
        public Task subway_get3()
        {
            int[] prizes = [4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007];
            if (isQuestStarted(2057))
            {
                gainItem(4031041, 1);
            }
            else
            {
                gainItem(Randomizer.Select(prizes), 1);
            }
            warp(103000100, 0);
            return Task.CompletedTask;
        }


        // Npc: 1052011 
        public async Task subway_out()
        {
            await SaySpeech(["这个设备连接到外部。", "Are you going to give up and leave this place?"]);
            if (await SayYesNo("下次进来的时候，你将不得不从头开始..."))
            {
                warp(103000100, 0);
            }

        }


        // Npc: 1052012 
        public async Task go_pc()
        {
            if (await SayYesNo("那么，你打算使用网吧吗？在那里使用空间是需要付费的，费用是#b5,000金币#k。你要进入网吧吗？"))
            {
                if (getMeso() < 5000)
                {
                    await SayOK("哦，你没有钱，对吧？抱歉，我不能让你进去。");
                }
                else
                {
                    gainMeso(-5000);
                    warp(193000000, "out00");
                }
            }
        }


        // Npc: 1052013 
        public Task go_pcmap()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1052015 
        public Task mouse()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 1052107 
        public async Task sca_Shade()
        {
            if (await SayAcceptDecline("This is a small lamp with a switch. Would you like to turn it on?"))
            {
                weakenAreaBoss(5090000, "You have turned the lamp on. Shade's strength will rapidly weaken due to the light.");
            }
        }


        // Npc: 1052109 
        public async Task givebubbleDoll1()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1052110 
        public async Task givebubbleDoll2()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1052111 
        public async Task givebubbleDoll3()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1052112 
        public async Task givebubbleDoll4()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1052115 
        public async Task metroIm()
        {
            await SayOK(GetDefault0());
            //if (getMapId() == 910320001)
            //{
            //    warp(910320000, 0);
            //    return;
            //}
            //else if (getMapId() == 910330001)
            //{
            //    var itemid = 4001321;
            //    if (!canHold(itemid))
            //    {
            //        await SayOK("请为1个杂项槽腾出空间。");
            //    }
            //    else
            //    {
            //        gainItem(itemid, 1);
            //        warp(910320000, 0);
            //    }
            //    return;
            //}
            //else if (getMapId() >= 910320100 && getMapId() <= 910320304)
            //{
            //    if (await SayYesNo("你想要离开这个地方吗？"))
            //    {
            //        warp(910320000, 0);
            //        return;
            //    }
            //}
            //else
            //{
            //    var option = await SayOption("我的名字是林先生。\r\n#b#e#L1#进入尘土飞扬的平台。#l#n\r\n#L2#前往999号列车。#l\r\n#L3#获得<荣誉员工>勋章。#l#k");
            //    switch (option)
            //    {
            //        case 1:
            //            if (getPlayer().getLevel() < 25 || getPlayer().getLevel() > 30 || !isLeader())
            //            {
            //                await SayOK("你必须处于25-30级的等级范围，并且是队伍的队长。");
            //            }
            //            else
            //            {
            //                if (!start_PyramidSubway(-1))
            //                {
            //                    sendOk("尘土飞扬平台目前已满。");
            //                }
            //            }
            //            break;
            //        case 2:
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }


        // Npc: 1052125 
        public async Task Depart_topFloorIn()
        {

            var option = await SayOption("等一下！由于装修，该区域的进入受到限制。我只能允许符合特定条件的人进入这里。#b\n\r\n#L0#我现在正在帮助#eBlake#n。#l\r\n#L1#我是这家购物中心的#rVIP#b！#l");
            switch (option)
            {
                case 0:
                    if (isQuestStarted(2286) || isQuestStarted(2287) || isQuestStarted(2288))
                    {
                        var em = GetEventManager<RockSpirit>("RockSpirit");
                        var r = em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                        return;
                    }
                    else
                    {
                        await SayOK("我没有听到布莱克说你在帮助他。");
                    }
                    break;
                case 1:
                    if (isQuestCompleted(2290))
                    {
                        if (getPlayer().getLevel() > 50)
                        {
                            await SayOK("VIP区域仅供50级或以下的玩家使用。");
                        }
                        else
                        {
                            await SayOK("VIP区域只有在完成“进入VIP区域”的任务并交出#r#t4032521#s#k后才能进入。");
                        }
                    }
                    else
                    {
                        await SayOK("#rVIP#k？是的，这很有趣 #rVIP先生#k，现在赶紧滚开，否则我就叫保安了。");
                    }
                    break;
                default:
                    break;
            }
        }





        // Npc: 1061006 
        public async Task flower_in()
        {
            await SayNext("你感觉到一股神秘的力量笼罩着这座雕像。");

            var zones = -1;
            int[] maps = [105040310, 105040312, 105040314];
            if (isQuestStarted(2054) || isQuestCompleted(2054))
            {
                zones = 3;
            }
            else if (isQuestStarted(2053) || isQuestCompleted(2053))
            {
                zones = 2;
            }
            else if (isQuestStarted(2052) || isQuestCompleted(2052))
            {
                zones = 1;
            }
            if (zones > 0)
            {
                var option = await SayOption("它的力量让你能够深入森林深处。", maps.Take(zones).Select(x => $"#m{x}#"));
                warp(maps[option], 0);
            }
        }


        // Npc: 1061007 
        public async Task flower_out()
        {
            if (await SayYesNo("你想离开吗？"))
            {
                warp(105040300, 0);
            }
        }


        /// <summary>
        /// 异界之门
        /// </summary>
        /// <returns></returns>
        // Npc: 1061009 
        public async Task crack()
        {
            Dictionary<int, int> allowedJob = new()
            {
                {1, 105070001 },
                {2, 100040106 },
                {3, 105040305 },
                {4, 107000402 },
                {5, 105070200 },
            };
            var jobStyle = getJob().GetJobNiche();
            if (isQuestStarted(-jobStyle * 10000 - 3000 - 1) && getMapId() == allowedJob.GetValueOrDefault(jobStyle) && !haveItem(4031059))
            {
                // getPlayer().SaveLocation(Shared.MapObjects.SavedLocationType.EVENT);
                var em = GetEventManager<S3rdJob>(nameof(S3rdJob) + jobStyle);
                await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }


        // Npc: 1061010 
        [ScriptName("3jobExit")]
        public async Task s_3jobExit()
        {
            if (await SayYesNo("你想离开吗？"))
            {
                var p = getPlayer().GetSavedLocation(Shared.MapObjects.SavedLocationType.EVENT);
                warpMap(p);
            }
        }


        // Npc: 1061012 
        public Task s4snipe()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1061014 
        public Task balog_accept()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1061016 
        public Task balog_scroll()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1061018 
        public async Task balog_InOut()
        {
            if (getEventInstance()?.isEventCleared() ?? false)
            {
                await SayOK("哇！你打败了巴尔洛格。");
                warp(getMapId() == 105100300 ? 105100301 : 105100401, 0);
            }
            else if (getPlayer().getMap().getAllPlayers().Count > 1)
            {
                if (await SayYesNo("你真的要离开这场战斗，让你的同伴们去死吗？"))
                {
                    warp(105100100);
                }
            }
            else
            {
                if (await SayYesNo("如果你是个懦夫，你会离开。"))
                {
                    warp(105100100);
                }
            }
        }


        // Npc: 1061100 
        public async Task hotel1()
        {

            await SayNext("欢迎光临。我们是冬青树镇酒店。我们的酒店一直努力为您提供最好的服务。如果您因打猎而感到疲惫不堪，不妨在我们的酒店放松一下吧？");
            var regcost = 499;
            var vipcost = 999;
            var option = await SayOption("我们提供两种房间供您选择。请选择您喜欢的一种。",
                [$"普通桑拿房（{regcost}金币）", $"VIP桑拿房（{vipcost}金币）"]);
            switch (option)
            {
                case 0:
                    if (await SayYesNo("你选择了普通桑拿房。你的生命值和魔法值将会快速恢复，你甚至可以在那里购买一些物品。你确定要进去吗？"))
                    {
                        if (getMeso() >= regcost)
                        {
                            warp(105040401);
                            gainMeso(-regcost);
                        }
                        else
                        {
                            await SayNext("对不起，看起来你的金币不够。至少需要" + regcost + "金币才能在我们的旅馆住宿。");
                        }
                        return;
                    }
                    break;
                case 1:
                    if (await SayYesNo("你选择了VIP桑拿房。你的HP和MP恢复速度甚至比普通桑拿还要快，而且你甚至可以在那里找到一个特别的物品。你确定要进去吗？"))
                    {
                        if (getMeso() >= vipcost)
                        {
                            warp(105040402);
                            gainMeso(-vipcost);
                        }
                        else
                        {
                            await SayNext("对不起，看起来你的金币不够。至少需要" + vipcost + "金币才能在我们的旅馆住宿。");
                        }
                        return;
                    }
                    break;
                default:
                    break;
            }
            await SayNext("我们也提供其他类型的服务，所以请仔细考虑后再做决定。");

        }


        // Npc: 1063000 
        public async Task viola_pink()
        {
            if (isQuestStarted(2052) && !haveItem(4031025, 30))
            {
                if (!canHold(4031025, 30))
                {
                    await SayNext("检查你的杂项物品栏是否有可用的空位。");
                    return;
                }

                gainItem(4031025, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext("检查你的杂项物品栏是否有可用的空位。");
                    return;
                }

                (int item, int count)[] repeatablePrizes = [(4010000, 3), (4010001, 3), (4010002, 3), (4010003, 3), (4010004, 3), (4010005, 3)];
                var itemPrize = Randomizer.Select(repeatablePrizes);
                gainItem(itemPrize.item, itemPrize.count);
            }

            warp(105040300, 0);
        }


        // Npc: 1063001 
        public async Task viola_blue()
        {
            if (isQuestStarted(2053) && !haveItem(4031026, 30))
            {
                if (!canHold(4031026, 30))
                {
                    await SayNext("检查你的杂项物品栏是否有可用的空位。");
                    return;
                }

                gainItem(4031026, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext("检查你的杂项物品栏是否有可用的空位。");
                    return;
                }

                (int item, int count)[] repeatablePrizes = [(4020000, 4), (4020002, 4), (4020006, 4)];
                var itemPrize = Randomizer.Select(repeatablePrizes);
                gainItem(itemPrize.item, itemPrize.count);
            }

            warp(105040300, 0);
        }


        // Npc: 1063002 
        public async Task viola_white()
        {
            if (isQuestStarted(2054) && !haveItem(4031028, 30))
            {
                if (!canHold(4031028, 30))
                {
                    await SayNext("Check for a available slot on your ETC inventory.");
                    return;
                }

                gainItem(4031028, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext("检查你的杂项物品栏是否有可用的空位。");
                    return;
                }

                (int item, int count)[] repeatablePrizes = [(4010006, 4), (4010007, 4), (4020007, 4)];
                var itemPrize = Randomizer.Select(repeatablePrizes);
                gainItem(itemPrize.item, itemPrize.count);
            }

            warp(105040300, 0);
        }


        // Npc: 1063012, 1063013 
        public async Task holySton()
        {
            int[] maps = [105050200, 105060000, 105070000, 105090000, 105090000, 105090100];
            if (getQuestStatus(2236) != 1)
            {
                return;
            }

            var mapId = getPlayer().getMapId();
            var playerY = getPlayer().getPosition().Y;
            int slot = -1;

            for (var i = 0; i < maps.Length; i++)
            {
                if (mapId == maps[i])
                {
                    if (mapId == 105090000 && playerY < 78)
                    {
                        slot = 4;
                    }
                    else if (mapId == 105090000 && playerY > 78)
                    {
                        slot = 3;
                    }
                    else
                    {
                        slot = i;
                    }
                    break;
                }
            }

            if (slot == -1)
            {
                return;
            }

            var progress = getQuestProgress(2236, 1);
            var ch = progress[slot];

            if (ch == '0')
            {
                var nextProgress = progress.Substring(0, slot) + '1' + progress.Substring(slot + 1);
                gainItem(4032263, -1);
                setQuestProgress(2236, 1, nextProgress);
                await SayOK("由于灵符的法力，封印了该地区的邪恶势力。");
            }
        }


        // Npc: 1063016 
        public async Task DollWayKeeper1()
        {
            if (await SayYesNo("你要退出这个试炼吗？"))
            {
                warp(105040201, 2);
            }
        }


        // Npc: 1063017 
        public async Task DollWayKeeper2()
        {
            if (await SayYesNo("欢迎回来，弗朗西斯主人。要进入主人的洞窟吗？"))
            {
                if (getMap(925020010).getAllPlayers().Count > 0)
                {
                    await SayOK("有人在里面了。请稍后再进入。");
                }
                else
                {
                    getWarpMap(910510202).spawnMonsterOnGroundBelow(9300346, 95, 200);
                    warp(910510202, 0);
                }
            }
        }




        // Npc: 1081001 
        public async Task florina1()
        {
            var returnmap = getPlayer().peekSavedLocation("FLORINA");
            if (returnmap == -1)
            {
                returnmap = 104000000;
            }
            if (await SayYesNo("所以你想离开 #b#m110000000##k 吗？如果你想的话，我可以带你回到 #b#m" + returnmap + "##k。"))
            {
                getPlayer().getSavedLocation("FLORINA");
                warp(returnmap);
            }
            else
            {
                await SayNext("你一定有一些事情要在这里处理。在#m" + returnmap + "#休息一下也不错。看看我，我是如此喜欢这里，结果我最终在这里定居了。哈哈哈，无论如何，当你想回去的时候再来找我说话。");
            }
        }





        // Npc: 1091003 
        public Task refine_nautillus()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1092000 
        public async Task nautil_cow()
        {
            await SaySpeech([
                "好的，我现在会把你送到我的牛棚去。小心那些喝光所有牛奶的小牛犊。你可不想白费力气。",
                "这不容易一眼就分辨出小牛和母牛。这些小牛可能只有一个月或两个月大，但它们已经长到和它们的母亲一样大了。它们甚至看起来很像……有时候连我自己都会感到困惑！祝你好运！"
                ]);
            if (canHold(4031847))
            {
                gainItem(4031847, 1);
                warp(912000100, 0);
            }
            else
            {
                await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
            }
        }


        // Npc: 1092007 
        public async Task nautil_black()
        {
            if (await SayYesNo("你想要被传送到黑魔法师的弟子那里吗？"))
            {
                warp(912000000, 0);
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }


        // Npc: 1092008 
        public async Task s4mind_in()
        {
            if (!isQuestStarted(6410))
            {
                await SayOK(GetDefault0());
                dispose();
            }
            else
            {
                if (await SayYesNo("让我们去救 #r#p2095000##k 吧？"))
                {
                    warp(925010000, 0);
                }
            }
        }


        // Npc: 1092010 
        public async Task remove_DirtytreasureMap()
        {
            if (!haveItem(4220153))
            {
                await SayOK("(抓抓抓...)");
            }
            else
            {
                if (await SayYesNo("嘿，你那张不错的#b藏宝图#k是从哪里得到的？如果你不需要了，我可以替诺特勒斯船员保管吗？"))
                {
                    gainItem(4220153, -1);
                }
            }
        }


        // Npc: 1092016 
        public async Task nautil_stone()
        {
            if (isQuestStarted(2166))
            {
                await SayNext("这是一块美丽而闪亮的岩石。我能感受到它周围的神秘力量。");
                completeQuest(2166);
            }
            else
            {
                await SayNext("我用手碰了一下闪闪发光的石头，感觉到一股神秘的力量流入我的身体。");
            }
        }


        // Npc: 1092018 
        public async Task nautil_letter()
        {
            var qs = getQuestStatus(2162);

            if ((qs == 0 || qs == 1) && !haveItem(4031839, 1))
            {
                if (canHold(4031839, 1))
                {
                    gainItem(4031839, 1);
                    await SayNext("你检索到了一个从垃圾桶中取出的皱巴巴的纸张。它的内容似乎很重要。");
                }
                else
                {
                    await SayNext("(你看到一个从垃圾桶中伸出的皱巴巴的纸张。内容似乎很重要，但由于你的背包已满，无法将其取出。)");
                }
            }
        }


        // Npc: 1092019 
        public async Task s4strike()
        {
            if (!isQuestStarted(6400))
            {
                await SayOK(GetDefault0());
            }
            else
            {
                var seagullProgress = getQuestProgressInt(6400, 1);
                if (seagullProgress == 0)
                {
                    string[] seagullQuestion = [
                    "One day, I went to the ocean and caught 62 Octopi for dinner. But then some kid came by and gave me 10 Octopi as a gift! How many Octopi do I have then, in total?"
                    ];
                    string[] seagullAnswer = ["72"];
                    await SayNext("好的！我现在就给你第一个问题！你最好准备好，因为这个问题很难。甚至这里的海鸥都觉得这个问题相当棘手。这是一个相当困难的问题。");
                    for (int i = 0; i < seagullQuestion.Length; i++)
                    {
                        var inputAnswer = await SayInputText(seagullQuestion[i]);
                        if (inputAnswer == seagullAnswer[i])
                        {
                            continue;
                        }
                        else
                        {
                            await SayOK("嗯，那不太符合我的记忆。再试一次吧！");
                            return;
                        }
                    }
                    setQuestProgress(6400, 1, 1);
                    await SayNext("什么！我简直不敢相信你有多聪明！不可思议！在海鸥世界里，这种智慧会让你获得博士学位，甚至更多。你真是太了不起了……我简直不敢相信……我简直不敢相信！");
                    return;
                }
                else if (seagullProgress == 1)
                {
                    await SaySpeech([
                        "现在~让我们继续下一个问题。这个问题真的很难。我要让巴特来帮我。你认识巴特，对吧？",
                        "我要把你送到尼奥之舟的一个空房间里。你会在那里看到9个巴特。哈哈哈~他们是双胞胎吗？不，不，当然不是。我在这个意志测试中使用了一点魔法。",
                        "无论如何，9个巴特中只有一个是真正的巴特。你知道海盗以他们与其他海盗的友谊和同伴关系而闻名。如果你是一个真正的海盗，你应该能够轻松地找到自己的伙伴。好了，那么我会把你送到巴特所在的房间。"
                        ]);
                    var em = GetEventManager<SoloEventManager>("4jaerial");
                    await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
                }
                else
                {
                    await SayOK("哦！哇，这真是令人印象深刻！我认为我的考验相当困难，而你竟然通过了……你确实是海盗家族中不可或缺的一员，也是海鸥的朋友。我们现在因着持久的友谊而结为知己！最重要的是，朋友就是在你陷入困境时伸出援手的。如果你遇到紧急情况，就呼唤我们海鸥吧。");
                }

            }

        }


        async Task MomCow(int idx)
        {
            if (getQuestProgressInt(2180, 1) == idx)
            {
                await SayNext("你最近已经从这头牛身上挤过奶了，请检查另一头牛。");
                return;
            }

            if (canHold(4031848) && haveItem(4031847))
            {
                gainItem(4031847, -1);
                gainItem(4031848, 1);

                setQuestProgress(2180, 1, idx);

                await SayNext("现在用牛奶把瓶子装满。瓶子现在装了三分之一的牛奶。");
            }
            else if (canHold(4031849, 1) && haveItem(4031848))
            {
                gainItem(4031848, -1);
                gainItem(4031849, 1);

                setQuestProgress(2180, 1, idx);

                await SayNext("现在用牛奶把瓶子装满。瓶子现在装了三分之二的牛奶。");
            }
            else if (canHold(4031850) && haveItem(4031849))
            {
                gainItem(4031849, -1);
                gainItem(4031850, 1);

                setQuestProgress(2180, 1, idx);

                await SayNext("现在用牛奶把瓶子装满。瓶子现在完全装满了牛奶。");
            }
            else
            {
                await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
            }
        }
        // Npc: 1092090, 1092091, 1092092 
        public Task mom_cow()
        {
            return MomCow(getNpc() - 1092089);
        }


        // Npc: 1092093, 1092094, 1092095 
        public async Task baby_cow()
        {
            if (haveItem(4031847))
            {
                await SayNext("小牛饿了，正在喝光所有的牛奶！奶瓶里一滴奶都没有了……");
            }
            else if (haveItem(4031848) || haveItem(4031849) || haveItem(4031850))
            {
                gainItem(4031848, -1);
                gainItem(4031849, -1);
                gainItem(4031850, -1);

                gainItem(4031847, 1);
                await SayNext("小牛很饿，正在喝光所有的牛奶！奶瓶现在空了。");
            }
        }

        // Npc: 1094002, 1094003, 1094004, 1094005, 1094006 
        public async Task nautil_Abel1()
        {
            if (!isQuestStarted(2186))
            {
                await SayOK("只是一堆箱子，没什么特别的……");
                return;
            }

            await SayNext("你想要获得一副眼镜吗？");
            if (!(haveItem(4031853) || haveItem(4031854) || haveItem(4031855)))
            {
                var rolled = Random.Shared.Next(3);

                if (rolled == 0)
                {
                    gainItem(4031853, 1);
                }
                else if (rolled == 1)
                {
                    gainItem(4031854, 1);
                }
                else
                {
                    gainItem(4031855, 1);
                }
            }
            else
            {
                await SayOK("你已经拥有了这里的眼镜！");
            }
        }


        // Npc: 1095000 
        public async Task s4mind_out()
        {
            if (await SayYesNo("#b#p2095000##k一定有办法爬上这个悬崖，根据我们最新的报告... 或者你是想要 #r离开这里#k 吗？"))
            {
                warp(120000104);
            }
        }


        // Npc: 1100003 
        public async Task contimoveEreEli()
        {
            var contiMove = GetEventManager<PrivateContiMove>("ShipEllin");
            if (await SayYesNo("嗯，你好...又来了。你想离开圣地去别的地方吗？如果是的话，你来对地方了。我经营着一艘从#b圣地#k到#b金银岛#k的渡船，如果你愿意的话，我可以带你去#b金银岛#k...你需要支付#b1000#k金币的费用。\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    var r = contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }

                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }
        }


        // Npc: 1100004 
        public async Task contimoveEreOrb()
        {
            var contiMove = GetEventManager<PrivateContiMove>("ShipOribs");
            if (await SayYesNo("嗯...风势正好。你是不是想离开圣地去别的地方？这艘渡船开往神秘岛的天空之城。你在圣地需要办的事情都处理好了吗？如果你正好要去#b天空之城#k，我可以带你去那里。你怎么样？要去天空之城吗？\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    var r = contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }

        }


        // Npc: 1100005 
        public async Task talkVic()
        {
            await SayNext(GetDefault0());
        }


        // Npc: 1100006 
        public async Task talkOrv()
        {
            await SayNext(GetDefault0());
        }


        // Npc: 1100007 
        public async Task contimoveEliEre()
        {
            var contiMove = GetEventManager<PrivateContiMove>("ShipEllin");
            if (await SayYesNo("嗯...那么...嗯...你是想离开金银岛去其他地区吗？你可以乘这艘船去#b圣地#k。在那里，你会看到明亮的阳光照在树叶上，感受到轻柔的微风拂过你的皮肤。那里是神兽和女皇所在的地方。你想去圣地吗？大约需要#b2分钟#k，费用是#b1000#k金币。\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    var r = contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }
        }


        // Npc: 1100008 
        public async Task contimoveOrbEre()
        {
            var contiMove = GetEventManager<PrivateContiMove>("ShipOribs");
            if (await SayYesNo("这艘船将驶向#b圣地#k，那里是一个浮空的岛屿，你会看到明亮的阳光照在树叶上，感受到轻柔的微风拂过你的皮肤，还有女皇——希纳斯。如果你有兴趣加入皇家骑士团，那么你一定要来这里看看。你有兴趣去圣地吗？这次旅行将花费你#b1000#k金币\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    var r = contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        gainMeso(-1000);
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }
        }


        // Npc: 1101001 
        public async Task createCygnus()
        {
            if (getPlayer().isCygnus() && GameConstants.getJobBranch(getJob()) > 2)
            {
                useItem(2022458);
                await SayOK("让我为你祈福，我的骑士。请保护冒险岛的世界……");
            }
            else
            {
                await SayOK("不要停止训练。你的每一分能量都需要用来保护冒险岛的世界……");
            }
        }


        // Npc: 1101008 
        public async Task helperCygnus()
        {
            var option = await SayOption("等等！等你达到10级的时候，你会自己搞清楚这些东西的，但如果你绝对想提前准备，你可以查看以下信息。\r\n\r\n 告诉我，你想知道什么？\r\n#b#L0#关于你#l\r\n#L1#小地图#l\r\n#L2#任务窗口#l\r\n#L3#背包#l\r\n#L4#普通攻击狩猎#l\r\n#L5#如何拾取物品#l\r\n#L6#如何装备物品#l\r\n#L7#技能窗口#l\r\n#L8#如何使用快捷栏#l\r\n#L9#如何打开箱子#l\r\n#L10#如何坐在椅子上#l\r\n#L11#世界地图#l\r\n#L12#任务通知#l\r\n#L13#增强属性#l\r\n#L14#谁是皇家骑士？#l");
            switch (option)
            {
                case 0:
                    await SaySpeech([
                        "我侍奉着守护女皇希纳斯。我的主人命令我引导所有来到枫之世界的人加入皇家骑士团。在你成为骑士或达到11级之前，我会一直在你身边协助和跟随你。如果你有任何问题，请告诉我。",
                        "现在没有必要检查这些信息。这些都是你在游戏中会逐渐学会的基础知识。当你达到10级后，你可以随时问我遇到的问题，所以放松一点吧。"
                        ]);
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                    guideHint(option);
                    break;
                case 14:
                    await SayOK("黑魔法师正试图复活并征服我们和平的枫之世界。作为对这一威胁的回应，女皇希纳斯组建了一个骑士团，现在被称为皇家骑士团。当你达到10级时，你可以成为一名骑士。");
                    break;
                default:
                    break;
            }
        }


        // Npc: 1102001 
        public async Task outSecondDH()
        {
            if (await SayYesNo("你想要退出训练大厅吗？"))
            {
                warp(130020000, 0);
            }
        }


        // Npc: 1102002 
        public async Task giveupRiding()
        {
            await SayOK(GetDefault0());
        }



        // Npc: 1103005 
        public async Task erebWarp()
        {
            if (await SayAcceptDecline("Becoming a Knight of Cygnus requires talent, faith, courage, and will power... and it looks like you are more than qualified to become a Knight of Cygnus. What do you think? If you wish to become one right this minute, I'll take you straight to Erev. Would you like to head over to Erev right now?"))
            {
                warp(130000000);
            }
            else
            {
                warp(getPlayer().getSavedLocation("CYGNUSINTRO"));
            }
        }


        // Npc: 1104000, 1204006 
        public async Task DollMaster()
        {
            if (getMapId() == 910400000)
            {
                await SaySpeech([
                    new SpeechText("哼……真的找来了，太容易了？看来你的变身术还是有点用处的嘛。巴洛克，我们走。", 1),
                    new SpeechText("切……这笔帐将来再算。", (byte)(NpcTalkSpeaker.ExtraNpc | NpcTalkSpeaker.WithoutEnd), 1204004),
                    new SpeechText("来得正好。上一次因为刚和冒险骑士团的骑士们战斗完，已经没什么余力了，才会让你得逞。这次我可没那么好惹了！碍眼的家伙，消失掉吧！",1)
                    ]);
                setQuestProgress(21733, 21762, 1);

                var mapObj = getMap();
                if (mapObj.countMonster(9300382) > 0)
                {
                    mapObj.killMonster(9300382);
                }

                var npcPos = mapObj.getMapObject(getNpcObjectId())!.getPosition();
                mapObj.spawnMonsterWithEffect(LifeFactory.Instance.GetMonsterTrust(9300345), 12, new Point(npcPos.X + 50, npcPos.Y));
                mapObj.destroyNPC(getNpc());
            }
            else
            {
                await SayNext("什么……你不属于这里！");
                var puppet = GetEventManager<Puppeteer>("Puppeteer");
                puppet.setProperty("player", getPlayer().getName());
                var r = puppet.StartInstance(getPlayer());
                await SayOK(puppet.HandleCreateInstanceResult(r, c));

            }
        }


        // Npc: 1104002 
        public async Task blackWitch()
        {
            if (!isQuestStarted(20407))
            {
                await SayOK("... Knight, you still #bseem unsure to face this fight#k, don't you? There's no grace in challenging someone when they are still not mentally ready for the battle. Talk your peace to that big clumsy bird of yours, maybe it'll put some guts on you.");
                return;
            }

            if (await SayAcceptDecline("Hahahahaha! This place's Empress is already under my domain, that's surely a great advance on the #bBlack Wings#k' overthrow towards Maple World... And you, there? Still wants to face us? Or, better yet, since you seem strong enough to be quite a supplementary reinforcement at our service, #rwill you meet our expectations and fancy joining us#k since there's nothing more you can do?"))
            {
                Pink("Eleanor: Oh, lost the Empress and still challenging us? Now you've done it! Prepare yourself!!!");

                getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.getMonster(9001010), new Point(850, 0));
                getMap().destroyNPC(1104002);

            }
            else
            {
                await SayOK("Heh, cowards have no place on the #rBlack Mage's#k army. Begone!");
            }
        }


        // Npc: 1104200 
        public async Task enterBlackEreb()
        {
            await SayNext("#b#p1104002##k... The black witch... Trapped me here... There's no time now, she's already on her way to #rattack Ereve#k!");
            if (await SayYesNo("Fellow Knight, you must reach to #rEreve#k right now, #rthe Empress is in danger#k!! Even in this condition, I can still Magic Warp you there. When you're ready talk to me. #bAre you ready to face Eleanor?#k"))
            {
                if (getWarpMap(913030000).countPlayers() == 0)
                {
                    warp(913030000, 0);
                }
                else
                {
                    await SayOK("There's someone already challenging her. Please wait awhile.");
                }
            }
        }


        // Npc: 1200003 
        public async Task contimoveRieRit()
        {
            var contiMove = GetEventManager<PrivateContiMove>("Whale");
            if (await SayYesNo("搭上了这艘船，你可以前往更大的大陆冒险。 只要給我 #e80 金币#n，我会帶你去 #b金银岛#k 你想要去金银岛吗？"))
            {
                if (haveItem(4031801))
                {
                    await SaySpeech([
                        "搭上了这艘船，你可以前往更大的大陆冒险。 只要給我 #e80 金币#n，我会帶你去 #b金银岛#k 你想要去金银岛吗？",
                        "既然你有推荐信，我不会收你任何的费用。收起來，我们前往金银岛，坐好，旅途中可能会有点动荡！"
                        ], current: 1);

                    var r = contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                    }
                    else
                    {
                        await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
                else
                {
                    await SayNext("真的只要 #e80 金币#n 就能搭船!!");

                    if (getLevel() >= 8)
                    {
                        if (getMeso() < 80)
                        {
                            await SayOK("什么？你说你想搭免费的船？ 你真是个怪人！");
                            return;
                        }
                        else
                        {
                            await SayNext("哇! #e80#n 金币我收到了！ 好，准备触发去明珠港喽！");


                            var r = contiMove.StartInstance(getPlayer());
                            if (r == CreateInstanceResult.Success)
                            {
                                gainMeso(-80);
                            }
                            else
                            {
                                await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                            }
                        }
                    }
                    else
                    {
                        await SayOK("让我看看... 我觉得你还不够强。 你至少要达到7级我才能让你到明珠港哦。");
                        return;
                    }
                }

            }
        }


        // Npc: 1200004 
        public async Task contimoveRitRie()
        {
            var contiMove = GetEventManager<PrivateContiMove>("Whale");

            await SayNext("你考虑离开金银岛前往我们的城镇吗？如果你登上这艘船，我可以带你从#b明珠港#k到#b里恩#k，然后再返回。但你必须支付#b800#k金币的费用。你想去#b里恩#k吗？");
            if (getMeso() < 800)
            {
                await SayNext("嗯... 你确定你有 #b800#k 金币吗？检查一下你的背包，确保你有足够的金币。你必须支付费用，否则我不能让你上船...");
            }
            else
            {
                var r = contiMove.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    gainMeso(-800);
                }
                else
                {
                    await SayNext(contiMove.HandleCreateInstanceResult(r, c));
                }
            }
        }


        // Npc: 1200005 
        public async Task PurotalkRie()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1200006 
        public async Task PurotalkVic()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1202009 
        public async Task enterWolf()
        {
            if (haveItemWithId(1902016, true))
            {
                warp(140010210, 0);
            }
            else
            {
                await SayOK("这是什么？如果你是来浪费我的时间的，滚开！");
            }
        }


        // Npc: 1204001 
        public async Task dollMaster00()
        {
            await SaySpeech([
                new SpeechText("我是黑色之翼的人偶师弗朗西斯。你把我安置的好几个人偶都给找出来了……坏了我的好事。虽然我很恼火，不过这次先放你一马。你要是再敢和我作对………我以黑魔法师大人的名义发誓，绝不放过你。", 9),
                new SpeechText("#b（……黑色之翼？作对？……到底是怎么回事？在怪兽的身上找到人偶与黑魔法师有什么关系？该去找特鲁商里商里。）#k", 3)
                ]);

            completeQuest(21719);
            warp(105040200, 10);
        }

        // Npc: 1204005 
        public async Task downTrue()
        {
            if (getQuestProgressInt(21733, 21762) == 2)
            {
                await SayOK(GetDefault0());
                return;
            }

            setQuestProgress(21733, 21762, 2);

            await SayNext("啊……那些家伙全都消灭了？哈哈……真不愧是英雄大人！呃，先整理整理再说。");
            warp(104000004);
        }


        // Npc: 1204010 
        public async Task giantDagoth()
        {
            await SaySpeech([
                new SpeechText("嗯？怎么回事，你？", 1),
                new SpeechText("前不久倒是听说金银岛上的人偶师被人打倒了，难道是你……", 1),
                new SpeechText("嘿嘿，那反倒好办了！既拿到了#b天空之城封印石#k，又能顺便打倒你的话，我就能在人偶师之上了！出招吧！", 1)
                ]);

            var mapObj = getMap();
            var npcPos = mapObj.getMapObject(getNpcObjectId())!.getPosition();
            mapObj.spawnMonsterWithEffect(LifeFactory.Instance.GetMonsterTrust(9300348), 12, new Point(npcPos.X + 50, npcPos.Y));
            mapObj.destroyNPC(getNpc());
        }

        // Npc: 1204020 
        public async Task ShadowWarrier()
        {
            await SaySpeech([
                new SpeechText("我一直在等你……英雄的后裔啊……", 8),
                new SpeechText("#b（英雄的后裔……？#o9300351#似乎知道一些关于英雄的事情。不过，他好像也和#p2091007#一样，不认为我是英雄本人啊。）", 2),
                new SpeechText("这个#b武陵封印石#k是英雄们撒下的种子……但收获的却是我们黑色之翼的东西。虽然你很漂亮地打败了#p1104000#和#p1204010#……再也不能让你为所欲为了。", 8),
                new SpeechText("英雄的后裔终于和敌人见面了，真是让人感慨万分……这也是没办法的事情。我要以黑色之翼的名义，干掉你！", 2)
                ]);

            var mapObj = getMap();
            var npcPos = mapObj.getMapObject(getNpcObjectId())!.getPosition();
            mapObj.spawnMonsterWithEffect(LifeFactory.Instance.GetMonsterTrust(9300351), 12, new Point(npcPos.X + 50, npcPos.Y));
            mapObj.destroyNPC(getNpc());
        }


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


        // Npc: 1300012 
        public async Task TD_MC_bossEnter()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 1300013 
        public Task TD_MC_violetaEnter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1300014 
        public async Task forself()
        {
            var MapId = getMapId();
            setQuestProgress(2322, 0);
            if (MapId == 106020300)
            {
                //蘑菇森林深处
                if (isQuestActive(2314) && getQuestProgressInt(2314) == 0)
                {
                    showInfo("Effect/OnUserEff/normalEffect/mushroomcastle/chatBalloon3");
                    await SayNext("这里...似乎有点奇怪...？！", 3);
                    await SayNext("嗯...似乎有一种无形的力量在阻止我通过入口。", 3);
                    await SayNext("显然这不是普通的障碍，否则我不可能过不去，也许...这应该是 #e#b#p1300003##k#n 提到的结界了。", 3);
                }
                else if (haveItem(2430014))
                {
                    await SayNext("在这附近使用#e#b#v2430014##t2430014##n#k应该就能消除魔法结界了吧。", 3);
                }
            }
            else if (MapId == 106020500)
            {
                //城墙中央
                if (isQuestActive(2322) && getQuestProgressInt(2322) == 0)
                {
                    await SayOK("嗯...城墙爬满了#r长着尖刺得藤蔓#k，确实有点棘手，看来是过不去了。\r\n\r\n还是先回去跟 #e#b#p1300003##k#n 报告一下吧。", 3);
                    setQuestProgress(2322, 1);
                }
            }
        }


        // Npc: 2001000 
        public async Task desc_tree()
        {
            await SaySpeech([
                "你看到那边站着一群雪人吗？去和他们中的一个交谈，它会带你去这里著名的巨大圣诞树。这棵树可以用各种装饰品来装饰。你觉得怎么样？听起来很有趣，对吧？",
                "只有6个人可以同时在树所在的地方，你不能在那里进行交易或者开设商店。你丢下的饰品只能由你自己捡起来，所以不用担心在这里丢失你的饰品。",
                "当然，掉落在那里的物品永远不会消失。一旦你通过里面的雪人离开那里，你在那张地图上掉落的所有物品都会回到你身边，所以你离开那个地方之前不必捡起所有这些物品。是不是很方便呢？",
                "Well then, go see #p2002001#, buy some Christmas ornaments there, and then decorate the tree with those~ Oh yeah! The biggest, the most beautiful ornament cannot be bought from him. It's probably ... taken by a monster ... huh huh .."
                ], finalNext: false);
        }


        // Npc: 2001001 
        public async Task go_tree1()
        {
            if (await SayYesNo("我们有一棵漂亮的圣诞树。你想看看/装饰它吗？"))
            {
                warp(209000001);
            }
        }


        // Npc: 2001002 
        public async Task go_tree2()
        {
            if (await SayYesNo("我们有一棵漂亮的圣诞树。你想看看/装饰它吗？"))
            {
                warp(209000002);
            }
        }


        // Npc: 2001003 
        public async Task go_tree3()
        {
            if (await SayYesNo("我们有一棵漂亮的圣诞树。你想看看/装饰它吗？"))
            {
                warp(209000003);
            }
        }


        // Npc: 2001004 
        public async Task out_tree()
        {
            if (await SayYesNo("那么，你准备好离开这里了吗？"))
            {
                warp(209000000);
            }
        }


        // Npc: 2001005 
        public Task job_3th()
        {
            // NOT USED
            return Task.CompletedTask;
        }


        // Npc: 2002000 
        public async Task go_victoria()
        {
            if (await SayYesNo("你想离开快乐村吗？"))
            {
                var map = getPlayer().GetSavedLocation(SavedLocationType.HAPPYVILLE);
                if (map == -1)
                {
                    map = 101000000;
                }

                warp(map, 0);
            }
        }





        // Npc: 2012006 
        public async Task getAboard()
        {
            (string map, string transport)[] options = [
                ("魔法密林","飞船"),
                ("玩具城","飞船"),
                ("神木村","飞艇"),
                ("武陵","鹤"),
                ("阿里安特","精灵"),
                ("圣地","渡船"),
                ];
            var option = await SayOption("天空之城的站台有许多个月台，请按照你的目的地进行选择。你想要前往哪里？", options.Select(x => $"前往{x.map}的{x.transport}站台"));
            var targetMap = 200000110 + (option * 10);
            await SayNext($"好的，我会把你送到 #b#m{targetMap}##k 的月台上。");
            warp(targetMap, "west00");
        }




        // Npc: 2012012 
        public async Task oldBook2()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 2012014 
        public async Task ossyria3_1()
        {
            if (haveItem(4001019))
            {
                await SayOK("你可以使用#b#t4001019##k来激活#b#p2012014##k。你要传送到#b#p2012015##k所在的地方吗？");
                gainItem(4001019, -1);
                warp(200082100, 0);
            }
            else
            {
                await SayOK("有一个#b#p2012014##k可以让你传送到#b#p2012015##k所在的地方，但如果没有卷轴就无法激活它。");
            }
        }


        // Npc: 2012015 
        public async Task ossyria3_2()
        {
            if (haveItem(4001019))
            {
                await SayOK("你可以使用#b#t4001019##k来激活#b#p2012014##k。你要传送到#b#p2012015##k所在的地方吗？");
                gainItem(4001019, -1);
                warp(200080200, 0);
            }
            else
            {
                await SayOK("有一个#b#p2012015##k可以让你传送到#b#p2012014##k所在的地方，但如果没有卷轴就无法激活它。");
            }
        }


        // Npc: 2012023 
        public Task s4tornado()
        {
            if (haveItem(4031476))
            {
                if (canHold(4031456, 1))
                {
                    gainItem(4031476, -1);
                    gainItem(4031456, 1);
                }
            }
            return Task.CompletedTask;
        }

        Task ElizaHarp(char harpNote)
        {
            string[] harpSounds = ["do", "re", "mi", "pa", "sol", "la", "si"];   // musical order detected thanks to Arufonsu
            var harpSong = "CCGGAAGFFEEDDC|GGFFEED|GGFFEED|CCGGAAGFFEEDDC|";
            getMap().broadcastMessage(PacketCreator.playSound("orbis/" + harpSounds[getNpc() - 2012027]));

            if (isQuestStarted(3114))
            {
                var idx = -1 * getQuestProgressInt(3114);  // infoEx without infoNumber, must use one progress only, critical hit!

                if (idx > -1)
                {
                    var nextNote = harpSong[idx];

                    if (harpNote != nextNote)
                    {
                        setQuestProgress(3114, 0);

                        getPlayer().sendPacket(PacketCreator.showEffect("quest/party/wrong_kor"));
                        getPlayer().sendPacket(PacketCreator.playSound("Party1/Failed"));

                        message("You've missed the note... Start over again.");
                    }
                    else
                    {
                        nextNote = harpSong[idx + 1];

                        if (nextNote == '|')
                        {
                            idx++;

                            if (idx == 45)
                            {     // finished lullaby
                                message("Twinkle, twinkle, little star, how I wonder what you are.");
                                setQuestProgress(3114, 42);

                                getPlayer().sendPacket(PacketCreator.showEffect("quest/party/clear"));
                                getPlayer().sendPacket(PacketCreator.playSound("Party1/Clear"));

                                return Task.CompletedTask;
                            }
                            else
                            {
                                if (idx == 14)
                                {
                                    message("Twinkle, twinkle, little star, how I wonder what you are!");
                                }
                                else if (idx == 22)
                                {
                                    message("Up above the world so high,");
                                }
                                else if (idx == 30)
                                {
                                    message("like a diamond in the sky.");
                                }
                            }
                        }

                        setQuestProgress(3114, -1 * (idx + 1));
                    }
                }
            }
            return Task.CompletedTask;
        }
        // Npc: 2012027 
        public Task elizaHarp1()
        {
            return ElizaHarp('C');
        }


        // Npc: 2012028 
        public Task elizaHarp2()
        {
            return ElizaHarp('D');
        }


        // Npc: 2012029 
        public Task elizaHarp3()
        {
            return ElizaHarp('E');
        }


        // Npc: 2012030 
        public Task elizaHarp4()
        {
            return ElizaHarp('F');
        }


        // Npc: 2012031 
        public Task elizaHarp5()
        {
            return ElizaHarp('G');
        }


        // Npc: 2012032 
        public Task elizaHarp6()
        {
            return ElizaHarp('A');
        }


        // Npc: 2012033 
        public Task elizaHarp7()
        {
            return ElizaHarp('B');
        }


        // Npc: 2020005 
        public Task oldBook1()
        {
            return SayOK(GetDefault0());
        }

        async Task Job3(int type, string preNpcMap, int preNpc)
        {
            var option = await SayOption("我可以帮你吗？", [
                "我想进行第三次职业转职。",
                "请允许我进行扎昆地牢任务。"
                ]);
            switch (option)
            {
                case 0:
                    // 自定义任务、用来记录状态
                    var questBase = -10000 * type - 3000;
                    var baseJob = JobFactory.GetById(type * 100);
                    var baseJobStr = c.CurrentCulture.GetJobName(baseJob);
                    if (IsQuestNotStarted(questBase - 1))
                    {
                        if (await SayYesNo($"欢迎。我是#b#p{getNpc()}##k，所有{baseJobStr}的首领。你似乎已经准备好迈出这一步，准备好迎接第三职业转职的挑战。太多的{baseJobStr}来来去去，无法达到第三职业转职的标准。你呢？你准备好接受考验，进行第三职业转职了吗？"))
                        {
                            await SaySpeech([
                                $"好的。你将在{baseJobStr}的两个重要方面进行测试：力量和智慧。我现在会向你解释测试的力量部分。还记得在{preNpcMap}的#b#p{preNpc}##k吗？去找他，他会告诉你第一部分测试的细节。请完成任务，并从#p{preNpc}#那里得到#b#t4031057##k。",
                                $"测试智慧的部分只能在你通过了力量测试之后才能开始。#b#t4031057##k 将证明你确实通过了测试。我会提前告诉#b#p{preNpc}##k你要前往那里，所以做好准备。这不会很容易，但我对你有信心。祝你好运。"
                                ]);
                            startQuest(questBase - 1);
                        }
                    }
                    else if (isQuestStarted(questBase - 1))
                    {
                        if (haveItem(4031057))
                        {
                            await SayNext("完成了测试的体能部分，做得很棒。我知道你能做到。现在你已经通过了测试的前半部分，接下来是后半部分。请先把项链给我。");
                            if (haveItem(4031057))
                            {
                                gainItem(4031057, -1);
                                completeQuest(questBase - 1);

                                await SayNext("这是测试的第二部分。这个测试将决定你是否足够聪明，可以迈向伟大的下一步。在冰封雪域的雪地上有一个被雪覆盖的黑暗区域，被称为圣地，甚至怪物也无法到达。在这个区域的中心，有一块被称为圣石的巨大石头。你需要献上一件特殊的物品作为祭品，然后圣石将在当场测试你的智慧。");
                                startQuest(questBase - 2);
                                return;
                            }
                            else
                            {
                                // 原先这里只需要判断进度，并不强制需要物品（力量项链），我这里强制任务道具丢失需要重做
                                await SayNext($"东西不见了？找#b#p{preNpc}##k重做");
                                return;
                            }

                        }
                        else
                        {
                            await SayNext($"去，和#b#p{preNpc}#对话#k，然后给我带来#b#t4031057#。");
                        }
                        return;
                    }
                    else if (IsQuestNotStarted(questBase - 2))
                    {
                        await SayNext("这是测试的第二部分。这个测试将决定你是否足够聪明，可以迈向伟大的下一步。在冰封雪域的雪地上有一个被雪覆盖的黑暗区域，被称为圣地，甚至怪物也无法到达。在这个区域的中心，有一块被称为圣石的巨大石头。你需要献上一件特殊的物品作为祭品，然后圣石将在当场测试你的智慧。");
                        startQuest(questBase - 2);
                    }
                    else if (isQuestStarted(questBase - 2))
                    {
                        if (haveItem(4031058))
                        {
                            await SayNext("做得好，完成了测试的智力部分。你正确地回答了所有问题。我必须说，你展现出的智慧水平让我印象深刻。在我们进行下一步之前，请先把项链交给我。");
                            if (haveItem(4031058))
                            {
                                gainItem(4031058, -1);
                                completeQuest(questBase - 2);

                                await Apply3rdJobChange();
                            }
                            else
                            {
                                await SayNext("东西不见了？找#b#p2030006##k重做");
                                return;
                            }
                        }
                        else
                        {
                            await SayNext("去，和#b#p2030006#对话#k，然后给我带来#b#t4031058##。");
                        }

                        return;
                    }
                    else if (isQuestCompleted(questBase - 2))
                    {
                        await Apply3rdJobChange();
                        return;
                    }
                    break;
                default:
                    break;
            }
        }

        async Task Apply3rdJobChange()
        {
            var nextJob = getJobId() + 1;
            var job = JobFactory.GetById(nextJob);
            var jobStr = c.CurrentCulture.GetJobName(job);
            if (!await SayYesNo($"好的！现在，我将让你成为{jobStr}。在这之前，请确保你的SP已经被充分使用了，你需要用完70级之前获得的所有SP。哦，还有，由于你已经在第二次转职时选择了职业方向，所以在第三次转职时就不需要再次选择了。你现在要进行转职吗？"))
            {
                return;
            }

            if (getPlayer().getRemainingSp() > (getLevel() - 70) * 3)
            {
                await SayNext("请在继续之前使用你所有的SP。");
                return;
            }

            changeJobById(nextJob);
            if (job == Job.CRUSADER)
            {
                await SaySpeech([
                    "你刚刚成为了#b勇士#k。一些新的攻击技能，比如#b怒吼#k和#b连击#k都非常强大，而#b破甲#k将会削弱怪物的防御能力。最好集中精力学习你在战士时期掌握的武器技能。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的战士。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }
            else if (job == Job.WHITEKNIGHT)
            {
                await SaySpeech([
                    "你刚刚成为了#b圣骑士#k。你将会接触到一本新的技能书，其中包含各种新的攻击技能以及基于元素的攻击。建议白骑士继续使用与骑士团成员相配的武器类型，无论是剑还是钝器。有一个名为#b冲锋#k的技能，可以给武器增加冰、火和闪电元素，使白骑士成为唯一能够进行基于元素的攻击的战士。用元素弱化怪物，然后用#b冲锋打击#k造成巨大伤害。这将使你在这里成为一股毁灭性的力量。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的战士。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }
            else if (job == Job.DRAGONKNIGHT)
            {
                await SaySpeech([
                    $"你刚刚成为了#b龙骑士#k。你将会接触到一本新的技能书，其中包含各种新的攻击技能以及基于元素的攻击。建议白骑士继续使用与骑士团成员相配的武器类型，无论是剑还是钝器。有一个名为#b冲锋#k的技能，可以给武器增加冰、火和闪电元素，使白骑士成为唯一能够进行基于元素的攻击的战士。用元素弱化怪物，然后用#b冲锋打击#k造成巨大伤害。这将使你在这里成为一股毁灭性的力量。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的战士。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }

            else if (getJob() == Job.FP_MAGE)
            {
                await SaySpeech([
                    "你现在是#b巫师（火毒）#k了。新的技能书包含了新的和改进的火毒系法术，还有像#b魔力激化#k（增强元素法术）和#b魔法狂暴#k（提高攻击法术的速度）这样的技能，能够让你快速有效地攻击怪物。防御性法术如#b火毒抗性#k（使你对某些元素攻击更强）和#b封印#k（封印怪物）将有助于弥补法师生命值不足的弱点。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的法师。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }
            else if (getJob() == Job.IL_MAGE)
            {
                await SaySpeech([
                    "你现在是#b巫师（冰雷）#k了。新的技能书包含了全新和改进的冰雷系法术，还有像#b魔力激化#k（增强元素法术）和#b魔法狂暴#k（提高攻击法术的速度）这样的技能，能够让你快速有效地攻击怪物。防御性法术如#b冰雷抗性#k（增强对特定元素攻击的抵抗力）和#b封印术#k（封印怪物）将有助于弥补法师生命值不足的弱点。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的法师。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }
            else if (getJob() == Job.PRIEST)
            {
                await SaySpeech([
                    "你现在是#b祭司#k了。新的技能书包含了新的和改进的神圣法术，比如#b圣光#k和#b圣龙召唤#k，以及#b时空门#k（创建通往最近城镇的门）和#b神圣祈祷#k（提高经验值获取）等技能对于组队游戏至关重要。像#b巫毒术#k（将怪物变成蜗牛）这样与众不同的法术使牧师成为所有职业中最独特的职业。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的法师。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }

        }






        // Npc: 2022004 
        public async Task s4common1_out()
        {
            await SayNext("你在那里做得很好，#h0#，干得漂亮。现在我会把你送回埃尔奈斯。当你准备好学习新技能时，带着护身符和我交谈。");
            warp(211000000, "in01");
        }


        // Npc: 2030000 
        public Task goDungeon()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030006 
        public async Task holyStone()
        {
            List<(string quest, string[] options, int answer)> questList = [
        ("在冒险岛中，从Lv1升到Lv2需要多少经验值？", ["20", "15", "4", "12", "16"], 1),
        ("各职业一转时，下列哪项要求是错误的？", ["魔法师 - 等级8级", "海盗 - 敏捷不低于20", "弓箭手 - 敏捷不低于25", "飞侠 - 敏捷不低于20", "战士 - 力量不低于35"], 3),
        ("被怪物攻击时特别的异常状态没有被正确说明的是哪一项？", ["封印 - 不能释放技能", "不死族 - 变成不死族 & 恢复效果减半", "虚弱-移动速度降低", "诅咒 - 经验减少", "昏迷 - 无法移动"], 2),
        ("各职业一转时，下列哪项要求是正确的？", ["海盗 - 25幸运", "魔法师 - 10级", "飞侠 - 25 幸运", "战士 - 30力量", "弓箭手 - 25敏捷"], 4),


        ("下列怪物中，哪组怪物与打倒他所能得到的战利品是正确的对应关系？", ["仙人掌宝宝 - 刺针", "钢甲猪 - 野猪牙", "红小丑 - 黄小丑的帽子", "松松 - 坚果", "蝙蝠 - 蝙蝠翅膀"], 4),
        ("下列怪物中，哪组怪物与打倒他所能得到的战利品是错误的对应关系", ["大侠 - 大侠勋章", "食人花 - 食人花的叶子", "古木妖 - 苗木", "小海象 - 海象尖牙", "僵尸 - 僵尸丢失的臼齿"], 1),
        ("In GM Event, how many FRUIT CAKE you can get as reward?", ["20", "200", "5", "25", "100"], 2),
        ("下列药品中，哪组药品与功能是正确的对应关系？", ["活力神水 - 攻击 +5 持续 3 分钟", "纯净水 - 回复 700 MP", "蛋糕 - 回复 150 HP & MP", "沙拉 - 回复 300 MP", "披萨饼 - 回复400 HP"], 4),
        ("下列药品中，哪组药品与功能是错误的对应关系？", ["活力神水 - 回复 300 MP", "补药 - 恢复虚弱状态", "苹果 - 回复 30 HP", "清晨之露 - 回复3000 MP", "拉面 - 回复 1000 HP"], 3),


        ("绿蘑菇，木妖，蓝水灵，斧木妖，三眼章鱼中级别最高的怪物是哪一个？", ["三眼章鱼", "蓝水灵", "斧木妖", "木妖", "绿蘑菇"], 2),
        ("往返于魔法密林/天空之城的船上会出现哪个怪物？", ["狼人", "绿水灵", "蝙蝠魔", "扎昆", "皮克西"], 2),
        ("在彩虹岛没有出现的怪物是？", ["蘑菇仔", "蓝蜗牛", "绿水灵", "红蜗牛", "飘飘猪"], 4),
        ("在金银岛没有出现的怪物是？", ["火独眼兽", "石球", "蝙蝠怪", "黑木妖", "绿蜗牛"], 1),
        ("在冰封雪域没有出现的怪物是？", ["黑雪人", "黑鳄鱼", "法老王企鹅", "火焰猎犬", "僵尸"], 1),
        ("以下哪种怪物会飞?", ["巫婆", "鳄鱼", "冰独眼兽", "猫鼬", "阿丽莎乐"], 0),
        ("在神秘岛没有出现的怪物是？", ["星光精灵", "幼黄独角狮", "幼红独角狮", "鳄鱼", "野狼"], 3),
        ("在彩虹岛没有出现的怪物是？", ["绿蜗牛", "蘑菇仔", "火独眼兽", "花蘑菇", "蓝水灵"], 2),

        ("唤醒麦吉不需要的材料是哪一个？", ["火焰羽毛", "旧战剑", "冰块", "星石", "妖精之翼"], 4),
        ("以下哪项任务是可以重复完成的?", ["医院之谜", "正义的捐赠", "幽灵的行踪", "艾温的玻璃鞋", "玛雅和奇怪的药物"], 3),
        ("以下哪项不是二转职业", ["巫师", "牧师", "刺客", "枪手", "勇士"], 0),
        ("以下哪项任务要求的等级最高？", ["丘比特信使", "迷失在海洋中", "阿尔卡斯特和黑暗水晶", "消灭兔子", "庞庞的战争"], 2),


        ("金银岛没有的村落是？", ["金海滩", "彩虹村", "明珠港", "勇士部落", "魔法密林"], 1),
        ("你在彩虹岛遇到的第一个NPC是谁？", ["塞拉", "希娜", "路卡斯", "罗杰", "尚克斯"], 1),
        ("在冰封雪域看不到的NPC是？", ["伯坚", "索菲亚", "佩德罗", "珀斯上尉", "卢米"], 1),
        ("在冰封雪域看不到的NPC是？", ["魔法石", "格里巴", "杰夫", "神圣的石头", "保姆珥玛"], 4),
        ("在勇士部落看不到的NPC是？", ["伊安", "索菲亚", "斯密斯", "易德", "麦吉"], 3),
        ("在射手村看不到的NPC是？", ["特奥", "赫丽娜", "玛亚", "皮亚", "莉娜"], 0),
        ("在魔法密林看不到的NPC是？", ["汉斯", "易德", "露饵", "妖精路易", "赛恩"], 2),
        ("在废弃都市看不到的NPC是？", ["后街吉姆", "马龙", "休咪", "鲁克", "钱老板"], 3),
        ("哪个NPC与宠物无关?", ["科尔", "比休斯", "帕特里沙", "威巴", "科洛伊"], 1),
        ("废弃都市中，离家出走的少年阿列克斯的父亲是谁？", ["长老斯坦", "后街吉姆", "铭仁", "比休斯", "卢克"], 0),
        ("哪个NPC不属于天空之城阿尔法小队？", ["查理中士", "巴伯下士", "伊吉上等兵", "珀斯上尉", "彼特"], 4),
        ("在二转过程中，收集30个黑色珠子给转职教官后可以得到什么？", ["古老的戒指", "记忆粉", "仙尘", "英雄证书", "秘密卷轴"], 3),
        ("为了给射手村的玛雅治病，需要给她什么？", ["苹果", "强力灵药", "奇怪的药", "菊花", "橙汁"], 2),
        ("以下与合成或冶炼工作没有关系的NPC是？", ["奈巴", "塞利尔", "塞恩", "易德", "后街吉姆"], 2),
        ("在彩虹岛看不到的NPC是？", ["巴里", "特奥", "皮奥", "赛德", "玛利亚"], 1),
        ("在导航室的监视器里能看到谁和Kyrin在一起？", ["路卡斯", "金博士", "长老斯坦", "斯卡德", "弗利维教授"], 1),
        ("你知道射手村的赫丽娜吗？他的眼睛是什么颜色？", ["蓝色", "绿色", "棕色", "红色", "黑色"], 1),
        ("勇士部落武术教练的帽子上有多少根羽毛？", ["7", "8", "3", "13", "16"], 3),
        ("魔法密林汉斯持有的宝珠是什么颜色?", ["白色", "橙色", "蓝色", "紫色", "绿色"], 2)
    ];

            var questId = -(getJobId() / 100) * 10000 - 3000 - 2;
            if (isQuestStarted(questId) && !haveItem(4031058))
            {
                if (haveItem(4005004, 1))
                {
                    if (!canHold(4031058))
                    {
                        await SayNext("接受此试炼前，请确保有一个空闲的ETC槽位。");
                        return;
                    }
                    else
                    {
                        await SayNext("好的...我将在这里测试你的智慧。所有问题回答正确，你就会通过测试，但是，如果你答错一次，那么你就得重新开始，好吗，我们开始吧。");
                        gainItem(4005004, -1);
                        var questions = questList.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
                        for (int i = 0; i < 5; i++)
                        {
                            var q = questions[i];
                            var inputAnswer = await SayOption(q.quest, q.options);
                            if (inputAnswer != q.answer)
                            {
                                await SayNext("答错了");
                                return;
                            }
                        }
                        gainItem(4031058, 1);
                        await SayOK("好的。你所有问题都答对了。你的智慧得到了验证。拿着这条项链回去吧。");
                    }
                }
                else
                {
                    await SayNext("给我带来一个 #b#t4005004##k 以继续进行试炼。");
                    return;
                }
            }
            await SayOK(GetDefault0());
        }


        // Npc: 2030014 
        public Task s4freeze_item()
        {
            if (haveItem(4031450, 1))
            {
                if (canHold(2280011, 1))
                {
                    gainItem(2280011, 1);
                    gainItem(4031450, -1);
                }
            }
            return Task.CompletedTask;
        }


        // Npc: 2032001 
        public async Task oldBook5()
        {
            if (isQuestCompleted(3034))
            {
                if (await SayYesNo("你对我帮助很大……如果你有任何黑暗水晶矿石，我可以为你精炼，每个只需#b500000金币#k。"))
                {
                    var inputNumber = await SayInputNumber("好的，那么你打算做多少个？", 1, 1, 100);
                    if (getMeso() < 500_000 * inputNumber)
                    {
                        await SayOK("对不起，但我不会免费做这件事。");
                        return;
                    }
                    else if (!haveItem(4004004, 10 * inputNumber))
                    {
                        await SayOK("我需要那些矿石来提炼水晶。没有例外。");
                        return;
                    }
                    else if (!canHold(4005004, inputNumber))
                    {
                        await SayOK("你的库存没有空位吗？先解决这个问题！");
                        return;
                    }

                    gainItem(4004004, -10 * inputNumber);
                    gainMeso(-500000 * inputNumber);
                    gainItem(4005004, inputNumber);
                    sendOk("明智地使用它。");
                }
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }


        // Npc: 2040002 
        public async Task ludi023()
        {
            if (!isQuestStarted(3230))
            {
                await SayOK(GetDefault0());
                return;
            }

            if (await SayYesNo("你准备好进入玩偶屋地图了吗？"))
            {
                var em = GetEventManager<DollHouse>(nameof(DollHouse));
                await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
            }
        }


        // Npc: 2040003 
        public async Task ludi020()
        {
            if (getMapId() == 922000000)
            {
                if (await SayYesNo("你准备好离开这个地方了吗？"))
                {
                    WarpOut();
                }
            }
            else
            {
                if (!isQuestStarted(3239))
                {
                    await SayOK(GetDefault0());
                    return;
                }

                if (await SayYesNo("你准备好进入#b#m922000000##k了吗？"))
                {
                    var em = GetEventManager<q3239>(nameof(q3239));
                    await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
                }
            }

        }


        // Npc: 2040024 
        public async Task ludi014()
        {
            if (haveItem(4001020))
            {
                if (await SayYesNo("您可以使用#b#t4001020##k来激活#b第一个玩具塔石#k。您要传送到第71层的#b第二个玩具塔石#k吗？"))
                {
                    if (haveItem(4001020))
                    {
                        gainItem(4001020, -1);
                        warp(221022900, 3);
                    }

                }
            }
            else
            {
                await SayOK("可以让你传送到#b第二个玩具塔石#k，但如果没有卷轴，它是无法激活的。");
            }
        }


        // Npc: 2040025 
        public async Task ludi015()
        {
            if (haveItem(4001020))
            {
                (string desc, int map)[] options = [("第一个玩具塔石（第100层）", 221024400), ("第三个玩具塔石（第41层）", 221021700)];
                var option = await SayOption("您可以使用#b#t4001020##k来激活#b第二个玩具塔石#k。你想要传送到哪块石头？", options.Select(x => x.desc));

                if (await SayYesNo($"您可以使用#b#t4001020##k来激活#b第二个玩具塔石#k。您要传送到#b{options[option].desc}#k？"))
                {
                    if (haveItem(4001020))
                    {
                        gainItem(4001020, -1);
                        warp(options[option].map, 3);
                    }
                }
            }
            else
            {
                await SayOK("可以让你传送到#b第一或者第三个玩具塔石#k，但如果没有卷轴，它是无法激活的。");
            }
        }


        // Npc: 2040026 
        public async Task ludi016()
        {
            if (haveItem(4001020))
            {
                (string desc, int map)[] options = [("第二个玩具塔石（第71层）", 221022900), ("第四个玩具塔石（第1层）", 221020000)];
                var option = await SayOption("您可以使用#b#t4001020##k来激活#b第三个玩具塔石#k。你想要传送到哪块石头？", options.Select(x => x.desc));

                if (await SayYesNo($"您可以使用#b#t4001020##k来激活#b第三个玩具塔石#k。您要传送到#b{options[option].desc}#k？"))
                {
                    if (haveItem(4001020))
                    {
                        gainItem(4001020, -1);
                        warp(options[option].map, 3);
                    }
                }
            }
            else
            {
                await SayOK("可以让你传送到#b第二或者第四个玩具塔石#k，但如果没有卷轴，它是无法激活的。");
            }
        }


        // Npc: 2040027 
        public async Task ludi017()
        {
            if (haveItem(4001020))
            {
                if (await SayYesNo("您可以使用#b#t4001020##k来激活#b第四个玩具塔石#k。您要传送到第41层的#b第三个玩具塔石#k吗？"))
                {
                    if (haveItem(4001020))
                    {
                        gainItem(4001020, -1);
                        warp(221021700, 3);
                    }

                }
            }
            else
            {
                await SayOK("玩具塔石可以让你传送到#b第三个玩具塔石#k，但如果没有卷轴，它是无法激活的。");
            }
        }


        // Npc: 2040028 
        public async Task ludi024()
        {
            var greeting = "感谢你找到了钟摆。你准备好返回玩具塔了吗？";
            if (isQuestStarted(3230))
            {
                if (haveItem(4031094))
                {
                    completeQuest(3230);
                    gainItem(4031094, -1);
                }
                else
                {
                    greeting = "你还没有找到那个钟摆。你想回到玩具塔吗？";
                }
            }
            if (await SayYesNo(greeting))
            {
                warp(221024400, 4);
            }
        }


        // Npc: 2040031 
        public Task ludi027() => SayOK(GetDefault0());


        // Npc: 2040046 
        public Task friend01()
        {
            return friend00();
        }


        // Npc: 2040052 
        public async Task library()
        {
            (int item, int quest)[] questList = [(4031235, 3615), (4031236, 3616), (4031237, 3617), (4031238, 3618), (4031270, 3630), (4031280, 3633), (4031298, 3639), (4031591, 3620)];

            var completed = questList.Where(x => isQuestCompleted(x.quest)).ToArray();
            if (completed.Length == 0)
            {
                await SayOK("#b#h##k还没有归还一本故事书。");
                return;
            }
            await SaySpeech([
                $"让我看看.. #b#h ##k 一共归还了 #b{completed.Length}#k 本书。归还的书目如下：{string.Join("\r\n", completed.Select(x => $"#v{x.item}# #b#t{x.item}##k"))}",
                "图书馆现在已经安定下来，这主要要归功于你，#b#h ##k的巨大帮助。如果故事再次混乱，那么我希望你能再次来修复它。"
                ]);
        }

        // Npc: 2041023 
        public Task s4efreet()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2041025 
        public async Task Populatus01()
        {
            if (await SayYesNo("滴滴……滴滴……你可以通过我逃到一个更安全的地方。滴滴……滴滴……你想离开这个地方吗？"))
            {
                WarpOut();
            }
        }


        // Npc: 2041026 
        public async Task giveupTimer()
        {
            if (haveItem(4220046, 1))
            {
                // quest completing here when "forfeiting Timer's Egg", as well as reporting missing quests on M. Shrine are thanks to drmdsr & Thora

                gainItem(4220046, -1);
                await SayOK("你想把#r#t4220046##k交给我，对吧？好的，我会替你拿着。");
            }
            else
            {
                await SayOK($"你好！我是#b#p{npc}##k，负责观察和报告这个地区的任何超自然活动。");
            }
        }



        Task Earth()
        {
            if (isQuestStarted(3421))
            {
                var meteoriteId = getNpc() - 2050014;

                var progress = getQuestProgressInt(3421, 1);
                if ((progress >> meteoriteId) % 2 == 0 || (progress == 63 && !haveItem(4031117, 6)))
                {
                    if (canHold(4031117, 1))
                    {
                        progress |= (1 << meteoriteId);

                        gainItem(4031117, 1);
                        setQuestProgress(3421, 1, progress);
                    }
                    else
                    {
                        getPlayer().dropMessage(1, "Have a ETC slot available for this item.");
                    }
                }
            }
            return Task.CompletedTask;
        }
        // Npc: 2050014 
        public Task earth009()
        {
            return Earth();
        }


        // Npc: 2050015 
        public Task earth010()
        {
            return Earth();
        }


        // Npc: 2050016 
        public Task earth011()
        {
            return Earth();
        }


        // Npc: 2050017 
        public Task earth012()
        {
            return Earth();
        }


        // Npc: 2050018 
        public Task earth013()
        {
            return Earth();
        }


        // Npc: 2050019 
        public Task earth014()
        {
            return Earth();
        }


        // Npc: 2060005 
        public async Task tamepig_enter()
        {
            if (isQuestCompleted(6002))
            {
                await SayOK("谢谢你救了那只猪。");
            }
            else if (isQuestStarted(6002))
            {
                if (haveItem(4031507, 5) && haveItem(4031508, 5))
                {
                    await SayOK("谢谢你救了那只猪。");
                }
                else
                {
                    var em = GetEventManager<SoloEventManager>("3rdJob_mount");
                    if (em == null)
                    {
                        await SayOK("抱歉，但是三转职业（骑宠）已关闭。");
                    }
                    else
                    {
                        if (em.StartInstance(getPlayer()) != CreateInstanceResult.Success)
                        {
                            removeAll(4031507);
                            removeAll(4031508);
                        }
                        else
                        {
                            await SayOK("当前地图上有其他玩家，稍后再来吧。");
                        }
                    }
                }
            }
            else
            {
                await SayOK("只有少数冒险者有资格保护守望者猪。");
            }
        }


        // Npc: 2060009 Map 251000100,230000000
        public async Task aqua_taxi()
        {
            // 230030200 水下世界/矩形地带
            // 230010000 水下世界/海底叉路
            // 230000000 水下世界
            // 251000000 百草堂
            // 251000100 百草堂/呼啸的大海
            Dictionary<int, string> dict = new()
            {
                {0,"我想用 #t4031242##k 移动到 #b#m230030200##k." },
                {1,"去 #b#m230030200##k 需支付 #b10000金币#k." },

                {2,"去 #b#m230000000##k 需支付 #b10000金币#k." },
                {3,"去 #b#m251000000##k 需支付 #b10000金币#k." },

                {4,"去 #b#m230010000##k 需支付 #b10000金币#k." },
            };

            List<string> optioins = [];
            if (!haveItem(4031242))
            {
                dict.Remove(0);
            }
            else
            {
                dict.Remove(1);
            }

            if (getMapId() == 251000100)
            {
                dict.Remove(3);
            }

            else if (getMapId() == 230000000)
            {
                dict.Remove(2);
            }

            var option = await SayOption("", dict);
            switch (option)
            {
                case 0:
                    if (haveItem(4031242))
                    {
                        gainItem(4031242, -1);
                        warp(230030200);
                    }
                    break;
                default:
                    if (getMeso() > 10000)
                    {
                        gainMeso(-10000);
                        Dictionary<int, int> maps = new()
                        {
                            {1, 230030200 },
                            {2, 230000000 },
                            {3, 251000000 },
                            {4, 230010000 },
                        };
                        warp(maps[option]);
                    }
                    break;
            }
        }


        // Npc: 2060100 
        public async Task s4common2()
        {
            if (isQuestStarted(6301))
            {
                if (haveItem(4000175))
                {
                    gainItem(4000175, -1);
                    warp(923000000, 0);
                }
                else
                {
                    await SayOK("为了打开维度裂缝，你必须拥有一块#t4000175#。这些可以通过击败皮亚奴斯来获得。");
                }
            }
            else
            {
                await SayOK($"我是#b#p{npc}##k。别和我胡闹，因为我有把人变成蠕虫的习惯。");
            }
        }



        // Npc: 2071012 
        public async Task foxLaidy()
        {
            if (getQuestProgressInt(23647, 1) != 0)
            {
                await SayOK(GetDefault0());
                return;
            }

            if (!haveItem(4031793, 1))
            {
                await SayOK("嗯...嘿...你能帮我找到我在树林里丢失的一块柔软而闪亮的银色毛皮吗？我需要它，我需要它，我非常非常需要它！");
                return;
            }

            if (await SayYesNo("嘿... 嗯... 你能帮我找到我在树林里丢失的一块#b柔软而闪亮的银色毛皮#k吗？我需要它，我需要它，我非常非常需要它！... 哦，你找到了它！！！你会把它给我吗？"))
            {
                await SayNext("嘿嘿嘿~这是你从我这里拿走的报酬，你值得拥有。");
                gainItem(4031793, -1);
                gainFame(-5);
                setQuestProgress(23647, 1, 1);
            }
        }



        // Npc: 2080000 
        public Task minar_weapon()
        {
            return Task.CompletedTask;
        }


        // Npc: 2081000 
        public async Task job4_item()
        {
            var option = await SayOption("...我可以帮你吗？\r\n#L0##b购买魔法种子#k#l\r\n#L1##b为利夫雷做点什么#k#l");
            switch (option)
            {
                case 0:
                    await SayOption("你好像不是本地人。我能帮你吗？#L0##b我想要一些#t4031346#。#k#l");
                    var inputNumber = await SayInputNumber("#b#t4031346##k is a precious iteml I cannot give it to you just like that. How about doing me a little favor? Then I'll give it to you. I'll sell the #b#t4031346##k to you for #b30,000 mesos#k each. Are you willing to make the purchase? How many would you like, then?", 1, 1, 99);
                    var cost = inputNumber * 30000;
                    if (await SayYesNo("购买 #b" + inputNumber + "个 #t4031346#(s)#k 将花费你 #b" + cost + " 金币#k。你确定要购买吗？"))
                    {
                        if (getMeso() < cost || !canHold(4031346, inputNumber))
                        {
                            await SayOK("请检查并查看您是否有足够的金币来进行购买。另外，我建议您检查杂项物品栏，看看是否有足够的空间来进行购买。");
                        }
                        else
                        {
                            await SayOK("再见~");
                            gainItem(4031346, inputNumber);
                            gainMeso(-cost);
                        }
                    }
                    else
                    {
                        await SayOK("请仔细考虑。一旦你做出了决定，请告诉我。");
                    }
                    break;
                case 1:
                    await SayNext("正在开发中...");
                    break;
                default:
                    break;
            }
        }



        // Npc: 2081009 
        public async Task s4blocking_enter()
        {
            if (isQuestStarted(6180) && getQuestProgressInt(6180, 9300096) < 200)
            {
                if (await SayYesNo("请注意：在你待在训练场内的时候，确保你已经装备了#t1092041#，这非常重要。你准备好去训练区了吗？"))
                {
                    if (getPlayer().haveItemEquipped(1092041))
                    {
                        warp(924000001, 0);
                    }
                    else
                    {
                        await SayOK("请在进入训练场之前装备#r#t1092041##k。");
                        return;
                    }
                }
            }
            else
            {
                await SayOK("只有指定人员才能进入训练场地。");
            }
        }


        // Npc: 2081010 
        public async Task s4blocking()
        {
            if (await SayYesNo("你想要退出这个区域吗？如果你退出，你将需要从头开始这个任务。"))
            {
                warp(240010400, "st00");
            }
        }


        // Npc: 2082003 
        public async Task flyminidraco()
        {
            await SayOption("如果你有翅膀，我相信你可以去那里。但是，仅仅这样还不够。如果你想穿越比刀锋还锋利的风，你还需要坚硬的鳞片。我是唯一知道回去的半人半龙……如果你想去那里，我可以变你。无论你是什么，此刻，你将成为一只#b龙#k……\r\n#L0##b我想成为一只龙。#k#l");
            useItem(2210016);
            warp(200090500, 0);
        }


        // Npc: 2082004 
        public async Task TD_neo_Andy()
        {
            await SayOK($"嗨，我是{npc}，来自一个并不那么遥远的未来的时间旅行者。我来阻止这个时代贪婪的人们制造机器。他们在我的时代变得疯狂，把一切都消耗成了灰尘。我必须不惜一切代价阻止它！");
        }


        // Npc: 2083005 
        public async Task s4holycharge()
        {
            if (isQuestStarted(6280))
            {
                if (hasItem(4031454))
                {
                    await SayOK("你从喷泉里往杯子里倒了一些水。");
                    gainItem(4031454, -1);
                    gainItem(4031455, 1);
                }
            }
        }


        // Npc: 2083006 
        public Task TD_neoCity_enter()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 2090005 
        public async Task crane()
        {
            var curMap = getMapId();
            PrivateContiMove? contiMove = null;
            if (curMap == MapId.HERB_TOWN)
            {
                if (await SayYesNo($"你好。旅行进行得怎么样？我一直在像你这样的旅行者运送到#b{c.CurrentCulture.GetMapStreetName(250000100)}#k，而且……你有兴趣吗？这种方式没有船稳定，所以你得紧紧抓住，但我可以比船快得多地到达那里。只要你支付#b金币#k，我就会带你去那里。"))
                {
                    if (getMeso() > 1500)
                    {
                        warp(250000100, 0);
                        gainMeso(-1500);
                    }
                    else
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                }
                else
                {
                    await SayNext("改变想法随时跟我搭话吧。");
                }
                return;

            }
            else if (curMap == 200000141)
            {
                // 天空之城
                await SayOption("你好。旅行进行得怎么样？我已无数次将像你这样的冒险家快速送达其他区域，现在……你有兴趣吗？若愿意，请选择你想前往的城镇。", [
                    $"#m250000100# (1500 金币)",
                    ]);
                if (!await SayYesNo($"你确定要去 #b{c.CurrentCulture.GetMapStreetName(250000100)}#k 吗？如果你有 #b1500 金币#k, 我现在就带你去。"))
                {
                    await SayNext("改变想法随时跟我搭话吧。");
                    return;
                }
                contiMove = GetEventManager<PrivateContiMove>("Crane");
            }
            else if (curMap == 250000100)
            {
                // 武陵
                int[] options = [200000141, 251000000];
                var option = await SayOption("你好。旅行进行得怎么样？我明白，与能翱翔天际的我相比，双腿行走要艰难得多。我已无数次将像你这样的冒险家快速送达其他区域，现在……你有兴趣吗？若愿意，请选择你想前往的城镇。",
                                    options.Select(x => $"{c.CurrentCulture.GetMapStreetName(x)} (1500 金币)"));

                if (option == 1)
                {
                    if (getMeso() > 1500)
                    {
                        warp(options[option], 0);
                        gainMeso(-1500);
                    }
                    else
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                    return;
                }
                else
                {
                    if (!await SayYesNo($"你确定要去 #b{c.CurrentCulture.GetMapStreetName(options[option])}#k 吗？如果你有 #b1500 金币#k, 我现在就带你去。"))
                    {
                        await SayNext("改变想法随时跟我搭话吧。");
                        return;
                    }
                    contiMove = GetEventManager<PrivateContiMove>("Crane");
                }


            }

            if (contiMove != null)
            {
                if (getMeso() > 1500)
                {
                    var r = contiMove.StartInstance(getPlayer());
                    if (r == CreateInstanceResult.Success)
                    {
                        gainMeso(-1500);
                    }
                    else
                    {
                        await SayOK(contiMove.HandleCreateInstanceResult(r, c));
                    }
                }
            }
        }


        // Npc: 2091009 
        public async Task enterShadow()
        {
            var inputText = await SayInputText("The entrance of the Sealed Shrine... #b暗号#k!");
            if (getWarpMap(925040100).countPlayers() > 0)
            {
                await SayOK("有人已经在前往封印神殿的路上了。");
                return;
            }
            if (getText() == "道可道非常道")
            {
                if (isQuestStarted(21747) && getQuestProgressInt(21747, 9300351) == 0)
                {
                    warp(925040100, 0);
                }
                else
                {
                    Pink("虽然你说的是正确的答案，但一些神秘的力量挡住了进来的路。");
                }

                return;
            }
            else
            {
                await SayOK("#r错误！");
            }
        }


        // Npc: 2095000 
        public Task s4mind()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 2101003 
        public async Task adin_enter()
        {
            await SayOK(GetDefaultRandom()); ;
        }


        // Npc: 2101011 
        public async Task cejan()
        {
            await SayOK(GetDefaultRandom()); ;
        }


        // Npc: 2101013 
        public async Task karakasa()
        {
            await SayNext("我不知道你是怎么发现这个的，但你来对地方了！对于那些在尼哈尔沙漠徘徊并开始想家的人，我提供直飞金银岛的航班，不停歇！别担心飞船——它只摔过一两次！你在那艘小飞船上长时间飞行时不觉得幽闭恐惧吗？");
            if (await SayYesNo("请记住两件事。一，这条线路实际上是用于海外运输，所以 #r我不能保证你会降落在哪个城镇#k。二，由于我要安排你乘坐这个特殊航班，费用会有点高。服务费是 #e#b10,000 枚金币#n#k。有一架航班即将起飞。你对这个直达航班感兴趣吗？"))
            {
                await SayNext("好的，准备起飞~");
                if (getMeso() >= 10000)
                {
                    gainMeso(-10000);
                    int[] towns = [100000000, 101000000, 102000000, 103000000, 104000000];
                    warp(Randomizer.Select(towns));
                }
                else
                {
                    await SayNext("嘿，你手头紧吗？我告诉过你，你需要 #b10,000#k 金币才能参与这个活动。");
                }
            }
            else
            {
                await SayNext("嗯...你是害怕速度还是高度？你不相信我的飞行技能？相信我，我已经解决了所有的问题！");
            }
        }


        // Npc: 2103013 
        public Task dooat()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2110005 
        public async Task nihal_taxi()
        {
            var toMagatia = "Would you like to take the #b骆驼中巴#k to #b玛加提亚#k, the town of Alchemy? The fare is #b1500 mesos#k.";
            var toAriant = "Would you like to take the #b骆驼中巴#k to #b阿里安特#k, the town of Burning Roads? The fare is #b1500 mesos#k.";

            if (await SayYesNo(getPlayer().getMapId() == 260020000 ? toMagatia : toAriant))
            {
                if (getMeso() < 1500)
                {
                    await SayNext("对不起，但我觉得你的金币不够。恐怕如果你没有足够的钱，我不能让你骑这个。请等你有足够的钱再来使用。");
                }
                else
                {
                    warp(getPlayer().getMapId() == 260020000 ? 261000000 : 260000000, 0);
                    gainMeso(-1500);
                }
            }
            else
            {
                await SayNext("嗯...现在太忙了？如果你想做的话，回来找我吧。");
            }
        }


        // Npc: 2111000 
        public async Task jenu_homun()
        {
            if (isQuestStarted(3310) && !haveItem(4031709, 1))
            {
                warp(926120100, "out00");
            }
            else
            {
                await SayNext("炼金术……还有炼金术士……它们两者都很重要。但更重要的是，是玛加提亚容忍了一切。马加提亚的荣誉应该由我来保护。");
            }
        }


        // Npc: 2111003 
        public async Task snow_rose()
        {
            if (isQuestStarted(3335) && !haveItem(4031695, 1))
            {
                warp(926120300, "out00");
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }


        // Npc: 2111006 
        public async Task drang_room1()
        {
            if (isQuestStarted(3320) || isQuestCompleted(3320))
            {
                warp(926120200, 1);
            }
            else
            {
                await SayOK("唔呼呼……为什么这里只有鬼魂？");
            }
        }


        // Npc: 2111010 
        public async Task magatia_dark1()
        {
            if (isQuestStarted(3309) && !haveItem(4031708, 1))
            {
                if (canHold(4031708, 1))
                {
                    gainItem(4031708, 1);
                }
                else
                {
                    await SayOK("有一个ETC槽位可用，可以获取Alcadno的秘密文件。");
                }
            }

        }


        // Npc: 2111011 
        public async Task absence_wall()
        {
            if (await SayYesNo("在一片蜘蛛网的拥挤中，有一堵墙后面似乎写着什么东西。也许你应该仔细看看墙？"))
            {
                setQuestProgress(3311, 5);
                await SayOK("在一面满是涂鸦的墙上，似乎有一句话格外显眼。#b它是以一种吊坠的形式出现……#k 这是什么意思？");
            }
        }


        // Npc: 2111012 
        public Task absence_box()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111013 
        public async Task absence_frame()
        {
            await SayOK("挂在画框后面的钩子被解开，揭示了画框内部的一个秘密空间。在那里，发现了一枚银色吊坠。小心地取下吊坠后，画框被合上，放回桌子上。");
            if (canHold(4031697, 1))
            {
                gainItem(4031697);
            }
            else
            {
                await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
            }
        }


        // Npc: 2111014 
        public async Task absence_desk()
        {
            if (isQuestStarted(3311))
            {
                await SayOK("德朗博士的日记。整本日记都充斥着大量的公式和浮夸的科学文本。");
            }
        }


        // Npc: 2111015 
        public async Task alcadno_potion()
        {
            if (isQuestStarted(3314) && !haveItem(2022198, 1) && getPlayer().getBuffSource(BuffStat.HPREC) != 2022198)
            {
                if (canHold(2022198, 1))
                {
                    gainItem(2022198, 1);
                    await SayOK("你拿了桌子上放着的药丸。");
                }
                else
                {
                    await SayOK("你没有可用的消耗槽来获取#t2022198#。");
                }
            }
        }


        // Npc: 2111017 
        public async Task pipe1()
        {
            if (isQuestStarted(3339))
            {
                var progress = getQuestProgressInt(23339, 1);

                if (progress == 3)
                {
                    var inputText = await SayInputText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        setQuestProgress(23339, 1, 4);
                        warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress == 0)
                {
                    setQuestProgress(23339, 1, 1);
                }
                else if (progress < 3)
                {
                    setQuestProgress(23339, 1, 0);
                }
                else
                {
                    warp(261000001, 1);
                }
            }
            else
            {
                if (isQuestCompleted(3339))
                {
                    warp(261000001, 1);
                }
            }
        }


        // Npc: 2111018 
        public async Task pipe2()
        {
            if (isQuestStarted(3339))
            {
                var progress = getQuestProgressInt(23339, 1);

                if (progress == 3)
                {
                    var inputText = await SayInputText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        setQuestProgress(23339, 1, 4);
                        warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress == 2)
                {
                    setQuestProgress(23339, 1, 3);
                    var inputText = await SayInputText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        setQuestProgress(23339, 1, 4);
                        warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress < 3)
                {
                    setQuestProgress(23339, 1, 0);
                }
                else
                {
                    warp(261000001, 1);
                }
            }
            else
            {
                if (isQuestCompleted(3339))
                {
                    warp(261000001, 1);
                }

                dispose();
            }
        }


        // Npc: 2111019 
        public async Task pipe3()
        {
            if (isQuestStarted(3339))
            {
                var progress = getQuestProgressInt(23339, 1);

                if (progress == 3)
                {
                    var inputText = await SayInputText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        setQuestProgress(23339, 1, 4);
                        warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress == 1)
                {
                    setQuestProgress(23339, 1, 2);
                }
                else if (progress < 3)
                {
                    setQuestProgress(23339, 1, 0);
                }
                else
                {
                    warp(261000001, 1);
                }
            }
            else
            {
                if (isQuestCompleted(3339))
                {
                    warp(261000001, 1);
                }
            }
        }


        // Npc: 2111020 
        public Task alceCircle1()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 0)
                {
                    setQuestProgress(3345, 1);
                }
                else if (progress < 4)
                {
                    setQuestProgress(3345, 0);
                }
            }
            return Task.CompletedTask;
        }


        // Npc: 2111021 
        public Task alceCircle2()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 0)
                {
                    setQuestProgress(3345, 2);
                }
                else if (progress < 4)
                {
                    setQuestProgress(3345, 0);
                }
            }
            return Task.CompletedTask;
        }


        // Npc: 2111022 
        public Task alceCircle3()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 0)
                {
                    setQuestProgress(3345, 3);
                }
                else if (progress < 4)
                {
                    setQuestProgress(3345, 0);
                }
            }
            return Task.CompletedTask;
        }


        // Npc: 2111023 
        public async Task alceCircle4()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 3 && haveItem(4031739, 1) && haveItem(4031740, 1) && haveItem(4031741, 1))
                {
                    setQuestProgress(3345, 4);
                    gainItem(4031739, -1);
                    gainItem(4031740, -1);
                    gainItem(4031741, -1);

                    await SayOK("当你放置碎片时，一道光芒照耀着圆圈，驱散了这件神器内部酝酿的不祥之兆。");
                }
                else if (progress < 4)
                {
                    setQuestProgress(3345, 0);
                }
            }
        }




        // Npc: 2111025 
        public async Task sca_auto()
        {
            if (await SayAcceptDecline("You can operate the automated security system using the control unit. Would you like to deactivate the automated security system?"))
            {
                weakenAreaBoss(7090000, "The automated security system has been deactivated. The intruder alarm will shut down.");
            }
        }


        // Npc: 2111026 
        public async Task sca_DitRoi()
        {
            if (await SayAcceptDecline("This Magic Pentagram is incomplete. Would you like to finish off the drawing of the Magic Pentagram?"))
            {
                weakenAreaBoss(8090000, "The Magic Pentagram has been completed. The spell to eliminate Deet and Roi has been summoned.");
            }
        }


        // Npc: 2112011 
        public async Task yurete2_dead()
        {
            await SayOption(
                "被打败了...这就是犹泰的遗产将如何结束的方式，哦，这是多么的悲哀...希望你们现在很开心，因为我将度过余生在一个黑暗的地窖里。我所做的一切都是为了马加提亚的利益！！（哭泣）",
                ["嘿，伙计，振作点！这里没有太多无法解决的损害。马加提亚制定了这些严厉的法律，是为了保护它的人民免受像这样的强大力量落入错误的手中所带来的危害。这并不是你的终结，接受社会的康复，一切都会好起来的！"]
                );
            await SayNext("…在我所做的一切之后，你们原谅我了吗？嗯，我想我被那种可以通过这种方式发现的巨大力量冲昏了头脑，也许他们说得对，人类不能简单地理解并运用这些力量，而不在途中腐化自己…我深感抱歉，为了弥补自己对每个人，我愿意在炼金术的进展中再次帮助各个组织。谢谢。");
            if (!isQuestCompleted(7770))
            {
                completeQuest(7770);
            }
            warp(926110600, 0);
        }


        // Npc: 2112016 
        public async Task q3367npc()
        {
            if (isQuestStarted(3367))
            {
                var c = getQuestProgressInt(3367, 30);
                if (c >= 30)
                {
                    await SayNext("（所有文件已整理好。向尤莱特报告找到的文件。）");
                    return;
                }

                var book = (getNpcObjectId() % 30);
                var prog = getQuestProgressInt(3367, book);
                if (prog == 0)
                {
                    c++;

                    if (book < 20)
                    {
                        if (!canHold(4031797, 1))
                        {
                            await SayNext("（你找到了一份报告文件，但由于你的杂项栏已满，你选择将文件放在你找到的地方。）");
                            return;
                        }
                        else
                        {
                            gainItem(4031797, 1);
                            setQuestProgress(3367, 31, getQuestProgressInt(3367, 31) + 1);
                        }
                    }

                    setQuestProgress(3367, book, 1);
                    setQuestProgress(3367, 30, c);
                    await SayNext("（整理文件。#r" + (30 - c) + "#k 剩余。）");
                }
            }
        }


        // Npc: 2112018 
        public Task rnj_start()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 2121005 
        public Task musicNote()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000021 
        public async Task getRank()
        {
            await SayNext($"嘿，旅行者！我是#p{npc}#，我的工作是招募像你这样渴望每天迎接新挑战的旅行者。现在，我的团队正在举办比赛，充分测试像你这样的冒险者的心理和身体能力。");
            await SayNext("这些比赛涉及#b连续的boss战#k，其中一些部分之间有一些休息点。这将需要一些策略时间和足够的物资在手，因为它们不是普通的战斗。");
            if (await SayAcceptDecline("如果你觉得自己足够强大，你可以像其他人一样在我们举办权力竞赛的地方加入。...那么，你的决定是什么？您现在进入举行比赛的地方吗？"))
            {
                await SayOK("非常好。记住，在那里你可以组建一个团队，或者独自进行战斗，这取决于你。祝你好运！");
                getPlayer().SaveLocation(SavedLocationType.BOSSPQ);
                warp(970030000, "out00");
            }
        }

        // Npc: 9000036 
        public async Task A_office()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 9000037 
        public Task Raid_solo()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000038 
        public Task Raid_party()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000040 
        public Task medal_rank()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000041 
        public Task Donation()
        {
            return Task.CompletedTask;
        }



        // Npc: 9000049 
        public Task treasureHunter()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9001102 
        public Task giveupMoonPicture()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9001105 
        public Task spaceGaGa_papa()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9010004 
        public async Task ludiEvent()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 9010021 
        public Task RyuhoRank() => SayOK(GetDefault0());


        // Npc: 9010022 
        public Task unityPortal()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9060000 
        public async Task tamepig_out()
        {
            if (haveItem(4031508, 5) && haveItem(4031507, 5))
            {
                await SayNext("哇~ 你成功收集了5个#b#t4031508##k和#b#t4031507##k。好的，那么我会送你去动物园。到了之后请再和我交谈。");
                //
            }
            else
            {
                if (await SayYesNo("你还没有完成要求。你确定要离开吗？"))
                {
                    warp(923010100, 0);
                }
            }
        }



        // Npc: 9101001 
        public async Task begin_jp2()
        {
            await SaySpeech([
                "你已经完成了所有的训练。干得好。你似乎已经准备好立刻开始旅程了！很好，我会让你继续前往下一个地方。",
                "但记住，一旦你离开这里，你将进入一个充满怪物的村庄。那么，再见！"
                ]);
            warp(40000, 0);
            gainExp(3);

        }


        // Npc: 9105004 
        [ScriptName("08_xmas")]
        public Task s_08_xmas()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9105005 
        public Task out_08Xmas()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9110002 
        [ScriptName("Life in Mushroom Shrine...")]
        public Task s_Life_in_Mushroom_Shrine()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9120003 
        public async Task in_bath()
        {
            var price = 300;
            if (await SayYesNo("你想进入浴池吗？这将是" + price + "金币。"))
            {
                if (getMeso() < price)
                {
                    await SayOK("请检查并查看您是否有" + price + "金币进入这个地方。");
                }
                else
                {
                    gainMeso(-price);
                    warp(801000100 + 100 * getPlayer().getGender(), "out00");
                }
            }
        }


        // Npc: 9120010 
        public Task whitto()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120013 
        public Task boss_cat()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120015 
        [ScriptName("To the Showa manor...")]
        public async Task s_To_the_Showa_manor()
        {
            var option = await SayOption("你想从我这里得到什么？\r\n#L0##b收集一些有关藏身之处的信息。#l\r\n#L1#带我去藏身之处。#l\r\n#L2#什么都不要。#k");
            switch (option)
            {
                case 0:
                    await SayNext("我可以带你去藏身之处，但那个地方到处都是寻衅滋事的暴徒。你需要既非常强壮又勇敢才能进入那个地方。在藏身之处，你会找到控制这个地区所有其他头目的老大。到达藏身之处很容易，但那个地方顶层的房间每天只能进入一次。老大的房间不是一个可以胡闹的地方。我建议你不要在那里待得太久；一旦进去，你需要迅速处理好事情。老大本人是一个强大的对手，但在去见老大的路上你会遇到一些非常强大的敌人！这并不会容易。");
                    break;
                case 1:
                    await SayNext("Oh, the brave one. I've been awaiting your arrival. If these\r\nthugs are left unchecked, there's no telling what going to\r\nhappen in this neighborhood. Before that happens, I hope\r\nyou take care of all them and beat the boss, who resides\r\non the 5th floor. You'll need to be on alert at all times, since\r\nthe boss is too tough for even wisemen to handle.\r\nLooking at your eyes, however, I can see that eye of the\r\ntiger, the eyes that tell me you can do this. Let's go!");
                    break;

                case 2:
                    await SayOK("我很忙！如果你只是需要这个，就别打扰我！");
                    break;
                default:
                    break;
            }
        }

        // Npc: 9120200 
        public async Task con2()
        {
            if (await SayYesNo("你就在藏身处前面！什么？你想返回#m801000000#？"))
            {
                warp(801000000);
            }
            else
            {
                await SayOK("如果你想回到#m801000000#，就来找我。");
            }
        }


        // Npc: 9120201 
        public Task s_dungeon()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120202 
        public Task con3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120203 
        public async Task con4()
        {
            await SayNext("啊，Boss已经被打败了。这真是个快乐的日子！恭喜大家。跟着这条路返回城镇。");
            warp(801000000);
        }


        // Npc: 9201021 
        public Task weddingParty()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9201033 
        public Task go_xmas06()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9201050 
        public Task About_NLC()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201051 
        public Task naomi()
        {
            return SayOK(GetDefault0());
        }



        // Npc: 9201054 
        public Task Lost_Trans1()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201056, 9310054 
        public async Task NLC_Taxi()
        {
            var fee = 15000;
            if (getMapId() == 682000000)
            {
                if (await SayYesNo("你想回到 #b新叶城 市区中心#k 吗？费用是" + fee + "金币。"))
                {
                    if (getMeso() >= fee)
                    {
                        gainMeso(-fee);
                        warp(600000000);
                    }
                    else
                    {
                        await SayOK("嘿，你想搞什么鬼？你没有足够的金币来支付费用。");
                    }
                }
            }
            else
            {
                if (await SayYesNo("你想乘坐这辆车前往 #b鬼屋入口#k 吗? 这个费用是 " + fee + " 金币."))
                {
                    if (getMeso() >= fee)
                    {
                        gainMeso(-fee);
                        warp(682000000, 0);
                    }
                    else
                    {
                        await SayOK("嘿，你想搞什么鬼？你没有足够的金币来支付费用。");
                    }
                }
            }
        }


        // Npc: 9201057 
        public async Task NLC_ticketing()
        {
            var subway = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(Subway)) ?? throw new BusinessNotsupportException(nameof(Subway));
            if (getMap() == subway.StationAMap || getMap() == subway.StationBMap)
            {
                var p = subway.GetTicket(getPlayer());
                if (p == null)
                {
                    await SayOK("无法与我对话");
                    return;
                }

                var target = subway.GetDestinationMapName(getPlayer());
                if (target == null)
                {
                    await SayOK("无法与我对话");
                    return;
                }

                if (await SayYesNo($"每分钟都有前往{target}的地铁出发，票价为#b{p.Value.TicketPrice}金币#k。您确定要购买#b#t{p.Value.TicketItemId}##k吗？"))
                {
                    if (!canHold(p.Value.TicketItemId))
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    }
                    else if (getMeso() >= p.Value.TicketPrice)
                    {
                        gainMeso(-p.Value.TicketPrice);
                        gainItem(p.Value.TicketItemId, 1);
                        await SayNext("请收好您的票。");
                    }
                    else
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                }
            }
            else
            {
                if (await SayYesNo("列车启动前你想离开吗？车票将不会退款。"))
                {
                    WarpReturn();
                }
            }

        }


        async Task ContiMove_NLC()
        {
            var subway = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(Subway)) ?? throw new BusinessNotsupportException(nameof(Subway));
            var p = subway.GetTicket(getPlayer());
            if (p == null)
            {
                await SayOK("无法与我对话");
                return;
            }
            if (subway.CanEnter)
            {
                if (await SayYesNo("列车已经进站,请出示你的票，这样我就可以让你进去了。乘坐时间不会很长，你会安全到达目的地的。你觉得怎么样？想要乘坐这趟列车吗？"))
                {
                    if (haveItem(p.Value.TicketItemId))
                    {
                        if (subway.Enter(getPlayer()))
                        {
                            gainItem(p.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayNext("我们将在列车开车前1分钟开始检票进入。进入之后请耐心等待几分钟。请注意，地铁将准时发车，我们将在那之前1分钟停止接收车票，请做好时间管理。");
                        }
                    }
                    else
                    {
                        await SayOK("抱歉，您需要一张门票才能进入！");
                    }
                    return;
                }
            }
            else
            {
                await SayNext("我们将在列车开车前1分钟开始检票进入。进入之后请耐心等待几分钟。请注意，地铁将准时发车，我们将在那之前1分钟停止接收车票，请做好时间管理。");
                return;
            }
        }
        // Npc: 9201068 
        public Task NLC_Move()
        {
            return ContiMove_NLC();
        }


        // Npc: 9201069 
        public Task NLC_FaceVip()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201070 
        public Task NLC_FaceExp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201071 
        public async Task Sunstone()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 9201072 
        public async Task Moonstone()
        {
            await SayOK(GetDefault0());
        }


        // Npc: 9201073 
        public async Task Tombstone()
        {
            await SayOK(GetDefault0());
        }

        // Npc: 9201082 
        public Task naomi1()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201083 
        public Task Lost_Trans2()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201084 
        public Task Tomb_Hall()
        {
            return SayOK(GetDefault0());
        }



        // Npc: 9201093 
        public Task suzy_lost()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201094 
        public Task TCG3()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201099 
        public async Task MoStore()
        {
            if (getQuestStatus(8224) == 2)
            {
                openShopNPC(getNpc());
            }
            else
            {
                await SayOK("嗯，你觉得你在看谁？");
            }

        }


        // Npc: 9201100 
        public async Task Fallen_Woods()
        {
            if (getQuestStatus(8224) == 2)
            {
                await SayOK("你好，同伴。如果你需要帮助，可以尝试与我们的成员交谈。");
            }
            else
            {
                await SayOK("你好，陌生人。我们是著名的渡鸦爪佣兵团，我是他们的领袖。");
            }
        }


        // Npc: 9201101 
        public Task tcg4_7()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201102 
        public Task tcg4_8()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9201103 
        public Task tcg4_6_8241()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201104 
        public Task Masteria_Sage01()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201105 
        public Task Masteria_Sage02()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201106 
        public Task TCG5()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201107 
        public Task glpqstatue0()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201108 
        public Task glpqstatue1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201109 
        public Task glpqstatue2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201110 
        public Task glpqstatue3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201111 
        public Task glpqstatue4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201112 
        public Task glpqStory()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201113 
        public Task glpqStart()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201114 
        public Task glpqEnter()
        {
            if (haveItem(3992041, 1))
            {
                warp(610030020, "out00");
            }
            else
            {
                playerMessage(5, "The giant gate of iron will not budge no matter what, however there is a visible key-shaped socket.");
            }
            return Task.CompletedTask;
        }


        // Npc: 9201115 
        public Task glpqStory2()
        {
            // TODO
            return Task.CompletedTask;
        }


        async Task JobWarp(Job job)
        {
            Dictionary<Job, int> jobMap = new()
            {
                { Job.WARRIOR, 102000003 },
                { Job.MAGICIAN, 101000003 },
                { Job.BOWMAN,  100000201},
                { Job.THIEF,  103000003},
                { Job.PIRATE,  120000101},
            };
            var jobType = job.Id / 100;
            if (getJob() == Job.BEGINNER)
            {
                var requiredLevel = job == Job.MAGICIAN ? 8 : 10;
                if (getLevel() >= requiredLevel && canGetFirstJob(jobType))
                {
                    var map = jobMap[job];
                    if (await SayYesNo("你好 #h0#，我可以把你送到#b#m" + map + "##k进行#b" + c.CurrentCulture.GetJobName(job) + "#k转职。你要过去吗？"))
                    {
                        warp(map, 0);
                    }
                    ;
                }
                else
                {
                    await SayOK($"如果你想成为#b{c.CurrentCulture.GetJobName(job)}#k，你需要到达#b{requiredLevel}级，{getFirstJobStatRequirement(jobType)}#k。");
                }
            }
            else
            {
                await SayOK("我只招待新人！");
            }
        }

        // Npc: 9201123 
        public Task goPerion()
        {
            return JobWarp(Job.WARRIOR);
        }


        // Npc: 9201124 
        public Task goHenesys()
        {
            return JobWarp(Job.BOWMAN);
        }


        // Npc: 9201125 
        public Task goElinia()
        {
            return JobWarp(Job.MAGICIAN);
        }


        // Npc: 9201126 
        public Task goKerningCity()
        {
            return JobWarp(Job.THIEF);
        }


        // Npc: 9201127 
        public Task goNautilus()
        {
            return JobWarp(Job.PIRATE);
        }


        // Npc: 9201128 
        public async Task Enter_Darkportal_W()
        {
            var map = 677000004;
            var quest = 28179;
            var questItem = 4032491;
            if (isQuestStarted(quest))
            {
                if (haveItem(questItem))
                {
                    if (await SayYesNo("你想要移动到 #b#m" + map + "##k 吗？"))
                    {
                        // 物品不存在，这个NPC也许并不会被使用？
                        gainItem(4001341, -1);
                        gainItem(4032478, -1);

                        warp(map, 0);
                    }
                }
                else
                {
                    await SayOK("入口被一种力量封锁，只有持有徽章的人才能解除。");
                }
            }
            else
            {
                await SayOK("入口被一股奇怪的力量阻挡住了。");
            }
        }


        // Npc: 9201129 
        public Task Enter_Darkportal_M()
        {
            // NOT USED
            return Task.CompletedTask;
        }


        // Npc: 9201130 
        public Task Enter_Darkportal_T()
        {
            // NOT USED
            return Task.CompletedTask;
        }


        // Npc: 9201131 
        public Task Enter_Darkportal_H()
        {
            // NOT USED
            return Task.CompletedTask;
        }


        // Npc: 9201132 
        public Task Enter_Darkportal_P()
        {
            // NOT USED
            return Task.CompletedTask;
        }


        // Npc: 9201133 
        public Task Astaroth_door()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201134 
        public Task Malay_Warp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201135 
        public Task Malay_Warp2()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9201142 
        public Task witchMaladyGL()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201143 
        public Task oliviaEnter()
        {
            return SayOK(GetDefault0());
        }


        // Npc: 9209000 
        public Task dealerA()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9209001 
        public Task MapleMarket7_Enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9209100 
        public Task xmas_party_ent()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9220004 
        public Task wxmasB()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9220005 
        public async Task Jump_rudolph()
        {
            if (getChar().getMapId() == 209000000)
            {
                if (await SayYesNo($"你想前往 #b#m209080000##k 吗？"))
                {
                    warp(209080000, 0);
                }
            }
            else if (getChar().getMapId() == 209080000)
            {
                if (await SayYesNo($"你想回到 #b#m209000000##k 吗？"))
                {
                    warp(209000000, 0);
                }
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }


        // Npc: 9220018 
        public Task guyfawkes_ch()
        {
            throw new NotImplementedException();
        }


        // Npc: 9220019 
        public Task guyfawkes_milla2()
        {
            throw new NotImplementedException();
        }


        // Npc: 9220020 
        public Task guyfawkes_ch2()
        {
            throw new NotImplementedException();
        }


        // Npc: 9270017 
        public async Task goback_kerning()
        {
            if (await SayYesNo("飞机马上就要起飞了，你现在要离开吗？你将不得不再次购买飞机票才能进来。"))
            {
                await SayNext("机票不可退，希望再次见到你！");
                WarpReturn();
            }
            else
            {
                await SayOK("请稍等一下，飞机即将起飞。谢谢您的耐心等待。");
            }
        }


        // Npc: 9270018 
        public async Task goback_cbd()
        {
            var airPlane = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(AirPlane)) ?? throw new BusinessNotsupportException(nameof(AirPlane));
            var target = airPlane.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            if (airPlane.IsTransporting(getMap()))
            {
                await SayOK($"我们马上就要到{target}了，请坐下等候。");
                return;
            }
            else if (airPlane.IsWaiting(getMap()))
            {
                if (await SayYesNo("飞机马上就要起飞了，你确定要现在离开吗？机票是不可退的。"))
                {
                    WarpReturn();
                }
                else
                {
                    await SayOK("请稍等一下，飞机即将起飞。谢谢您的耐心等待。");
                }
            }
        }

        async Task ContiMoveAirPlane()
        {
            var airPlane = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(AirPlane)) as AirPlane ?? throw new BusinessNotsupportException(nameof(AirPlane));
            var target = airPlane.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var t = airPlane.GetTicket(getPlayer());
            if (t == null)
            {
                await SayOK("无法与我对话");
                return;
            }

            var option = await SayOption($"你好，我是来自新加坡机场的#p{getNpc()}#。我可以帮助你迅速到达{target}。你想去{target}吗？\r\n#b#L0#我想买一张去{target}的飞机票\r\n#b#L1#让我进入出发点。");
            switch (option)
            {
                case 0:
                    if (await SayYesNo($"机票的价格是{t.Value.TicketPrice}金币。你要购买吗？"))
                    {
                        if (!canHold(t.Value.TicketItemId))
                        {
                            await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
                            return;
                        }
                        if (getMeso() < t.Value.TicketPrice)
                        {
                            await SayOK(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                            return;
                        }

                        gainMeso(-t.Value.TicketPrice);
                        gainItem(t.Value.TicketItemId);
                        await SayOK("Thank you for choosing Wizet Airline! Enjoy your flight!");

                    }
                    break;
                case 1:
                    if (haveItem(t.Value.TicketItemId))
                    {
                        if (airPlane.Enter(getPlayer()))
                        {
                            gainItem(t.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayOK("抱歉，飞机已经起飞，请稍等几分钟。");
                        }
                        return;
                    }
                    else
                    {
                        await SayOK($"你需要一个#b#t{t.Value.TicketItemId}##k才能上飞机！");
                    }

                    break;
                default:
                    break;
            }
        }

        // Npc: 9270038 
        public Task sellticket_cbd()
        {
            return ContiMoveAirPlane();
        }


        // Npc: 9270041 
        public Task sellticket_sg()
        {
            return ContiMoveAirPlane();
        }


        // Npc: 9270047 
        public Task MalaysiaBoss_GL()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9310000 
        public async Task goshanghai1()
        {
            var fee = 2000;
            if (await SayYesNo($"嘿！我是#b#e驾驶员 洪#n#k，我负责驾驶飞往上海的飞机。\r\n经过长年的飞行，我的驾驶技术已经很了不得。\r\n有兴趣跟我一起前往美丽的#b#e上海外滩#k#n吗？\r\n只需要#r{fee}金币#k哦！"))
            {
                if (getMeso() < fee)
                {
                    await SayNext($"你确定你有 #b{fee} 金币#k？ 如果没有，我可不能免费送你去。");
                }
                else
                {
                    gainMeso(-fee);
                    warp(701000000);
                }
            }
        }

        // Npc: 9310013 
        public async Task goshanghai2()
        {
            var fee = 2000;
            if (await SayYesNo($"嘿！我是#b#e驾驶员 洪#n#k，我负责驾驶飞往#b金银岛#k的飞机。\r\n经过长年的飞行，我的驾驶技术已经很了不得。\r\n有兴趣跟我一起前往古朴的#b#e勇士部落#k#n吗？\r\n只需要#r{fee}金币#k哦！"))
            {
                if (getMeso() < fee)
                {
                    await SayNext($"你确定你有 #b{fee} 金币#k？ 如果没有，我可不能免费送你去。");
                }
                else
                {
                    gainMeso(-fee);
                    warp(102000000);
                }
            }
            else
            {
                await SayNext("充满勇者气息的#b#e金银岛勇士部落#k#n，难道你不想回去看看吗！真遗憾。");
            }
        }


        // Npc: 9310039 
        public Task q8535s()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9310044 
        public async Task outshaolinBoss()
        {
            var mapID_out = 702070400;
            if (await SayYesNo($"你要离开#b#e#m{getMapId()}##k#n 回到 #b#e#m${mapID_out}##k#n 吗？"))
            {
                warp(mapID_out);
            }
        }

        // Npc: 9310058 
        public async Task Jump_event()
        {
            await SayOK("欢迎来到#b快乐小镇#k，年轻的旅行者。你有什么愿望吗？");
        }

        // Npc: 9900000 
        public Task levelUP()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9900001 
        public Task levelUP2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9901000, 9901001, 9901002, 9901003, 9901004, 9901005, 9901006, 9901007, 9901008, 9901009, 9901010, 9901011 ...
        public Task rank_user()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9977777 
        public Task rank_developer()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1012119 
        public async Task EnterTranningMap(int start)
        {
            if (getLevel() >= 20)
            {
                await SayOK("只有低于20级才能进入训练中心。");
                return;
            }

            var option = await SayOption("你要进入训练中心吗？",
                Enumerable.Range(0, 5).Select(i => $"训练中心 {i}")
                );
            warp(start + option, 0);
        }

    }
}