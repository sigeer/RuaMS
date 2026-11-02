using Application.Core.Channel.Net.Packets;
using Application.Core.Game.ContiMove;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Infrastructure;
using Application.Core.Server.life;
using Application.Plugin.Script.Features.Zakum;
using Application.Resources.Messages;
using Application.Shared.Constants;
using Application.Shared.Constants.Inventory;
using Application.Shared.Constants.Job;
using Application.Shared.Constants.Map;
using Application.Shared.MapObjects;
using Application.Shared.Quest;
using Application.Utility;
using Application.Utility.Configs;
using Application.Utility.Exceptions;
using Humanizer;
using System.Drawing;

namespace Application.Plugin.Script.Npc
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

    internal partial class NpcScript
    {
        // Npc: 2003 
        public async Task begin5()
        {
            switch (await AskMenu("现在...问我任何你可能对旅行有的问题！", [
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

        // Npc: 2104 
        public async Task HL_LADDER()
        {
            await SayNext("好的，那么我们出发吧。冒险旅途开始了！");
            await warp(1);
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
            if (await AskYesNo("要去金银岛吗？只需要支付 #e150 金币#n，我会把你送到 #b明珠港#k。#r但是#k一旦离开这里，你就不能再回来了，要出发吗？"))
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
                        await gainItem(4031801, -1);
                        await warp(MapId.LITH_HARBOUR_Animation, 0);
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
                        await gainMeso(-150);
                        await warp(MapId.LITH_HARBOUR_Animation, 0);
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
            switch (await AskMenu("如果这是你第一次来到这个地方，可能会对这里感到困惑是可以理解的。如果你对这个地方有任何问题，尽管问吧。",
                [
                "金银岛上有哪些城镇？",
                "请带我去别的地方。"
                ]))
            {
                case 0:
                    {
                        int[] imaps = [104000000, 102000000, 101000000, 100000000, 103000000, 120000000, 105040300];
                        switch (await AskMenu("金银岛中存在7大主城区域. 你想了解哪一个？", imaps.Select(x => $"#m{x}#")))
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

                    var option = await AskMenu(selStr, maps.Select(x => $"#m{x}#"));

                    var realCost = DiscountForNovice(cost[option]);
                    if (await AskYesNo("我猜你不需要在这里。你真的想要移动到 #b#m" + maps[option] + "##k 吗？好吧，这将花费你 #b" + realCost + " 金币#k。你觉得怎么样？"))
                    {
                        if (getMeso() < realCost)
                        {
                            await SayOK("你的金币不够。以你的能力来说，不该只挣这么点！");
                        }
                        else
                        {
                            await gainMeso(-realCost);
                            await warp(maps[option]);
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
            var option = await AskMenu(
                $"你听说过位于明珠港附近，能够欣赏到壮观海景的海滩#b黄金海滩#k吗？我可以带你去那里，只需#b{cost}金币#k，或者如果你有#b#t{ticket}##k的话，那就可以免费。",
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
                        await gainMeso(-1500);
                        getPlayer().saveLocation("FLORINA");
                        await warp(110000000, "st00");
                    }
                    break;
                case 1:
                    if (await AskYesNo($"所以你有一张#b#t{ticket}##k吗？你可以随时用它前往黄金海滩。好的，但要注意可能会遇到一些怪物。好的，你现在想前往黄金海滩吗？"))
                    {
                        if (!haveItem(ticket))
                        {
                            await SayNext($"嗯，你的 #b#t{ticket}##k 到底在哪里？你确定你有吗？请再检查一遍。");
                            return;
                        }
                        getPlayer().saveLocation("FLORINA");
                        await warp(110000000, "st00");
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
            if (await AskYesNo("我希望我能赚到像昨天一样多的钱...嗨，你好！你不想扩展你的好友列表吗？你看起来像是有很多朋友的人...那么，你觉得呢？只要有一些钱，我就可以帮你实现。不过要记住，这只适用于一个角色，不会影响你账号上的其他角色。你想扩展你的好友列表吗？"))
            {
                if (await AskYesNo("好的，不错的选择！其实并不是很贵。#b24万金币，我就可以给你的好友列表增加5个名额#k。不，我不会单独出售它们。一旦你购买了，它就会永久地出现在你的好友列表上。所以如果你是那些需要更多空间的人，那么你最好去做。你觉得呢？你会花24万金币吗？"))
                {
                    var capacity = getPlayer().getBuddylist().Capacity;
                    if (capacity >= 50 || getMeso() < 240000)
                    {
                        await SayNext("嘿... 你确定你有 #b240,000金币#k 吗？如果是的话，那么检查一下你是否已经将好友列表扩展到了最大。即使你付了钱，你的好友列表上限也只能有 #b50#k 个。");
                    }
                    else
                    {
                        var newcapacity = capacity + 5;
                        await gainMeso(-240000);
                        await getPlayer().setBuddyCapacity(newcapacity);
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
                await SayNext(GetDefault1());

                (int item, int price, string desc)[] items = [(2000002, 310, "300 HP."), (2022003, 1060, "1000 HP."), (2022000, 1600, "800 MP."), (2001000, 3120, "1000 HP and MP.")];
                var option = await AskMenu("你想购买哪些药水?#b", items.Select(x => "#i" + x.item + "# (价格 : " + x.price + " 金币)"));
                var inputNumber = await AskNumber("你想买 #b#t" + items[option].item + "##k? #t" + items[option].item + "# 允许您恢复 " + items[option].desc + " 你想买多少个?", 1, 1, 100);
                var totalCost = inputNumber * items[option].price;
                if (await AskYesNo($"你将购买这些 #r{inputNumber}#k #b#t{items[option].item}#(s)#k 吗？#t{items[option].item}# 一个需要 {items[option].price} 金币，所以总共需要 #r{totalCost}#k 金币。"))
                {
                    if (getMeso() < totalCost)
                    {
                        await SayNext("你是否缺少金币？请检查一下你的消耗物品栏中是否有空位，并且你是否携带了至少 #r" + totalCost + "#k 金币。");
                    }
                    else
                    {
                        if (canHold(items[option].item, inputNumber))
                        {
                            await gainMeso(totalCost);
                            await gainItem(items[option].item, inputNumber);
                            await SayNext("谢谢你的光临。如果你有什么需要，随时可以再来。");
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
                    await SayOK(GetDefault0());
                }
                return;
            }
        }

        // Npc: 1012008, 2040014 
        public async Task minigame00()
        {
            var option = await AskMenu("嘿，看起来你需要休息一下，远离那些狩猎。你应该像我一样享受生活。嗯，如果你有一些物品，我可以和你交换一个可以玩迷你游戏的物品。现在...我能为你做些什么？", [
                "制作一个迷你游戏物品",
                "向我解释一下迷你游戏是什么"
            ]);

            if (option == 0)
            {
                var gameType = await AskMenu("你想制作迷你游戏道具吗？迷你游戏并不是你随便就能玩的。每个迷你游戏都需要一组特定的道具。你想制作哪个迷你游戏的道具？", [
                    "五子棋套装",
                    "配对卡套装"
                ]);

                if (gameType == 0)
                {
                    int[] omok = [4080000, 4080001, 4080002, 4080003, 4080004, 4080005];
                    int[] omok1piece = [4030000, 4030000, 4030000, 4030010, 4030011, 4030011];
                    int[] omok2piece = [4030001, 4030010, 4030011, 4030001, 4030010, 4030001];
                    int omokamount = 99;

                    await SayNext("你想玩#b五子棋#k，是吧？要玩这个游戏，你需要五子棋套装。只有拥有这个物品的人才能开设五子棋游戏房间，而且你几乎可以在市场以外的任何地方玩这个游戏。");

                    var omokType = await AskMenu("棋具的选择也取决于你想要使用哪种棋子。你想制作哪种棋具呢？",
                        omok.Select(x => $"#t{x}#"));

                    if (haveItem(omok1piece[omokType], omokamount) && haveItem(omok2piece[omokType], omokamount) && haveItem(4030009, 1))
                    {
                        await gainItem(omok1piece[omokType], -omokamount);
                        await gainItem(omok2piece[omokType], -omokamount);
                        await gainItem(4030009, -1);
                        await gainItem(omok[omokType], 1);
                        await SayOK("制作成功！");
                    }
                    else
                    {
                        await SayOK($"#b你准备好 #t{omok[omokType]}##k了嘛？嗯...给我材料，我就可以那么做。仔细听，你需要的材料将会是：#r{omokamount} #t{omok1piece[omokType]}#, {omokamount} #t{omok2piece[omokType]}#, 1 #t{4030009}##k。怪物们可能会时不时地掉落那些东西……");
                    }
                }
                else
                {
                    if (haveItem(4030012, 15))
                    {
                        await gainItem(4030012, -15);
                        await gainItem(4080100, 1);
                        await SayOK("制作成功！");
                    }
                    else
                    {
                        await SayOK("你想要 #b一套配对卡#k 吗？嗯...要制作一套配对卡，你需要一些 #b怪物卡#k。怪物卡可以通过在整个岛上打倒怪物来获得。收集15张怪物卡，你就可以制作一套配对卡。");
                    }
                }
            }
            else
            {
                var explainType = await AskMenu("你想了解更多关于小游戏吗？太棒了！问我任何事情。你想了解哪个小游戏？", [
                    "五子棋",
                    "配对卡片"
                ]);

                if (explainType == 0)
                {
                    await SaySpeech([
                        "这里是Omok的规则，请仔细听。Omok是一种游戏，你和对手轮流在桌子上放置棋子，直到有人找到一种方法在一条线上放置5个连续的棋子，无论是水平、对角线还是垂直线。首先，只有拥有#bOmok Set#k的人才能开设游戏房间。",
                        "每局五子棋游戏将花费你#r100金币#k。即使你没有#b五子棋套装#k，你也可以进入房间并进行游戏。然而，如果你没有100金币，那么你将根本无法进入房间。开设游戏房间的人也需要100金币来开房间（否则就没有游戏）。如果在游戏过程中你的金币用完了，那么你将被自动踢出房间！",
                        "进入房间，当你准备好玩的时候，点击#b准备#k。一旦访客点击#b准备#k，房间所有者可以按#b开始#k开始游戏。如果一个不受欢迎的访客走进来，而你不想和那个人玩，房间所有者有权将访客踢出房间。",
                        "当第一局开始时，#b房主先走#k。请注意，你会被给予一个时间限制，如果你没有在规定时间内行动，你可能会失去你的回合。通常情况下，3 x 3 是不允许的，但如果有一个必须把你的棋子放在那里或者面临游戏结束的时刻，那么你可以把它放在那里。3 x 3 是允许作为最后的防线！哦，如果是#r6或7连#k，那就不算。只有5连！"
                    ]);
                }
                else
                {
                    await SaySpeech([
                        "这里是Match Cards的规则，请仔细听。顾名思义，Match Cards就是在桌子上找到一对匹配的牌。当所有匹配的对都被找到时，拥有更多匹配对的人将赢得游戏。就像五子棋一样，你需要一副Match Cards牌来开启游戏房间。",
                        "每一局配对卡游戏都需要花费你 #r100金币#k。即使你没有 #b一套配对卡#k，你也可以进入房间并进行游戏。然而，如果你没有100金币，那么你将根本无法进入房间。开设游戏房间的人也需要100金币来开房间（否则就没有游戏）。如果你在游戏过程中金币用完了，那么你将被自动踢出房间！",
                        "进入房间，当你准备好玩的时候，点击#b准备#k。一旦访客点击#b准备#k，房间所有者可以按#b开始#k开始游戏。如果一个不受欢迎的访客走进来，而你不想和那个人玩，房间所有者有权将访客踢出房间。"
                    ]);
                }
            }
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
                await forceCompleteQuest(20706);
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
            if (await AskYesNo("圣诞老人告诉我来到这里，只是他没有告诉我什么时候……我希望我来的时间对了！哦！顺便说一下，我是鲁尼，我可以带你去#b快乐村#k。你准备好了吗？"))
            {
                getPlayer().SaveLocation(SavedLocationType.HAPPYVILLE);
                await warp(209000000, 0);
            }
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
                if (await AskYesNo($"嗨，我是#p{npc}#。我可以让你进入忍苦树林，只需要支付一小笔费用。你想用 #b5000#k 金币进入吗？"))
                {
                    if (getMeso() < 5000)
                    {
                        await SayOK(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
                    }
                    else
                    {
                        if (isQuestStarted(2050))
                        {
                            await warp(101000100, 0);
                        }
                        else if (isQuestStarted(2051))
                        {
                            await warp(101000102, 0);
                        }
                        else if (getLevel() >= 25 && getLevel() < 50)
                        {
                            await warp(101000100, 0);
                        }
                        else if (getLevel() >= 50)
                        {
                            await warp(101000102, 0);
                        }
                        await gainMeso(-5000);
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
            if (await AskYesNo("你想回到#m101000000#吗？"))
            {
                await warp(101000000, 0);
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
                var option = await AskMenu("您好，我是天空之城售票员，我负责销售开往各地船票。你想购买去那里的船票呢？",
                    list.Select(x => x.GetDestinationMapName(getPlayer())!));

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
            if (await AskYesNo($"你好，我负责出售前往{target}的船票。前往{target}的船每{TimeSpan.FromMilliseconds(currentContiMove.GetTransportationTime(currentContiMove.RideTime)).Humanize()}出发一次，票价为#b{p.Value.TicketPrice}金币#k。你确定要购买#b#t{p.Value.TicketItemId}##k吗？"))
            {
                if (getMeso() >= p.Value.TicketPrice && canHold(p.Value.TicketItemId))
                {
                    await gainItem(p.Value.TicketItemId, 1);
                    await gainMeso(-p.Value.TicketPrice);
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
                    if (await AskYesNo($"你想去{target}吗？"))
                    {
                        if (await currentContiMove.Enter(getPlayer()))
                        {
                            await gainItem(p.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayOK($"飞往{target}的船只已经启程，请耐心等待下一班。下一班将在 {next}抵达。");
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
                    await SayOK($"飞往{target}的船只已经启程，请耐心等待下一班。下一班将在 {next}抵达。");
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
            if (await AskYesNo("你想离开吗？"))
            {
                await SayNext("好的，下次见。保重。");
                await WarpReturn();
            }

        }

        // Npc: 1032100
        public Task owen() => SayOK(GetDefaultTalk(2017));



        // Npc: 1032111 
        public async Task giveSap()
        {
            if (isQuestStarted(20716))
            {
                if (!hasItem(4032142))
                {
                    if (canHold(4032142))
                    {
                        await gainItem(4032142, 1);
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
                    await gainItem(4032479, 1);
                    await SayOK("哼，你在找我吗？是长官派你来的，对吧？但是嘿，我不是你要找的嫌疑人。如果我有证据呢？拿着这个，把它还给 #b#p1012003##k。");
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
        public async Task bush1()
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
                await gainItem(4031020, 1);
            }
            await gainItem(prizes[choice], 1);
            await warp(101000000, 0);
        }

        // Npc: 1043001 
        public async Task bush2()
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
                await gainItem(4031032, 1);
            }
            await gainItem(prizes[choice], 1);
            await warp(101000000, 0);
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

            var selStr = "您想要哪张票?#b";
            for (var i = 0; i < zones; i++)
            {
                selStr += "\r\n#L" + i + "#建筑工地 B" + (i + 1) + " (" + cost + " 金币)#l";
            }
            var option = await AskMenu(selStr);
            if (getMeso() < cost)
            {
                await SayOK(GetClientMessage(nameof(ClientMessage.MesoNotEnough)));
            }
            else
            {
                await gainMeso(-cost);
                await gainItem(4031036 + option, 1);
            }
        }

        // Npc: 1052008 
        public async Task subway_get1()
        {
            int[] prizes = [4020000, 4020001, 4020002, 4020003, 4020004];
            if (isQuestStarted(2055))
            {
                await gainItem(4031039, 1);
            }
            else
            {
                await gainItem(Randomizer.Select(prizes), 1);
            }
            await warp(103000100, 0);
        }

        // Npc: 1052009 
        public async Task subway_get2()
        {
            int[] prizes = [4020005, 4020006, 4020007, 4020008, 4010000];
            if (isQuestStarted(2056))
            {
                await gainItem(4031040, 1);
            }
            else
            {
                await gainItem(Randomizer.Select(prizes), 1);
            }
            await warp(103000100, 0);
        }

        // Npc: 1052010 
        public async Task subway_get3()
        {
            int[] prizes = [4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007];
            if (isQuestStarted(2057))
            {
                await gainItem(4031041, 1);
            }
            else
            {
                await gainItem(Randomizer.Select(prizes), 1);
            }
            await warp(103000100, 0);
        }

        // Npc: 1052011 
        public async Task subway_out()
        {
            await SaySpeech(["这个设备连接到外部。", "你打算放弃并离开这个地方吗？"]);
            if (await AskYesNo("下次进来的时候，你将不得不从头开始..."))
            {
                await warp(103000100, 0);
            }

        }


        // Npc: 1052107 
        public async Task sca_Shade()
        {
            if (await SayAcceptDecline("这是一个带着开关的小路灯。你要打开他吗？"))
            {
                await weakenAreaBoss(5090000, "You have turned the lamp on. Shade's strength will rapidly weaken due to the light.");
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
            if (isQuestStarted(20710))
            {
                if (!hasItem(4032136))
                {
                    if (canHold(4032136))
                    {
                        await gainItem(4032136, 1);
                        await SayNext("你在垃圾桶里找到了一个#b#t4032136##k！ #i4032136#");
                    }
                    else
                    {
                        await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
                    }
                    return;
                }
            }
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
            //    await warp(910320000, 0);
            //    return;
            //}
            //else if (getMapId() == 910330001)
            //{
            //    var itemid = 4001321;
            //    if (!canHold(itemid))
            //    {
            //        await SayOK("请为1个其他槽腾出空间。");
            //    }
            //    else
            //    {
            //        await gainItem(itemid, 1);
            //        await warp(910320000, 0);
            //    }
            //    return;
            //}
            //else if (getMapId() >= 910320100 && getMapId() <= 910320304)
            //{
            //    if (await SayYesNo("你想要离开这个地方吗？"))
            //    {
            //        await warp(910320000, 0);
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
                var option = await AskMenu("它的力量让你能够深入森林深处。", maps.Take(zones).Select(x => $"#m{x}#"));
                await warp(maps[option], 0);
            }
        }

        // Npc: 1061007 
        public async Task flower_out()
        {
            if (await AskYesNo("你想离开吗？"))
            {
                await warp(105040300, 0);
            }
        }

        // Npc: 1061100 
        public async Task hotel1()
        {

            await SayNext("您好。这里是林中之城宾馆。如果您想恢复体力，就入住我们宾馆吧。");
            var regcost = 499;
            var vipcost = 999;
            var option = await AskMenu("我们提供两种房间供您选择。请选择您喜欢的一种。",
                [$"普通桑拿房（{regcost}金币）", $"VIP桑拿房（{vipcost}金币）"]);
            switch (option)
            {
                case 0:
                    if (await AskYesNo("你选择了普通桑拿房。你的生命值和魔法值将会快速恢复，你甚至可以在那里购买一些物品。你确定要进去吗？"))
                    {
                        if (getMeso() >= regcost)
                        {
                            await warp(105040401);
                            await gainMeso(-regcost);
                        }
                        else
                        {
                            await SayNext("对不起，看起来你的金币不够。至少需要" + regcost + "金币才能在我们的旅馆住宿。");
                        }
                        return;
                    }
                    break;
                case 1:
                    if (await AskYesNo("你选择了VIP桑拿房。你的HP和MP恢复速度甚至比普通桑拿还要快，而且你甚至可以在那里找到一个特别的物品。你确定要进去吗？"))
                    {
                        if (getMeso() >= vipcost)
                        {
                            await warp(105040402);
                            await gainMeso(-vipcost);
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
                    await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    return;
                }

                await gainItem(4031025, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    return;
                }

                (int item, int count)[] repeatablePrizes = [(4010000, 3), (4010001, 3), (4010002, 3), (4010003, 3), (4010004, 3), (4010005, 3)];
                var itemPrize = Randomizer.Select(repeatablePrizes);
                await gainItem(itemPrize.item, itemPrize.count);
            }

            await warp(105040300, 0);
        }

        // Npc: 1063001 
        public async Task viola_blue()
        {
            if (isQuestStarted(2053) && !haveItem(4031026, 30))
            {
                if (!canHold(4031026, 30))
                {
                    await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    return;
                }

                await gainItem(4031026, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    return;
                }

                (int item, int count)[] repeatablePrizes = [(4020000, 4), (4020002, 4), (4020006, 4)];
                var itemPrize = Randomizer.Select(repeatablePrizes);
                await gainItem(itemPrize.item, itemPrize.count);
            }

            await warp(105040300, 0);
        }

        // Npc: 1063002 
        public async Task viola_white()
        {
            if (isQuestStarted(2054) && !haveItem(4031028, 30))
            {
                if (!canHold(4031028, 30))
                {
                    await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    return;
                }

                await gainItem(4031028, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    return;
                }

                (int item, int count)[] repeatablePrizes = [(4010006, 4), (4010007, 4), (4020007, 4)];
                var itemPrize = Randomizer.Select(repeatablePrizes);
                await gainItem(itemPrize.item, itemPrize.count);
            }

            await warp(105040300, 0);
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
                await gainItem(4032263, -1);
                await setQuestProgress(2236, 1, nextProgress);
                await SayOK("由于灵符的法力，封印了该地区的邪恶势力。");
            }
        }


        // Npc: 1081001 
        public async Task florina1()
        {
            var returnmap = getPlayer().PeekSavedLocation(SavedLocationType.FLORINA);
            if (returnmap == -1)
            {
                returnmap = 104000000;
            }
            if (await AskYesNo("所以你想离开 #b#m110000000##k 吗？如果你想的话，我可以带你回到 #b#m" + returnmap + "##k。"))
            {
                getPlayer().clearSavedLocation(SavedLocationType.FLORINA);
                await warp(returnmap);
            }
            else
            {
                await SayNext("你一定有一些事情要在这里处理。在#m" + returnmap + "#休息一下也不错。看看我，我是如此喜欢这里，结果我最终在这里定居了。哈哈哈，无论如何，当你想回去的时候再来找我说话。");
            }
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
                await gainItem(4031847, 1);
                await warp(912000100, 0);
            }
            else
            {
                await SayOK(GetClientMessage(nameof(ClientMessage.SlotFull), GetClientMessage(nameof(ClientMessage.ETC))));
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
                if (await AskYesNo("嘿，你那张不错的#b藏宝图#k是从哪里得到的？如果你不需要了，我可以替诺特勒斯船员保管吗？"))
                {
                    await gainItem(4220153, -1);
                }
            }
        }

        // Npc: 1092016 
        public async Task nautil_stone()
        {
            if (isQuestStarted(2166))
            {
                await SayNext("这是一块美丽而闪亮的岩石。我能感受到它周围的神秘力量。");
                await completeQuest(2166);
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
                    await gainItem(4031839, 1);
                    await SayNext("你检索到了一个从垃圾桶中取出的皱巴巴的纸张。它的内容似乎很重要。");
                }
                else
                {
                    await SayNext("(你看到一个从垃圾桶中伸出的皱巴巴的纸张。内容似乎很重要，但由于你的背包已满，无法将其取出。)");
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
                await gainItem(4031847, -1);
                await gainItem(4031848, 1);

                await setQuestProgress(2180, 1, idx);

                await SayNext("现在用牛奶把瓶子装满。瓶子现在装了三分之一的牛奶。");
            }
            else if (canHold(4031849, 1) && haveItem(4031848))
            {
                await gainItem(4031848, -1);
                await gainItem(4031849, 1);

                await setQuestProgress(2180, 1, idx);

                await SayNext("现在用牛奶把瓶子装满。瓶子现在装了三分之二的牛奶。");
            }
            else if (canHold(4031850) && haveItem(4031849))
            {
                await gainItem(4031849, -1);
                await gainItem(4031850, 1);

                await setQuestProgress(2180, 1, idx);

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
                await gainItem(4031848, -1);
                await gainItem(4031849, -1);
                await gainItem(4031850, -1);

                await gainItem(4031847, 1);
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
                    await gainItem(4031853, 1);
                }
                else if (rolled == 1)
                {
                    await gainItem(4031854, 1);
                }
                else
                {
                    await gainItem(4031855, 1);
                }
            }
            else
            {
                await SayOK("你已经拥有了这里的眼镜！");
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

        // Npc: 1101001 
        public async Task createCygnus()
        {
            if (getPlayer().isCygnus() && GameConstants.getJobBranch(getJob()) > 2)
            {
                await useItem(2022458);
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
            var option = await AskMenu("等等！等你达到10级的时候，你会自己搞清楚这些东西的，但如果你绝对想提前准备，你可以查看以下信息。\r\n\r\n 告诉我，你想知道什么？\r\n#b#L0#关于你#l\r\n#L1#小地图#l\r\n#L2#任务窗口#l\r\n#L3#背包#l\r\n#L4#普通攻击狩猎#l\r\n#L5#如何拾取物品#l\r\n#L6#如何装备物品#l\r\n#L7#技能窗口#l\r\n#L8#如何使用快捷栏#l\r\n#L9#如何打开箱子#l\r\n#L10#如何坐在椅子上#l\r\n#L11#世界地图#l\r\n#L12#任务通知#l\r\n#L13#增强属性#l\r\n#L14#谁是皇家骑士？#l");
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
                    await guideHint(option);
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
            if (await AskYesNo("你想要退出训练大厅吗？"))
            {
                await warp(130020000, 0);
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
                await warp(130000000);
            }
            else
            {
                await warp(getPlayer().getSavedLocation("CYGNUSINTRO"));
            }
        }

        // Npc: 1104200 
        public async Task enterBlackEreb()
        {
            await SayNext("#b#p1104002##k... The black witch... Trapped me here... There's no time now, she's already on her way to #rattack Ereve#k!");
            if (await AskYesNo("Fellow Knight, you must reach to #rEreve#k right now, #rthe Empress is in danger#k!! Even in this condition, I can still Magic Warp you there. When you're ready talk to me. #bAre you ready to face Eleanor?#k"))
            {
                var map = await getWarpMap(913030000);
                if (map.countPlayers() == 0)
                {
                    await warp(map.Id, 0);
                }
                else
                {
                    await SayOK("There's someone already challenging her. Please wait awhile.");
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
                await warp(140010210, 0);
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
                new SpeechText("我是黑色之翼的人偶师弗朗西斯。你把我安置的好几个人偶都给找出来了……坏了我的好事。虽然我很恼火，不过这次先放你一马。你要是再敢和我作对………我以黑魔法师大人的名义发誓，绝不放过你。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                new SpeechText("#b（……黑色之翼？作对？……到底是怎么回事？在怪兽的身上找到人偶与黑魔法师有什么关系？该去找特鲁商议商议。）#k", NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                ]);

            await completeQuest(21719);
            await warp(105040200, 10);
        }

        // Npc: 1204010 
        public async Task giantDagoth()
        {
            await SaySpeech([
                new SpeechText("嗯？怎么回事，你？", NpcTalkSpeaker.NoEnd),
                new SpeechText("前不久倒是听说金银岛上的人偶师被人打倒了，难道是你……", NpcTalkSpeaker.NoEnd),
                new SpeechText("嘿嘿，那反倒好办了！既拿到了#b天空之城封印石#k，又能顺便打倒你的话，我就能在人偶师之上了！出招吧！", NpcTalkSpeaker.NoEnd)
                ]);

            var mapObj = getMap();
            var npcPos = mapObj.getMapObject(getNpcObjectId())!.getPosition();
            await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9300348), new Point(npcPos.X + 50, npcPos.Y), mob =>
            {
                mob.setSpawnEffect(12);
            });
            await mapObj.destroyNPC(getNpc());
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
            if (await AskYesNo("我们有一棵漂亮的圣诞树。你想看看/装饰它吗？"))
            {
                await warp(209000001);
            }
        }

        // Npc: 2001002 
        public async Task go_tree2()
        {
            if (await AskYesNo("我们有一棵漂亮的圣诞树。你想看看/装饰它吗？"))
            {
                await warp(209000002);
            }
        }

        // Npc: 2001003 
        public async Task go_tree3()
        {
            if (await AskYesNo("我们有一棵漂亮的圣诞树。你想看看/装饰它吗？"))
            {
                await warp(209000003);
            }
        }

        // Npc: 2001004 
        public async Task out_tree()
        {
            if (await AskYesNo("那么，你准备好离开这里了吗？"))
            {
                await warp(209000000);
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
            if (await AskYesNo("你想离开快乐村吗？"))
            {
                var map = getPlayer().GetSavedLocation(SavedLocationType.HAPPYVILLE);
                if (map == -1)
                {
                    map = 101000000;
                }

                await warp(map, 0);
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
            var option = await AskMenu("天空之城的站台有许多个月台，请按照你的目的地进行选择。你想要前往哪里？", options.Select(x => $"前往{x.map}的{x.transport}站台"));
            var targetMap = 200000110 + (option * 10);
            await SayNext($"好的，我会把你送到 #b#m{targetMap}##k 的月台上。");
            await warp(targetMap, "west00");
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
                await gainItem(4001019, -1);
                await warp(200082100, 0);
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
                await gainItem(4001019, -1);
                await warp(200080200, 0);
            }
            else
            {
                await SayOK("有一个#b#p2012015##k可以让你传送到#b#p2012014##k所在的地方，但如果没有卷轴就无法激活它。");
            }
        }

        async Task ElizaHarp(char harpNote)
        {
            string[] harpSounds = ["do", "re", "mi", "pa", "sol", "la", "si"];   // musical order detected thanks to Arufonsu
            var harpSong = "CCGGAAGFFEEDDC|GGFFEED|GGFFEED|CCGGAAGFFEEDDC|";
            await getMap().broadcastMessage(FieldEffectPacket.Sound("orbis/" + harpSounds[getNpc() - 2012027]));

            if (isQuestStarted(3114))
            {
                var idx = -1 * getQuestProgressInt(3114);  // infoEx without infoNumber, must use one progress only, critical hit!

                if (idx > -1)
                {
                    var nextNote = harpSong[idx];

                    if (harpNote != nextNote)
                    {
                        await setQuestProgress(3114, 0);

                        await getPlayer().SendPacket(FieldEffectPacket.Screen("quest/party/wrong_kor"));
                        await getPlayer().SendPacket(FieldEffectPacket.Sound("Party1/Failed"));

                        await Pink("You've missed the note... Start over again.");
                    }
                    else
                    {
                        nextNote = harpSong[idx + 1];

                        if (nextNote == '|')
                        {
                            idx++;

                            if (idx == 45)
                            {
                                // finished lullaby
                                await Pink("Twinkle, twinkle, little star, how I wonder what you are.");
                                await setQuestProgress(3114, 42);

                                await getPlayer().SendPacket(FieldEffectPacket.Screen("quest/party/clear"));
                                await getPlayer().SendPacket(FieldEffectPacket.Sound("Party1/Clear"));

                            }
                            else
                            {
                                if (idx == 14)
                                {
                                    await Pink("Twinkle, twinkle, little star, how I wonder what you are!");
                                }
                                else if (idx == 22)
                                {
                                    await Pink("Up above the world so high,");
                                }
                                else if (idx == 30)
                                {
                                    await Pink("like a diamond in the sky.");
                                }
                            }
                        }

                        await setQuestProgress(3114, -1 * (idx + 1));
                    }
                }
            }
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

        async Task Job2Enter(Job job, int letter)
        {
            var questId = QuestId.Get2ndJobQuest(job);
            if (haveItem(4031012) || isQuestCompleted(questId))
            {
                await SayOK("你真是个英雄！");
                return;
            }

            var questProgress = getQuestProgressInt(questId);
            if (questProgress == 1)
            {
                await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你通过了测试。祝你好运。");

                await TryStartSoloQuest(questId);
            }
            else
            {
                if (!haveItem(letter))
                {
                    await SayOK($"没有#b#t{letter}##k我是不会让你来冒险的！");
                    return;
                }

                await SaySpeech([
                    $"嗯...这绝对是#b#t{letter}##k...所以你来到这里是为了接受测试，进行{c.CurrentCulture.GetJobName(job)}第二次职业转职。好吧，我来给你解释一下测试。不要太担心，它并不是那么复杂。",
                    "我会把你送到一个隐藏的地图。你会看到一些平常不会见到的怪物。它们看起来和普通的怪物一样，但态度完全不同。它们既不会提升你的经验等级，也不会给你提供物品。",
                    "打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你通过了测试。",
                    ]);
                if (await AskYesNo("一旦你进去，就不能离开，直到完成你的任务。如果你死了，你的经验会减少...所以你最好做好准备...那么，你现在想去吗？"))
                {
                    await TryStartSoloQuest(questId, async () =>
                    {
                        await setQuestProgress(questId, 1);
                        await gainItem(letter, -1);
                    });
                }
                else
                {
                    await SayOK("等你准备好了再来找我，我会带你过去。");
                }
            }
        }

        async Task Job2Exit(Job job)
        {
            var questId = QuestId.Get2ndJobQuest(job);
            var eim = GetEventInstanceTrust();
            if (haveItem(4031013, 30))
            {
                await SayOK("哦哦哦.. 你收集了所有30个#t4031013#！！这应该很困难... 简直不可思议！好吧。你通过了测试，为此，我会奖励你 #b#t4031012##k。拿着它回去吧。");
                await removeAll(4031013);
                await gainItem(4031012);

                await eim.clearPQ();
            }
            else
            {
                await AskMenu("你需要收集 #b30 个 #t4031013##k。祝你好运。", ["我想离开"]);
            }
            await eim.exitPlayer(getPlayer());
        }

        async Task Job3(int type, string preNpcMap, int preNpc)
        {
            var option = await AskMenu("我可以帮你吗？", [
                "我想进行第三次职业转职。",
                "请允许我进行挑战扎昆任务。"
                ]);
            switch (option)
            {
                case 0:
                    var baseJob = JobFactory.GetById(type * 100);
                    var questId = QuestId.Get3rdJobQuest(baseJob);
                    var baseJobStr = c.CurrentCulture.GetJobName(baseJob);

                    if (IsQuestNotStarted(questId))
                    {
                        if (await AskYesNo($"欢迎。我是#b#p{getNpc()}##k，所有{baseJobStr}的首领。你似乎已经准备好迈出这一步，准备好迎接第三职业转职的挑战。太多的{baseJobStr}来来去去，无法达到第三职业转职的标准。你呢？你准备好接受考验，进行第三职业转职了吗？"))
                        {
                            await SaySpeech([
                                $"好的。你将在{baseJobStr}的两个重要方面进行测试：力量和智慧。我现在会向你解释测试的力量部分。还记得在{preNpcMap}的#b#p{preNpc}##k吗？去找他，他会告诉你第一部分测试的细节。请完成任务，并从#p{preNpc}#那里得到#b#t4031057##k。",
                                $"测试智慧的部分只能在你通过了力量测试之后才能开始。#b#t4031057##k 将证明你确实通过了测试。我会提前告诉#b#p{preNpc}##k你要前往那里，所以做好准备。这不会很容易，但我对你有信心。祝你好运。"
                                ]);
                            await startQuest(questId);
                        }
                    }
                    else if (isQuestStarted(questId))
                    {
                        var questProgress = getQuestProgressInt(questId);
                        if (questProgress == 0)
                        {
                            if (haveItem(4031057))
                            {
                                await SayNext("完成了测试的体能部分，做得很棒。我知道你能做到。现在你已经通过了测试的前半部分，接下来是后半部分。请先把项链给我。");
                                if (haveItem(4031057))
                                {
                                    await gainItem(4031057, -1);
                                    await setQuestProgress(questId, 1);

                                    await SayNext("这是测试的第二部分。这个测试将决定你是否足够聪明，可以迈向伟大的下一步。在冰封雪域的雪地上有一个被雪覆盖的黑暗区域，被称为圣地，甚至怪物也无法到达。在这个区域的中心，有一块被称为圣石的巨大石头。你需要献上一件特殊的物品作为祭品，然后圣石将在当场测试你的智慧。");
                                    return;
                                }
                                else
                                {
                                    // 原先这里只需要判断进度，并不强制需要物品（力量项链），我这里强制任务道具丢失需要重做
                                    await SayNext($"去，和#b#p{preNpc}#对话#k，然后给我带来#b#t4031057#。");
                                    return;
                                }
                            }
                            else
                            {
                                await SayNext($"去，和#b#p{preNpc}#对话#k，然后给我带来#b#t4031057#。");
                            }
                        }
                        else if (questProgress == 1)
                        {
                            if (haveItem(4031058))
                            {
                                await SayNext("做得好，完成了测试的智力部分。你正确地回答了所有问题。我必须说，你展现出的智慧水平让我印象深刻。在我们进行下一步之前，请先把项链交给我。");
                                if (haveItem(4031058))
                                {
                                    await gainItem(4031058, -1);
                                    await setQuestProgress(questId, 2);

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
                        }
                        else if (questProgress == 2)
                        {
                            await Apply3rdJobChange();
                        }
                        return;
                    }
                    else
                    {
                        await SayOK(GetDefault1());
                    }
                    break;
                default:
                    if (getPlayer().getLevel() >= 50)
                    {
                        await SayOK("长老会授予你#b特许#k，让你成为#r反击扎昆团队的一员#k。祝你前程似锦。");
                        if (IsQuestNotStarted(ZakumFeature.QuestId))
                        {
                            await startQuest(ZakumFeature.QuestId);
                        }
                    }
                    else
                    {
                        await SayOK("你太弱了，无法成为#r反击扎昆团队#k的一员#k。至少达到#b50级#k，然后再和我说话。");
                    }
                    break;
            }
        }

        async Task Apply3rdJobChange()
        {
            var nextJob = getJobId() + 1;
            var job = JobFactory.GetById(nextJob);
            var jobStr = c.CurrentCulture.GetJobName(job);
            if (!await AskYesNo($"好的！现在，我将让你成为{jobStr}。在这之前，请确保你的SP已经被充分使用了，你需要用完70级之前获得的所有SP。哦，还有，由于你已经在第二次转职时选择了职业方向，所以在第三次转职时就不需要再次选择了。你现在要进行转职吗？"))
            {
                return;
            }

            if (getPlayer().getRemainingSp() > (getLevel() - 70) * 3)
            {
                await SayNext("请在继续之前使用你所有的SP。");
                return;
            }

            await changeJobById(nextJob);
            await forceCompleteQuest(QuestId.Get3rdJobQuest(getJob()));
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
                    "你刚刚成为了#b圣骑士#k。你将会接触到一本新的技能书，其中包含各种新的攻击技能以及基于元素的攻击。建议圣骑士继续使用与骑士团成员相配的武器类型，无论是剑还是钝器。有一个名为#b冲锋#k的技能，可以给武器增加冰、火和闪电元素，使圣骑士成为唯一能够进行基于元素的攻击的战士。用元素弱化怪物，然后用#b冲锋打击#k造成巨大伤害。这将使你在这里成为一股毁灭性的力量。",
                        "我也给了你一些SP和AP，这将帮助你开始。你现在确实已经成为一个强大的战士。但请记住，现实世界将等待着你，那里会有更艰难的障碍需要克服。当你觉得无法自我训练来达到更高的境界时，那时候，只有那时候，来找我。我会在这里等着。"
                ]);
            }
            else if (job == Job.DRAGONKNIGHT)
            {
                await SaySpeech([
                    $"你刚刚成为了#b龙骑士#k。你将会接触到一本新的技能书，其中包含各种新的攻击技能以及基于元素的攻击。建议龙骑士继续使用与骑士团成员相配的武器类型，无论是剑还是钝器。有一个名为#b冲锋#k的技能，可以给武器增加冰、火和闪电元素，使圣骑士成为唯一能够进行基于元素的攻击的战士。用元素弱化怪物，然后用#b冲锋打击#k造成巨大伤害。这将使你在这里成为一股毁灭性的力量。",
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
            var jobId = QuestId.Get3rdJobQuest(getJob());
            if (isQuestStarted(jobId) && getMapId() == allowedJob.GetValueOrDefault(jobStyle) && !haveItem(4031059))
            {
                await TryStartSoloQuest(jobId);
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
            var eim = GetEventInstanceTrust();
            if (await AskYesNo("你想离开吗？"))
            {
                await eim.exitPlayer(getPlayer());
            }
        }

        // Npc: 2022004 
        public async Task s4common1_out()
        {
            await SayNext("你在那里做得很好，#h #，干得漂亮。现在我会把你送回#m211000000#。当你准备好学习新技能时，带着护身符和我交谈。");
            await warp(211000000, "in01");
        }

        // Npc: 2030000 
        public async Task goDungeon()
        {
            if (haveItem(4031450, 1))
            {
                await warp(921100100, 1);
                return;
            }

            await SayNext("嘿，你看起来好像想要继续前进，深入这个地方的更深处。不过，在那边，你会发现自己被攻击性强、危险的怪物包围，所以即使你觉得自己已经准备好了，也请小心。很久以前，我们镇上的一些勇敢的人进去想要消灭威胁镇上的任何人，但是从来没有回来过…");
            if (getLevel() > 49)
            {
                if (await AskYesNo("如果你打算进去，我建议你改变主意。但如果你真的想进去……我只会让那些足够强大以在那里生存下来的人进去。我不希望看到其他人死去。让我看看……嗯……！你看起来相当强壮。好吧，你想进去吗？"))
                {
                    await warp(211040300, 5);
                }
            }
            else
            {
                await SayOK("如果你打算进去，我建议你改变主意。但如果你真的想进去……我只会让那些足够强大以在那里生存下来的人进去。我不希望看到其他人死去。让我看看……嗯……！你还没到50级，我不能让你进去送死。");
            }
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
        // ("In GM Event, how many FRUIT CAKE you can get as reward?", ["20", "200", "5", "25", "100"], 2),
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

            var questId = QuestId.Get3rdJobQuest(getJob());
            var questProgres = getQuestProgressInt(questId);
            if (questProgres == 1 && !haveItem(4031058))
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
                        await gainItem(4005004, -1);
                        var questions = questList.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
                        for (int i = 0; i < 5; i++)
                        {
                            var q = questions[i];
                            var inputAnswer = await AskMenu(q.quest, q.options);
                            if (inputAnswer != q.answer)
                            {
                                await SayNext("答错了");
                                return;
                            }
                        }
                        await gainItem(4031058, 1);
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


        // Npc: 2032001 
        public async Task oldBook5()
        {
            if (isQuestCompleted(3034))
            {
                if (await AskYesNo("你对我帮助很大……如果你有任何黑暗水晶矿石，我可以为你精炼，每个只需#b500000金币#k。"))
                {
                    var inputNumber = await AskNumber("好的，那么你打算做多少个？", 1, 1, 100);
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

                    await gainItem(4004004, -10 * inputNumber);
                    await gainMeso(-500000 * inputNumber);
                    await gainItem(4005004, inputNumber);
                    await SayOK("明智地使用它。");
                }
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }

        // Npc: 2040024 
        public async Task ludi014()
        {
            if (haveItem(4001020))
            {
                if (await AskYesNo("您可以使用#b#t4001020##k来激活#b第一个玩具塔石#k。您要传送到第71层的#b第二个玩具塔石#k吗？"))
                {
                    if (haveItem(4001020))
                    {
                        await gainItem(4001020, -1);
                        await warp(221022900, 3);
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
                var option = await AskMenu("您可以使用#b#t4001020##k来激活#b第二个玩具塔石#k。你想要传送到哪块石头？", options.Select(x => x.desc));

                if (await AskYesNo($"您可以使用#b#t4001020##k来激活#b第二个玩具塔石#k。您要传送到#b{options[option].desc}#k？"))
                {
                    if (haveItem(4001020))
                    {
                        await gainItem(4001020, -1);
                        await warp(options[option].map, 3);
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
                var option = await AskMenu("您可以使用#b#t4001020##k来激活#b第三个玩具塔石#k。你想要传送到哪块石头？", options.Select(x => x.desc));

                if (await AskYesNo($"您可以使用#b#t4001020##k来激活#b第三个玩具塔石#k。您要传送到#b{options[option].desc}#k？"))
                {
                    if (haveItem(4001020))
                    {
                        await gainItem(4001020, -1);
                        await warp(options[option].map, 3);
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
                if (await AskYesNo("您可以使用#b#t4001020##k来激活#b第四个玩具塔石#k。您要传送到第41层的#b第三个玩具塔石#k吗？"))
                {
                    if (haveItem(4001020))
                    {
                        await gainItem(4001020, -1);
                        await warp(221021700, 3);
                    }

                }
            }
            else
            {
                await SayOK("玩具塔石可以让你传送到#b第三个玩具塔石#k，但如果没有卷轴，它是无法激活的。");
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
                await SayOK("#b#h ##k还没有归还一本故事书。");
                return;
            }
            await SaySpeech([
                $"让我看看.. #b#h ##k 一共归还了 #b{completed.Length}#k 本书。归还的书目如下：{string.Join("\r\n", completed.Select(x => $"#v{x.item}# #b#t{x.item}##k"))}",
                "图书馆现在已经安定下来，这主要要归功于你，#b#h ##k的巨大帮助。如果故事再次混乱，那么我希望你能再次来修复它。"
                ]);
        }

        // Npc: 2041025 
        public async Task Populatus01()
        {
            if (await AskYesNo("滴滴……滴滴……你可以通过我逃到一个更安全的地方。滴滴……滴滴……你想离开这个地方吗？"))
            {
                await WarpOut();
            }
        }

        // Npc: 2041026 
        public async Task giveupTimer()
        {
            if (haveItem(4220046, 1))
            {
                // quest completing here when "forfeiting Timer's Egg", as well as reporting missing quests on M. Shrine are thanks to drmdsr & Thora
                await SayOK("你想把#r#t4220046##k交给我，对吧？好的，我会替你拿着。");
                await gainItem(4220046, -1);
            }
            else
            {
                await SayOK($"你好！我是#b#p{npc}##k，负责观察和报告这个地区的任何超自然活动。");
            }
        }

        async Task Earth()
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

                        await gainItem(4031117, 1);
                        await setQuestProgress(3421, 1, progress);
                    }
                    else
                    {
                        await Popup("Have a ETC slot available for this item.");
                    }
                }
            }
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

        // Npc: 2060009 Map 251000100,230000000
        public async Task aqua_taxi()
        {
            // 230030200 水下世界/矩形地带
            // 230010000 水下世界/海底叉路
            // 230000000 水下世界
            // 251000000 百草堂
            // 251000100 百草堂/呼啸的大海
            Dictionary<int, (int Destination, string Desc)> dict = new()
            {
                {0,(230030200, "我想用 #t4031242##k 移动到 #b#m230030200##k.") },
                {1,(230030200, "去 #b#m230030200##k 需支付 #b10000金币#k.") },

                {2,(230000000, "去 #b#m230000000##k 需支付 #b10000金币#k.") },
                {3,(251000000, "去 #b#m251000000##k 需支付 #b10000金币#k.") },

                {4,(230010000, "去 #b#m230010000##k 需支付 #b10000金币#k.") },
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

            var option = await AskMenu("", dict.ToDictionary(x => x.Key, x => x.Value.Desc));
            switch (option)
            {
                case 0:
                    if (haveItem(4031242))
                    {
                        await gainItem(4031242, -1);
                        await warp(230030200);
                    }
                    break;
                default:
                    if (getMeso() > 10000)
                    {
                        await gainMeso(-10000);
                        await warp(dict[option].Destination);
                    }
                    break;
            }
        }

        // Npc: 2093004 — Cabin Assistant (Herb Town)
        public async Task aqua_taxi2()
        {
            if (await AskYesNo("你现在要移动到 #b#m230000000##k 吗？价格是 10000 金币。"))
            {
                if (getMeso() < 10000)
                {
                    await SayOK("我认为你没有足够的钱……");
                }
                else
                {
                    await gainMeso(-10000);
                    await warp(230000000);
                }
            }
            else
            {
                await SayNext("嗯...现在太忙了？如果你想做的话，回来找我吧。");
            }
        }

        // Npc: 2060100 
        public async Task s4common2()
        {
            if (isQuestStarted(6301))
            {
                if (haveItem(4000175))
                {
                    await gainItem(4000175, -1);
                    await warp(923000000, 0);
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

        // Npc: 2081000 
        public async Task job4_item()
        {
            var option = await AskMenu("...我可以帮你吗？\r\n#L0##b购买魔法种子#k#l\r\n#L1##b为利夫雷做点什么#k#l");
            switch (option)
            {
                case 0:
                    await AskMenu("你好像不是本地人。我能帮你吗？#L0##b我想要一些#t4031346#。#k#l");
                    var inputNumber = await AskNumber("#b#t4031346##k is a precious iteml I cannot give it to you just like that. How about doing me a little favor? Then I'll give it to you. I'll sell the #b#t4031346##k to you for #b30,000 mesos#k each. Are you willing to make the purchase? How many would you like, then?", 1, 1, 99);
                    var cost = inputNumber * 30000;
                    if (await AskYesNo("购买 #b" + inputNumber + "个 #t4031346##k 将花费你 #b" + cost + " 金币#k。你确定要购买吗？"))
                    {
                        if (getMeso() < cost || !canHold(4031346, inputNumber))
                        {
                            await SayOK("请检查并查看您是否有足够的金币来进行购买。另外，我建议您检查其他物品栏，看看是否有足够的空间来进行购买。");
                        }
                        else
                        {
                            await SayOK("再见~");
                            await gainItem(4031346, inputNumber);
                            await gainMeso(-cost);
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


        // Npc: 2082003 
        public async Task flyminidraco()
        {
            await AskMenu("如果你有翅膀，我相信你可以去那里。但是，仅仅这样还不够。如果你想穿越比刀锋还锋利的风，你还需要坚硬的鳞片。我是唯一知道回去的半人半龙……如果你想去那里，我可以变你。无论你是什么，此刻，你将成为一只#b龙#k……\r\n#L0##b我想成为一只龙。#k#l");
            await useItem(2210016);
            await warp(200090500, 0);
        }

        // Npc: 2082004 
        public async Task TD_neo_Andy()
        {
            await SayOK($"嗨，我是#p{npc}#，来自一个并不那么遥远的未来的时间旅行者。我来阻止这个时代贪婪的人们制造机器。他们在我的时代变得疯狂，把一切都消耗成了灰尘。我必须不惜一切代价阻止它！");
        }



        // Npc: 2083006 
        public async Task TD_neoCity_enter()
        {
            if (!isQuestCompleted(3718))
            {
                await SayOK("时间机器尚未启动。");
                return;
            }

            int[] quests = [3719, 3724, 3730, 3736, 3742, 3748];
            string[] areas = [
                "Year 2021 - Average Town Entrance",
                "Year 2099 - Midnight Harbor Entrance",
                "Year 2215 - Bombed City Center Retail District",
                "Year 2216 - Ruined City Intersection",
                "Year 2230 - Dangerous Tower Lobby",
                "Year 2503 - Air Battleship Bow"
            ];

            int limit;
            for (limit = 0; limit < quests.Length; limit++)
            {
                if (!isQuestCompleted(quests[limit]))
                {
                    break;
                }
            }

            if (limit == 0)
            {
                await SayOK("在解锁下一个新奥城地图之前，向#b守护者尼克斯#k证明你的勇气。");
                return;
            }

            var menuItems = areas.Take(limit).Select((x, i) => x).ToList();
            var selection = await AskMenu("", menuItems);

            int[] mapids = [240070100, 240070200, 240070300, 240070400, 240070500, 240070600];
            if (selection >= 0 && selection < mapids.Length)
            {
                await warp(mapids[selection], 1);
            }
            else
            {
                await SayOK("先完成你的任务。");
            }
        }

        // Npc: 1204020 
        public async Task ShadowWarrier()
        {
            await SaySpeech([
                new SpeechText("我一直在等你……英雄的后裔啊……", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                new SpeechText("#b（英雄的后裔……？#o9300351#似乎知道一些关于英雄的事情。不过，他好像也和#p2091007#一样，不认为我是英雄本人啊。）", NpcTalkSpeaker.PlayerRight),
                new SpeechText("这个#b武陵封印石#k是英雄们撒下的种子……但收获的却是我们黑色之翼的东西。虽然你很漂亮地打败了#p1104000#和#p1204010#……再也不能让你为所欲为了。", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                new SpeechText("英雄的后裔终于和敌人见面了，真是让人感慨万分……这也是没办法的事情。我要以黑色之翼的名义，干掉你！", NpcTalkSpeaker.PlayerRight)
                ]);

            var mapObj = getMap();
            var npcPos = mapObj.getMapObject(getNpcObjectId())!.getPosition();
            await mapObj.spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9300351), new Point(npcPos.X + 50, npcPos.Y), mob =>
            {
                mob.setSpawnEffect(12);
            });
            await mapObj.destroyNPC(getNpc());
        }

        // Npc: 2101003 
        public async Task adin_enter()
        {
            await SayOK(GetDefaultTalk(3933)); ;
        }

        // Npc: 2101011 
        public async Task cejan()
        {
            await SayOK(GetDefaultTalk(3948));
        }

        // Npc: 2101013 
        public async Task karakasa()
        {
            await SayNext("我不知道你是怎么发现这个的，但你来对地方了！对于那些在尼哈尔沙漠徘徊并开始想家的人，我提供直飞金银岛的航班，不停歇！别担心飞船——它只摔过一两次！你在那艘小飞船上长时间飞行时不觉得幽闭恐惧吗？");
            if (await AskYesNo("请记住两件事。一，这条线路实际上是用于海外运输，所以 #r我不能保证你会降落在哪个城镇#k。二，由于我要安排你乘坐这个特殊航班，费用会有点高。服务费是 #e#b10,000 枚金币#n#k。有一架航班即将起飞。你对这个直达航班感兴趣吗？"))
            {
                await SayNext("好的，准备起飞~");
                if (getMeso() >= 10000)
                {
                    await gainMeso(-10000);
                    int[] towns = [100000000, 101000000, 102000000, 103000000, 104000000];
                    await warp(Randomizer.Select(towns));
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
        public async Task dooat()
        {
            await SayOK("Pyramid PQ is currently unavailable.");
        }

        // Npc: 2110005 
        public async Task nihal_taxi()
        {
            var toMagatia = "要搭乘 #b骆驼中巴#k 前往炼金术之城 #b玛加提亚#k 吗？车费是#b1500金币#k。";
            var toAriant = "要搭乘 #b骆驼出租车#k 前往灼热之路的城镇 #b阿里安特#k 吗？车费是#b1500金币#k。";

            if (await AskYesNo(getPlayer().getMapId() == 260020000 ? toMagatia : toAriant))
            {
                if (getMeso() < 1500)
                {
                    await SayNext("对不起，你的金币不够。");
                }
                else
                {
                    await warp(getPlayer().getMapId() == 260020000 ? 261000000 : 260000000, 0);
                    await gainMeso(-1500);
                }
            }
            else
            {
                await SayNext("嗯...现在太忙了？有需要的话随时来找我。");
            }
        }

        // Npc: 2111000 
        public async Task jenu_homun()
        {
            if (TryGetDefaultTalk(out var msg, 3310) || !haveItem(4031709, 1))
            {
                await SayOK(msg);
            }

            await warp(926120100, "out00");
        }

        // Npc: 2111006 
        public async Task drang_room1()
        {
            if (isQuestStarted(3320) || isQuestCompleted(3320))
            {
                await warp(926120200, 1);
            }
            else
            {
                await SayOK("唔呼呼……为什么这里只有鬼魂？");
            }
        }

        // Npc: 2111011 
        public async Task absence_wall()
        {
            if (await AskYesNo("在一片蜘蛛网的拥挤中，有一堵墙后面似乎写着什么东西。也许你应该仔细看看墙？"))
            {
                await setQuestProgress(3311, 5);
                await SayOK("在一面满是涂鸦的墙上，似乎有一句话格外显眼。#b它是以一种吊坠的形式出现……#k 这是什么意思？");
            }
        }

        // Npc: 2111012 
        public async Task absence_box()
        {
            // 2111012 空箱子，无对话内容
        }

        // Npc: 2111013 
        public async Task absence_frame()
        {
            await SayOK("挂在画框后面的钩子被解开，揭示了画框内部的一个秘密空间。在那里，发现了一枚银色吊坠。小心地取下吊坠后，画框被合上，放回桌子上。");
            if (canHold(4031697, 1))
            {
                await gainItem(4031697);
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

        // Npc: 2111017 
        public async Task pipe1()
        {
            if (isQuestStarted(3339))
            {
                var progress = getQuestProgressInt(23339, 1);

                if (progress == 3)
                {
                    var inputText = await AskText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress == 0)
                {
                    await setQuestProgress(23339, 1, 1);
                }
                else if (progress < 3)
                {
                    await setQuestProgress(23339, 1, 0);
                }
                else
                {
                    await warp(261000001, 1);
                }
            }
            else
            {
                if (isQuestCompleted(3339))
                {
                    await warp(261000001, 1);
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
                    var inputText = await AskText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress == 2)
                {
                    await setQuestProgress(23339, 1, 3);
                    var inputText = await AskText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress < 3)
                {
                    await setQuestProgress(23339, 1, 0);
                }
                else
                {
                    await warp(261000001, 1);
                }
            }
            else
            {
                if (isQuestCompleted(3339))
                {
                    await warp(261000001, 1);
                }
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
                    var inputText = await AskText("当水流开始流动时，那根管道发生了反应；一个装有键盘的秘密隔间随即显现了出来。#b密码#k!");
                    if (inputText == "琵丽雅是我的爱")
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK("#r错误！");
                    }
                }
                else if (progress == 1)
                {
                    await setQuestProgress(23339, 1, 2);
                }
                else if (progress < 3)
                {
                    await setQuestProgress(23339, 1, 0);
                }
                else
                {
                    await warp(261000001, 1);
                }
            }
            else
            {
                if (isQuestCompleted(3339))
                {
                    await warp(261000001, 1);
                }
            }
        }

        // Npc: 2111020 
        public async Task alceCircle1()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 0)
                {
                    await setQuestProgress(3345, 1);
                }
                else if (progress < 4)
                {
                    await setQuestProgress(3345, 0);
                }
            }
        }

        // Npc: 2111021 
        public async Task alceCircle2()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 0)
                {
                    await setQuestProgress(3345, 2);
                }
                else if (progress < 4)
                {
                    await setQuestProgress(3345, 0);
                }
            }
        }

        // Npc: 2111022 
        public async Task alceCircle3()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 0)
                {
                    await setQuestProgress(3345, 3);
                }
                else if (progress < 4)
                {
                    await setQuestProgress(3345, 0);
                }
            }
        }

        // Npc: 2111023 
        public async Task alceCircle4()
        {
            if (isQuestStarted(3345))
            {
                var progress = getQuestProgressInt(3345);

                if (progress == 3 && haveItem(4031739, 1) && haveItem(4031740, 1) && haveItem(4031741, 1))
                {
                    await setQuestProgress(3345, 4);
                    await gainItem(4031739, -1);
                    await gainItem(4031740, -1);
                    await gainItem(4031741, -1);

                    await SayOK("当你放置碎片时，一道光芒照耀着圆圈，驱散了这件神器内部酝酿的不祥之兆。");
                }
                else if (progress < 4)
                {
                    await setQuestProgress(3345, 0);
                }
            }
        }

        // Npc: 2111025 
        public async Task sca_auto()
        {
            if (await SayAcceptDecline("You can operate the automated security system using the control unit. Would you like to deactivate the automated security system?"))
            {
                await weakenAreaBoss(7090000, "The automated security system has been deactivated. The intruder alarm will shut down.");
            }
        }

        // Npc: 2111026 
        public async Task sca_DitRoi()
        {
            if (await SayAcceptDecline("This Magic Pentagram is incomplete. Would you like to finish off the drawing of the Magic Pentagram?"))
            {
                await weakenAreaBoss(8090000, "The Magic Pentagram has been completed. The spell to eliminate Deet and Roi has been summoned.");
            }
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
                await warp(970030000, "out00");
            }
        }

        // Npc: 9000036 
        public async Task A_office()
        {
            await SayOK(GetDefault0());
        }

        // Npc: 9000040 
        public async Task medal_rank()
        {
            if (!YamlConfig.config.server.USE_ENABLE_CUSTOM_NPC_SCRIPT)
            {
                await SayOK("勋章排名系统目前不可用。");
                return;
            }

            var levelLimit = !getPlayer().isCygnus() ? 160 : 110;
            var selStr = "The medal ranking system is currently unavailable... Therefore, I am providing the #bEquipment Merge#k service! ";
            if (!YamlConfig.config.server.USE_STARTER_MERGE && (getPlayer().getLevel() < levelLimit || getPlayer().GetMakerSkillLevel() < 3))
            {
                selStr += "However, you must have #rMaker level 3#k and at least #rlevel 110#k (Cygnus Knight), #rlevel 160#k (other classes) and a fund of #r" + numberWithCommas(50000) + " mesos#k to use the service.";
                await SayOK(selStr);
                return;
            }
            if (getMeso() < 50000)
            {
                selStr += "I'm sorry, but this service tax is of #r" + numberWithCommas(50000) + " mesos#k, which it seems you unfortunately don't have right now... Please, stop by again later.";
                await SayOK(selStr);
                return;
            }
        }

        // Npc: 9000041 
        public async Task Donation()
        {
            if (!YamlConfig.config.server.USE_ENABLE_CUSTOM_NPC_SCRIPT)
            {
                await SayOK("勋章排名系统目前不可用。");
                return;
            }

            string[] options = ["装备", "消耗", "设置", "其他"];
            var selStr = "你好，我是#b巴扎 NPC#k！把背包里不需要的道具卖给我吧。#r警告#b：请确保你选中出售的道具槽位#r之后#b的格子都已准备好出售。#k 选中道具#b下方#k的道具会被一并售出。";
            for (var i = 0; i < options.Length; i++)
            {
                selStr += $"\r\n#L{i}# {options[i]}#l";
            }

            var selectedType = await AskMenu(selStr);
            if (selectedType < 0 || selectedType >= options.Length)
                return;

            var name = await AskText($"从你的#r{options[selectedType]}#k背包中的哪一件道具开始交易？");
            if (string.IsNullOrWhiteSpace(name))
                return;

            var res = await getPlayer().sellAllItemsFromName((sbyte)(selectedType + 1), name);
            if (res > -1)
            {
                await SayOK($"交易完成！你从这个行动中获得了#r{numberWithCommas(res)}金币#k。");
            }
            else
            {
                await SayOK($"你的#b'{name}'#k物品栏中没有#b{options[selectedType]}#k！");
            }
        }

        // Npc: 9000049 
        public async Task treasureHunter()
        {
            if (getPlayer().isGM())
            {
                var stage = c.CurrentServer.getStoredVar(9000049);
                var eventStatus = stage switch
                {
                    1 => "EASY",
                    2 => "MEDIUM",
                    3 => "HARD",
                    _ => "CLOSED"
                };

                int selection = await AskMenu($"你好GM。\r\n当前活动是：#r{eventStatus}#k\r\n你想做什么？\r\n#b#L0#参加活动#l\r\n#L1#关闭活动#l\r\n#L2#将活动设置为简单#l\r\n#L3#将活动设置为中等#l\r\n#L4#将活动设置为困难#l");

                if (selection == 0)
                {
                    stage = c.CurrentServer.getStoredVar(9000049);
                    if (stage == 0)
                    {
                        await SayOK("看起来塔还没有解锁。请等待GM解锁！");
                    }
                    else
                    {
                        await warp(980040000 + stage * 1000, 0);
                    }
                }
                else
                {
                    c.CurrentServer.setStoredVar(9000049, selection - 1);
                }
            }
            else
            {
                var stage = c.CurrentServer.getStoredVar(9000049);
                if (stage == 0)
                {
                    await SayOK("看起来塔还没有解锁。请等待GM解锁！");
                }
                else
                {
                    await warp(980040000 + stage * 1000, 0);
                }
            }
        }

        // Npc: 9001102 
        public async Task giveupMoonPicture()
        {
            if (getPlayer().getMapId() != 100000000)
            {
                return;
            }

            await SayNext("看那儿！你看到了吗？没有？一个飞碟刚刚飞过去了……那边！！看，有人被拖进了飞碟……啊啊啊啊，是嘎嘎！#r嘎嘎刚刚被飞碟绑架了！#k");

            if (getPlayer().getLevel() >= 12)
            {
                if (await AskYesNo("我们现在该怎么办？这只是一个谣言，但是……我听说如果你被外星人绑架，会发生可怕的事情……或许这就是现在发生在Gaga身上的事情！请，请救救Gaga！#bGaga可能有点不确定和迷茫，但#k他的心真的很好。我不能让可怕的事情发生在他身上。对了！月球上的爷爷可能知道如何救他！我会送你去月球，所以请去见爷爷，救出Gaga！！！"))
                {
                    await SayNext("非常感谢。请救救嘎嘎！月球上的爷爷会帮助你。");
                    await warp(922240200, 0);
                }
            }
            else
            {
                await SayOK("哦！看来你还没有达到救出嘎嘎的等级要求。请在达到12级或更高级时再回来。");
            }
        }

        // Npc: 9010004 
        public async Task ludiEvent()
        {
            await SayOK(GetDefault0());
        }

        // Npc: 9010021 
        public Task RyuhoRank() => SayOK(GetDefault0());

        // Npc: 9010022 
        public async Task unityPortal()
        {
            var level = getLevel();
            if (level < 20)
            {
                await SayOK("There is no place for you to transport to from here.");
                return;
            }

            var menu = "";
            if (level >= 20 && level <= 30)
            {
                menu += "#0# Ariant Coliseum";
            }

            if (level >= 25)
            {
                menu += "#1# Mu Lung Dojo";
            }

            if (level >= 30 && level <= 50)
            {
                menu += "#2# Monster Carnival 1";
            }

            if (level >= 51 && level <= 70)
            {
                menu += "#3# Monster Carnival 2";
            }

            int selection = await AskDimensionalMirror(menu);

            getPlayer().saveLocation("MIRROR");
            switch (selection)
            {
                case 0:
                    await warp(980010000, 3);
                    break;
                case 1:
                    await warp(925020000, 0);
                    break;
                case 2:
                    getPlayer().saveLocation("MONSTER_CARNIVAL");
                    await warp(980000000, 3);
                    break;
                case 3:
                    getPlayer().saveLocation("MONSTER_CARNIVAL");
                    await warp(980030000, 3);
                    break;
            }
        }

        // Npc: 9060000 
        public async Task tamepig_out()
        {
            if (haveItem(4031508, 5) && haveItem(4031507, 5))
            {
                await SayNext("哇~ 你成功收集了5个#b#t4031508##k和#b#t4031507##k。好的，那么我会送你去动物园。到了之后请再和我交谈。");
                //
                await getEventInstance()!.clearPQ();
            }
            else
            {
                if (await AskYesNo("你还没有完成要求。你确定要离开吗？"))
                {
                    await warp(923010100, 0);
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
            await warp(40000, 0);
            await gainExp(3);

        }

        // Npc: 9110002 
        [ScriptName("Life in Mushroom Shrine...")]
        public async Task s_Life_in_Mushroom_Shrine()
        {
            await sendDefault(8074);
        }

        // Npc: 9120003 
        public async Task in_bath()
        {
            var price = 300;
            if (await AskYesNo("你想进入浴池吗？这将是" + price + "金币。"))
            {
                if (getMeso() < price)
                {
                    await SayOK("请检查并查看您是否有" + price + "金币进入这个地方。");
                }
                else
                {
                    await gainMeso(-price);
                    await warp(801000100 + 100 * getPlayer().getGender(), "out00");
                }
            }
        }

        // Npc: 9120010 
        public async Task whitto()
        {
            var eQuestChoices = new[] { 4000064, 4000065, 4000066, 4000075, 4000077, 4000089, 4000090, 4000091, 4000092, 4000093, 4000094 };
            var eQuestPrizes = new Dictionary<int, int[][]>
            {
                { 0, new[] { new[] { 2000000, 1 }, new[] { 2000006, 1 }, new[] { 2000003, 5 }, new[] { 2000002, 5 }, new[] { 4020006, 2 }, new[] { 4020000, 2 }, new[] { 4020004, 2 }, new[] { 2000003, 10 }, new[] { 2000003, 20 }, new[] { 2000002, 10 }, new[] { 2000002, 20 }, new[] { 2022026, 15 }, new[] { 2022024, 15 }, new[] { 1002393, 1 } } },
                { 1, new[] { new[] { 2000006, 1 }, new[] { 2000002, 5 }, new[] { 4020006, 2 }, new[] { 2000002, 10 }, new[] { 2000003, 10 }, new[] { 2000002, 20 }, new[] { 2000003, 20 }, new[] { 2022024, 15 }, new[] { 2022026, 15 } } },
                { 2, new[] { new[] { 2000006, 1 }, new[] { 2000002, 5 }, new[] { 2000003, 5 }, new[] { 4020000, 2 }, new[] { 2000003, 10 }, new[] { 2000002, 10 }, new[] { 2000003, 20 }, new[] { 2000002, 20 }, new[] { 2022024, 15 }, new[] { 1002393, 1 } } },
                { 3, new[] { new[] { 2060003, 1000 }, new[] { 4010004, 2 }, new[] { 4010006, 2 }, new[] { 2022022, 5 }, new[] { 2022022, 10 }, new[] { 2022022, 15 }, new[] { 2022019, 5 }, new[] { 2022019, 10 }, new[] { 2022019, 15 }, new[] { 2001002, 15 }, new[] { 2001001, 15 }, new[] { 1102040, 1 }, new[] { 1102043, 1 } } },
                { 4, new[] { new[] { 2000003, 1 }, new[] { 2022019, 5 }, new[] { 2000006, 5 }, new[] { 4010002, 2 }, new[] { 4010003, 2 }, new[] { 2000006, 10 }, new[] { 2000006, 15 }, new[] { 2022019, 10 }, new[] { 2022019, 15 }, new[] { 2060003, 1000 }, new[] { 2061003, 1000 }, new[] { 1082150, 1 }, new[] { 1082149, 1 } } },
                { 5, new[] { new[] { 2000006, 1 }, new[] { 2000003, 5 }, new[] { 2000002, 5 }, new[] { 2000003, 10 }, new[] { 2000003, 20 }, new[] { 2000002, 10 }, new[] { 2000002, 15 }, new[] { 2060003, 1000 }, new[] { 2061003, 1000 }, new[] { 2022026, 15 }, new[] { 1002395, 1 } } },
                { 6, new[] { new[] { 2022019, 5 }, new[] { 2000006, 5 }, new[] { 4010003, 2 }, new[] { 2022019, 10 }, new[] { 2022019, 15 }, new[] { 2000006, 10 }, new[] { 2000006, 15 }, new[] { 2060003, 1000 }, new[] { 2061003, 1000 } } },
                { 7, new[] { new[] { 2000003, 1 }, new[] { 2000006, 1 }, new[] { 2022019, 1 }, new[] { 2000006, 5 }, new[] { 4010002, 2 }, new[] { 4020001, 2 }, new[] { 2022019, 10 }, new[] { 2022019, 15 }, new[] { 2000006, 10 }, new[] { 2000006, 15 }, new[] { 2060003, 1000 }, new[] { 2061003, 1000 } } },
                { 8, new[] { new[] { 2022019, 5 }, new[] { 2022022, 5 }, new[] { 4010006, 2 }, new[] { 2022019, 10 }, new[] { 2022019, 15 }, new[] { 2022022, 10 }, new[] { 2022022, 15 }, new[] { 2001002, 15 }, new[] { 2001001, 15 }, new[] { 1102043, 1 } } },
                { 9, new[] { new[] { 4010004, 5 }, new[] { 2022019, 5 }, new[] { 2022022, 15 }, new[] { 2022019, 15 }, new[] { 2001002, 15 }, new[] { 2001001, 15 }, new[] { 1102043, 1 } } },
                { 10, new[] { new[] { 1102207, 1 }, new[] { 1442026, 1 }, new[] { 1302037, 1 }, new[] { 2070007, 1 }, new[] { 2340000, 1 }, new[] { 2330005, 1 }, new[] { 2022060, 25 }, new[] { 2022061, 20 }, new[] { 2022062, 15 } } }
            };

            if (!await AskYesNo("如果你正在寻找一个能够准确描述各种物品特征的人，那么你现在就找到了。我目前正在寻找一样东西。你想听听我的故事吗？"))
            {
                await SayOK("真的吗？如果你改变主意了，记得告诉我。");
                return;
            }

            var choiceStr = "The items I'm looking for are 1,2,3 ... phew, too many to\r\nmention. Anyhow, if you gather up 100 of the same items,\r\nthen i may trade it with something similiar. What? You may\r\nnot know this, but i keep my end of the promise, so you\r\nneed not worry. Now, shall we trade?\r\n";
            for (int i = 0; i < eQuestChoices.Length; i++)
            {
                choiceStr += " #L" + i + "##v" + eQuestChoices[i] + "##t" + eQuestChoices[i] + "##l\r\n";
            }

            int selection = await AskMenu(choiceStr);

            int requiredItem = eQuestChoices[selection];
            var reward = eQuestPrizes[selection];
            var rnd = new Random();
            var itemSet = rnd.Next(reward.Length);
            int prizeItem = reward[itemSet][0];
            int prizeQuantity = reward[itemSet][1];

            if (!canHold(prizeItem))
            {
                await SayNext("如果你的装备、使用或其他物品栏已满，我无法给你奖励。请立即去看一下。");
            }
            else if (haveItem(requiredItem, 100))
            {
                await gainItem(requiredItem, -100);
                await gainItem(prizeItem, prizeQuantity);
                await SayOK("嗯...如果不是这个小小的划痕...唉。恐怕我只能认定这是一个标准品质的物品。好吧，这是给你的#t" + prizeItem + "#。");
            }
            else
            {
                await SayOK("嘿，你以为你在干什么？去欺骗那些不懂得在说什么的人。不要来骗我！");
            }
        }

        // Npc: 9120013 
        public async Task boss_cat()
        {
            // 检查任务8012是否进行中且没有4031064
            if (!isQuestStarted(8012) || haveItem(4031064))
            {
                return;
            }

            if (!await AskYesNo("你都找到了吗？你打算尝试回答我所有的问题吗？"))
            {
                return;
            }

            if (!haveItem(2020001, 300))
            {
                await SayOK("什么？不行！300！三百。不少。如果你想要更多，就给我，但我至少需要300。我们不是所有人都像你一样又大又饱满…");
                return;
            }

            await gainItem(2020001, -300);
            await SayNext("干得好！现在等一下……嘿，看这里！我这里有些食物！自己拿吧。好了，现在是时候问你们一些问题了。我相信你们已经意识到了，但记住，如果你们答错了，一切都结束了。要么全赢，要么全输！", NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd);

            // 问题和答案数据
            var questions = new List<string>
            {
                "Which of these items does the Flaming Raccoon NOT drop?",
                "Which NPC is responsible for transporting travellers from Kerning City to Zipangu, and back?",
                "Which of the items sold at the Mushroom Shrine increases your attack power?",
                "Which of these items do the Extras NOT drop?",
                "Which of these items DO NOT exist??",
                "What's the name of the vegetable store owner in Showa Town?",
                "Which of these items DO exist?",
                "What is the name of the strongest boss in the Mushroom Shrine?",
                "Which one of these items has a mis-matched class or level description?",
                "Which of these noodles are NOT being sold by Robo at the Mushroom Shrine?",
                "Which of these NPCs do NOT stand in front of Showa Movie Theater?"
            };

            var answers = new List<string[]>
            {
                new[] { "狸猫柴火", "独角狮硬角", "Red Brick" },
                new[] { "Peli", "Spinel", "Poli" },
                new[] { "Takoyaki", "Yakisoba", "Tempura" },
                new[] { "Extra A's Badge", "Extra B's Corset", "Extra C's Necklace" },
                new[] { "Frozen Tuna", "Fan", "Fly Swatter" },
                new[] { "Sami", "Kami", "Umi" },
                new[] { "Cloud Fox's Tooth", "Ghost's Bouquet", "Dark Cloud Fox's Tail" },
                new[] { "Black Crow", "Blue Mushmom", "Himegami" },
                new[] { "Bamboo Spear - Warrior-only Weapon", "Pico-Pico Hammer - One-handed Sword", "Mystic Cane - Level 51 equip" },
                new[] { "Kinoko Ramen (Pig Skull)", "Kinoko Ramen (Salt)", "Mushroom Miso Ramen" },
                new[] { "Skye", "Furano", "Shinta" }
            };

            var correctAnswer = new List<int> { 1, 1, 0, 1, 2, 2, 2, 0, 0, 2, 2 };

            // 随机选择5个问题
            var rng = new Random();
            var indices = Enumerable.Range(0, questions.Count).OrderBy(_ => rng.Next()).Take(5).ToList();

            for (int i = 0; i < 5; i++)
            {
                int idx = indices[i];
                var prompt = $"Question no.{i + 1}: {questions[idx]}";
                for (int j = 0; j < answers[idx].Length; j++)
                {
                    prompt += $"\r\n#b#L{j}#{answers[idx][j]}#l#k";
                }

                int selection = await AskMenu(prompt);

                if (selection != correctAnswer[idx])
                {
                    await SayNext("嗯……反正所有人都会犯错！如果你想再试一次，那就给我拿300只炸鸡来。");
                    return;
                }
            }

            await gainItem(4031064, 1);
            await SayNext("哎呀，你所有的问题都答对了。总的来说，我可能不喜欢人类，但我讨厌食言，所以，正如我所承诺的，这是#i4031064#。");
            await SayOK("我们的交易已经结束，非常感谢！你可以离开了！");
        }



        // Npc: 9201050 
        public async Task About_NLC()
        {
            const int minlevel = 10;

            if (isQuestCompleted(4911))
            {
                await SayNext("干得好！你解决了我关于NLC的所有问题。祝你旅途愉快！");
                return;
            }

            if (isQuestCompleted(4900) || isQuestStarted(4900))
            {
                await SayNext("嘿，注意一下，我要考你另一个问题，伙计！");
                return;
            }

            var info = new[]
            {
                "What is this place?",
                "Who is Professor Foxwit?",
                "What's a Foxwit Door?",
                "Where are the MesoGears?",
                "What is the Krakian Jungle?",
                "What's a Gear Portal?",
                "What do the street signs mean?",
                "What's the deal with Jack Masque?",
                "Lita Lawless looks like a tough cookie, what's her story?",
                "When will new boroughs open up in the city?",
                "I want to take the quiz!"
            };

            var selStr = "What up! Name's Icebyrd Slimm, mayor of New Leaf City! Happy to see you accepted my invite. So, what can I do for you?#b";
            for (int i = 0; i < info.Length; i++)
            {
                selStr += "\r\n#L" + i + "# " + info[i] + "#l";
            }

            int selection = await AskMenu(selStr);

            switch (selection)
            {
                case 0:
                    await SayNext("我一直梦想建造一座城市。不是普通的城市，而是一个每个人都受欢迎的城市。我曾经住在废弃都市，所以我决定看看我是否能创建一个城市。在寻找实现这一目标的途径时，我遇到了许多人，其中一些我已经视为朋友。比如福克斯维特教授-他是我们的天才；我救他脱离了一群吃人植物。杰克·马斯克是我在阿莫利亚的老猎友-说话太圆滑了，对自己不利。莉塔和我是废弃都市的老朋友-她用她的武器救过我几次；所以我觉得她是镇长的完美选择。需要一些说服，但她最终相信她的命运在这里。至于我们的探险家，巴里卡德来寻找某样东西；他同意把他找到的东西带到博物馆。我在废弃都市的时候就听说过他和他的兄弟的故事。至于#n9201055#……嗯，我们就说他不是这里的人。完全不是。我们之前交谈过，他似乎心地善良，所以我允许他留下。我刚意识到我说了很多废话！你还想知道什么？");
                    await About_NLC();
                    break;
                case 1:
                    await SayNext("一个97岁的老家伙，动作还挺敏捷。有一天我在城外遇到了他，他是个时空旅行者。这个老家伙在丛林里遇到了一些麻烦，好像有些丛林生物想要吃掉他。作为我救了他的回报，他答应建造一个时空博物馆。我感觉他来这里有其他目的，因为他多次提到新叶城在未来将扮演一个有趣的角色。也许你可以多了解一些……");
                    await About_NLC();
                    break;
                case 2:
                    await SayNext("嘿，当我看到教授在建造它们时，我也问了同样的问题。它们是传送点。按上键会将你传送到另一个位置。我建议你熟悉它们，它们是我们的交通系统。");
                    await About_NLC();
                    break;
                case 3:
                    await SayNext("MesoGears位于比格本塔下方。这是巴里卡德发现的一个充满怪物的区域。看起来它似乎位于塔的一个独立部分，如果你问我，这相当奇怪。我听说他需要一点帮助来探索它，你应该去看看他。不过要小心，那里的狼蜘蛛可不是闹着玩的。");
                    await About_NLC();
                    break;
                case 4:
                    await SayNext("啊...嗯。克拉基亚丛林位于新叶城的郊外。许多新的、强大的生物在那些地区漫游，所以如果你要去那里，最好做好战斗的准备。它位于城镇的右端。有传言说丛林通向一个失落的城市，但我们还没有找到任何东西。");
                    await About_NLC();
                    break;
                case 5:
                    await SayNext("嗯，当约翰发现自己在大本钟的迷思齿轮部分时，他站在一个上，然后去了另一个地方。然而，他只能来回走动，它们不像狐狡之门那样循环。这就是古老的科技。");
                    await About_NLC();
                    break;
                case 6:
                    await SayNext("嗯，你几乎可以在任何地方看到它们。它们是正在施工的区域。红灯表示还没完工，但绿灯表示已经开放。经常回来看看，我们一直在建设中！");
                    await About_NLC();
                    break;
                case 7:
                    await SayNext("啊，杰克。你知道那些自以为很酷、总是能逃脱一切惩罚的家伙吗？还能追到女孩？嗯，那就是杰克，只是没有女孩。他觉得自己错失了机会，于是开始戴上面具隐藏真实身份。我对他的真实身份守口如瓶，但他来自阿莫利亚。如果你问他，他可能会告诉你更多。");
                    await About_NLC();
                    break;
                case 8:
                    await SayNext("我认识莉塔有一段时间了，虽然我们最近才重新燃起友谊。我有一段时间没见到她，但我理解为什么。她作为一个盗贼进行了非常非常长时间的训练。事实上，那就是我们第一次见面的方式！我被一群迷失的蘑菇围攻，她跳了出来帮忙。当选举警长的时候，毫无疑问是她。她承诺要帮助其他人进行训练并保护城市，所以如果你对公民义务感兴趣，可以和她交谈一下。");
                    await About_NLC();
                    break;
                case 9:
                    await SayNext("很快，我的朋友。虽然你看不到他们，但城市开发者们正在努力工作。当他们准备好了，我们就会打开它们。我知道你期待着这一刻，我也是！");
                    await About_NLC();
                    break;
                case 10:
                    if (getLevel() >= minlevel)
                    {
                        await SayNext("没问题。如果你回答正确，我会给你一些好东西！");
                        await startQuest(4900);
                    }
                    else
                    {
                        await SayNext("急切了吗？在我让你参加测验之前，你想再多探索一下吗？");
                    }
                    break;
            }
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
                if (await AskYesNo("你想回到 #b新叶城 市区中心#k 吗？费用是" + fee + "金币。"))
                {
                    if (getMeso() >= fee)
                    {
                        await gainMeso(-fee);
                        await warp(600000000);
                    }
                    else
                    {
                        await SayOK("嘿，你想搞什么鬼？你没有足够的金币来支付费用。");
                    }
                }
            }
            else
            {
                if (await AskYesNo("你想乘坐这辆车前往 #b鬼屋入口#k 吗? 这个费用是 " + fee + " 金币."))
                {
                    if (getMeso() >= fee)
                    {
                        await gainMeso(-fee);
                        await warp(682000000, 0);
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

                if (await AskYesNo($"每分钟都有前往{target}的地铁出发，票价为#b{p.Value.TicketPrice}金币#k。您确定要购买#b#t{p.Value.TicketItemId}##k吗？"))
                {
                    if (!canHold(p.Value.TicketItemId))
                    {
                        await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
                    }
                    else if (getMeso() >= p.Value.TicketPrice)
                    {
                        await gainMeso(-p.Value.TicketPrice);
                        await gainItem(p.Value.TicketItemId, 1);
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
                if (await AskYesNo("列车启动前你想离开吗？车票将不会退款。"))
                {
                    await WarpReturn();
                }
            }

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
                await openShopNPC(getNpc());
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
        public async Task tcg4_6_8241()
        {
            if (getLevel() >= 100)
            {
                await SayOK("远征活动经常在绯红要塞内由像你这样的冒险者举行，许多来自不同队伍的人合作解决谜题，击败强大的敌人，在此过程中能够获得许多奖品。要了解更多信息，请前往城堡内右上方的房间。");
            }
            else
            {
                await SayOK("在要塞内，可以组建远征队尝试绯红森林要塞组队任务，需要等级达到100级或更高的冒险岛玩家。看起来你还不适合尝试这个任务，如果想要尝试的话，还需要多加练习。");
            }
        }

        // Npc: 9201104 
        public async Task Masteria_Sage01()
        {
            if (getMapId() == 610020000)
            {
                await SayOK("哦，勇敢的冒险者。仅仅到达这个地方，你就已经在众多人中脱颖而出，恭喜你。然而，要注意：前方通往强大的#b绯红要塞#k的道路上，部署了#r致命的巨石#k，对于那些对前方危险毫无防备的人来说，这些巨石就像陷阱一样。#r一次被击中就足以让你倒下#k，所以要小心。如果你打算到达城堡，一定要小心地跟随前方的小径。");
            }
            else if (getMapId() == 610020003)
            {
                await SayOK("你现在似乎配得上得到一个关于前方的提示。一旦进入要塞的主房间，请确保记住你所看到的雕像的布局。就是这样。");
            }
            else if (getMapId() == 610020004)
            {
                await SayOK("你现在似乎配得上得到一个关于接下来的提示。被称为\"封印\"的装置会在某些职业的技能在附近被激活时被检测到而激活，请确保你的团队在时机到来时是完整的。就是这样。");
            }
            else
            {
                await SayOK("到目前为止，你的进展非常出色，干得好。然而，要想进入要塞，你必须面对并完成这一考验，继续前行。");
            }
        }

        // Npc: 9201105 
        public async Task Masteria_Sage02()
        {
            if (getMapId() == 610020005)
            {
                await SayOK("绯红要塞就在前方，你今天取得了伟大的成就，向你致敬。穿过这片树林，进入城堡的大门。");
            }
            else
            {
                await SayOK("到目前为止，你的进展非常出色，干得好。然而，要想进入要塞，你必须面对并完成这一考验，继续前行。");
            }
        }

        // Npc: 9201106 
        public async Task TCG5()
        {
            await SayOK(GetDefault0());
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
                    if (await AskYesNo("你好 #h #，我可以把你送到#b#m" + map + "##k进行#b" + c.CurrentCulture.GetJobName(job) + "#k转职。你要过去吗？"))
                    {
                        await warp(map, 0);
                    }
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

        async Task Enter_Darkportal(int questId, int questItem, int mapId, int[] enterConsumeItems)
        {
            if (isQuestStarted(questId))
            {
                if (haveItem(questItem))
                {
                    if (await AskYesNo("你想要移动到 #b#m" + mapId + "##k 吗？"))
                    {
                        foreach (var item in enterConsumeItems)
                        {
                            await gainItem(item, -1);
                        }

                        switch (mapId)
                        {
                            case 677000000:
                                await SpawnBossIfAbsent(mapId, 9400612, new Point(461, 61), "Marbas 出现了！");
                                break;
                            case 677000002:
                                await SpawnBossIfAbsent(mapId, 9400610, new Point(467, 0), "Amdusias 出现了！");
                                break;
                            case 677000004:
                                await SpawnBossIfAbsent(mapId, 9400609, new Point(201, 80), "Andras 出现了！");
                                break;
                            case 677000006:
                                await SpawnBossIfAbsent(mapId, 9400611, new Point(171, 50), "Crocell 出现了！");
                                break;
                            case 677000008:
                                await SpawnBossIfAbsent(mapId, 9400613, new Point(251, -841), "Valefor 出现了！");
                                break;
                            default:
                                break;
                        }
                        await warp(mapId, 0);

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

        private async Task SpawnBossIfAbsent(int mapId, int mobId, Point pos, string? notice = null)
        {
            var map = await getWarpMap(mapId);
            if (map.getMonsterById(mobId) != null)
                return;

            var mob = LifeFactory.Instance.GetMonsterTrust(mobId);
            await map.spawnMonsterOnGroundBelow(mob, pos);

            if (notice != null)
            {
                await getPlayer().TypedMessage(5, notice);
            }
        }

        // Npc: 9201128 
        public Task Enter_Darkportal_W() => Enter_Darkportal(28179, 4032491, 677000004, [4001341, 4032478]);

        // Npc: 9201129 
        public Task Enter_Darkportal_M() => Enter_Darkportal(28198, 4032495, 677000000, []);

        // Npc: 9201130 
        public Task Enter_Darkportal_T() => Enter_Darkportal(28219, 4032493, 677000008, [4032485, 4001355]);

        // Npc: 9201131 
        public Task Enter_Darkportal_H() => Enter_Darkportal(28238, 4032492, 677000002, [4032481, 4032482, 4032483]);

        // Npc: 9201132 
        public Task Enter_Darkportal_P() => Enter_Darkportal(28256, 4032494, 677000006, [4001362, 4001363, 4032486, 4032488, 4032489, 4220153]);

        // Npc: 9201133 
        public async Task Astaroth_door()
        {
            if ((getMapId() >= 677000010 && getMapId() <= 677000012))
            {
                if (getMapId() == 677000011)
                {
                    if (await AskYesNo("你想要移动到 #b#m677000012##k 吗？"))
                    {
                        await warp(677000012, 0);
                    }
    ;
                }
                else
                {
                    if (await AskYesNo("你想要#b离开这个地方#k吗？"))
                    {
                        await warp(105050400, 0);
                    }

                }
            }
            else
            {
                if (isQuestStarted(28283))
                {
                    if (!getPlayer().haveItemEquipped(1003036))
                    {
                        await SayOK("前方的道路有一股奇怪的气味... 进入之前装备 #r#t1003036##k。");
                        return;
                    }

                    if (await AskYesNo("你想要移动到 #b#m677000010##k 吗？"))
                    {
                        await warp(677000010, 0);
                    }
                }
                else
                {
                    await SayOK("入口被一股奇怪的力量阻挡住了。");
                }
            }
        }

        // Npc: 9201142 
        public Task witchMaladyGL() => SayOK(GetDefault0());

        // Npc: 9201143 
        public Task oliviaEnter()
        {
            return SayOK(GetDefault0());
        }

        // Npc: 9220005 
        public async Task Jump_rudolph()
        {
            if (getChar().getMapId() == 209000000)
            {
                if (await AskYesNo($"你想前往 #b#m209080000##k 吗？"))
                {
                    await warp(209080000, 0);
                }
            }
            else if (getChar().getMapId() == 209080000)
            {
                if (await AskYesNo($"你想回到 #b#m209000000##k 吗？"))
                {
                    await warp(209000000, 0);
                }
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }

        // Npc: 9220019 
        public async Task guyfawkes_milla2()
        {
            var mapId = getMapId();
            if (mapId == 674030100)
            {
                await SayNext("嗨，我是 #p9220019#。");
            }
            else if (mapId == 674030300)
            {
                await SayNext("嗨，#h0#。这里是MV的宝藏房间。利用你在这里的时间做任何你想做的事情，实际上这里有很多东西等着你去发现。或者你可以使用这里的传送门 #rgo back#k 返回入口处。");
            }
            else
            {
                if (await AskYesNo("你确定要返回吗？现在返回意味着你要把你的伙伴们留在这里，你真的想这样做吗？"))
                {
                    await warp(674030100);
                }
            }
        }

        // Npc: 9220020 
        public async Task guyfawkes_ch2()
        {
            if (!isEventLeader())
            {
                await SayNext("请让你们的队长和我交谈，以便得到进入下一阶段的进一步指示。");
                return;
            }

            var eim = getEventInstance()!;
            if (eim.getIntProperty("statusStg1") == 1)
            {
                await SayNext("穿过这条隧道进行Boss战。");
            }
            else
            {
                if (haveItem(4032118, 15))
                {
                    await gainItem(4032118, -15);
                    eim.setIntProperty("statusStg1", 1);
                    await eim.showClearEffect();
                    await SayNext("你得到了这些信件，太棒了！现在，你可以通过这条隧道前往MV所在的房间。做好准备！");
                }
                else
                {
                    await SayNext("请把 #r15封秘密信#k 交给我。");
                }
            }
        }

        // Npc: 9270017 
        public async Task goback_kerning()
        {
            if (await AskYesNo("飞机马上就要起飞了，你现在要离开吗？你将不得不再次购买飞机票才能进来。"))
            {
                await SayNext("机票不可退，希望再次见到你！");
                await WarpReturn();
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
                if (await AskYesNo("飞机马上就要起飞了，你确定要现在离开吗？机票是不可退的。"))
                {
                    await WarpReturn();
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

            var option = await AskMenu($"你好，我是来自新加坡机场的#p{getNpc()}#。我可以帮助你迅速到达{target}。你想去{target}吗？\r\n#b#L0#我想买一张去{target}的飞机票\r\n#b#L1#让我进入出发点。");
            switch (option)
            {
                case 0:
                    if (await AskYesNo($"机票的价格是{t.Value.TicketPrice}金币。你要购买吗？"))
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

                        await gainMeso(-t.Value.TicketPrice);
                        await gainItem(t.Value.TicketItemId);
                        await SayOK("Thank you for choosing Wizet Airline! Enjoy your flight!");

                    }
                    break;
                case 1:
                    if (haveItem(t.Value.TicketItemId))
                    {
                        if (await airPlane.Enter(getPlayer()))
                        {
                            await gainItem(t.Value.TicketItemId, -1);
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

        // Npc: 9310000 
        public async Task goshanghai1()
        {
            var fee = 2000;
            if (await AskYesNo($"嘿！我是#b#e驾驶员 洪#n#k，我负责驾驶飞往#b上海#k的飞机。\r\n经过长年的飞行，我的驾驶技术已经很了不得。\r\n有兴趣跟我一起前往美丽的#b#e上海外滩#k#n吗？\r\n只需要#r{fee}金币#k哦！"))
            {
                if (getMeso() < fee)
                {
                    await SayNext($"你确定你有 #b{fee} 金币#k？ 如果没有，我可不能免费送你去。");
                }
                else
                {
                    await gainMeso(-fee);
                    await warp(701000000);
                }
            }
        }

        // Npc: 9310013 
        public async Task goshanghai2()
        {
            var fee = 2000;
            if (await AskYesNo($"嘿！我是#b#e驾驶员 洪#n#k，我负责驾驶飞往#b金银岛#k的飞机。\r\n经过长年的飞行，我的驾驶技术已经很了不得。\r\n有兴趣跟我一起前往古朴的#b#e勇士部落#k#n吗？\r\n只需要#r{fee}金币#k哦！"))
            {
                if (getMeso() < fee)
                {
                    await SayNext($"你确定你有 #b{fee} 金币#k？ 如果没有，我可不能免费送你去。");
                }
                else
                {
                    await gainMeso(-fee);
                    await warp(102000000);
                }
            }
            else
            {
                await SayNext("充满勇者气息的#b#e金银岛勇士部落#k#n，难道你不想回去看看吗！真遗憾。");
            }
        }

        // Npc: 9310058 
        public async Task Jump_event()
        {
            await SayOK("欢迎来到#b快乐小镇#k，年轻的旅行者。你有什么愿望吗？");
        }

        // Npc: 9901000, 9901001, 9901002, 9901003, 9901004, 9901005, 9901006, 9901007, 9901008, 9901009, 9901010, 9901011 ...
        public async Task rank_user()
        {
            var pnpc = getPlayerNPCByScriptid(getNpc());
            if (pnpc is PlayerNpc playerNpc)
            {
                var branchJobName = GetJobName(playerNpc.Job);
                var rankStr = $"你好，我是#b{playerNpc.Name}#k，在#r{branchJobName}#k职业中第#r{c.CurrentCulture.Ordinal(playerNpc.JobRank)}#k位达到最高等级。\r\n";
                rankStr += $"\r\n    世界排名：#e#b{c.CurrentCulture.Ordinal(playerNpc.OverallRank)}#k#n";
                await SayOK(rankStr);
            }
            else
            {
                await SayOK("你好，你好吗？");
            }
        }

        async Task EnterTranningMap(int start)
        {
            if (getLevel() >= 20)
            {
                await SayOK("只有低于20级才能进入训练中心。");
                return;
            }

            var option = await AskMenu("你要进入训练中心吗？",
                Enumerable.Range(0, 5).Select(i => $"训练中心 {i}")
                );
            await warp(start + option, 0);
        }


        public async Task MaybeItsGrendel_end()
        {
            await SaySpeech([
                "一个黑色的阴影人物出现并攻击了你？这怎么可能发生在#b#p1032001##k的家里？这听起来像是一个大阴谋……",
                "我得在脑子里整理一下这一切。等一会儿再和我说话。"
                ]);
        }

        // Npc: 2103008
        /// <summary>
        /// <see cref="PortalScript.thief_in1"/>
        /// </summary>
        /// <returns></returns>
        public async Task thief_in2()
        {
            var pwd = await AskText("一个可疑的声音打破了沉默。 #b暗号#k ！");
            if (pwd == "芝麻开门")
            {
                if (isQuestCompleted(3925))
                {
                    await warp(260010402, 1);
                }
                else
                {
                    await Pink("尽管你说出了正确的答案，但门依然没有动静！");
                }
            }
            else
            {
                await SayOK("#r错误！");
            }
        }

        // Npc: 2043000 — Papulatus Time Leap
        public async Task s4time()
        {
            await SayNext("You don't belong to this world... Return now.");
            await warp(220080000);
        }

        // Npc: 2007
        public async Task tutorialSkip()
        {
            if (getPlayer().Level <= 10)
            {
                if (await AskYesNo("您想要跳过教程，直接前往明珠港吗？"))
                {
                    await warp(MapId.LITH_HARBOUR, 0);
                }
                else
                {
                    await SayNext("旅行愉快。");
                }
            }
            else
            {
                await SayNext("旅行愉快。");
            }
        }

        // Npc: 2096000 — 武陵修炼场告示牌
        public async Task sca_dollBear()
        {
            await SayOK("新手在这张地图上训练。精通者在下一个地图上训练。专业人士在最后一个地图上训练，那里会有boss等待。");
        }

        // Npc: 9900001 — GM 升级 NPC
        public async Task levelUP2()
        {
            if (getPlayer().gmLevel() <= 1)
            {
                await SayOK("嘿，怎么了？");
                return;
            }

            if (await AskYesNo("你想升级吗？"))
            {
                await getPlayer().levelUp(true);
            }
        }

        const int HONOR_NPC_FEE = 7000000;

        /// <summary>
        /// 名人堂 NPC：在对应职业的名人堂地图放置一个与玩家当前形象相同的 NPC。
        /// 冒险家（1102003）、骑士团（1102003）、战神（1202010）共用同一流程，
        /// 差别只在职业判定与"没有资格"时的提示语。
        /// </summary>
        private async Task SpawnHonorPlayerNpc(Func<int, bool> hasRequiredJob, string noQualifyMessage)
        {
            var jobType = getJobId() / 100;
            if (!hasRequiredJob(jobType))
            {
                await SayOK(noQualifyMessage);
                return;
            }

            var honorMapId = GameConstants.getHallOfFameMapid(getJob());
            if (!await canSpawnPlayerNpc(honorMapId))
            {
                await SayOK(noQualifyMessage);
                return;
            }

            var message = "你一路走来，才拥有今天的力量、智慧与勇气，不是吗？你想不想现在就把你的角色形象，以#rNPC#k的形式留在名人堂里呢？";
            if (HONOR_NPC_FEE > 0)
            {
                message += $" 我可以为你办到，费用是#b {numberWithCommas(HONOR_NPC_FEE)} 冒险币#k。";
            }

            if (!await AskYesNo(message))
            {
                return;
            }

            if (getMeso() < HONOR_NPC_FEE)
            {
                await SayOK("抱歉，您没有足够的冒险币购买在名人堂上的位置。");
                return;
            }

            if (!await canSpawnPlayerNpc(honorMapId))
            {
                await SayOK("抱歉，名人堂目前已满...");
                return;
            }

            c.CurrentServer.NodeService.PlayerNPCService.SpawnPlayerNPCByHonor(getPlayer());
            await gainMeso(-HONOR_NPC_FEE);
            await SayOK("给你了！希望你会喜欢它。");
        }

        // Npc: 1102003 — 骑士团名人堂（职业类型 11~15）
        public async Task cygnus_lv120()
        {
            await SpawnHonorPlayerNpc(jobType => jobType >= 11 && jobType <= 15, "欢迎来到骑士之间。");
        }

        // Npc: 1202010 — 战神名人堂（职业类型 21）
        public async Task aran_lv200()
        {
            await SpawnHonorPlayerNpc(jobType => jobType == 21,
                "瞧，这里是利恩的杰出勇敢英雄！那些坚定的心灵一直以来一直在保护我们的人民，他们是我们勇敢的战友。");
        }
    }

}