using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.AmoriaPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9201042 — Mr. Sandman (Tick Shop)
        public async Task TickShop()
        {
            var wishPrizes = new[] { 2000000, 2010004, 2020011, 2000004, 2000006, 2022015, 2000005, 1082174, 1002579, 1032039, 1002578, 1002580, 1002577, 1102078 };
            var wishPrizesQty = new[] { 10, 10, 5, 5, 5, 5, 10, 1, 1, 1, 1, 1, 1, 1 };
            var wishPrizesCst = new[] { 10, 15, 20, 30, 30, 50, 100, 400, 450, 500, 500, 530, 550, 600 };

            var slctTicket = getTierTicket(getPlayer().getLevel());
            var amntTicket = getItemQuantity(slctTicket);

            await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_832_TickShopIntro));

            var listStr = "";
            for (var i = 0; i < wishPrizes.Length; i++)
            {
                listStr += $"#b#L{i}#{wishPrizesQty[i]} #z{wishPrizes[i]}##k";
                listStr += $" - {wishPrizesCst[i]} wish tickets";
                listStr += "#l\r\n";
            }

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.Amoria_ShopMenu, amntTicket, slctTicket, slctTicket, listStr));

            if (amntTicket < wishPrizesCst[sel])
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_NotEnoughTickets, wishPrizesCst[sel], slctTicket));
                return;
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Amoria_ConfirmBuy, wishPrizesQty[sel], wishPrizes[sel], wishPrizesCst[sel], slctTicket)))
                return;

            if (canHold(wishPrizes[sel], wishPrizesQty[sel]))
            {
                await gainItem(wishPrizes[sel], wishPrizesQty[sel]);
                await gainItem(slctTicket, -wishPrizesCst[sel]);
                await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_832_ThanksPurchase));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_832_NeedInventorySlot));
            }
        }

        // Npc: 9201043 — Amos the Strong (Entrance)
        public async Task PartyAmoria_enter()
        {
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201043_Menu));

            if (sel == 0)
            {
                if (!haveItem(4031592))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201043_NeedEntranceTicket));
                    return;
                }

                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201043_ConfirmEnter)))
                    return;

                await warp(670010100, 0);
                await gainItem(4031592, -1);
            }
            else if (sel == 1)
            {
                if (haveItem(4031592))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201043_AlreadyHaveTicket));
                    return;
                }

                if (!haveItem(4031593, 10))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201043_NeedTenKeys));
                    return;
                }

                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201043_ConfirmTrade)))
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_PortalOpen));
                return;
            }

            if (!eim.isEventLeader(getPlayer()))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_NeedLeader));
                return;
            }

            var state = eim.getIntProperty("statusStg" + stage);

            if (state == -1)
            {
                if (stage == 1)
                    await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_Play_Stage1Intro, stage));
                else if (stage == 2)
                    await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_Play_Stage2Intro, stage));
                else if (stage == 3)
                    await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_Play_Stage3Intro, stage));

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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_Play_TalkForInfo));
            }
            else if (stage == 2 || stage == 3)
            {
                if (map.countMonsters() > 0)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_DefeatMobsFirst));
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
                        await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_HintFiveExact));
                    else
                        await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_HintDifferentPlatforms));
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
                            await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_RopeDifferent));
                            await this.Pink(this.GetTalkMessage(ScriptMessage.AmoriaPQNpcHandler_AmosHmm));
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_RopeSame));
                            await this.Pink(this.GetTalkMessage(ScriptMessage.AmoriaPQNpcHandler_AmosHmmOne));
                        }
                    }
                    else
                    {
                        await spawnMobsAPQ(stage, 0);
                        eim.setIntProperty("missCount", 0);
                        if (stage == 2)
                        {
                            eim.setProperty("stage2combo", "");
                            await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_CombinationFailed));
                            await this.Pink(this.GetTalkMessage(ScriptMessage.AmoriaPQNpcHandler_AmosHave));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_PortalOpen));
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
                        await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_WaitLeader));
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
                        await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_AlreadyDead));
                    }
                }
                else
                {
                    if (isEventLeader())
                    {
                        if (haveItem(4031594, 1))
                        {
                            await gainItem(4031594, -1);
                            await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_Congrats));

                            await clearStageAPQ3(stage, eim, curMap);
                            await eim.clearPQ();
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_LastTrial));
                        }
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_NeedLeader));
                    }
                }
                return;
            }

            if (!eim.isEventLeader(getPlayer()))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play_NeedLeader));
                return;
            }

            var state = eim.getIntProperty("statusStg" + stage);

            if (state == -1)
            {
                if (stage == 4)
                    await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_Play3_Stage4Intro, stage));
                else if (stage == 5)
                    await SayOK(this.GetTalkMessage(ScriptMessage.Amoria_Play3_Stage5Intro, stage));

                eim.setProperty("statusStg" + stage, 0);
                return;
            }

            if (stage == 4)
            {
                if (!haveItem(4031597, 50))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_NeedFifty));
                    return;
                }

                await gainItem(4031597, -50);

                var tl = eim.getTimeLeft();
                if (tl >= 5 * 60 * 1000)
                {
                    eim.setProperty("timeLeft", tl.ToString());
                    await eim.restartEventTimer(4 * 60 * 1000);
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_GateOpen));
                await this.Pink(this.GetTalkMessage(ScriptMessage.AmoriaPQNpcHandler_AmosTime));
                await clearStageAPQ3(stage, eim, curMap);
            }
            else if (stage == 5)
            {
                if (!eim.isEventTeamTogether())
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_NotGathered));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_NotGathered));
                    return;
                }

                if (!isAllGatesOpenAPQ())
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_TeleportedShame));
                    return;
                }

                var timeLeftProp = eim.getProperty("timeLeft");
                if (timeLeftProp != null)
                {
                    var tr = eim.getTimeLeft();
                    var tl = double.Parse(timeLeftProp);
                    await eim.restartEventTimer((long)(tl - (4 * 60 * 1000 - tr)));
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Play3_ReadyForBalrog));
                await this.Pink(this.GetTalkMessage(ScriptMessage.AmoriaPQNpcHandler_AmosOnly));
                await clearStageAPQ3(stage, eim, curMap);
            }
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Bonus_HowToClaim));
                    return;
                }

                if (eim.getIntProperty("marriedGroup") == 0)
                {
                    if (!canHold(1102101))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Bonus_CheckSlot));
                        return;
                    }

                    eim.setIntProperty("marriedGroup", 1);

                    var baseId = getPlayer().getGender() == 0 ? 1102101 : 1102104;
                    var rnd = new Random().Next(3);
                    await gainItem(baseId + rnd);

                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Bonus_FirstPlace));
                    await gainItem(4031597, -35);
                    await gainExp((int)(4000 * getPlayer().getExpRate()));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Bonus_TooLate));
                    await gainItem(4031597, -35);
                    await gainExp((int)(4000 * getPlayer().getExpRate()));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Amoria_Bonus_Hurry));
            }
        }

        // Npc: 9201047 — The Glimmer Man (Stage 1 helper)
        public async Task PartyAmoria_play2()
        {
            if (getMapId() != 670010200)
            {
                if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201047_ConfirmLeave)))
                    return;

                await warp(670010000, "st00");
                return;
            }

            if (!isEventLeader())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201047_TaskWithLeader));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201047_RetrievedMirror));
            }
            else if (st < 1 && getMap().countMonsters() == 0)
            {
                eim.setIntProperty("statusStg" + stage, 1);

                var mapObj = getMap();
                mapObj.toggleDrops();
                await mapObj.spawnMonsterOnGroundBelow(9400518, -245, 810);

                await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201047_FierryAppeared));
            }
            else
            {
                if (st < 1)
                    await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201047_TaskRecoverShard));
                else
                    await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201047_TaskDefeatFierry));
            }
        }

        // Npc: 9201048 — Amos (Entrance Lobby)
        public async Task PartyAmoria_enter2()
        {
            var em = GetEventManager(AmoriaPQFeature.EventName);
            if (isUsingOldPqNpcStyle())
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                return;
            }

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201048_EntryMenu,
                em.Template.GetRequirementDescription(getClient()),
                getPlayer().isRecvPartySearchInviteEnabled()
                    ? this.GetTalkMessage(ScriptMessage.Magatia_PartySearchDisable)

                    : this.GetTalkMessage(ScriptMessage.Magatia_PartySearchEnable)));
            if (sel == 0)
            {
                if (getParty() == null)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedParty));
                    return;
                }

                if (!isLeader())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedLeader));
                    return;
                }

                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201048_Intro, npc));
            }
        }

        // Npc: 9201049 — Ames the Wise (Wedding Exit)
        public async Task ExitWedding()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201049_EnjoyWedding));

            var eim = getEventInstance();
            if (eim != null)
            {
                var boxId = (getPlayer().getId() == eim.getIntProperty("groomId") || getPlayer().getId() == eim.getIntProperty("brideId")) ? 4031424 : 4031423;

                if (canHold(boxId, 1))
                {
                    await gainItem(boxId, 1);
                    await warp(680000000);
                    await SayNext(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201049_OnyxChest));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.AmoriaPQ_9201049_NeedEtcSlot));
                }
            }
            else
            {
                await warp(680000000);
            }
        }

        private static int getTierTicket(int level)
        {
            if (level < 50) return 4031543;
            if (level < 120) return 4031544;
            return 4031545;
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
    }
}
