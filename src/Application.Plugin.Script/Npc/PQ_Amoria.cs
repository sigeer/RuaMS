using Application.Core.scripting.Events.Instances;
using server.maps;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9201042 — Mr. Sandman (Tick Shop)
        public async Task TickShop()
        {
            var wishPrizes = new[] { 2000000, 2010004, 2020011, 2000004, 2000006, 2022015, 2000005, 1082174, 1002579, 1032039, 1002578, 1002580, 1002577, 1102078 };
            var wishPrizesQty = new[] { 10, 10, 5, 5, 5, 5, 10, 1, 1, 1, 1, 1, 1, 1 };
            var wishPrizesCst = new[] { 10, 15, 20, 30, 30, 50, 100, 400, 450, 500, 500, 530, 550, 600 };

            var slctTicket = getTierTicket(getPlayer().getLevel());
            var amntTicket = getItemQuantity(slctTicket);

            await SayNext("嗨，你好吗？既然你路过阿莫利亚，你听说过我哥哥阿莫斯主持的副本吗？这就是 #b阿莫利亚挑战#k，一个供40级以上玩家参与的副本。\r\n\r\n在那里，你可以找到 #i4031543# #i4031544# #i4031545# #b祝愿券#k，可以带到这里兑换奖品。");

            var listStr = "";
            for (var i = 0; i < wishPrizes.Length; i++)
            {
                listStr += $"#b#L{i}#{wishPrizesQty[i]} #z{wishPrizes[i]}##k";
                listStr += $" - {wishPrizesCst[i]} wish tickets";
                listStr += "#l\r\n";
            }

            var sel = await AskMenu($"您目前拥有#b{amntTicket} #i{slctTicket}# #t{slctTicket}##k。\r\n\r\n购买奖品：\r\n\r\n{listStr}");

            if (amntTicket < wishPrizesCst[sel])
            {
                await SayOK($"You will need #b{wishPrizesCst[sel]} #t{slctTicket}##k to purchase that! If you want this, come back another time when you have all the tickets at hand.");
                return;
            }

            if (!await AskYesNo($"您已选择#b{wishPrizesQty[sel]} #z{wishPrizes[sel]}##k，这将需要#b{wishPrizesCst[sel]} #t{slctTicket}##k。您要购买吗？"))
                return;

            if (canHold(wishPrizes[sel], wishPrizesQty[sel]))
            {
                await gainItem(wishPrizes[sel], wishPrizesQty[sel]);
                await gainItem(slctTicket, -wishPrizesCst[sel]);
                await SayOK("祝你一天愉快！");
            }
            else
            {
                await SayOK("领取物品前，请确保您的背包有足够的空位。");
            }
        }

        private static int getTierTicket(int level)
        {
            if (level < 50) return 4031543;
            if (level < 120) return 4031544;
            return 4031545;
        }


        // Npc: 9201043 — Amos the Strong (Entrance)
        public async Task PartyAmoria_enter()
        {
            var sel = await AskMenu("我的名字是强壮的阿莫斯。你想做什么？\r\n#b#L0#参加阿莫利亚挑战！#l\r\n#L1#用10把钥匙交换门票！#l#k");

            if (sel == 0)
            {
                if (!haveItem(4031592))
                {
                    await SayOK("您必须有入场券才能进入。");
                    return;
                }

                if (!await AskYesNo("所以你想进入 #b入口#k？"))
                    return;

                await warp(670010100, 0);
                await gainItem(4031592, -1);
            }
            else if (sel == 1)
            {
                if (haveItem(4031592))
                {
                    await SayOK("你已经有一张入场券了！");
                    return;
                }

                if (!haveItem(4031593, 10))
                {
                    await SayOK("请先给我拿到10把钥匙！");
                    return;
                }

                if (!await AskYesNo("所以你想要一张门票？"))
                    return;

                await gainItem(4031593, -10);
                await gainItem(4031592, 1);
            }
        }


        // Npc: 9201044 — Amos (PQ Host, stage 1-3)
        // Combo guessing puzzle: players stand on platforms to guess a combination
        public async Task PartyAmoria_play()
        {
            var curMap = getMapId();
            var stage = (curMap - 670010200) / 100 + 1;

            var eim = getEventInstance();
            if (eim == null) return;

            if (eim.getProperty(stage + "stageclear") != null)
            {
                await SayNext("传送门已经打开，前进去迎接等待你的考验。");
                return;
            }

            if (!eim.isEventLeader(getPlayer()))
            {
                await SayNext("请告诉你的#b队伍领袖#k来找我谈话。");
                return;
            }

            var state = eim.getIntProperty("statusStg" + stage);

            if (state == -1)
            {
                if (stage == 1)
                    await SayOK("嗨。欢迎来到阿莫利亚挑战的#b舞台#k。在这个阶段，与#p9201047#交谈，他会向你传达任务的进一步细节。在打碎下面的魔镜后，将碎片交给#p9201047#，然后来这里获得进入下一个阶段的权限。");
                else if (stage == 2)
                    await SayOK("嗨。欢迎来到阿莫利亚挑战的#b舞台#k。在这个阶段，让你的5名队员以某种方式爬上平台，尝试组合解锁通往下一级的传送门。当你感觉准备好了，和我交谈，我会告诉你情况。然而，请做好准备，如果传送门在几次尝试后没有解锁，怪物将会生成。");
                else if (stage == 3)
                    await SayOK("嗨。欢迎来到阿莫利亚挑战的#b舞台#k。在这个阶段，让你的5名队员分别爬上平台，尝试组合以解锁通往下一级的传送门。当你准备好时，和我交谈，我会告诉你情况。提示：失败时，数一下场景中出现的史莱姆数量，这将告诉你有多少人的位置是正确的。");

                eim.setProperty("statusStg" + stage, 0);
                return;
            }

            if (state == 2)
            {
                eim.setProperty("statusStg" + stage, 1);
                await clearStageAPQ(stage, eim, curMap);
                return;
            }

            var map = getMap();
            if (stage == 1)
            {
                if (eim.getIntProperty("statusStg" + stage) == 1)
                    await clearStageAPQ(stage, eim, curMap);
                else
                    await SayOK("与#p9201047#交谈，了解更多关于这个阶段的信息。");
            }
            else if (stage == 2 || stage == 3)
            {
                if (map.countMonsters() > 0)
                {
                    await SayNext("在尝试组合之前先击败所有的怪物。");
                    return;
                }

                var objset = new int[9];
                var playersOnCombo = 0;
                var party = eim.getPlayers();
                for (var i = 0; i < party.Count; i++)
                {
                    for (var y = 0; y < map.getAreas().Count; y++)
                    {
                        if (map.getArea(y).Contains(party[i].getPosition()))
                        {
                            playersOnCombo++;
                            objset[y] += 1;
                            break;
                        }
                    }
                }

                if (playersOnCombo != 5)
                {
                    if (stage == 2)
                        await SayNext("看起来你们还没有找到这个试炼的方法。考虑一下在平台上安排5名成员。记住，只允许有5人站在平台上，如果你移动了，可能就不算作答案了，所以请记住这一点。继续努力！");
                    else
                        await SayNext("看起来你们还没有找到这个试炼的方法。考虑一下在不同平台上安排队伍成员的方式。记住，只允许有5个人站在平台上，如果你移动了，可能就不算作答案了，所以请记住这一点。继续努力！");
                    return;
                }

                var comboStr = eim.getProperty("stage" + stage + "combo");
                if (string.IsNullOrEmpty(comboStr))
                {
                    comboStr = stage == 2 ? generateCombo1() : generateCombo2();
                    eim.setProperty("stage" + stage + "combo", comboStr);
                }

                var combo = comboStr.Split(',');
                var correctCombo = true;
                var guessedRight = objset.Length;
                var playersRight = 0;

                for (var i = 0; i < objset.Length; i++)
                {
                    if (int.Parse(combo[i]) != objset[i])
                    {
                        correctCombo = false;
                        guessedRight--;
                    }
                    else
                    {
                        if (objset[i] > 0) playersRight++;
                    }
                }

                if (correctCombo)
                {
                    eim.setProperty("statusStg" + stage, 1);
                    await clearStageAPQ(stage, eim, curMap);
                }
                else
                {
                    var miss = eim.getIntProperty("missCount") + 1;
                    var maxMiss = stage == 2 ? 7 : 1;

                    if (miss < maxMiss)
                    {
                        eim.setIntProperty("missCount", miss);

                        if (guessedRight == 6)
                        {
                            await SayNext("所有的绳子重量都不同。考虑你接下来的行动，然后再试一次。");
                            await mapMessage(5, "Amos: Hmm... All ropes weigh differently.");
                        }
                        else
                        {
                            await SayNext("一根绳子重量相同。考虑你接下来的行动，然后再试一次。");
                            await mapMessage(5, "Amos: Hmm... One rope weigh the same.");
                        }
                    }
                    else
                    {
                        await spawnMobsAPQ(stage, 0);
                        eim.setIntProperty("missCount", 0);
                        if (stage == 2)
                        {
                            eim.setProperty("stage2combo", "");
                            await SayNext("你已经未能发现正确的组合，现在将被重置。重新开始吧！");
                            await mapMessage(5, "Amos: You have failed to discover the right combination, now it shall be reset. Start over again!");
                        }
                    }

                    await eim.showWrongEffect();
                }
            }
        }

        private async Task clearStageAPQ(int stage, AbstractEventInstanceManager eim, int curMap)
        {
            eim.setProperty(stage + "stageclear", "true");
            if (stage > 1)
            {
                await eim.showClearEffect(true);
                // TODO: eim.linkToNextStage(stage, "apq", curMap) — API not available in C#
            }
            else
            {
                getMap().getPortal("go01")?.setPortalState(false);

                var val = new Random().Next(3);
                await eim.showClearEffect(670010200, "gate" + val, 2);

                getMap().getPortal("go0" + val)?.setPortalState(true);
                // TODO: eim.linkPortalToScript(stage, "go0" + val, "apq0" + val, curMap) — API not available in C#
            }
        }

        private static string generateCombo1()
        {
            var positions = new int[9];
            var rndPicked = new Random().Next((int)Math.Pow(3, 5));

            while (rndPicked > 0)
            {
                positions[rndPicked % 3]++;
                rndPicked /= 3;
            }

            return string.Join(",", positions);
        }

        private static string generateCombo2()
        {
            var toPick = 5;
            var positions = new int[9];
            var rng = new Random();

            while (toPick > 0)
            {
                var rndPicked = rng.Next(9);
                if (positions[rndPicked] == 0)
                {
                    positions[rndPicked] = 1;
                    toPick--;
                }
            }

            return string.Join(",", positions);
        }

        private async Task spawnMobsAPQ(int stage, int maxSpawn)
        {
            var mapObj = getMap();
            if (stage == 2)
            {
                var spawnPosX = new[] { 619, 299, 47, -140, -471 };
                var spawnPosY = new[] { -840, -840, -840, -840, -840 };

                for (var i = 0; i < 5; i++)
                {
                    for (var j = 0; j < 2; j++)
                    {
                        await mapObj.spawnMonsterOnGroundBelow(9400515, spawnPosX[i], spawnPosY[i]);
                        await mapObj.spawnMonsterOnGroundBelow(9400516, spawnPosX[i], spawnPosY[i]);
                        await mapObj.spawnMonsterOnGroundBelow(9400517, spawnPosX[i], spawnPosY[i]);
                    }
                }
            }
            else
            {
                var spawnPosX = new[] { 2303, 1832, 1656, 1379, 1171 };
                var spawnPosY = new[] { 240, 150, 300, 150, 240 };
                var rng = new Random();

                for (var i = 0; i < maxSpawn; i++)
                {
                    var rndMob = 9400519 + rng.Next(4);
                    var rndPos = rng.Next(5);
                    await mapObj.spawnMonsterOnGroundBelow(rndMob, spawnPosX[rndPos], spawnPosY[rndPos]);
                }
            }
        }


        // Npc: 9201045 — Amos (PQ Host, stage 4-6)
        // Last stages: item collection, gate opening, boss fight
        public async Task PartyAmoria_play3()
        {
            var curMap = getMapId();
            var stage = (curMap - 670010200) / 100 + 1;

            var eim = getEventInstance();
            if (eim == null) return;

            if (eim.getProperty(stage + "stageclear") != null)
            {
                if (stage < 5)
                {
                    await SayNext("传送门已经打开，前往那里等待你的考验。");
                }
                else if (stage == 5)
                {
                    await eim.warpEventTeamToMapSpawnPoint(670010700, 0);
                }
                else
                {
                    if (isEventLeader())
                    {
                        if (eim.getIntProperty("marriedGroup") == 0)
                        {
                            await eim.restartEventTimer(1 * 60 * 1000);
                            await eim.warpEventTeam(670010800);
                        }
                        else
                        {
                            eim.setIntProperty("marriedGroup", 0);
                            await eim.restartEventTimer(2 * 60 * 1000);
                            await eim.warpEventTeamToMapSpawnPoint(670010750, 1);
                        }
                    }
                    else
                    {
                        await SayNext("等待队长的指令开始奖励阶段。");
                    }
                }
                return;
            }

            if (stage == 6)
            {
                var area = getMap().getArea(0);
                if (area.Contains(getPlayer().getPosition()))
                {
                    if (getPlayer().isAlive())
                    {
                        await warp(670010700, "st01");
                    }
                    else
                    {
                        await SayNext("喂，退后一点……你已经死了。");
                    }
                }
                else
                {
                    if (isEventLeader())
                    {
                        if (haveItem(4031594, 1))
                        {
                            await gainItem(4031594, -1);
                            await SayNext("恭喜！你的队伍打败了鬼魂巴尔洛格，因此#b完成了阿莫利亚挑战#k！再次与我交谈以开始奖励阶段。");

                            await clearStageAPQ3(stage, eim, curMap);
                            await eim.clearPQ();
                        }
                        else
                        {
                            await SayNext("How is it? Are you going to retrieve me the #b#t4031594##k? That's your last trial, hold on!");
                        }
                    }
                    else
                    {
                        await SayNext("请告诉你的#b队长#k来找我谈话。");
                    }
                }
                return;
            }

            if (!eim.isEventLeader(getPlayer()))
            {
                await SayNext("请告诉你的#b队长#k来找我谈话。");
                return;
            }

            var state = eim.getIntProperty("statusStg" + stage);

            if (state == -1)
            {
                if (stage == 4)
                    await SayOK("嗨。欢迎来到阿莫利亚挑战的#b舞台#k。在这个阶段，从这里周围的怪物身上收集#b50个#t4031597##k。");
                else if (stage == 5)
                    await SayOK("嗨。欢迎来到阿莫利亚挑战的#b舞台#k。要到达这里可是一场不小的奔跑，是吧？好吧，无论如何，这个阶段的任务就是生存！首先，确保有人活着聚集在这里，然后再挑战boss。");

                eim.setProperty("statusStg" + stage, 0);
                return;
            }

            if (stage == 4)
            {
                if (!haveItem(4031597, 50))
                {
                    await SayNext("嘿，你没听清楚吗？我要求 #r50 #t4031597##k 作为这次试炼的成功报酬。");
                    return;
                }

                await gainItem(4031597, -50);

                var tl = eim.getTimeLeft();
                if (tl >= 5 * 60 * 1000)
                {
                    eim.setProperty("timeLeft", tl.ToString());
                    await eim.restartEventTimer(4 * 60 * 1000);
                }

                await SayNext("干得好！现在让我为你打开大门。");
                await mapMessage(5, "Amos: The time runs short now. Your objective is to open the gates and gather together on the other side of the next map. Good luck!");
                await clearStageAPQ3(stage, eim, curMap);
            }
            else if (stage == 5)
            {
                if (!eim.isEventTeamTogether())
                {
                    await SayNext("你的团队还没有聚集在附近。给他们一些时间到达这里。");
                    return;
                }

                var pass = true;
                var party = eim.getPlayers();
                var area = getMap().getArea(2);

                foreach (var chr in party)
                {
                    if (chr.isAlive() && !area.Contains(chr.getPosition()))
                    {
                        pass = false;
                        break;
                    }
                }

                if (!pass)
                {
                    await SayNext("你的团队还没有聚集在附近。给他们一些时间到达这里。");
                    return;
                }

                if (!isAllGatesOpenAPQ())
                {
                    await SayNext("你们是通过传送到达这里的，是吗？我能感觉到。真是遗憾，所有的门都必须打开才能完成这个阶段。如果你们还有时间的话，回头走一遍你们的路，把那些门都关掉。");
                    return;
                }

                var timeLeftProp = eim.getProperty("timeLeft");
                if (timeLeftProp != null)
                {
                    var tr = eim.getTimeLeft();
                    var tl = double.Parse(timeLeftProp);
                    await eim.restartEventTimer((long)(tl - (4 * 60 * 1000 - tr)));
                }

                await SayNext("好的，你的团队已经集合好了。当你们感觉准备好与 #rGeist Balrog#k 战斗时，和我交谈。");
                await mapMessage(5, "Amos: Now only the boss fight remains! Once inside, talk to me only if you want to join the boss fight, you will be transported to action immediately.");
                await clearStageAPQ3(stage, eim, curMap);
            }
        }

        private bool isAllGatesOpenAPQ()
        {
            var map = getMap();
            for (var i = 0; i < 7; i++)
            {
                var gate = map.getReactorByName("gate0" + i);
                if (gate == null || gate.getState() != 4)
                    return false;
            }
            return true;
        }

        private async Task clearStageAPQ3(int stage, AbstractEventInstanceManager eim, int curMap)
        {
            eim.setProperty(stage + "stageclear", "true");
            await eim.showClearEffect(true);
            // TODO: eim.linkToNextStage(stage, "apq", curMap) — API not available in C#
        }


        // Npc: 9201046 — Amos (Bonus Stage)
        public async Task PartyAmoria_playBo()
        {
            var curMap = getMapId();
            var eim = getEventInstance();
            if (eim == null) return;

            if (curMap == 670010750)
            {
                if (!haveItem(4031597, 35))
                {
                    await SayNext("要在这里领取奖品，从生成的怪物身上收集35个#t4031597#给我。只有#r第一个玩家可以领取大奖#k，尽管其他人仍然可以从这个壮举中获得经验加成。或者，你可以选择#b跳过这个奖励阶段#k，通过#b传送门#k继续进行通常的游戏。");
                    return;
                }

                if (eim.getIntProperty("marriedGroup") == 0)
                {
                    if (!canHold(1102101))
                    {
                        await SayNext("在谈论领取奖品之前，请检查您是否有可用的槽位！");
                        return;
                    }

                    eim.setIntProperty("marriedGroup", 1);

                    var baseId = getPlayer().getGender() == 0 ? 1102101 : 1102104;
                    var rnd = new Random().Next(3);
                    await gainItem(baseId + rnd);

                    await SayNext("太棒了！你是第一个领取35个#t4031597#奖励的人。拿着这件披风作为你的功绩奖赏吧。");
                    await gainItem(4031597, -35);
                    await gainExp((int)(4000 * getPlayer().getExpRate()));
                }
                else
                {
                    await SayNext("35 #t4031597#。做得很好，可惜有人先拿走了奖品。赶紧去抓住奖励阶段的最后时刻！");
                    await gainItem(4031597, -35);
                    await gainExp((int)(4000 * getPlayer().getExpRate()));
                }
            }
            else
            {
                await SayNext("赶紧去抓住奖励阶段的最后时刻！");
            }
        }


        // Npc: 9201047 — The Glimmer Man (Stage 1 helper)
        public async Task PartyAmoria_play2()
        {
            if (getMapId() != 670010200)
            {
                if (!await AskYesNo("那么，你打算离开这个地方吗？"))
                    return;

                await warp(670010000, "st00");
                return;
            }

            if (!isEventLeader())
            {
                await SayOK("你的任务是找回魔镜的碎片。为此，你需要一枚#b#t4031596##k，这个物品会在其他怪物全部被消灭后出现的火焰怪身上掉落。要进入怪物所在的房间，选择与你性别对应的传送门，然后消灭那里的所有怪物。女士们走左边，先生们走右边。#b你们的领袖#k必须携带#b#t4031595##k才能获得我的通行证。");
                return;
            }

            var eim = getEventInstance();
            if (eim == null) return;

            var stage = (getMapId() - 670010200) / 100 + 1;
            var st = eim.getIntProperty("statusStg" + stage);

            if (haveItem(4031595, 1))
            {
                await gainItem(4031595, -1);
                eim.setIntProperty("statusStg" + stage, 1);
                await SayOK("你已经找到了#t4031595#，太棒了！你可以向阿莫斯报告你在这个任务中的成功。");
            }
            else if (st < 1 && getMap().countMonsters() == 0)
            {
                eim.setIntProperty("statusStg" + stage, 1);

                var mapObj = getMap();
                mapObj.toggleDrops();
                await mapObj.spawnMonsterOnGroundBelow(9400518, -245, 810);

                await SayOK("烈焰魔出现了！打败它就能获得#b#t4031596##k！");
            }
            else
            {
                if (st < 1)
                    await SayOK("你的任务是恢复魔镜的碎片。为此，你需要一枚#b#t4031596##k，这枚物品会在其他怪物全部被杀死后出现的火焰怪身上掉落。要进入怪物所在的房间，选择与你性别对应的传送门，然后消灭那里的所有怪物。女士们走左边，先生们走右边。");
                else
                    await SayOK("你的任务是找回魔镜碎片。打败火焰精灵就能得到#b#t4031596##k。");
            }
        }


        // Npc: 9201048 — Amos (Entrance Lobby)
        public async Task PartyAmoria_enter2()
        {
            var em = getEventManager("AmoriaPQ");
            if (em == null)
            {
                await SayOK("阿莫利亚组队任务遇到了一个错误。");
                return;
            }

            if (isUsingOldPqNpcStyle())
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                return;
            }

            var sel = await AskMenu("#e#b<组队任务：阿莫利亚挑战>\r\n#k#n" + em.Template.GetRequirementDescription(getClient()) + "\r\n\r\n如果你足够勇敢去尝试阿莫利亚挑战，和其他像你一样的人一起加入，让你的#b队长#k与我交谈。如果一个由整对已婚夫妇组成的队伍注册参加挑战，将会有更好的奖品等着他们。#b\r\n#L0#我想参加这个组队任务。\r\n#L2#我想了解更多细节。");

            if (sel == 0)
            {
                if (getParty() == null)
                {
                    await SayOK("只有当你加入一个队伍时，你才能参加派对任务。");
                    return;
                }

                if (!isLeader())
                {
                    await SayOK("你的队长必须与我交谈才能开始这个组队任务。");
                    return;
                }

                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else
            {
                await SayOK("#e#b<组队任务：阿莫利亚挑战>#k#n\r\n我是阿莫斯，主办了备受赞誉的阿莫利亚挑战。这个副本包含许多团队谜题，合作是取得进展的基本关键。与其他玩家组队尝试挑战奖励阶段，在副本结束时可以获得许多好东西。如果组成全是情侣的队伍，他们可以在额外的奖励阶段获得更好的奖品。");
            }
        }


        // Npc: 9201049 — Ames the Wise (Wedding Exit)
        public async Task ExitWedding()
        {
            await SayNext("嘿，你喜欢婚礼吗？我现在会带你回 #b阿莫利亚#k。");

            var eim = getEventInstance();
            if (eim != null)
            {
                var boxId = (getPlayer().getId() == eim.getIntProperty("groomId") || getPlayer().getId() == eim.getIntProperty("brideId")) ? 4031424 : 4031423;

                if (canHold(boxId, 1))
                {
                    await gainItem(boxId, 1);
                    await warp(680000000);
                    await SayNext("你刚刚收到了一个玛瑙宝箱。寻找#b#p9201014##k，她在阿莫利亚的顶部，她知道如何打开这些宝箱。");
                }
                else
                {
                    await SayOK("请在您的 ETC 背包中腾出空间，以便接收黑曜石宝箱。");
                }
            }
            else
            {
                await warp(680000000);
            }
        }
    }
}
