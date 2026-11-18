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
            switch (await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2003_TravelQuestions)))
            {
                case 0:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Move1),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Move2)
                        ]);
                    break;
                case 1:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Attack1),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Attack2)
                        ]);
                    break;
                case 2:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_PickUp1),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_PickUp2)
                        ]);
                    break;
                case 3:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Die1),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Die2)
                        ]);
                    break;
                case 4:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Job1),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Job2)
                        ]);
                    break;
                case 5:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Island1),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Island2)
                        ]);
                    break;
                case 6:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Warrior1)
                        ]);
                    break;
                case 7:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Bowman)
                        ]);
                    break;
                case 8:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Magician),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Magician2)
                        ]);
                    break;
                case 9:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Thief)
                        ]);
                    break;
                case 10:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Stats),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Stats2)
                        ]);
                    break;
                case 11:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_CheckItems)
                        ]);
                    break;
                case 12:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_WearItem)
                        ]);
                    break;
                case 13:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_CheckEquip)
                        ]);
                    break;
                case 14:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Skills)
                        ]);
                    break;
                case 15:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_ToVictoria),
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_WorldMap)
                        ]);
                    break;
                case 16:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc2003_A_Meso)
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc2104_LetsGo));
            await warp(1);
        }

        // Npc: 12101 
        public async Task rein()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Npc12101_LithHarbor),
                this.GetTalkMessage(ScriptMessage.Npc12101_GoSouthperry),
                this.GetTalkMessage(ScriptMessage.Npc12101_ChooseJob)
                ]);
        }

        // Npc: 22000 
        public async Task begin7()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc22000_ToVictoria)))
            {
                if (haveItem(4031801))
                {
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc22000_LucasLetter));
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc22000_LetsGo));

                    // ！！！ 判断物品是否存在之后又等待了对话，虽然此时玩家无法操作物品，但是可能有其他后台操作（比如道具过期）恰好在等待期间移除了道具。需要重新判断
                    if (!haveItem(4031801))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc22000_WhatLetter));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc22000_LetsGo));
                    if (getMeso() < 150)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc22000_NotEnoughMeso));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc22000_LevelTooLow));
                    return;
                }
            }
        }

        // Npc: 1002000 
        public async Task rithTeleport()
        {

            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002000_Message0));
            switch (await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1002000_Message2)))
            {
                case 0:
                    {
                        int[] imaps = [104000000, 102000000, 101000000, 100000000, 103000000, 120000000, 105040300];
                        switch (await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1002000_Message3), imaps.Select(x => $"#m{x}#")))
                        {
                            case 0:
                                await SaySpeech([
                                    this.GetTalkMessage(ScriptMessage.Npc1002000_LithHarbor1),
                                    this.GetTalkMessage(ScriptMessage.Npc1002000_LithHarbor2),
                                    this.GetTalkMessage(ScriptMessage.Npc1002000_LithHarbor3)
                                    ]);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case 1:
                    var selStr = getJobId() == 0 ? this.GetTalkMessage(ScriptMessage.Npc1002000_CoditionNoviceTrue) : this.GetTalkMessage(ScriptMessage.Npc1002000_CoditionNoviceFalse);
                    int[] maps = [102000000, 100000000, 101000000, 103000000, 120000000];
                    int[] cost = [1000, 1000, 800, 1000, 800];

                    var option = await AskMenu(selStr, maps.Select(x => $"#m{x}#"));

                    var realCost = DiscountForNovice(cost[option]);
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1002000_ConfirmMove, maps[option], realCost)))
                    {
                        if (getMeso() < realCost)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Npc1002000_MesoNotEnough));
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
                this.GetTalkMessage(ScriptMessage.Npc1002002_FlorinaIntro),
                [this.GetTalkMessage(ScriptMessage.Npc1002002_OptPay, cost), this.GetTalkMessage(ScriptMessage.Npc1002002_OptTicket, ticket), this.GetTalkMessage(ScriptMessage.Npc1002002_OptWhatIs, ticket)]);
            switch (option)
            {
                case 0:
                    if (getMeso() < 1500)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002002_LackMeso));
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
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1002002_ConfirmFlorina)))
                    {
                        if (!haveItem(ticket))
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002002_WhereIsTicket));
                            return;
                        }
                        getPlayer().saveLocation("FLORINA");
                        await warp(110000000, "st00");
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002002_TakeARest));
                    }
                    break;
                case 2:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002002_WhatIsTicket));
                    break;
                default:
                    break;
            }
        }

        // Npc: 1002003 
        public async Task friend00()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1002003_ExtendIntro)))
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1002003_ExtendConfirm)))
                {
                    var capacity = getPlayer().getBuddylist().Capacity;
                    if (capacity >= 50 || getMeso() < 240000)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002003_CheckMeso));
                    }
                    else
                    {
                        var newcapacity = capacity + 5;
                        await gainMeso(-240000);
                        await getPlayer().setBuddyCapacity(newcapacity);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc1002003_ExtendSuccess));
                    }
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002003_NoMesoFriends));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002003_NotManyFriends));
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
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1002100_BuyPotions));
                var inputNumber = await AskNumber(this.GetTalkMessage(ScriptMessage.Npc1002100_PickQuantity, items[option].item, items[option].desc), 1, 1, 100);
                var totalCost = inputNumber * items[option].price;
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1002100_ConfirmBuy, inputNumber, items[option].item, items[option].price, totalCost)))
                {
                    if (getMeso() < totalCost)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002100_CheckMeso, totalCost));
                    }
                    else
                    {
                        if (canHold(items[option].item, inputNumber))
                        {
                            await gainMeso(totalCost);
                            await gainItem(items[option].item, inputNumber);
                            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002100_ComeAgain));
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.USE)));
                        }
                    }
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002100_Materials));
                }
            }
            else
            {
                if (isQuestCompleted(2010))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1002100_NotStrong));
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
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1012008_Menu));

            if (option == 0)
            {
                var gameType = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1012008_ChooseGame));

                if (gameType == 4)
                {
                    int[] omok = [4080000, 4080001, 4080002, 4080003, 4080004, 4080005];
                    int[] omok1piece = [4030000, 4030000, 4030000, 4030010, 4030011, 4030011];
                    int[] omok2piece = [4030001, 4030010, 4030011, 4030001, 4030010, 4030001];
                    int omokamount = 99;

                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1012008_OmokIntro));

                    var omokType = await AskMenu(this.GetTalkMessage(ScriptMessage.Omok_ChooseSet),
                        omok.Select(x => $"#t{x}#"));

                    if (haveItem(omok1piece[omokType], omokamount) && haveItem(omok2piece[omokType], omokamount) && haveItem(4030009, 1))
                    {
                        await gainItem(omok1piece[omokType], -omokamount);
                        await gainItem(omok2piece[omokType], -omokamount);
                        await gainItem(4030009, -1);
                        await gainItem(omok[omokType], 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc1012008_CraftSuccess));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc1012008_MaterialsNeeded, omok[omokType], omokamount, omok1piece[omokType], omok2piece[omokType], 4030009));
                    }
                }
                else
                {
                    if (haveItem(4030012, 15))
                    {
                        await gainItem(4030012, -15);
                        await gainItem(4080100, 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc1012008_CraftSuccess));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc1012008_MatchCardsIntro));
                    }
                }
            }
            else
            {
                var explainType = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1012008_ExplainMenu));

                if (explainType == 2)
                {
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Omok_Rules),
                        this.GetTalkMessage(ScriptMessage.Omok_Cost),
                        this.GetTalkMessage(ScriptMessage.Omok_Ready),
                        this.GetTalkMessage(ScriptMessage.Omok_FirstMove)
                    ]);
                }
                else
                {
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.MatchCards_Rules),
                        this.GetTalkMessage(ScriptMessage.MatchCards_Cost2),
                        this.GetTalkMessage(ScriptMessage.MatchCards_Ready2)
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1012115_NothingSuspicious));
            }
            else if (status == 1)
            {
                await forceCompleteQuest(20706);
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1012115_FoundShadow));
            }
            else if (status == 2)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1012115_ShadowAlreadyFound));
            }
        }

        // Npc: 1012116 
        public async Task blackShadowHene2()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1012115_NothingSuspicious));
        }

        // Npc: 1022101 
        public async Task go_xmas()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1022101_RoonieIntro, npc)))
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1032003_LevelTooLow));
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1032003_ConfirmEnter)))
                {
                    if (getMeso() < 5000)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.MesoNotEnough));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pet_SeeYou));
                }
            }
        }

        // Npc: 1032004 
        public async Task herb_out()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1032004_ConfirmReturn)))
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
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Ticket_Greeting),
                    list.Select(x => x.GetDestinationMapName(getPlayer())!));

                currentContiMove = list[option];
            }
            else
            {
                currentContiMove = GetContiMove();
            }

            if (currentContiMove == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            var target = currentContiMove.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            var p = currentContiMove.GetTicket(getPlayer());
            if (p == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Ticket_Offer, target, TimeSpan.FromMilliseconds(currentContiMove.GetTransportationTime(currentContiMove.RideTime)).Humanize(), p.Value.TicketPrice, p.Value.TicketItemId)))
            {
                if (getMeso() >= p.Value.TicketPrice && canHold(p.Value.TicketItemId))
                {
                    await gainItem(p.Value.TicketItemId, 1);
                    await gainMeso(-p.Value.TicketPrice);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_NotEnoughMeso, p.Value.TicketPrice));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Ticket_Busy));
            }
        }

        // Id 1032008, 2012001, 2012013, 2012021, 2012025, 2041000, 2082001, 2102000
        public async Task get_ticket()
        {
            var currentContiMove = GetContiMove();
            if (currentContiMove == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            var target = currentContiMove.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            var p = currentContiMove.GetTicket(getPlayer());
            if (p == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            var next = DateTimeOffset.FromUnixTimeMilliseconds(currentContiMove.ArriveAt).ToLocalTime().Humanize();

            if (haveItem(p.Value.TicketItemId))
            {
                if (currentContiMove.CanEnter)
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Ticket_ConfirmDest, target)))
                    {
                        if (await currentContiMove.Enter(getPlayer()))
                        {
                            await gainItem(p.Value.TicketItemId, -1);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_Departed, target, next));
                        }
                        return;
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_TalkIfChangeMind));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_Departed, target, next));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_NeedTicket, target));
            }
        }

        // Id 1032009, 2012002, 2012022, 2012024, 2041001, 2082002, 2102001
        public async Task goOutWaitingRoom()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Ticket_ConfirmLeave)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Ticket_TakeCare));
                await WarpOut();
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc1032111_BottledSap));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc1032111_SapFlowing));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1032111_SapFlowing));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc1040000_NotSuspect));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1040000_Sleeping));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Ticket_ConductorHi));
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

            var selStr = this.GetTalkMessage(ScriptMessage.Npc1052006_SelectTicket);
            for (var i = 0; i < zones; i++)
            {
                selStr += "\r\n#L" + i + "#" + this.GetTalkMessage(ScriptMessage.Npc1052006_BuildSite, i + 1, cost) + "#l";
            }
            var option = await AskMenu(selStr);
            if (getMeso() < cost)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.MesoNotEnough));
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
            await SaySpeech([this.GetTalkMessage(ScriptMessage.Npc1052011_Connected), this.GetTalkMessage(ScriptMessage.Npc1052011_GiveUpLeave)]);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1052011_ConfirmLeave)))
            {
                await warp(103000100, 0);
            }

        }


        // Npc: 1052107 
        public async Task sca_Shade()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Npc1052107_TurnOnLamp)))
            {
                await weakenAreaBoss(5090000, this.GetTalkMessage(ScriptMessage.Weaken_LampOn));
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
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc1052111_FoundItem));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
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
            //        await SayOK(this.GetTalkMessage(ScriptMessage.NpcScript_PleaseFree));
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
            //                await SayOK(this.GetTalkMessage(ScriptMessage.NpcScript_BetweenLevels));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1061006_StatueForce));

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
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1061006_DeeperForest), maps.Take(zones).Select(x => $"#m{x}#"));
                await warp(maps[option], 0);
            }
        }

        // Npc: 1061007 
        public async Task flower_out()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Ticket_ConfirmLeave)))
            {
                await warp(105040300, 0);
            }
        }

        // Npc: 1061100 
        public async Task hotel1()
        {

            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1061100_Welcome));
            var regcost = 499;
            var vipcost = 999;
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1061100_RoomMenu, regcost, vipcost));
            switch (option)
            {
                case 0:
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1061100_ConfirmRegular)))
                    {
                        if (getMeso() >= regcost)
                        {
                            await warp(105040401);
                            await gainMeso(-regcost);
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1061100_NotEnoughMeso, regcost));
                        }
                        return;
                    }
                    break;
                case 1:
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1061100_ConfirmVip)))
                    {
                        if (getMeso() >= vipcost)
                        {
                            await warp(105040402);
                            await gainMeso(-vipcost);
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1061100_NotEnoughMeso, vipcost));
                        }
                        return;
                    }
                    break;
                default:
                    break;
            }
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1061100_OtherServices));

        }

        // Npc: 1063000 
        public async Task viola_pink()
        {
            if (isQuestStarted(2052) && !haveItem(4031025, 30))
            {
                if (!canHold(4031025, 30))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
                    return;
                }

                await gainItem(4031025, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
                    return;
                }

                await gainItem(4031026, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
                    return;
                }

                await gainItem(4031028, 30);
            }
            else
            {
                if (getPlayer().getInventory(InventoryType.ETC).getNumFreeSlot() < 1)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1063012_SealedByTalisman));
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1081001_ConfirmLeave, 110000000, returnmap)))
            {
                getPlayer().clearSavedLocation(SavedLocationType.FLORINA);
                await warp(returnmap);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1081001_RestHere, returnmap));
            }
        }

        // Npc: 1092000 
        public async Task nautil_cow()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Npc1092000_ToStable),
                this.GetTalkMessage(ScriptMessage.Npc1092000_CalfVsCow)
                ]);
            if (canHold(4031847))
            {
                await gainItem(4031847, 1);
                await warp(912000100, 0);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
            }
        }

        // Npc: 1092010 
        public async Task remove_DirtytreasureMap()
        {
            if (!haveItem(4220153))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1092010_Hmm));
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1092010_TreasureMap)))
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092016_ShinyRock));
                await completeQuest(2166);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092016_TouchedRock));
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092018_GotPaper));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092018_PaperNoSlot));
                }
            }
        }

        async Task MomCow(int idx)
        {
            if (getQuestProgressInt(2180, 1) == idx)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092018_AlreadyMilked));
                return;
            }

            if (canHold(4031848) && haveItem(4031847))
            {
                await gainItem(4031847, -1);
                await gainItem(4031848, 1);

                await setQuestProgress(2180, 1, idx);

                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092018_BottleOneThird));
            }
            else if (canHold(4031849, 1) && haveItem(4031848))
            {
                await gainItem(4031848, -1);
                await gainItem(4031849, 1);

                await setQuestProgress(2180, 1, idx);

                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092018_BottleTwoThirds));
            }
            else if (canHold(4031850) && haveItem(4031849))
            {
                await gainItem(4031849, -1);
                await gainItem(4031850, 1);

                await setQuestProgress(2180, 1, idx);

                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092018_BottleFull));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092093_CalfDrinking));
            }
            else if (haveItem(4031848) || haveItem(4031849) || haveItem(4031850))
            {
                await gainItem(4031848, -1);
                await gainItem(4031849, -1);
                await gainItem(4031850, -1);

                await gainItem(4031847, 1);
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc1092093_CalfEmpty));
            }
        }

        // Npc: 1094002, 1094003, 1094004, 1094005, 1094006 
        public async Task nautil_Abel1()
        {
            if (!isQuestStarted(2186))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1094002_JustBoxes));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Npc1094002_WantGlasses));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1094002_AlreadyHave));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1101001_Blessing));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1101001_KeepTraining));
            }
        }

        // Npc: 1101008 
        public async Task helperCygnus()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc1101008_Intro));
            switch (option)
            {
                case 0:
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Npc1101008_AboutMe),
                        this.GetTalkMessage(ScriptMessage.Npc1101008_NoNeedInfo)
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc1101008_CygnusKnights));
                    break;
                default:
                    break;
            }
        }

        // Npc: 1102001 
        public async Task outSecondDH()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc1102001_ConfirmExit)))
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
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.CygnusJoin_Offer)))
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Eleanor_WarpPrompt));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Eleanor_WarpPrompt)))
            {
                var map = await getWarpMap(913030000);
                if (map.countPlayers() == 0)
                {
                    await warp(map.Id, 0);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Eleanor_Busy));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc1202009_WhatIsIt));
            }
        }

        // Npc: 1204001 
        public async Task dollMaster00()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204001_Francis, npc), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd | NpcTalkSpeaker.Face),
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204001_InnerThought), NpcTalkSpeaker.PlayerRight | NpcTalkSpeaker.NoEnd)
                ]);

            await completeQuest(21719);
            await warp(105040200, 10);
        }

        // Npc: 1204010 
        public async Task giantDagoth()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204010_WhoAreYou), NpcTalkSpeaker.NoEnd),
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204010_HeardAboutPuppeteer), NpcTalkSpeaker.NoEnd),
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204010_LetsFight), NpcTalkSpeaker.NoEnd)
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
                this.GetTalkMessage(ScriptMessage.Npc2001000_Snowmen),
                this.GetTalkMessage(ScriptMessage.Npc2001000_OrnamentRules),
                this.GetTalkMessage(ScriptMessage.Npc2001000_ItemsReturn),
                this.GetTalkMessage(ScriptMessage.Npc2001000_BuyOrnaments)
                ], finalNext: false);
        }

        // Npc: 2001001 
        public async Task go_tree1()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2001000_XmasTree)))
            {
                await warp(209000001);
            }
        }

        // Npc: 2001002 
        public async Task go_tree2()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2001000_XmasTree)))
            {
                await warp(209000002);
            }
        }

        // Npc: 2001003 
        public async Task go_tree3()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2001000_XmasTree)))
            {
                await warp(209000003);
            }
        }

        // Npc: 2001004 
        public async Task out_tree()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2001004_ReadyToLeave)))
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2002000_LeaveHappyville)))
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
                (this.GetTalkMessage(ScriptMessage.Job_Magician_Ellinia), this.GetTalkMessage(ScriptMessage.Npc2012006_Ship)),
                (this.GetTalkMessage(ScriptMessage.Npc2012006_DestLudibrium), this.GetTalkMessage(ScriptMessage.Npc2012006_Ship)),
                (this.GetTalkMessage(ScriptMessage.Npc2012006_DestLeafre), this.GetTalkMessage(ScriptMessage.Npc2012006_Bird)),
                (this.GetTalkMessage(ScriptMessage.Npc2012006_DestMuLung), this.GetTalkMessage(ScriptMessage.Npc2012006_Hak)),
                (this.GetTalkMessage(ScriptMessage.Npc2012006_DestAriant), this.GetTalkMessage(ScriptMessage.Npc2012006_Genie)),
                (this.GetTalkMessage(ScriptMessage.Npc2012006_DestEreve), this.GetTalkMessage(ScriptMessage.Npc2012006_Boat)),
                ];
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2012006_ChooseDestination), options.Select(x => this.GetTalkMessage(ScriptMessage.Npc2012006_OptPlatform, x.map, x.transport)));
            var targetMap = 200000110 + (option * 10);
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc2012006_WarpToPlatform, targetMap));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2012014_TeleportPrompt));
                await gainItem(4001019, -1);
                await warp(200082100, 0);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2012014_NeedScroll));
            }
        }

        // Npc: 2012015 
        public async Task ossyria3_2()
        {
            if (haveItem(4001019))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2012015_TeleportPrompt));
                await gainItem(4001019, -1);
                await warp(200080200, 0);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2012015_NeedScroll));
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

                        await this.Pink(this.GetTalkMessage(ScriptMessage.NpcScript_YouveMissed));
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
                                await this.Pink(this.GetTalkMessage(ScriptMessage.NpcScript_TwinkleTwinkle));
                                await setQuestProgress(3114, 42);

                                await getPlayer().SendPacket(FieldEffectPacket.Screen("quest/party/clear"));
                                await getPlayer().SendPacket(FieldEffectPacket.Sound("Party1/Clear"));

                            }
                            else
                            {
                                if (idx == 14)
                                {
                                    await this.Pink(this.GetTalkMessage(ScriptMessage.NpcScript_TwinkleTwinkleLittle));
                                }
                                else if (idx == 22)
                                {
                                    await this.Pink(this.GetTalkMessage(ScriptMessage.NpcScript_AboveWorld));
                                }
                                else if (idx == 30)
                                {
                                    await this.Pink(this.GetTalkMessage(ScriptMessage.NpcScript_LikeDiamond));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Job2_TrulyHero));
                return;
            }

            var questProgress = getQuestProgressInt(questId);
            if (questProgress == 1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job2_LetYouIn));

                await TryStartSoloQuest(questId);
            }
            else
            {
                if (!haveItem(letter))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job2_NeedLetter, letter));
                    return;
                }

                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job2_HiddenLetter, letter, c.CurrentCulture.GetJobName(job)),
                    this.GetTalkMessage(ScriptMessage.Job2_HiddenMap),
                    this.GetTalkMessage(ScriptMessage.Job2_CollectMarbles),
                    ]);
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job2_ConfirmEnter)))
                {
                    await TryStartSoloQuest(questId, async () =>
                    {
                        await setQuestProgress(questId, 1);
                        await gainItem(letter, -1);
                    });
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job2_ComeBackLater));
                }
            }
        }

        async Task Job2Exit(Job job)
        {
            var questId = QuestId.Get2ndJobQuest(job);
            var eim = GetEventInstanceTrust();
            if (haveItem(4031013, 30))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job2_CollectedAll));
                await removeAll(4031013);
                await gainItem(4031012);

                await eim.clearPQ();
            }
            else
            {
                await AskMenu(this.GetTalkMessage(ScriptMessage.Job2_NeedMarbles), [this.GetTalkMessage(ScriptMessage.Job2_OptLeave)]);
            }
            await eim.exitPlayer(getPlayer());
        }

        async Task Job3(int type, string preNpcMap, int preNpc)
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job3_PickJobMenu), [
                this.GetTalkMessage(ScriptMessage.Job3_OptThirdJob),
                this.GetTalkMessage(ScriptMessage.Job3_OptZakum)
                ]);
            switch (option)
            {
                case 0:
                    var baseJob = JobFactory.GetById(type * 100);
                    var questId = QuestId.Get3rdJobQuest(baseJob);
                    var baseJobStr = c.CurrentCulture.GetJobName(baseJob);

                    if (IsQuestNotStarted(questId))
                    {
                        if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job3_Welcome, getNpc(), baseJobStr)))
                        {
                            await SaySpeech([
                                this.GetTalkMessage(ScriptMessage.Job3_TestIntro1, baseJobStr, preNpcMap, preNpc),
                                this.GetTalkMessage(ScriptMessage.Job3_TestIntro2, preNpc)
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
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job3_PhysicalDone));
                                if (haveItem(4031057))
                                {
                                    await gainItem(4031057, -1);
                                    await setQuestProgress(questId, 1);

                                    await SayNext(this.GetTalkMessage(ScriptMessage.Job3_WisdomIntro));
                                    return;
                                }
                                else
                                {
                                    // 原先这里只需要判断进度，并不强制需要物品（力量项链），我这里强制任务道具丢失需要重做
                                    await SayNext(this.GetTalkMessage(ScriptMessage.Job3_BringNecklace, preNpc));
                                    return;
                                }
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job3_BringNecklace, preNpc));
                            }
                        }
                        else if (questProgress == 1)
                        {
                            if (haveItem(4031058))
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job3_WisdomDone));
                                if (haveItem(4031058))
                                {
                                    await gainItem(4031058, -1);
                                    await setQuestProgress(questId, 2);

                                    await Apply3rdJobChange();
                                }
                                else
                                {
                                    await SayNext(this.GetTalkMessage(ScriptMessage.Job3_ItemLost));
                                    return;
                                }
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job3_BringSecondNecklace));
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.Job3_ZakumApproved));
                        if (IsQuestNotStarted(ZakumFeature.QuestId))
                        {
                            await startQuest(ZakumFeature.QuestId);
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Job3_ZakumTooWeak));
                    }
                    break;
            }
        }

        async Task Apply3rdJobChange()
        {
            var nextJob = getJobId() + 1;
            var job = JobFactory.GetById(nextJob);
            var jobStr = c.CurrentCulture.GetJobName(job);
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Job3_ConfirmJobChange, jobStr)))
            {
                return;
            }

            if (getPlayer().getRemainingSp() > (getLevel() - 70) * 3)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job3_UseAllSp));
                return;
            }

            await changeJobById(nextJob);
            await forceCompleteQuest(QuestId.Get3rdJobQuest(getJob()));
            if (job == Job.CRUSADER)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job3_Warrior_Bonus),
                        this.GetTalkMessage(ScriptMessage.Job3_SpApGranted, c.CurrentCulture.GetJobName(getJob()))
                ]);
            }
            else if (job == Job.WHITEKNIGHT)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job3_Paladin_Bonus),
                        this.GetTalkMessage(ScriptMessage.Job3_SpApGranted, c.CurrentCulture.GetJobName(getJob()))
                ]);
            }
            else if (job == Job.DRAGONKNIGHT)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job3_DragonKnight_Bonus),
                        this.GetTalkMessage(ScriptMessage.Job3_SpApGranted, c.CurrentCulture.GetJobName(getJob()))
                ]);
            }

            else if (getJob() == Job.FP_MAGE)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job3_FirePoison_Bonus),
                        this.GetTalkMessage(ScriptMessage.Job3_SpApGranted, c.CurrentCulture.GetJobName(getJob()))
                ]);
            }
            else if (getJob() == Job.IL_MAGE)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job3_IceLightning_Bonus),
                        this.GetTalkMessage(ScriptMessage.Job3_SpApGranted, c.CurrentCulture.GetJobName(getJob()))
                ]);
            }
            else if (getJob() == Job.PRIEST)
            {
                await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.Job3_Cleric_Bonus),
                        this.GetTalkMessage(ScriptMessage.Job3_SpApGranted, c.CurrentCulture.GetJobName(getJob()))
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Ticket_ConfirmLeave)))
            {
                await eim.exitPlayer(getPlayer());
            }
        }

        // Npc: 2022004 
        public async Task s4common1_out()
        {
            // 技能 稳如泰山
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc2022004_Done));
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

            await SayNext(this.GetTalkMessage(ScriptMessage.Npc2030000_Warning));
            if (getLevel() > 49)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2030000_ConfirmEnter)))
                {
                    await warp(211040300, 5);
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2030000_LevelTooLow));
            }
        }

        // Npc: 2030006 
        public async Task holyStone()
        {
            List<(string quest, string[] options, int answer)> questList = [
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q1), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q1_Text), this.GetTalkMessage(ScriptMessage.Npc2030006_Q1_Opt15), this.GetTalkMessage(ScriptMessage.Npc2030006_Q1_Opt4), this.GetTalkMessage(ScriptMessage.Npc2030006_Q1_Opt12), this.GetTalkMessage(ScriptMessage.Npc2030006_Q1_Opt16)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q2), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q2_MagicianLevel), this.GetTalkMessage(ScriptMessage.Npc2030006_Q2_PirateDex), this.GetTalkMessage(ScriptMessage.Npc2030006_Q2_ArcherDex), this.GetTalkMessage(ScriptMessage.Npc2030006_Q2_ThiefLuk), this.GetTalkMessage(ScriptMessage.Npc2030006_Q2_SwordmanStr)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q3), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q3_SealedSkills), this.GetTalkMessage(ScriptMessage.Npc2030006_Q3_UndeadTurns), this.GetTalkMessage(ScriptMessage.Npc2030006_Q3_WeakenSlow), this.GetTalkMessage(ScriptMessage.Npc2030006_Q3_CursedExp), this.GetTalkMessage(ScriptMessage.Npc2030006_Q3_StunnedCannot)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q4), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q4_PirateLuk), this.GetTalkMessage(ScriptMessage.Npc2030006_Q4_MagicianLevel), this.GetTalkMessage(ScriptMessage.Npc2030006_Q4_ThiefLuk), this.GetTalkMessage(ScriptMessage.Npc2030006_Q4_WarriorStr), this.GetTalkMessage(ScriptMessage.Npc2030006_Q4_BowmanDex)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q5), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q5_RoyalCactus), this.GetTalkMessage(ScriptMessage.Npc2030006_Q5_WildBoar), this.GetTalkMessage(ScriptMessage.Npc2030006_Q5_LazyBuffy), this.GetTalkMessage(ScriptMessage.Npc2030006_Q5_ChipmunkNut), this.GetTalkMessage(ScriptMessage.Npc2030006_Q5_StirgeStirges)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q6), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q6_GreatestOldies), this.GetTalkMessage(ScriptMessage.Npc2030006_Q6_NependeathNependeaths), this.GetTalkMessage(ScriptMessage.Npc2030006_Q6_GhostStump), this.GetTalkMessage(ScriptMessage.Npc2030006_Q6_SparkerSeal), this.GetTalkMessage(ScriptMessage.Npc2030006_Q6_MinerZombie)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q7), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q7_WarriorElixir), this.GetTalkMessage(ScriptMessage.Npc2030006_Q7_PureWater), this.GetTalkMessage(ScriptMessage.Npc2030006_Q7_CakeRecover), this.GetTalkMessage(ScriptMessage.Npc2030006_Q7_SaladRecover), this.GetTalkMessage(ScriptMessage.Npc2030006_Q7_PizzaRecover)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q8), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q8_ManaElixir), this.GetTalkMessage(ScriptMessage.Npc2030006_Q8_TonicCures), this.GetTalkMessage(ScriptMessage.Npc2030006_Q8_AppleRecover), this.GetTalkMessage(ScriptMessage.Npc2030006_Q8_SunriseDew), this.GetTalkMessage(ScriptMessage.Npc2030006_Q8_RamenRecover)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q9), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q9_TreeStump), this.GetTalkMessage(ScriptMessage.Npc2030006_Q9_Bubbling), this.GetTalkMessage(ScriptMessage.Npc2030006_Q9_AxeStump), this.GetTalkMessage(ScriptMessage.Npc2030006_Q9_Octopus), this.GetTalkMessage(ScriptMessage.Npc2030006_Q9_GreenMushroom)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q10), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q10_Werewolf), this.GetTalkMessage(ScriptMessage.Npc2030006_Q10_Slime), this.GetTalkMessage(ScriptMessage.Npc2030006_Q10_CrimsonBalrog), this.GetTalkMessage(ScriptMessage.Npc2030006_Q10_Zakum), this.GetTalkMessage(ScriptMessage.Npc2030006_Q10_StarPixie)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q11), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q11_Shroom), this.GetTalkMessage(ScriptMessage.Npc2030006_Q11_BlueSnail), this.GetTalkMessage(ScriptMessage.Npc2030006_Q10_Slime), this.GetTalkMessage(ScriptMessage.Npc2030006_Q11_RedSnail), this.GetTalkMessage(ScriptMessage.Npc2030006_Q11_Pig)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q12), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_EvilEye), this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_Sentinel), this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_JrBalrog), this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_GhostStump), this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_Snail)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q13), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q13_DarkYeti), this.GetTalkMessage(ScriptMessage.Npc2030006_Q13_DarkLigator), this.GetTalkMessage(ScriptMessage.Npc2030006_Q13_YetiPepe), this.GetTalkMessage(ScriptMessage.Npc2030006_Q13_Bain), this.GetTalkMessage(ScriptMessage.Npc2030006_Q13_CoolieZombie)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q14), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q14_Malady), this.GetTalkMessage(ScriptMessage.Npc2030006_Q14_Ligator), this.GetTalkMessage(ScriptMessage.Npc2030006_Q14_ColdEye), this.GetTalkMessage(ScriptMessage.Npc2030006_Q14_Meerkat), this.GetTalkMessage(ScriptMessage.Npc2030006_Q14_Alishar)], 0),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q15), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q15_LunarPixie), this.GetTalkMessage(ScriptMessage.Npc2030006_Q15_Lioner), this.GetTalkMessage(ScriptMessage.Npc2030006_Q15_Cellion), this.GetTalkMessage(ScriptMessage.Npc2030006_Q13_DarkLigator), this.GetTalkMessage(ScriptMessage.Npc2030006_Q15_Hector)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q16), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_Snail), this.GetTalkMessage(ScriptMessage.Npc2030006_Q11_Shroom), this.GetTalkMessage(ScriptMessage.Npc2030006_Q12_EvilEye), this.GetTalkMessage(ScriptMessage.Npc2030006_Q16_OrangeMushroom), this.GetTalkMessage(ScriptMessage.Npc2030006_Q11_BlueSnail)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q17), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q17_FlamingFeather), this.GetTalkMessage(ScriptMessage.Npc2030006_Q17_OldGladius), this.GetTalkMessage(ScriptMessage.Npc2030006_Q17_PieceIce), this.GetTalkMessage(ScriptMessage.Npc2030006_Q17_AncientScroll), this.GetTalkMessage(ScriptMessage.Npc2030006_Q17_FairyWing)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q18), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q18_MysteryNiora), this.GetTalkMessage(ScriptMessage.Npc2030006_Q18_RightfulDonation), this.GetTalkMessage(ScriptMessage.Npc2030006_Q18_GhostWhereabout), this.GetTalkMessage(ScriptMessage.Npc2030006_Q18_ArwenGlass), this.GetTalkMessage(ScriptMessage.Npc2030006_Q18_MayaWeird)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q19), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q19_Mage), this.GetTalkMessage(ScriptMessage.Npc2030006_Q19_Cleric), this.GetTalkMessage(ScriptMessage.Npc2030006_Q19_Assassin), this.GetTalkMessage(ScriptMessage.Npc2030006_Q19_Gunslinger), this.GetTalkMessage(ScriptMessage.Npc2030006_Q19_Fighter)], 0),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q20), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q20_CupidsCourier), this.GetTalkMessage(ScriptMessage.Npc2030006_Q20_LostOcean), this.GetTalkMessage(ScriptMessage.Npc2030006_Q20_AlcasterDark), this.GetTalkMessage(ScriptMessage.Npc2030006_Q20_EliminatingDrumming), this.GetTalkMessage(ScriptMessage.Npc2030006_Q20_WarPang)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q21), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q21_FlorinaBeach), this.GetTalkMessage(ScriptMessage.Npc2030006_Q21_AmherstSouthperry), this.GetTalkMessage(ScriptMessage.Npc2030006_Q21_KerningCity), this.GetTalkMessage(ScriptMessage.Npc2030006_Q21_PerionEllinia), this.GetTalkMessage(ScriptMessage.Npc2030006_Q21_Sleepywood)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q22), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q22_Sera), this.GetTalkMessage(ScriptMessage.Npc2030006_Q22_Heena), this.GetTalkMessage(ScriptMessage.Npc2030006_Q22_Lucas), this.GetTalkMessage(ScriptMessage.Npc2030006_Q22_Roger), this.GetTalkMessage(ScriptMessage.Npc2030006_Q22_Shanks)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q23), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_Vogen), this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_Sophia), this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_Pedro), this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_MasterSergeant), this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_Rumi)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q24), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q24_HiddenRock), this.GetTalkMessage(ScriptMessage.Npc2030006_Q24_Glibber), this.GetTalkMessage(ScriptMessage.Npc2030006_Q24_Jeff), this.GetTalkMessage(ScriptMessage.Npc2030006_Q24_HolyStone), this.GetTalkMessage(ScriptMessage.Npc2030006_Q24_ElmaHousekeeper)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q25), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q25_Ayan), this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_Sophia), this.GetTalkMessage(ScriptMessage.Npc2030006_Q25_MrSmith), this.GetTalkMessage(ScriptMessage.Npc2030006_Q25_Francois), this.GetTalkMessage(ScriptMessage.Npc2030006_Q25_Manji)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q26), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q26_Teo), this.GetTalkMessage(ScriptMessage.Npc2030006_Q26_Vicious), this.GetTalkMessage(ScriptMessage.Npc2030006_Q26_Mia), this.GetTalkMessage(ScriptMessage.Npc2030006_Q26_Doofus), this.GetTalkMessage(ScriptMessage.Npc2030006_Q26_Casey)], 0),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q27), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q27_MrPark), this.GetTalkMessage(ScriptMessage.Npc2030006_Q27_MarFairy), this.GetTalkMessage(ScriptMessage.Npc2030006_Q27_Roel), this.GetTalkMessage(ScriptMessage.Npc2030006_Q27_Ria), this.GetTalkMessage(ScriptMessage.Npc2030006_Q27_Shane)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q28), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_DrFaymus), this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_MongKong), this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_Ervine), this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_Luke), this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_Nella)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q29), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q29_Doofus), this.GetTalkMessage(ScriptMessage.Npc2030006_Q29_Vicious), this.GetTalkMessage(ScriptMessage.Npc2030006_Q29_Patricia), this.GetTalkMessage(ScriptMessage.Npc2030006_Q29_Weaver), this.GetTalkMessage(ScriptMessage.Npc2030006_Q29_Cloy)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q30), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q30_ChiefStan), this.GetTalkMessage(ScriptMessage.Npc2030006_Q30_JmTha), this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_DrFaymus), this.GetTalkMessage(ScriptMessage.Npc2030006_Q29_Vicious), this.GetTalkMessage(ScriptMessage.Npc2030006_Q28_Luke)], 0),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q31), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q31_StaffSergeant), this.GetTalkMessage(ScriptMessage.Npc2030006_Q31_SergeantBravo), this.GetTalkMessage(ScriptMessage.Npc2030006_Q31_CorporalEasy), this.GetTalkMessage(ScriptMessage.Npc2030006_Q23_MasterSergeant), this.GetTalkMessage(ScriptMessage.Npc2030006_Q31_Peter)], 4),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q32), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q32_OldRing), this.GetTalkMessage(ScriptMessage.Npc2030006_Q32_MemoryPowder), this.GetTalkMessage(ScriptMessage.Npc2030006_Q32_FairyDust), this.GetTalkMessage(ScriptMessage.Npc2030006_Q32_ProofHero), this.GetTalkMessage(ScriptMessage.Npc2030006_Q32_ScrollSecrets)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q33), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q33_Apple), this.GetTalkMessage(ScriptMessage.Npc2030006_Q33_PowerElixir), this.GetTalkMessage(ScriptMessage.Npc2030006_Q33_WeirdMedicine), this.GetTalkMessage(ScriptMessage.Npc2030006_Q33_Chrysanthemum), this.GetTalkMessage(ScriptMessage.Npc2030006_Q33_OrangeJuice)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q34), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q34_Neve), this.GetTalkMessage(ScriptMessage.Npc2030006_Q34_Serryl), this.GetTalkMessage(ScriptMessage.Npc2030006_Q27_Shane), this.GetTalkMessage(ScriptMessage.Npc2030006_Q25_Francois), this.GetTalkMessage(ScriptMessage.Npc2030006_Q30_JmTha)], 2),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q35), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q35_Bari), this.GetTalkMessage(ScriptMessage.Npc2030006_Q26_Teo), this.GetTalkMessage(ScriptMessage.Npc2030006_Q35_Pio), this.GetTalkMessage(ScriptMessage.Npc2030006_Q35_Sid), this.GetTalkMessage(ScriptMessage.Npc2030006_Q35_Maria)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q36), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q22_Lucas), this.GetTalkMessage(ScriptMessage.Npc2030006_Q36_DrKim), this.GetTalkMessage(ScriptMessage.Npc2030006_Q30_ChiefStan), this.GetTalkMessage(ScriptMessage.Npc2030006_Q36_Scadur), this.GetTalkMessage(ScriptMessage.Npc2030006_Q36_ProfessorFoxwit)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q37), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Blue), this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Green), this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Brown), this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Red), this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Black)], 1),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q38), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q38_Text), this.GetTalkMessage(ScriptMessage.Npc2030006_Q38_Opt8), this.GetTalkMessage(ScriptMessage.Npc2030006_Q38_Opt3), this.GetTalkMessage(ScriptMessage.Npc2030006_Q38_Opt13), this.GetTalkMessage(ScriptMessage.Npc2030006_Q1_Opt16)], 3),
                (this.GetTalkMessage(ScriptMessage.Npc2030006_Q39), [this.GetTalkMessage(ScriptMessage.Npc2030006_Q39_White), this.GetTalkMessage(ScriptMessage.Npc2030006_Q39_Orange), this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Blue), this.GetTalkMessage(ScriptMessage.Npc2030006_Q39_Purple), this.GetTalkMessage(ScriptMessage.Npc2030006_Q37_Green)], 2),
            ];


            var questId = QuestId.Get3rdJobQuest(getJob());
            var questProgres = getQuestProgressInt(questId);
            if (questProgres == 1 && !haveItem(4031058))
            {
                if (haveItem(4005004, 1))
                {
                    if (!canHold(4031058))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc2030006_NeedEtcSlot));
                        return;
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc2030006_QuizIntro));
                        await gainItem(4005004, -1);
                        var questions = questList.OrderBy(x => Guid.NewGuid()).Take(5).ToList();
                        for (int i = 0; i < 5; i++)
                        {
                            var q = questions[i];
                            var inputAnswer = await AskMenu(q.quest, q.options);
                            if (inputAnswer != q.answer)
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2030006_WrongAnswer));
                                return;
                            }
                        }
                        await gainItem(4031058, 1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2030006_Passed));
                    }
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc2030006_BringItem));
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
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2032001_RefineOffer)))
                {
                    var inputNumber = await AskNumber(this.GetTalkMessage(ScriptMessage.Npc2032001_AskQuantity), 1, 1, 100);
                    if (getMeso() < 500_000 * inputNumber)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2032001_NotFree));
                        return;
                    }
                    else if (!haveItem(4004004, 10 * inputNumber))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2032001_NeedOre));
                        return;
                    }
                    else if (!canHold(4005004, inputNumber))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2032001_NoSlot));
                        return;
                    }

                    await gainItem(4004004, -10 * inputNumber);
                    await gainMeso(-500000 * inputNumber);
                    await gainItem(4005004, inputNumber);
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2032001_UseWisely));
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
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2040024_TeleportPrompt)))
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2040024_NeedScroll));
            }
        }

        // Npc: 2040025 
        public async Task ludi015()
        {
            if (haveItem(4001020))
            {
                (string desc, int map)[] options = [(this.GetTalkMessage(ScriptMessage.Npc2040025_OptFirst), 221024400), (this.GetTalkMessage(ScriptMessage.Npc2040025_OptThird), 221021700)];
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2040025_ChoosePrompt), options.Select(x => x.desc));

                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2040025_ConfirmPrompt, options[option].desc)))
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2040025_NeedScroll));
            }
        }

        // Npc: 2040026 
        public async Task ludi016()
        {
            if (haveItem(4001020))
            {
                (string desc, int map)[] options = [(this.GetTalkMessage(ScriptMessage.Npc2040026_OptSecond), 221022900), (this.GetTalkMessage(ScriptMessage.Npc2040026_OptFourth), 221020000)];
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2040026_ChoosePrompt), options.Select(x => x.desc));

                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2040026_ConfirmPrompt, options[option].desc)))
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2040026_NeedScroll));
            }
        }

        // Npc: 2040027 
        public async Task ludi017()
        {
            if (haveItem(4001020))
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2040027_TeleportPrompt)))
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2040027_NeedScroll));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2040052_NoneReturned));
                return;
            }
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Npc2040052_ReturnList, completed.Length, string.Join("\r\n", completed.Select(x => $"#v{x.item}# #b#t{x.item}##k"))),
                this.GetTalkMessage(ScriptMessage.Npc2040052_Calm2)
                ]);
        }

        // Npc: 2041025 
        public async Task Populatus01()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2041025_Escape)))
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2041026_HoldItem));
                await gainItem(4220046, -1);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2041026_ObserverIntro, npc));
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
                        await this.Popup(this.GetTalkMessage(ScriptMessage.NpcScript_HaveEtc));
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
                {0,(230030200, this.GetTalkMessage(ScriptMessage.Npc2060009_UseTicket, 230030200)) },
                {1,(230030200, this.GetTalkMessage(ScriptMessage.Npc2060009_PayMeso, 230030200, 10000)) },

                {2,(230000000, this.GetTalkMessage(ScriptMessage.Npc2060009_PayMeso, 230000000, 10000)) },
                {3,(251000000, this.GetTalkMessage(ScriptMessage.Npc2060009_PayMeso, 251000000, 10000)) },

                {4,(230010000, this.GetTalkMessage(ScriptMessage.Npc2060009_PayMeso, 230010000, 10000)) },
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2093004_ConfirmMove)))
            {
                if (getMeso() < 10000)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2110005_ShortOnMeso));
                }
                else
                {
                    await gainMeso(-10000);
                    await warp(230000000);
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2110005_ComeBack));
            }
        }

        // Npc: 2060100 
        public async Task s4common2()
        {
            // 技能 勇士的意志
            if (isQuestStarted(6301))
            {
                if (haveItem(4000175))
                {
                    await gainItem(4000175, -1);
                    await warp(923000000, 0);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2060100_NeedItem));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2060100_DontMessWithMe, npc));
            }
        }


        // Npc: 2081000 
        public async Task job4_item()
        {
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2081000_Menu));
            switch (option)
            {
                case 0:
                    await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2081000_SeedMenu));
                    var inputNumber = await AskNumber(this.GetTalkMessage(ScriptMessage.Npc2081000_AskQuantity), 1, 1, 99);
                    var cost = inputNumber * 30000;
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2081000_ConfirmBuy, inputNumber, cost)))
                    {
                        if (getMeso() < cost || !canHold(4031346, inputNumber))
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Npc2081000_CheckMeso));
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Npc2081000_SeeYou));
                            await gainItem(4031346, inputNumber);
                            await gainMeso(-cost);
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2081000_ThinkCarefully));
                    }
                    break;
                case 1:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc2081000_UnderDevelopment));
                    break;
                default:
                    break;
            }
        }


        // Npc: 2082003 
        public async Task flyminidraco()
        {
            await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2082003_Transform2));
            await useItem(2210016);
            await warp(200090500, 0);
        }

        // Npc: 2082004 
        public async Task TD_neo_Andy()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Npc2082004_TimeTraveler, npc));
        }



        // Npc: 2083006 
        public async Task TD_neoCity_enter()
        {
            if (!isQuestCompleted(3718))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2083006_TimeMachineInactive));
                return;
            }

            int[] quests = [3719, 3724, 3730, 3736, 3742, 3748];
            string[] areas = [
                this.GetTalkMessage(ScriptMessage.Npc2083006_Area2021),
                this.GetTalkMessage(ScriptMessage.Npc2083006_Area2099),
                this.GetTalkMessage(ScriptMessage.Npc2083006_Area2215),
                this.GetTalkMessage(ScriptMessage.Npc2083006_Area2216),
                this.GetTalkMessage(ScriptMessage.Npc2083006_Area2230),
                this.GetTalkMessage(ScriptMessage.Npc2083006_Area2503)
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2083006_ProveValor));
                return;
            }

            var menuItems = areas.Take(limit).Select((x, i) => x).ToList();
            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc2083006_SelectArea), menuItems);

            int[] mapids = [240070100, 240070200, 240070300, 240070400, 240070500, 240070600];
            if (selection >= 0 && selection < mapids.Length)
            {
                await warp(mapids[selection], 1);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2083006_CompleteMission));
            }
        }

        // Npc: 1204020 
        public async Task ShadowWarrier()
        {
            await SaySpeech([
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204020_Waiting), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204020_InnerThought), NpcTalkSpeaker.PlayerRight),
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204020_SealingStone), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.Face),
                new SpeechText(this.GetTalkMessage(ScriptMessage.Npc1204020_LetsFight), NpcTalkSpeaker.PlayerRight)
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc2101013_FlightOffer));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2101013_FlightTerms)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2101013_ReadyTakeoff));
                if (getMeso() >= 10000)
                {
                    await gainMeso(-10000);
                    int[] towns = [100000000, 101000000, 102000000, 103000000, 104000000];
                    await warp(Randomizer.Select(towns));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc2101013_ShortOnMeso));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2101013_ScaredOfHeights));
            }
        }

        // Npc: 2103013 
        public async Task dooat()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Pyramid_Unavailable));
        }

        // Npc: 2110005 
        public async Task nihal_taxi()
        {
            var toMagatia = this.GetTalkMessage(ScriptMessage.Npc2110005_ToMagatia);
            var toAriant = this.GetTalkMessage(ScriptMessage.Npc2110005_ToAriant);

            if (await AskYesNo(getPlayer().getMapId() == 260020000 ? toMagatia : toAriant))
            {
                if (getMeso() < 1500)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc2110005_ShortOnMeso));
                }
                else
                {
                    await warp(getPlayer().getMapId() == 260020000 ? 261000000 : 260000000, 0);
                    await gainMeso(-1500);
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2110005_ComeBack));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111006_OnlyGhosts));
            }
        }

        // Npc: 2111011 
        public async Task absence_wall()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2111011_InspectWall)))
            {
                await setQuestProgress(3311, 5);
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111011_PendantPhrase));
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
            await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111013_SilverPendant));
            if (canHold(4031697, 1))
            {
                await gainItem(4031697);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
            }
        }

        // Npc: 2111014 
        public async Task absence_desk()
        {
            if (isQuestStarted(3311))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111014_DeLangDiary));
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
                    var inputText = await AskText(this.GetTalkMessage(ScriptMessage.Npc2111017_PasswordPrompt));
                    if (inputText == this.GetTalkMessage(ScriptMessage.Npc2111017_Password))
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103008_Error));
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
                    var inputText = await AskText(this.GetTalkMessage(ScriptMessage.Npc2111018_PasswordPrompt));
                    if (inputText == this.GetTalkMessage(ScriptMessage.Npc2111017_Password))
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111018_Wrong));
                    }
                }
                else if (progress == 2)
                {
                    await setQuestProgress(23339, 1, 3);
                    var inputText = await AskText(this.GetTalkMessage(ScriptMessage.Npc2111018_PasswordPrompt));
                    if (inputText == this.GetTalkMessage(ScriptMessage.Npc2111017_Password))
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111018_Wrong));
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
                    var inputText = await AskText(this.GetTalkMessage(ScriptMessage.Npc2111018_PasswordPrompt));
                    if (inputText == this.GetTalkMessage(ScriptMessage.Npc2111017_Password))
                    {
                        await setQuestProgress(23339, 1, 4);
                        await warp(261000001, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111018_Wrong));
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

                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2111023_ShardPlaced));
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
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Security_Deactivate)))
            {
                await weakenAreaBoss(7090000, this.GetTalkMessage(ScriptMessage.Weaken_SecurityOff));
            }
        }

        // Npc: 2111026 
        public async Task sca_DitRoi()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Pentagram_Finish)))
            {
                await weakenAreaBoss(8090000, this.GetTalkMessage(ScriptMessage.Weaken_PentagramDone));
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
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc9000021_RecruitIntro, npc));
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc9000021_BossGauntlet));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Npc9000021_JoinPrompt)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000021_GoodLuck));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000040_MedalUnavailable));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000040_MedalUnavailable));
                return;
            }

            string[] options = [
                this.GetTalkMessage(ScriptMessage.Npc9000041_InvEquip),
                this.GetTalkMessage(ScriptMessage.Npc9000041_InvUse),
                this.GetTalkMessage(ScriptMessage.Npc9000041_InvSetup),
                this.GetTalkMessage(ScriptMessage.Npc9000041_InvEtc)
            ];
            var selectedType = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9000041_Menu));
            if (selectedType < 0 || selectedType >= options.Length)
                return;

            var name = await AskText(this.GetTalkMessage(ScriptMessage.Npc9000041_ChooseInv, options[selectedType]));
            if (string.IsNullOrWhiteSpace(name))
                return;
            var res = await getPlayer().sellAllItemsFromName((sbyte)(selectedType + 1), name);
            if (res > -1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000041_DoneTrade, numberWithCommas(res)));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000041_NoItems, name, options[selectedType]));
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
                    1 => this.GetTalkMessage(ScriptMessage.Npc9000049_Easy),
                    2 => this.GetTalkMessage(ScriptMessage.Npc9000049_Medium),
                    3 => this.GetTalkMessage(ScriptMessage.Npc9000049_Hard),
                    _ => this.GetTalkMessage(ScriptMessage.Npc9000049_Off)
                };

                int selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9000049_Menu, eventStatus));

                if (selection == 0)
                {
                    stage = c.CurrentServer.getStoredVar(9000049);
                    if (stage == 0)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000049_TowerLocked));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9000049_TowerLocked));
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

            await SayNext(this.GetTalkMessage(ScriptMessage.Npc9001102_UfoKidnap));

            if (getPlayer().getLevel() >= 12)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9001102_RescueGaga)))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9001102_Thanks));
                    await warp(922240200, 0);
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9001102_LevelTooLow));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9010022_NoWay));
                return;
            }

            var menu = "";
            if (level >= 20 && level <= 30)
            {
                menu += this.GetTalkMessage(ScriptMessage.Npc9010022_Coliseum);
            }

            if (level >= 25)
            {
                menu += this.GetTalkMessage(ScriptMessage.Npc9010022_Dojo);
            }

            if (level >= 30 && level <= 50)
            {
                menu += this.GetTalkMessage(ScriptMessage.Npc9010022_Carnival1);
            }

            if (level >= 51 && level <= 70)
            {
                menu += this.GetTalkMessage(ScriptMessage.Npc9010022_Carnival2);
            }

            int selection = await AskDimensionalMirror(menu);

            getPlayer().SaveLocation(SavedLocationType.MIRROR);
            switch (selection)
            {
                case 0:
                    await warp(980010000, 3);
                    break;
                case 1:
                    await warp(925020000, 0);
                    break;
                case 2:
                    getPlayer().SaveLocation(SavedLocationType.MONSTER_CARNIVAL);
                    await warp(980000000, 3);
                    break;
                case 3:
                    getPlayer().SaveLocation(SavedLocationType.MONSTER_CARNIVAL);
                    await warp(980030000, 3);
                    break;
            }
        }

        // Npc: 9060000 
        public async Task tamepig_out()
        {
            if (haveItem(4031508, 5) && haveItem(4031507, 5))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9060000_Collected));
                //
                await getEventInstance()!.clearPQ();
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9060000_ConfirmLeave)))
                {
                    await warp(923010100, 0);
                }
            }
        }

        // Npc: 9101001 
        public async Task begin_jp2()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Npc9901000_TrainingDone),
                this.GetTalkMessage(ScriptMessage.Npc9901000_WarningMonsters)
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9120003_EnterBath, price)))
            {
                if (getMeso() < price)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9120003_CheckMeso, price));
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

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9120010_StoryPrompt)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9120010_ChangeMind));
                return;
            }

            var choiceStr = this.GetTalkMessage(ScriptMessage.Npc9120010_TradeIntro) + "\r\n";
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9120010_InventoryFull));
            }
            else if (haveItem(requiredItem, 100))
            {
                await gainItem(requiredItem, -100);
                await gainItem(prizeItem, prizeQuantity);
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9120010_StandardQuality, prizeItem));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9120010_StopLying));
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

            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9120013_ConfirmQuiz)))
            {
                return;
            }

            if (!haveItem(2020001, 300))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9120013_Need300));
                return;
            }

            await gainItem(2020001, -300);
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc9120013_QuizIntro), NpcTalkSpeaker.NpcLeft | NpcTalkSpeaker.NoEnd);

            // 问题和答案数据
            var questions = new List<string>
            {
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q1),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q2),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q3),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q4),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q5),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q6),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q7),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q8),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q9),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q10),
                this.GetTalkMessage(ScriptMessage.Npc9120013_Q11),
            };

            var answers = new List<string[]>
            {
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A1_RaccoonFirewood), this.GetTalkMessage(ScriptMessage.Npc9120013_A1_SolidHorn), this.GetTalkMessage(ScriptMessage.Npc9120013_A1_RedBrick) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A2_Peli), this.GetTalkMessage(ScriptMessage.Npc9120013_A2_Spinel), this.GetTalkMessage(ScriptMessage.Npc9120013_A2_Poli) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A3_Takoyaki), this.GetTalkMessage(ScriptMessage.Npc9120013_A3_Yakisoba), this.GetTalkMessage(ScriptMessage.Npc9120013_A3_Tempura) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A4_ExtraAs), this.GetTalkMessage(ScriptMessage.Npc9120013_A4_ExtraBs), this.GetTalkMessage(ScriptMessage.Npc9120013_A4_ExtraCs) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A5_FrozenTuna), this.GetTalkMessage(ScriptMessage.Npc9120013_A5_Fan), this.GetTalkMessage(ScriptMessage.Npc9120013_A5_FlySwatter) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A6_Sami), this.GetTalkMessage(ScriptMessage.Npc9120013_A6_Kami), this.GetTalkMessage(ScriptMessage.Npc9120013_A6_Umi) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A7_CloudFoxs), this.GetTalkMessage(ScriptMessage.Npc9120013_A7_GhostsBouquet), this.GetTalkMessage(ScriptMessage.Npc9120013_A7_DarkCloud) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A8_BlackCrow), this.GetTalkMessage(ScriptMessage.Npc9120013_A8_BlueMushmom), this.GetTalkMessage(ScriptMessage.Npc9120013_A8_Himegami) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A9_BambooSpear), this.GetTalkMessage(ScriptMessage.Npc9120013_A9_PicoPico), this.GetTalkMessage(ScriptMessage.Npc9120013_A9_MysticCane) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A10_KinokoRamen), this.GetTalkMessage(ScriptMessage.Npc9120013_A10_KinokoRamenSalt), this.GetTalkMessage(ScriptMessage.Npc9120013_A10_MushroomMiso) },
                new[] { this.GetTalkMessage(ScriptMessage.Npc9120013_A11_Skye), this.GetTalkMessage(ScriptMessage.Npc9120013_A11_Furano), this.GetTalkMessage(ScriptMessage.Npc9120013_A11_Shinta) },
            };

            var correctAnswer = new List<int> { 1, 1, 0, 1, 2, 2, 2, 0, 0, 2, 2 };

            // 随机选择5个问题
            var rng = new Random();
            var indices = Enumerable.Range(0, questions.Count).OrderBy(_ => rng.Next()).Take(5).ToList();

            for (int i = 0; i < 5; i++)
            {
                int idx = indices[i];
                var prompt = this.GetTalkMessage(ScriptMessage.Npc9120013_QuestionNo, i + 1, questions[idx]);
                for (int j = 0; j < answers[idx].Length; j++)
                {
                    prompt += $"\r\n#b#L{j}#{answers[idx][j]}#l#k";
                }

                int selection = await AskMenu(prompt);

                if (selection != correctAnswer[idx])
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9120013_TryAgain));
                    return;
                }
            }

            await gainItem(4031064, 1);
            await SayNext(this.GetTalkMessage(ScriptMessage.Npc9120013_AllCorrect));
            await SayOK(this.GetTalkMessage(ScriptMessage.Npc9120013_BusinessDone));
        }



        // Npc: 9201050 
        public async Task About_NLC()
        {
            const int minlevel = 10;

            if (isQuestCompleted(4911))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_SolvedAll));
                return;
            }

            if (isQuestCompleted(4900) || isQuestStarted(4900))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_AnotherQuestion));
                return;
            }

            var info = new[]
            {
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptPlace),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptFoxwit),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptDoor),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptMesoGears),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptJungle),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptPortal),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptSigns),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptJack),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptLita),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptBoroughs),
                this.GetTalkMessage(ScriptMessage.Npc9201050_OptQuiz)
            };

            int selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9201050_Menu));

            switch (selection)
            {
                case 0:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_BuildCity));
                    await About_NLC();
                    break;
                case 1:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_Foxwit));
                    await About_NLC();
                    break;
                case 2:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_GearPortal));
                    await About_NLC();
                    break;
                case 3:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_MesoGears));
                    await About_NLC();
                    break;
                case 4:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_KrakianJungle));
                    await About_NLC();
                    break;
                case 5:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_JohnStory));
                    await About_NLC();
                    break;
                case 6:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_StreetSigns));
                    await About_NLC();
                    break;
                case 7:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_JackMasque));
                    await About_NLC();
                    break;
                case 8:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_LitaLawless));
                    await About_NLC();
                    break;
                case 9:
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_NewBoroughs));
                    await About_NLC();
                    break;
                case 10:
                    if (getLevel() >= minlevel)
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_QuizOffer));
                        await startQuest(4900);
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201050_EagerQuiz));
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
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201056_ReturnToNlc, fee)))
                {
                    if (getMeso() >= fee)
                    {
                        await gainMeso(-fee);
                        await warp(600000000);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201056_NotEnough));
                    }
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201056_GoHauntedHouse, fee)))
                {
                    if (getMeso() >= fee)
                    {
                        await gainMeso(-fee);
                        await warp(682000000, 0);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201056_NotEnough));
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                    return;
                }

                var target = subway.GetDestinationMapName(getPlayer());
                if (target == null)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                    return;
                }

                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201057_SubwayPrompt, p.Value.TicketPrice, p.Value.TicketItemId)))
                {
                    if (!canHold(p.Value.TicketItemId))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.SlotFull, ScriptMessage.ETC));
                    }
                    else if (getMeso() >= p.Value.TicketPrice)
                    {
                        await gainMeso(-p.Value.TicketPrice);
                        await gainItem(p.Value.TicketItemId, 1);
                        await SayNext(this.GetTalkMessage(ScriptMessage.Npc9201057_HereIsTicket));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.MesoNotEnough));
                    }
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201057_ConfirmLeaveTrain)))
                {
                    await WarpOut();
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201099_LookingAtMe));
            }

        }

        // Npc: 9201100 
        public async Task Fallen_Woods()
        {
            if (getQuestStatus(8224) == 2)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201100_FellowClan));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201100_ClanLeader));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201103_ExpeditionInfo));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201103_ExpeditionRequirement));
            }
        }

        // Npc: 9201104 
        public async Task Masteria_Sage01()
        {
            if (getMapId() == 610020000)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201104_BraveAdventurer));
            }
            else if (getMapId() == 610020003)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201104_HintStatue));
            }
            else if (getMapId() == 610020004)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201104_HintSigil));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201104_KeepOrdeal));
            }
        }

        // Npc: 9201105 
        public async Task Masteria_Sage02()
        {
            if (getMapId() == 610020005)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201105_KeepAhead));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201104_KeepOrdeal));
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
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201127_WarpPrompt, map, c.CurrentCulture.GetJobName(job))))
                    {
                        await warp(map, 0);
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201127_WarpRequirement, c.CurrentCulture.GetJobName(job), requiredLevel, getFirstJobStatRequirement(jobType)));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201127_OnlyBeginners));
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
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201127_EnterPrompt, mapId)))
                    {
                        foreach (var item in enterConsumeItems)
                        {
                            await gainItem(item, -1);
                        }

                        switch (mapId)
                        {
                            case 677000000:
                                await SpawnBossIfAbsent(mapId, 9400612, new Point(461, 61), this.GetTalkMessage(ScriptMessage.BossAppear, "Marbas"));
                                break;
                            case 677000002:
                                await SpawnBossIfAbsent(mapId, 9400610, new Point(467, 0), this.GetTalkMessage(ScriptMessage.BossAppear, "Amdusias"));
                                break;
                            case 677000004:
                                await SpawnBossIfAbsent(mapId, 9400609, new Point(201, 80), this.GetTalkMessage(ScriptMessage.BossAppear, "Andras"));
                                break;
                            case 677000006:
                                await SpawnBossIfAbsent(mapId, 9400611, new Point(171, 50), this.GetTalkMessage(ScriptMessage.BossAppear, "Crocell"));
                                break;
                            case 677000008:
                                await SpawnBossIfAbsent(mapId, 9400613, new Point(251, -841), this.GetTalkMessage(ScriptMessage.BossAppear, "Valefor"));
                                break;
                            default:
                                break;
                        }
                        await warp(mapId, 0);

                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201133_LockedByEmblem));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201133_Blocked));
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
                await getPlayer().TypedMessage(NoticeType.Pink, notice);
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
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201133_ConfirmMove, 677000012)))
                    {
                        await warp(677000012, 0);
                    }
                }
                else
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201133_ConfirmExit)))
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
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201133_GasMask));
                        return;
                    }

                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9201133_ConfirmMove, 677000010)))
                    {
                        await warp(677000010, 0);
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9201133_Blocked));
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
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9220005_GoSnowSprinkler)))
                {
                    await warp(209080000, 0);
                }
            }
            else if (getChar().getMapId() == 209080000)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9220005_ReturnHappyville)))
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9220019_Hi));
            }
            else if (mapId == 674030300)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9220019_TreasureRoom));
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9220019_ConfirmReturn)))
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
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9220020_WaitLeader));
                return;
            }

            var eim = getEventInstance()!;
            if (eim.getIntProperty("statusStg1") == 1)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9220020_TunnelBoss));
            }
            else
            {
                if (haveItem(4032118, 15))
                {
                    await gainItem(4032118, -15);
                    eim.setIntProperty("statusStg1", 1);
                    await eim.showClearEffect();
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9220020_GotLetters));
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9220020_BringLetters));
                }
            }
        }

        // Npc: 9270017 
        public async Task goback_kerning()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9270017_ConfirmLeave)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9270017_NoRefund));
                await WarpOut();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9270018_TicketFailed));
            }
        }

        // Npc: 9270018 
        public async Task goback_cbd()
        {
            var airPlane = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(AirPlane)) ?? throw new BusinessNotsupportException(nameof(AirPlane));
            var target = airPlane.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            if (airPlane.IsTransporting(getMap()))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9270018_SitDownSingapore));
                return;
            }
            else if (airPlane.IsWaiting(getMap()))
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9270018_ConfirmLeave)))
                {
                    await WarpOut();
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc9270018_TicketFailed));
                }
            }
        }

        async Task ContiMoveAirPlane()
        {
            var airPlane = c.CurrentServer.ContiMoves.GetValueOrDefault(nameof(AirPlane)) as AirPlane ?? throw new BusinessNotsupportException(nameof(AirPlane));
            var target = airPlane.GetDestinationMapName(getPlayer());
            if (target == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

            var t = airPlane.GetTicket(getPlayer());
            if (t == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Ticket_CannotTalk));
                return;
            }

                        var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Npc9270018_PurchasePrompt, getNpc(), target));
            switch (option)
            {
                case 0:
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9270018_ConfirmPurchase, t.Value.TicketPrice)))
                    {
                        if (!canHold(t.Value.TicketItemId))
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.SlotFull, this.GetTalkMessage(ScriptMessage.ETC)));
                            return;
                        }
                        if (getMeso() < t.Value.TicketPrice)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.MesoNotEnough));
                            return;
                        }

                        await gainMeso(-t.Value.TicketPrice);
                        await gainItem(t.Value.TicketItemId);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9270018_PurchaseSuccess));

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
                            await SayOK(this.GetTalkMessage(ScriptMessage.Npc9270018_PlaneGone));
                        }
                        return;
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Npc9270018_NeedTicket, t.Value.TicketItemId));
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9310000_ShanghaiFee, fee)))
            {
                if (getMeso() < fee)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9310000_CheckFee, fee));
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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9310013_PerionFee, fee)))
            {
                if (getMeso() < fee)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc9310000_CheckFee, fee));
                }
                else
                {
                    await gainMeso(-fee);
                    await warp(102000000);
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc9310013_MissPerion));
            }
        }

        // Npc: 9310058 
        public async Task Jump_event()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Npc9310058_Welcome));
        }

        // Npc: 9901000, 9901001, 9901002, 9901003, 9901004, 9901005, 9901006, 9901007, 9901008, 9901009, 9901010, 9901011 ...
        public async Task rank_user()
        {
            var pnpc = getPlayerNPCByScriptid(getNpc());
            if (pnpc is PlayerNpc playerNpc)
            {
                var branchJobName = GetJobName(playerNpc.Job);
                var rankStr = this.GetTalkMessage(ScriptMessage.Npc9901000_RankIntro,
                    playerNpc.Name, branchJobName, c.CurrentCulture.Ordinal(playerNpc.JobRank), c.CurrentCulture.Ordinal(playerNpc.OverallRank));
                await SayOK(rankStr);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc9901000_Hello));
            }
        }

        async Task EnterTranningMap(int start)
        {
            if (getLevel() >= 20)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Plain2_OnlyCharacters));
                return;
            }

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Plain2_WantEnterTraining),
                Enumerable.Range(0, 5).Select(i => this.GetTalkMessage(ScriptMessage.Npc9901000_TrainingCenter, i))
                );
            await warp(start + option, 0);
        }


        public async Task MaybeItsGrendel_end()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Npc9901000_ShadowPerson),
                this.GetTalkMessage(ScriptMessage.Npc9901000_SortThoughts2)
                ]);
        }

        // Npc: 2103008
        /// <summary>
        /// <see cref="PortalScript.thief_in1"/>
        /// </summary>
        /// <returns></returns>
        public async Task thief_in2()
        {
            var pwd = await AskText(this.GetTalkMessage(ScriptMessage.Npc2103008_SecretCode));
            if (pwd == this.GetTalkMessage(ScriptMessage.Shared_OpenSesame))
            {
                if (isQuestCompleted(3925))
                {
                    await warp(260010402, 1);
                }
                else
                {
                    await this.Pink(this.GetTalkMessage(ScriptMessage.Npc2103008_CodeAccepted));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103008_Error));
            }
        }

        // Npc: 2043000 — Papulatus Time Leap
        public async Task s4time()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.WrongWorld_Return));
            await warp(220080000);
        }

        // Npc: 2007
        public async Task tutorialSkip()
        {
            if (getPlayer().Level <= 10)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc2007_SkipTutorial)))
                {
                    await warp(MapId.LITH_HARBOUR, 0);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Npc2007_HaveNiceTrip));
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Npc2007_HaveNiceTrip));
            }
        }

        // Npc: 2096000 — 武陵修炼场告示牌
        public async Task sca_dollBear()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Npc2096000_TrainingMaps));
        }

        // Npc: 9900001 — GM 升级 NPC
        public async Task levelUP2()
        {
            if (getPlayer().gmLevel() <= 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Salo_T43));
                return;
            }

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Npc9900001_WantLevelUp)))
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
        private async Task SpawnHonorPlayerNpc(Func<int, bool> hasRequiredJob, string noQualifyMessage,
            string honorOffer, string honorFee, string notEnoughMeso, string hallFull, string hereYouGo)
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

            var message = honorOffer;
            if (HONOR_NPC_FEE > 0)
            {
                message += honorFee;
            }

            if (!await AskYesNo(message))
            {
                return;
            }

            if (getMeso() < HONOR_NPC_FEE)
            {
                await SayOK(notEnoughMeso);
                return;
            }

            if (!await canSpawnPlayerNpc(honorMapId))
            {
                await SayOK(hallFull);
                return;
            }

            c.CurrentServer.NodeService.PlayerNPCService.SpawnPlayerNPCByHonor(getPlayer());
            await gainMeso(-HONOR_NPC_FEE);
            await SayOK(hereYouGo);
        }

        // Npc: 1102003 — 骑士团名人堂（职业类型 11~15）
        public async Task cygnus_lv120()
        {
            await SpawnHonorPlayerNpc(jobType => jobType >= 11 && jobType <= 15,
                this.GetTalkMessage(ScriptMessage.Npc1102003_Qualify),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HonorOffer),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HonorFee, numberWithCommas(HONOR_NPC_FEE)),
                this.GetTalkMessage(ScriptMessage.Npc1102003_NotEnoughMeso),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HallFull),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HereYouGo));
        }

        // Npc: 1202010 — 战神名人堂（职业类型 21）
        public async Task aran_lv200()
        {
            await SpawnHonorPlayerNpc(jobType => jobType == 21,
                this.GetTalkMessage(ScriptMessage.Npc1202010_Qualify),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HonorOffer),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HonorFee, numberWithCommas(HONOR_NPC_FEE)),
                this.GetTalkMessage(ScriptMessage.Npc1102003_NotEnoughMeso),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HallFull),
                this.GetTalkMessage(ScriptMessage.Npc1102003_HereYouGo));
        }
    }

}