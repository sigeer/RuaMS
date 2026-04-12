using Application.Core.Client;
using Application.Core.Scripting.Events;
using Application.Shared.Constants.Inventory;
using Application.Shared.Events;
using Application.Utility;
using Humanizer;
using Microsoft.Win32.SafeHandles;
using Quartz.Util;
using scripting.npc;
using server.life;
using System.Drawing;
using System.Linq;
using tools;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

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
        public NpcScript(IChannelClient c, int npc, int npcOId) : base(c, npc, npcOId, null)
        {
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
            }
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
                        await SayNext("我觉得你缺少冒险币。有很多方法可以赚钱，比如...卖掉你的盔甲...打败怪物...做任务...你知道我在说什么。");
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
                if (await SayYesNo($"你将购买这些 #r{inputNumber}#k #b#t{items[option].item}#(s)#k 吗？#t{items[option].item}# 一个需要 {items[option].price} 冒险币，所以总共需要 #r{totalCost}#k 冒险币。"))
                {
                    if (getMeso() < totalCost)
                    {
                        await SayNext("你是否缺少冒险币？请检查一下你的消耗物品栏中是否有空位，并且你是否携带了至少 #r" + totalCost + "#k 冒险币。");
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
                            await SayNext("请检查并查看您的消耗物品栏中是否有空的槽位可用。");
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

        // Npc: 1012002 
        public Task refine_henesys()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 1012119 
        public Task enter_archer()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1022000 
        public Task fighter()
        {
            // TODO
            return Task.CompletedTask;
        }





        // Npc: 1022003 
        public Task refine_perion()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1022004 
        public Task refine_perion2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1022101 
        public Task go_xmas()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1022103 
        public Task s4strike_statue()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1022105 
        public Task enter_warrior()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 1032001 
        public Task magician()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1032002 
        public Task refine_ellinia()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1032003 
        public async Task herb_in()
        {
            if (getLevel() < 25)
            {
                await SayOK("你必须达到更高的等级才能进入耐心森林。");
            }
            else
            {
                if (await SayYesNo("嗨，我是谢恩。我可以让你进入耐心森林，只需要支付一小笔费用。你想用 #b5000#k 金币进入吗？"))
                {
                    if (getMeso() < 5000)
                    {
                        await SayOK("抱歉，你好像没有足够的金币！");
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
            // TODO
            if (await SayYesNo("你想回到魔法密林吗？"))
            {
                warp(101000000, 0);
            }
        }


        // Npc: 1032007, 2012000, 2040000, 2082000, 2102002 
        public async Task sell_ticket()
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

            if (await SayYesNo($"你好，我负责出售前往{target}的船票。前往{target}的船每15分钟出发一次，票价为#b{currentContiMove.TicketPrice}金币#k。你确定要购买#b#t${currentContiMove.TicketItemId}##k吗？"))
            {
                if (getMeso() >= currentContiMove.TicketPrice && canHold(currentContiMove.TicketItemId))
                {
                    gainItem(currentContiMove.TicketItemId, 1);
                    gainMeso(-currentContiMove.TicketPrice);
                }
                else
                {
                    await SayOK("你确定你有 #b" + currentContiMove.TicketPrice + " 金币#k 吗？如果是的话，请检查你的其它物品栏，看看是否已经满了。");
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

            var next = DateTimeOffset.FromUnixTimeMilliseconds(currentContiMove.ArriveAt).ToLocalTime().Humanize();

            if (haveItem(currentContiMove.TicketItemId))
            {
                if (currentContiMove.CanEnter)
                {
                    if (await SayYesNo($"你想去{target}吗？"))
                    {
                        if (currentContiMove.Enter(getPlayer()))
                        {
                            gainItem(currentContiMove.TicketItemId, -1);
                        }
                        else
                        {
                            await SayOK($"飞往{target}的船只已经启程，请耐心等待下一班。下一班将在 ${next}抵达。");
                        }
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
            // TODO
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
                        await SayOK("确保你的ETC背包有空位。");
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
                    await SayOK("嘿，在和我交谈之前先腾出一个槽位。");
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

        // Npc: 1052002 
        public Task refine_kerning()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1052003 
        public Task refine_kerning2()
        {
            // TODO
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
            var option =  await SayOption(selStr);
            if (getMeso() < cost)
            {
               await SayOK("你没有足够的金币。");
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
                    var em0 = GetEventManager<SoloEventManager>("KerningTrain");
                    if (!em0.startInstance(getPlayer()))
                    {
                        await SayOK("客运车已经满了。稍后再试一次。");
                    }
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
                        sendOk("看起来你好像没有票！");

                    }
                    break;
                case 2:
                    if (!haveItem(4031711) && getPlayer().getMapId() == 103000100)
                    {
                        await SayOK("看起来你没有票！你可以从贝尔那里买一张。");
                        return;
                    }
                    var em2 = GetEventManager<SoloEventManager>("KerningTrain");
                    if (em2.getProperty("entry") == "true")
                    {
                        if (await SayYesNo("列车已经进站,请出示你的票，这样我就可以让你进去了。乘坐时间不会很长，你会安全到达目的地的。你觉得怎么样？想要乘坐这趟列车吗？"))
                        {
                            if (haveItem(4031711))
                            {
                                if (em2.getProperty("entry") == "false")
                                {
                                    await SayNext("我们将在列车开车前1分钟开始检票进入。进入之后请耐心等待几分钟。请注意，地铁将准时发车，我们将在那之前1分钟停止接收车票，请做好时间管理。");
                                }
                                else
                                {
                                    gainItem(4031711, -1);
                                    warp(600010004);
                                }
                            }
                        }
                    }
                    else
                    {
                        await SayNext("我们将在列车开车前1分钟开始检票进入。进入之后请耐心等待几分钟。请注意，地铁将准时发车，我们将在那之前1分钟停止接收车票，请做好时间管理。");
                        return;

                    }
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
        public Task go_pc()
        {
            // TODO
            return Task.CompletedTask;
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
                        var em = GetEventManager<SoloEventManager>("RockSpirit");
                        if (!em.startInstance(getPlayer()))
                        {
                            await SayOK("嗯...看起来前面的房间有点拥挤。请在这里等一会，好吗？");
                        }
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


        // Npc: 1061000 
        public Task refine_sleepy()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 1061009 
        public Task crack()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1061010 
        [ScriptName("3jobExit")]
        public Task s_3jobExit()
        {
            // TODO
            return Task.CompletedTask;
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
            if (await SayYesNo("前方等待着大师本人。你准备好面对他了吗？"))
            {
                if (getMap(925020010).getAllPlayers().Count > 0)
                {
                    await SayOK("有人已经在挑战大师了。请稍后再试。");
                }
                else
                {
                    getWarpMap(910510202).spawnMonsterOnGroundBelow(9300346, 95, 200);
                    warp(910510202, 0);
                }
            }
        }


        // Npc: 1072008 
        public Task inside_pirate()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 1090000 
        public Task kairinT()
        {
            // TODO
            return Task.CompletedTask;
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
                ], finalNext: true);
            if (canHold(4031847))
            {
                gainItem(4031847, 1);
                warp(912000100, 0);
            }
            else
            {
                await SayOK("我无法给你空瓶，因为你的背包已满。请在杂项窗口中腾出一些空间。");
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
        public Task s4strike()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1092090, 1092091, 1092092 
        public Task mom_cow()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1092093, 1092094, 1092095 
        public Task baby_cow()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 1095002 
        public Task enter_pirate()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1100003 
        public async Task contimoveEreEli()
        {
            if (await SayYesNo("嗯，你好...又来了。你想离开圣地去别的地方吗？如果是的话，你来对地方了。我经营着一艘渡船，从#b圣地#k到#b金银岛#k，如果你愿意的话，我可以带你去#b金银岛#k...你需要支付#b1000#k金币的费用。\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 冒险币吗？检查一下你的背包，确保你有足够的冒险币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    gainMeso(-1000);
                    warp(200090031);
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
            if (await SayYesNo("嗯...风势正好。你是不是想离开圣地去别的地方？这艘渡船开往神秘岛的天空之城。你在圣地需要办的事情都处理好了吗？如果你正好要去#b天空之城#k，我可以带你去那里。你怎么样？要去天空之城吗？\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 冒险币吗？检查一下你的背包，确保你有足够的冒险币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    gainMeso(-1000);
                    warp(200090021);
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
            if (await SayYesNo("嗯...那么...嗯...你是想离开金银岛去其他地区吗？你可以乘这艘船去#b圣地#k。在那里，你会看到明亮的阳光照在树叶上，感受到轻柔的微风拂过你的皮肤。那里是神兽和女皇所在的地方。你想去圣地吗？大约需要#b2分钟#k，费用是#b1000#k金币。\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 冒险币吗？检查一下你的背包，确保你有足够的冒险币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    gainMeso(-1000);
                    warp(200090030);
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
            if (await SayYesNo("这艘船将驶向#b圣地#k，那里是一个浮空的岛屿，你会看到明亮的阳光照在树叶上，感受到轻柔的微风拂过你的皮肤，还有女皇——希纳斯。如果你有兴趣加入皇家骑士团，那么你一定要来这里看看。你有兴趣去圣地吗？这次旅行将花费你#b1000#k金币\r\n"))
            {
                if (getMeso() < 1000)
                {
                    await SayNext("嗯... 你确定你有 #b1000#k 冒险币吗？检查一下你的背包，确保你有足够的冒险币。你必须支付费用，否则我不能让你上船...");
                }
                else
                {
                    gainMeso(-1000);
                    warp(200090020);
                }
            }
            else
            {
                await SayNext("好的。如果你改变主意了，请告诉我。");
            }
        }


        // Npc: 1101001 
        public Task createCygnus()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1101008 
        public Task helperCygnus()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 1102003 
        public Task cygnus_lv120()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 1103005 
        public Task erebWarp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104000 
        public Task DollMaster()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104002 
        public Task blackWitch()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104100 
        public Task desguiseSoul()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104101 
        public Task desguiseFlame()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104102 
        public Task desguiseWind()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104103 
        public Task desguiseNight()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104104 
        public Task desguiseStrike()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1104200 
        public Task enterBlackEreb()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1200003 
        public Task contimoveRieRit()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1200004 
        public Task contimoveRitRie()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1200005 
        public Task PurotalkRie()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1200006 
        public Task PurotalkVic()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1202000 
        public Task awake()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1202009 
        public Task enterWolf()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1202010 
        public Task aran_lv200()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1204001 
        public Task dollMaster00()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 1209000 
        public async Task talkHelena()
        {
            await SayNext("战神，你醒了！你感觉怎么样？嗯？你想知道发生了什么事情吗？");
            await SayNext("我们几乎准备好逃跑了。你不用担心。我尽可能找到的每个人都已经登上了方舟，神树也同意引领我们的道路。一旦完成剩下的准备工作，我们就会立即前往金银岛。");
            await SayNext("其他的英雄们？他们已经离开去对抗黑魔法师了。他们在为我们争取时间逃跑。什么？你想和他们一起战斗吗？不！你不能！你受伤了。你必须和我们一起离开！");
            showIntro("Effect/Direction1.img/aranTutorial/Trio");
        }


        // Npc: 1300012 
        public Task TD_MC_bossEnter()
        {
            // TODO
            return Task.CompletedTask;
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
                    await SayNext("await SayNext(\"显然这不是普通的障碍，否则我不可能过不去，也许...这应该是 #e#b#p1300003##k#n 提到的结界了。\", 3);", 3);
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
        public Task desc_tree()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2001001 
        public Task go_tree1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2001002 
        public Task go_tree2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2001003 
        public Task go_tree3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2001004 
        public Task out_tree()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2001005 
        public Task job_3th()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2002000 
        public Task go_victoria()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2010000 
        public Task carlie()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2010003 
        public Task make_orbis()
        {
            // TODO
            return Task.CompletedTask;
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
        public Task oldBook2()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 2013000 
        public Task party3_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2013001 
        public Task party3_play()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2013002 
        public Task party3_minerva()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020000 
        public Task refine_elnath()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020002 
        public Task make_elnath()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020005 
        public Task oldBook1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020008 
        public Task warrior3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020009 
        public Task wizard3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020010 
        public Task bowman3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020011 
        public Task thief3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2020013 
        public Task pirate3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2022004 
        public Task s4common1_out()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2023000 
        public Task ossyria_taxi()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030000 
        public Task goDungeon()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030006 
        public Task holyStone()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030008 
        public Task Zakum00()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030010 
        public Task Zakum06()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030011 
        public Task Zakum04()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030013 
        public Task zakum_accept()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2030014 
        public Task s4freeze_item()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2032001 
        public Task oldBook5()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2032002 
        public Task Zakum01()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2032003 
        public Task Zakum02()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040002 
        public Task ludi023()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040003 
        public Task ludi020()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040016 
        public Task make_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040019 
        public Task face_ludi2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040020 
        public Task make_ludi2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040021 
        public Task make_ludi3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040022 
        public Task make_ludi4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040024 
        public Task ludi014()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040025 
        public Task ludi015()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040026 
        public Task ludi016()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040027 
        public Task ludi017()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040028 
        public Task ludi024()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040030 
        public Task ludi026()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040031 
        public Task ludi027()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040032 
        public Task ludi028()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040033 
        public Task ludi029()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040034 
        public Task party2_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040035, 2040036, 2040037, 2040038, 2040039, 2040040, 2040041, 2040042, 2040043, 2040044, 2040045 
        public Task party2_play()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040046 
        public Task friend01()
        {
            // TODO
            return friend00();
        }


        // Npc: 2040047 
        public Task party2_out()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2040050 
        public Task make_ston()
        {
            // TODO
            return Task.CompletedTask;
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
        public Task Populatus01()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2041026 
        public Task giveupTimer()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 2043000 
        public Task s4time()
        {
            // TODO
            return Task.CompletedTask;
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
                    var em = getEventManager("3rdJob_mount") as SoloEventManager;
                    if (em == null)
                    {
                        await SayOK("抱歉，但是三转职业（骑宠）已关闭。");
                    }
                    else
                    {
                        if (em.startInstance(getPlayer()))
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


        // Npc: 2060009 
        public Task aqua_taxi()
        {
            // TODO
            return Task.CompletedTask;
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
                    await SayOK("为了打开维度裂缝，你必须拥有一块小型钢琴怪的碎片。这些可以通过击败钢琴怪来获得。");
                }
            }
            else
            {
                await SayOK("我是#b海巫卡塔#k。别和我胡闹，因为我有把人变成蠕虫的习惯。");
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


        // Npc: 2081005 
        public Task hontale_keroben()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 2081100 
        public Task warrior4()
        {
            // TODO
            return Task.CompletedTask;
        }





        // Npc: 2081300 
        public Task archer4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2081400 
        public Task thief4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2081500 
        public Task pirate4()
        {
            // TODO
            return Task.CompletedTask;
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
            await SayOK("嗨，我是安迪，来自一个并不那么遥远的未来的时间旅行者。我来阻止这个时代贪婪的人们制造机器。他们在我的时代变得疯狂，把一切都消耗成了灰尘。我必须不惜一切代价阻止它！");
        }


        // Npc: 2083000 
        public Task hontale_enterToE()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2083001 
        public Task hontale_enter1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2083002 
        public Task hontale_out()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 2083004 
        public Task hontale_accept()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2083005 
        public Task s4holycharge()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2083006 
        public Task TD_neoCity_enter()
        {
            // TODO
            return Task.CompletedTask;
        }






        // Npc: 2090004 
        public Task make_murueng()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2090005 
        public Task crane()
        {
            // TODO
            return Task.CompletedTask;
        }


      


        // Npc: 2091009 
        public Task enterShadow()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2093004 
        public Task aqua_taxi2()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 2095000 
        public Task s4mind()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2096000 
        public Task sca_dollBear()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2100001 
        public Task make_ariant1()
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
            var toMagatia = "Would you like to take the #bCamel Cab#k to #bMagatia#k, the town of Alchemy? The fare is #b1500 mesos#k.";
            var toAriant = "Would you like to take the #bCamel Cab#k to #bAriant#k, the town of Burning Roads? The fare is #b1500 mesos#k.";

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
        public Task jenu_homun()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111003 
        public Task snow_rose()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111006 
        public Task drang_room1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111010 
        public Task magatia_dark1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111011 
        public Task absence_wall()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111012 
        public Task absence_box()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111013 
        public Task absence_frame()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111014 
        public Task absence_desk()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111015 
        public Task alcadno_potion()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111017 
        public Task pipe1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111018 
        public Task pipe2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111019 
        public Task pipe3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111020 
        public Task alceCircle1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111021 
        public Task alceCircle2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111022 
        public Task alceCircle3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2111023 
        public Task alceCircle4()
        {
            // TODO
            return Task.CompletedTask;
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


        // Npc: 2112000 
        public Task yurete_mad()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112001 
        public Task yurete_dead()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 2112003 
        public Task juliet_start()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112004 
        public Task romio_start()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112005 
        public Task juliet()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112006 
        public Task romio()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112007 
        public Task rnj_look()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112008 
        public Task juliet_dead()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112009 
        public Task romio_dead()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112010 
        public Task yurete2_mad()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112011 
        public async Task yurete2_dead()
        {
            await SayOption(
                "被打败了...这就是Yulete的遗产将如何结束的方式，哦，这是多么的悲哀...希望你们现在很开心，因为我将度过余生在一个黑暗的地窖里。我所做的一切都是为了马加提亚的利益！！（哭泣）",
                ["嘿，伙计，振作点！这里没有太多无法解决的损害。马加提亚制定了这些严厉的法律，是为了保护它的人民免受像这样的强大力量落入错误的手中所带来的危害。这并不是你的终结，接受社会的康复，一切都会好起来的！"]
                );
            await SayNext("…在我所做的一切之后，你们原谅我了吗？嗯，我想我被那种可以通过这种方式发现的巨大力量冲昏了头脑，也许他们说得对，人类不能简单地理解并运用这些力量，而不在途中腐化自己…我深感抱歉，为了弥补自己对每个人，我愿意在炼金术的进展中再次帮助各个组织。谢谢。");
            if (!isQuestCompleted(7770))
            {
                completeQuest(7770);
            }
            warp(926110600, 0);
        }



        // Npc: 2112013 
        public Task jnr_look()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2112016 
        public Task q3367npc()
        {
            // TODO
            return Task.CompletedTask;
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



        // Npc: 2141000 
        public Task PinkBeen_Summon()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2141001 
        public Task PinkBeen_accept()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 2141002 
        public Task PinkBeen_Out()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000000, 9000001, 9000011, 9000013 
        public Task Event00()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000002 
        public Task Event02()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000003, 9000004, 9000005, 9000006 
        public Task Event03()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000007 
        public Task Event04()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000008 
        public Task Event05()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000009 
        public Task Event03_1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000010 
        public Task Event06()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000012 
        public Task Event09()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9000020 
        public Task world_trip()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000021 
        public async Task getRank()
        {
            await SayNext("嘿，旅行者！我是#p9000021#，我的工作是招募像你这样渴望每天迎接新挑战的旅行者。现在，我的团队正在举办比赛，充分测试像你这样的冒险者的心理和身体能力。");
            await SayNext("这些比赛涉及#b连续的boss战#k，其中一些部分之间有一些休息点。这将需要一些策略时间和足够的物资在手，因为它们不是普通的战斗。");
            if (await SayAcceptDecline("如果你觉得自己足够强大，你可以像其他人一样在我们举办权力竞赛的地方加入。...那么，你的决定是什么？您现在进入举行比赛的地方吗？"))
            {
                await SayOK("非常好。记住，在那里你可以组建一个团队，或者独自进行战斗，这取决于你。祝你好运！");
                getPlayer().saveLocation("BOSSPQ");
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



        // Npc: 9010001, 9010002, 9010003 
        public Task Event07()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9010004 
        public Task ludiEvent()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9010021 
        public Task RyuhoRank()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9010022 
        public Task unityPortal()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9020000 
        public Task party1_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9020001 
        public Task party1_play()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9020002 
        public Task party1_out()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040000 
        public Task guildquest1_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040001 
        public Task guildquest1_clear()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040002 
        public Task guildquest1_comment()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040003 
        public Task guildquest1_NPC1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040005 
        public Task guildquest1_out()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040006 
        public Task guildquest1_baseball()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040007 
        public Task guildquest1_will()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040009 
        public Task guildquest1_statue()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040010 
        public Task guildquest1_bonus()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040011 
        public Task guildquest1_board()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9040012 
        public Task guildquest1_knight()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050000, 9100107, 9110017, 9310023 
        public Task gachapon8()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050001, 9100108, 9100111, 9310024 
        public Task gachapon9()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050002, 9100109, 9310025 
        public Task gachapon10()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050003, 9100110, 9310026 
        public Task gachapon11()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050004, 9310027 
        public Task gachapon12()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050005, 9100112, 9270043, 9310028 
        public Task gachapon13()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050006, 9310029 
        public Task gachapon14()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050007, 9310061 
        public Task gachapon15()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050009 
        public Task pigmy_guide()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9050010 
        public Task gachapon16()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9060000 
        public Task tamepig_out()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9100100 
        public Task gachapon1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100101 
        public Task gachapon2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100102, 9110011 
        public Task gachapon3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100103, 9110012 
        public Task gachapon4()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100104, 9110013 
        public Task gachapon5()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100105, 9110014 
        public Task gachapon6()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100106, 9110016 
        public Task gachapon7()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9100117, 9310092 
        public Task gachapon18()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9101001 
        public Task begin_jp2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9102100 
        public Task multipet_success()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9102101 
        public Task multipet_fail()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9103000 
        public Task party_ludimaze_goal()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9103001 
        public Task party_ludimaze_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9103002 
        public Task party_ludimaze_success()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9103003 
        public Task party_ludimaze_fail()
        {
            // TODO
            return Task.CompletedTask;
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
        public Task in_bath()
        {
            // TODO
            return Task.CompletedTask;
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
        public Task s_To_the_Showa_manor()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9120100 
        public Task hair_shouwa1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120101 
        public Task hair_shouwa2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120102 
        public Task face_shouwa1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9120103 
        public Task face_shouwa2()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9120200 
        public Task con2()
        {
            // TODO
            return Task.CompletedTask;
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
        public Task con4()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9200100 
        public Task lens_henesys1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9200101 
        public Task lens_orbis1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9200102 
        public Task lens_ludi1()
        {
            // TODO
            return Task.CompletedTask;
        }





        // Npc: 9201015 
        public Task hair_wedding1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201016 
        public Task hair_wedding2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201017 
        public Task lens_wedding1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201018 
        public Task face_wedding1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201019 
        public Task face_wedding2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201021 
        public Task weddingParty()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201022 
        public Task Thomas()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201023 
        public Task ProofKern()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201024 
        public Task ProofElli()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201025 
        public Task ProofOrbi()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201026 
        public Task ProofLudi()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201027 
        public Task ProofPeri()
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


        // Npc: 9201039 
        public Task hair_wedding3()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9201042 
        public Task TickShop()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201043 
        public Task PartyAmoria_enter()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201044 
        public Task PartyAmoria_play()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201045 
        public Task PartyAmoria_play3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201046 
        public Task PartyAmoria_playBo()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201047 
        public Task PartyAmoria_play2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201048 
        public Task PartyAmoria_enter2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201049 
        public Task ExitWedding()
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
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201052 
        public Task refine_TCG1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201054 
        public Task Lost_Trans1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201056, 9310054 
        public Task NLC_Taxi()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201057 
        public Task NLC_ticketing()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201061 
        public Task NLC_LensExp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201062 
        public Task NLC_LensVip()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201063 
        public Task NLC_HairExp()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201064 
        public Task NLC_HairVip()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201065 
        public Task NLC_Skin()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201068 
        public Task NLC_Move()
        {
            // TODO
            return Task.CompletedTask;
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
        public Task Sunstone()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201072 
        public Task Moonstone()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201073 
        public Task Tombstone()
        {
            // TODO
            return Task.CompletedTask;
        }




        // Npc: 9201082 
        public Task naomi1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201083 
        public Task Lost_Trans2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201084 
        public Task Tomb_Hall()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9201093 
        public Task suzy_lost()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201094 
        public Task TCG3()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201095 
        public Task Gear_Upgrade()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201096 
        [ScriptName("Jack_Additional ")]
        public Task s_Jack_Additional()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201097 
        public Task Badge_Bounty()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201098 
        public Task Brewing_Storm()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201099 
        public Task MoStore()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201100 
        public Task Fallen_Woods()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201101 
        public Task tcg4_7()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201102 
        public Task tcg4_8()
        {
            // TODO
            return Task.CompletedTask;
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
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201115 
        public Task glpqStory2()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9201123 
        public Task goPerion()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201124 
        public Task goHenesys()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201125 
        public Task goElinia()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201126 
        public Task goKerningCity()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201127 
        public Task goNautilus()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201128 
        public Task Enter_Darkportal_W()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201129 
        public Task Enter_Darkportal_M()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201130 
        public Task Enter_Darkportal_T()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201131 
        public Task Enter_Darkportal_H()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9201132 
        public Task Enter_Darkportal_P()
        {
            // TODO
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
            // TODO
            return Task.CompletedTask;
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
        public Task Jump_rudolph()
        {
            // TODO
            return Task.CompletedTask;
        }



        // Npc: 9220018 
        public Task guyfawkes_ch()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9220019 
        public Task guyfawkes_milla2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9220020 
        public Task guyfawkes_ch2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270017 
        public Task goback_kerning()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270018 
        public Task goback_cbd()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270023 
        public Task face_sg2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270024 
        public Task face_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270025 
        public Task skin_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270026 
        public Task lens_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270033 
        public Task captinsg01()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270036 
        public Task hair_sg1()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270037 
        public Task hair_sg2()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270038 
        public Task sellticket_cbd()
        {
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9270041 
        public Task sellticket_sg()
        {
            // TODO
            return Task.CompletedTask;
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
            if (await SayYesNo("嘿！我是#b#e驾驶员 洪#n#k，我负责驾驶飞往上海的飞机。\r\n经过长年的飞行，我的驾驶技术已经很了不得。\r\n有兴趣跟我一起前往美丽的#b#e上海外滩#k#n吗？\r\n只需要#r2000金币#k哦！"))
            {
                if (getMeso() < 2000)
                {
                    await SayNext("你确定你有 #b2000 金币#k？ 如果没有，我可不能免费送你去。");
                }
                else
                {
                    gainMeso(-2000);
                    warp(701000000);
                }
            }
        }

        // Npc: 9310013 
        public async Task goshanghai2()
        {
            if (await SayYesNo("嘿！我是#b#e驾驶员 洪#n#k，我负责驾驶飞往#b金银岛#k的飞机。\r\n经过长年的飞行，我的驾驶技术已经很了不得。\r\n有兴趣跟我一起前往古朴的#b#e勇士部落#k#n吗？\r\n只需要#r2000金币#k哦！"))
            {
                if (getMeso() < 2000)
                {
                    await SayNext("你确定你有 #b2000 金币#k？ 如果没有，我可不能免费送你去。");
                }
                else
                {
                    gainMeso(-2000);
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

    }
}