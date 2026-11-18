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

            await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_9000000_PlayerName, npcId == 9000000 ? this.GetTalkMessage(ScriptMessage.NormalEvent_Paul) : npcId == 9000001 ? this.GetTalkMessage(ScriptMessage.NormalEvent_Jean) : this.GetTalkMessage(ScriptMessage.NormalEvent_Tony)));
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.NormalEvent_HuhKindEvent));
            if (sel == 0)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_AllMonth));
            }
            else if (sel == 1)
            {
                var game = await AskMenu(this.GetTalkMessage(ScriptMessage.NormalEvent_ManyGames));
                var gameDesc = game switch
                {
                    0 => this.GetTalkMessage(ScriptMessage.NormalEvent_OlaOla),
                    1 => this.GetTalkMessage(ScriptMessage.NormalEvent_MaplestoryPhysical),
                    2 => this.GetTalkMessage(ScriptMessage.NormalEvent_SnowballConsists),
                    3 => this.GetTalkMessage(ScriptMessage.NormalEvent_CoconutHarvest),
                    4 => this.GetTalkMessage(ScriptMessage.NormalEvent_OxQuiz),
                    5 => this.GetTalkMessage(ScriptMessage.NormalEvent_TreasureHunt),
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_EitherEvent));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_EitherEvent));
                }
            }
        }

        private async Task Event0011()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_ImYoure));
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.NormalEvent_HuhKind));
            if (sel == 0)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_AllMonth));
            }
            else if (sel == 1)
            {
                var game = await AskMenu(this.GetTalkMessage(ScriptMessage.NormalEvent_ManyGames));
                var gameDesc = game switch
                {
                    0 => this.GetTalkMessage(ScriptMessage.NormalEvent_OlaOla),
                    1 => this.GetTalkMessage(ScriptMessage.NormalEvent_MaplestoryPhysical),
                    2 => this.GetTalkMessage(ScriptMessage.NormalEvent_SnowballConsists),
                    3 => this.GetTalkMessage(ScriptMessage.NormalEvent_CoconutHarvest),
                    4 => this.GetTalkMessage(ScriptMessage.NormalEvent_OxQuiz),
                    5 => this.GetTalkMessage(ScriptMessage.NormalEvent_TreasureHunt),
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_HaveAlready));
                }
                else if (!canHold(4031019))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_PleaseMake));
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
                        await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_EitherEvent));
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_ExchangeRequired));
                    }
                    else if (getInventory(1).getNextFreeSlot() <= -1 || getInventory(2).getNextFreeSlot() <= -1 || getInventory(3).getNextFreeSlot() <= -1 || getInventory(4).getNextFreeSlot() <= -1)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_NeedMake));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_BamBam));
            await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_YoullAwarded));
            await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_ScrollSecrets));
            if (canHold(4031019))
            {
                await gainItem(4031019);
                await warp(getPlayer().getSavedLocation("EVENT"));
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
            }
        }


        // Npc: 9000003, 9000004, 9000005, 9000006 
        public async Task Event03()
        {
            var eim = GetEventInstanceTrust();
            const string nthText = "last";

            var preamble = eim.getProperty("leader" + nthText + "preamble");
            var mobcount = eim.getProperty("leader" + nthText + "mobcount");

            // 首次对话：标记前导完成，等待队长带队进入
            if (preamble == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_WelcomeFinal));
                eim.setProperty("leader" + nthText + "preamble", "done");
                return;
            }

            if (!isEventLeader())
            {
                if (mobcount == null)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_PleaseTell));
                }
                else
                {
                    await warp(109020001, 0);
                }
                return;
            }

            if (mobcount != null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_ContinueNext));
                return;
            }

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.NormalEvent_YoureFinished)))
                return;

            if (getMap().countMonsters() != 0)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_TalkingAbout));
                return;
            }

            await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_GoodJob));

            eim.setProperty("leader" + nthText + "mobcount", "done");

            var members = eim.getPlayers();
            await warpParty(109020001, 0);
            await eim.giveEventPlayersExp(2500);
        }


        // Npc: 9000007 
        public async Task Event04()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_JustKid));
        }


        // Npc: 9000008 
        public async Task Event05()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_HaveLocked));
        }


        // Npc: 9000009 
        public async Task Event03_1()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_FindTreasure));
        }


        // Npc: 9000010 
        public async Task Event06()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.NormalEvent_ImSorry));
            await warp(getPlayer().getSavedLocation("EVENT"));
        }


        // Npc: 9010001, 9010002, 9010003 
        public async Task Event07()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Event07_Greeting, npc));
        }


        // Npc: 9000012 
        public async Task Event09()
        {
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.NormalEvent_ManHot));
            if (sel == 0)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.NormalEvent_LeaveCant)))
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.NormalEvent_DontHave));
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
