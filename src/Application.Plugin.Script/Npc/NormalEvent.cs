using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9000000, 9000001, 9000011, 9000013 
        public async Task Event00()
        {
            var npcId = getNpc();
            if (npcId == 9000011)
            {
                await Event0011();
                return;
            }

            await SayNext("嘿，我是#b" + (npcId == 9000000 ? "保罗" : npcId == 9000001 ? "让" : "托尼") + "#k，如果你不忙的话……那我能和你一起玩吗？我听说这附近有人聚集起来参加一个#r活动#k，但我不想一个人去……嗯，你想和我一起去看看吗？");
            var sel = await AskMenu("哦？是什么样的活动？嗯，那个是……\r\n#L0##e1.#n#b 是什么样的活动？#k#l\r\n#L1##e2.#n#b 给我解释一下活动游戏吧。#k#l\r\n#L2##e3.#n#b 好的，让我们开始吧！#k#l");
            if (sel == 0)
            {
                await SayOK("这个月，冒险岛全球版正在庆祝其三周年！GM们将在整个活动期间举行惊喜GM活动，所以保持警惕，并确保参加至少一个活动以赢取丰厚奖品！");
            }
            else if (sel == 1)
            {
                var game = await AskMenu("这个活动有很多游戏。在玩游戏之前了解如何玩游戏会对你有很大帮助。选择你想了解更多的游戏！#b\r\n#L0# 欧拉欧拉#l\r\n#L1# 冒险岛体能测试#l\r\n#L2# 雪球#l\r\n#L3# 椰子收获#l\r\n#L4# OX问答#l\r\n#L5# 寻宝#l#k");
                var gameDesc = game switch
                {
                    0 => "#b[Ola Ola]#k 是一个游戏，参与者需要爬梯子到达顶部。通过选择正确的传送门，爬上去并移动到下一个级别。\r\n\r\n游戏包括三个级别，时间限制为 #b6 分钟#k。在 [Ola Ola] 中，你 #b无法跳跃、传送、加速，或使用药水或物品提高速度#k。还有一些欺诈性的传送门会把你带到奇怪的地方，所以请注意。",
                    1 => "#b[冒险岛体能测试]是一个类似于耐心之森的障碍赛跑#k。你可以通过克服各种障碍，在规定时间内到达最终目的地来赢得比赛。\r\n游戏包括四个关卡，时间限制为#b15分钟#k。在[冒险岛体能测试]期间，你将无法使用传送或加速技能。",
                    2 => "#b[雪球]#k 由两个队伍组成，枫叶队和故事队，两个队伍在有限的时间内争夺看哪个队伍将雪球滚得更远更大。如果比赛在规定时间内无法决定胜负，那么滚得更远的队伍获胜。\r\n要滚动雪球，按下#bCtrl#k进行攻击。所有远程攻击和技能攻击在这里都不起作用，#b只有近距离攻击才有效#k。\r\n如果角色触碰到雪球，他/她将被送回起点。攻击起点前面的雪人，以阻止对方队伍将雪球滚向前方。这是一个精心策划的战略，因为队伍将决定是攻击雪球还是雪人。",
                    3 => "#b[椰子收获]#k 由两个队伍组成，枫叶队和故事队，两个队伍将争夺看谁能收集到最多的椰子。时间限制为#b5分钟#k。如果比赛以平局结束，将额外奖励2分钟以确定胜者。如果由于某种原因比分保持平局，比赛将以平局结束。\r\n所有远程攻击和技能攻击在这里都不起作用，#b只有近距离攻击才有效#k。如果你没有近距离攻击的武器，你可以通过活动地图内的NPC购买。无论角色的等级、武器或技能如何，所有造成的伤害都是相同的。\r\n注意地图内的障碍和陷阱。如果角色在游戏中死亡，将被淘汰出局。最后一击椰子掉落之前的玩家获胜。只有掉落在地面上的椰子才计数，这意味着没有掉落的树上的椰子，或者偶尔爆炸的椰子都不计数。地图底部的贝壳中有一个隐藏的传送门，所以明智地使用它！",
                    4 => "#b[OX Quiz]#k 是冒险岛中通过X和O来展示智慧的游戏。一旦你加入游戏，按下 #bM#k 打开小地图，看看X和O的位置。一共会有 #r10个问题#k，回答所有问题正确的角色将赢得游戏。\r\n问题给出后，使用梯子进入可能包含正确答案的区域，无论是X还是O。如果角色没有选择答案或者在时间限制内仍然挂在梯子上，角色将被淘汰。在屏幕上的 [CORRECT] 消失之前，请保持你的位置。为了防止任何形式的作弊，OX Quiz期间所有聊天功能将被关闭。",
                    5 => "#b[寻宝]#k 是一个游戏，你的目标是在地图上的 #rin 10分钟#k 内找到隐藏的 #b宝藏卷轴#k。地图上隐藏着许多神秘的宝箱，一旦打开它们，会有许多物品从宝箱中出现。你的任务是从这些物品中挑选出宝藏卷轴。\r\n宝箱可以用 #b普通攻击#k 打开，一旦你拥有了宝藏卷轴，你可以通过负责交易物品的NPC将其交换成《秘密卷轴》。交易NPC可以在寻宝地图上找到，但你也可以通过立石镇的 #bVikin#k 进行交易。\r\n\r\n这个游戏中有许多隐藏的传送门和隐藏的传送点。要使用它们，只需在特定位置按下 #b上箭头#k，你就会被传送到另一个地方。试着跳来跳去，也许你会碰到隐藏的楼梯或绳索。还会有一个能带你到隐藏地点的宝箱，以及一个只能通过隐藏传送门找到的隐藏宝箱，所以试着四处寻找。\r\n\r\n在寻宝游戏中，所有攻击技能都将被 #r禁用#k，请使用普通攻击打开宝箱。",
                    _ => ""
                };
                await SayOK(gameDesc);
            }
            else if (sel == 2)
            {
                if (npcId == 9000001)
                {
                    var evt = getEvent();
                    if (evt != null && evt.getLimit() > 0)
                    {
                        getPlayer().saveLocation("EVENT");
                        if (evt.getMapId() == 109080000 || evt.getMapId() == 109060001)
                        {
                            divideTeams();
                        }
                        evt.minusLimit();
                        await warp(evt.getMapId(), 0);
                    }
                    else
                    {
                        await SayOK("要么活动还没有开始，你已经拥有了#b秘密卷轴#k，或者你在过去24小时内已经参与了这个活动。请稍后再试！");
                    }
                }
                else
                {
                    await SayOK("要么活动还没有开始，你已经拥有了#b秘密卷轴#k，或者你在过去24小时内已经参与了这个活动。请稍后再试！");
                }
            }
        }

        private async Task Event0011()
        {
            await SayNext("嘿，我是#b#p9000011##k，如果你不忙的话……那我能跟你一起玩吗？我听说这附近有人聚集起来举办一个#revent#k，但我不想一个人去……嗯，你想和我一起去看看吗？");
            var sel = await AskMenu("哦？是什么样的活动？那就是...\r\n#L0##e1.#n#b 这是什么样的活动？#k#l\r\n#L1##e2.#n#b 给我解释一下活动游戏吧。#k#l\r\n#L2##e3.#n#b 好的，我们走吧！#k#l\r\n#L3##e4.#n#b 请用胜利证书兑换奖励物品。#k#l");
            if (sel == 0)
            {
                await SayOK("这个月，冒险岛全球版正在庆祝其三周年！GM们将在整个活动期间举行惊喜GM活动，所以保持警惕，并确保参加至少一个活动以赢取丰厚奖品！");
            }
            else if (sel == 1)
            {
                var game = await AskMenu("这个活动有很多游戏。在玩游戏之前了解如何玩会对你有很大帮助。选择你想了解更多的游戏吧！#b\r\n#L0# 欧拉欧拉#l\r\n#L1# 冒险岛体能测试#l\r\n#L2# 雪球#l\r\n#L3# 椰子收获#l\r\n#L4# OX问答#l\r\n#L5# 寻宝#l#k");
                var gameDesc = game switch
                {
                    0 => "#b[Ola Ola]#k 是一个游戏，参与者需要爬梯子到达顶部。通过选择正确的传送门，爬上去并移动到下一个级别。\r\n\r\n游戏包括三个级别，时间限制为#b6分钟#k。在[Ola Ola]期间，你#b无法跳跃、传送、加速，或使用药水或物品提高速度#k。还有一些欺诈性的传送门会把你带到一个奇怪的地方，所以请注意。",
                    1 => "#b[冒险岛体能测试]是一个类似于耐心之森的障碍赛跑#k。你可以通过克服各种障碍，在规定时间内到达最终目的地来赢得比赛。\r\n游戏包括四个关卡，时间限制为#b15分钟#k。在[冒险岛体能测试]期间，你将无法使用传送或加速技能。",
                    2 => "#b[雪球]#k 由两个队伍组成，枫叶队和故事队，两个队伍在有限的时间内争夺看哪个队伍将雪球滚得更远更大。如果比赛在规定时间内无法决定胜负，那么滚得更远的队伍获胜。\r\n要滚动雪球，按下#bCtrl#k进行攻击。所有远程攻击和技能攻击在这里都不起作用，#b只有近距离攻击才有效#k。\r\n如果角色触碰到雪球，他/她将被送回起点。攻击起点前面的雪人，以阻止对方队伍将雪球滚向前方。这是一个精心策划的战略，因为队伍将决定是攻击雪球还是雪人。",
                    3 => "#b[椰子收获]#k 由两个队伍组成，枫叶队和故事队，两个队伍将争夺看谁能收集到最多的椰子。时间限制为#b5分钟#k。如果比赛打成平局，将额外奖励2分钟来决定胜者。如果由于某种原因比分保持平局，比赛将以平局结束。\r\n所有远程攻击和技能攻击在这里都不起作用，#b只有近身攻击才有效#k。如果你没有近身攻击的武器，你可以通过活动地图内的NPC购买。无论角色的等级、武器或技能如何，所有造成的伤害都是相同的。\r\n小心地图内的障碍和陷阱。如果角色在游戏中死亡，角色将被淘汰。最后一击椰子掉落之前的玩家获胜。只有掉落在地面上的椰子才计数，这意味着没有掉落的树上的椰子，或者偶尔的椰子爆炸都不计数。地图底部的贝壳中有一个隐藏的传送门，所以明智地使用它！",
                    4 => "#b[OX Quiz]#k 是冒险岛中通过X和O来展示智慧的游戏。一旦你加入游戏，按下 #bM#k 打开小地图，看看X和O的位置。一共会有 #r10个问题#k，回答所有问题正确的角色将赢得游戏。\r\n\r\n问题给出后，使用梯子进入可能包含正确答案的区域，无论是X还是O。如果角色没有选择答案或者在时间限制内挂在梯子上，角色将被淘汰。请等到屏幕上的 [CORRECT] 消失后再继续前进。为了防止任何形式的作弊，OX Quiz期间所有聊天功能将被关闭。",
                    5 => "#b[寻宝]#k 是一个游戏，你的目标是在地图上的各个角落找到 #b宝藏卷轴#k，在 #r10分钟#k 内。会有许多神秘的宝箱隐藏起来，一旦你打开它们，会有许多物品从宝箱里浮出来。你的任务是从这些物品中挑选出宝藏卷轴。\r\n宝箱可以用 #b普通攻击#k 打开，一旦你拿到了宝藏卷轴，你可以通过负责交易物品的NPC将其交换成秘密卷轴。交易NPC可以在寻宝地图上找到，但你也可以通过立石港的 #bVikin#k 进行交易。\r\n\r\n这个游戏有许多隐藏的传送门和隐藏的传送点。要使用它们，只需在特定位置按下 #b上箭头#k，你就会被传送到另一个地方。试着跳来跳去，也许你会碰到隐藏的楼梯或绳索。还会有一个能带你到隐藏地点的宝箱，以及一个只能通过隐藏传送门找到的隐藏宝箱，所以试着四处寻找。\r\n\r\n在寻宝游戏中，所有攻击技能都将被 #r禁用#k，请使用普通攻击打开宝箱。",
                    _ => ""
                };
                await SayOK(gameDesc);
            }
            else if (sel == 2)
            {
                var marr = getQuestRecord(100295);
                if (marr.getCustomData() == null)
                {
                    marr.setCustomData("0");
                }
                var dat = int.Parse(marr.getCustomData()!);
                if (dat + 3600000 >= getCurrentTime())
                {
                    await SayNext("你在过去的一个小时内已经参加了这个活动。");
                }
                else if (!canHold(4031019))
                {
                    await SayNext("在你的背包里留点空间。");
                }
                else
                {
                    var evt = getEvent();
                    if (evt != null && !haveItem(4031019))
                    {
                        getPlayer().saveLocation("EVENT");
                        getPlayer().setChalkboard(null);
                        marr.setCustomData("" + getCurrentTime());
                        if (evt.getMapId() == 109080000 || evt.getMapId() == 109080010)
                            await warp(evt.getMapId(), 0);
                        else
                            await warp(evt.getMapId(), "join00");
                    }
                    else
                    {
                        await SayNext("要么活动还没有开始，你已经拥有了#b秘密卷轴#k，或者你在过去24小时内已经参与了这个活动。请稍后再试！");
                    }
                }
            }
            else if (sel == 3)
            {
                var quantities = new[] { 10, 8, 6, 5, 4, 3, 2, 1, 1, 1 };
                var prizes = new[]
                {
                    new[] { 1442047, 2000000, 2000001, 2000002, 2000003, 2000004, 2000005, 2430036, 2430037, 2430038, 2430039, 2430040 },
                    new[] { 1442047, 4080100, 4080001, 4080002, 4080003, 4080004, 4080005, 4080006, 4080007, 4080008, 4080009, 4080010, 4080011 },
                    new[] { 1442047, 1442048, 2022070 },
                    new[] { 1442048, 2430082, 2430072 },
                    new[] { 1442048, 2430091, 2430092, 2430093, 2430101, 2430102 },
                    new[] { 1442048, 1442050, 2430073, 2430074, 2430075, 2430076, 2430077 },
                    new[] { 1442050, 3010183, 3010182, 3010053, 2430080 },
                    new[] { 1442050, 3010178, 3010177, 3010075, 1442049, 2430053, 2430054, 2430055, 2430056, 2430103, 2430136 },
                    new[] { 1442049, 3010123, 3010175, 3010170, 3010172, 3010173, 2430201, 2430228, 2430229 },
                    new[] { 1442049, 3010172, 3010171, 3010169, 3010168, 3010161, 2430117, 2430118, 2430119, 2430120, 2430137 }
                };

                var selStr = "Which Certificate of straight Win do you wish to exchange?";
                for (var i = 0; i < quantities.Length; i++)
                {
                    selStr += $"\r\n#b#L{i}# #t{4031332 + i}# Exchange({quantities[i]})#l";
                }

                var certSel = await AskMenu(selStr);
                if (certSel >= 0 && certSel < quantities.Length)
                {
                    var ite = 4031332 + certSel;
                    var quan = quantities[certSel];
                    if (!haveItem(ite, quan))
                    {
                        await SayOK($"你需要 #b{quan} #t{ite}##k 与物品交换。");
                    }
                    else if (getInventory(1).getNextFreeSlot() <= -1 || getInventory(2).getNextFreeSlot() <= -1 || getInventory(3).getNextFreeSlot() <= -1 || getInventory(4).getNextFreeSlot() <= -1)
                    {
                        await SayOK("你需要为这个物品腾出空间。");
                    }
                    else
                    {
                        var pri = prizes[certSel];
                        var rand = Random.Shared.Next(pri.Length);
                        await gainItem(pri[rand], 1);
                        await gainItem(ite, -quan);
                        await gainMeso(100000 * certSel);
                    }
                }
            }
        }


        // Npc: 9000002 
        public async Task Event02()
        {
            await SayNext("轰隆隆隆！！你已经从#b活动#k中赢得了游戏。恭喜你走到这一步！");
            await SayNext("你将获得#b#t4031019##k作为胜利奖励。卷轴上写有古代文字的秘密信息。");
            await SayNext("#t4031019#可以由鲁德斯里姆的#r#p9000007##k或#r#p9000014##k解读。带上它，一定会发生好事。");
            if (canHold(4031019))
            {
                await gainItem(4031019);
                await warp(getPlayer().getSavedLocation("EVENT"));
            }
            else
            {
                await SayNext(GetClientMessage(nameof(ClientMessage.SlotFull), nameof(ClientMessage.ETC)));
            }
        }


        // Npc: 9000003, 9000004, 9000005, 9000006 
        public Task Event03()
        {
            // Only 9000004 has JS — contains complex party/boss stage logic
            // TODO
            return Task.CompletedTask;
        }


        // Npc: 9000007 
        public async Task Event04()
        {
            await SayOK("只是个孩子...别跟我说话...");
        }


        // Npc: 9000008 
        public async Task Event05()
        {
            await SayOK("如果你有一个想要打开的锁定的盒子，把它带给我。");
        }


        // Npc: 9000009 
        public async Task Event03_1()
        {
            await SayOK("嘿嘿!!! 找到宝藏卷轴！我把地图丢在某个地方了，没有地图我就不能离开这里。");
        }


        // Npc: 9000010 
        public async Task Event06()
        {
            await SayNext("很抱歉，但恐怕你没有赢得这个活动。以后再试试吧。你可以通过我回到原来的地方。");
            await warp(getPlayer().getSavedLocation("EVENT"));
        }


        // Npc: 9010001, 9010002, 9010003 
        public async Task Event07()
        {
            await SayOK($"嗨，我是 #p{getNpc()}#。");
        }


        // Npc: 9000012 
        public async Task Event09()
        {
            var sel = await AskMenu("哥们...太热了！！！~ 我能帮你什么吗？\r\n#L0##b离开活动游戏。#l\r\n#L1#购买武器（木棍 1 枚）");
            if (sel == 0)
            {
                if (await AskYesNo("如果你现在离开，接下来的24小时内你将无法参与这个活动。你确定要离开吗？"))
                {
                    var evt = getEvent();
                    if (evt != null)
                    {
                        evt.addLimit();
                    }
                    await warp(109050001, 0);
                }
            }
            else
            {
                if (getMeso() < 1 && !canHold(1322005))
                {
                    await SayOK("你的金币不够，或者你的背包没有空间。");
                }
                else
                {
                    await gainItem(1322005);
                    await gainMeso(-1);
                }
            }
        }
    }
}
