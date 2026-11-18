using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.OrbisPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2013000 — Orbis PQ entrance (Wonky)
        public async Task party3_enter()
        {
            var em = GetEventManager(OrbisPQFeature.EventName);
            if (em == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013000_PQError));
                return;
            }

            if (getMapId() == 200080101)
            {
                if (isUsingOldPqNpcStyle())
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    return;
                }

                var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013000_EntryMenu,
                    em.Template.GetRequirementDescription(getClient())));
                if (sel == 0)
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
                else if (sel == 1)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013000_Intro));
                }
                else
                {
                    if (!haveItem(1082232) && haveItem(4001158, 10))
                    {
                        await gainItem(1082232, 1);
                        await gainItem(4001158, -10);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013000_CannotClaim));
                    }
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013000_ConfirmDropout)))
                    await warp(920011200);
            }
        }


        // Npc: 2013001 — Orbis PQ stage manager (Chamberlain Eak)
        public async Task party3_play()
        {
            var eim = getEventInstance();
            var mapId = getMapId();

            if (mapId == 920011200)
            {
                await warp(200080101);
                return;
            }

            if (eim == null)
                return;

            if (!isEventLeader())
            {
                if (mapId == 920010000)
                    await warp(920010000, 2);
                else
                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_OnlyLeader));
                return;
            }

            switch (mapId)
            {
                case 920010000:
                    if (eim.getIntProperty("statusStg0") != 1)
                    {
                        await eim.warpEventTeamToMapSpawnPoint(920010000, 2);
                        await eim.giveEventPlayersExp(3500);
                        await clearStage(eim, 0);
                        await SayNext(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_Intro, npc));
                    }
                    else
                    {
                        await warp(920010000, 2);
                    }
                    break;

                case 920010100:
                    if (isStatueComplete())
                    {
                        if (eim.getIntProperty("statusStg7") == 0)
                            await eim.warpEventTeam(920010800);
                        else if (eim.getIntProperty("statusStg8") == 0)
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_BroughtRoot));
                        else
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_ThanksSaving));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_GatherSix));
                    }
                    break;

                case 920010200:
                    if (!haveItem(4001050, 30))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_Gather30));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_GotAll1st));
                        await removeAll(4001050);
                        await gainItem(4001044, 1);
                        await eim.giveEventPlayersExp(3500);
                        await clearStage(eim, 1);
                    }
                    break;

                case 920010300:
                    if (eim.getIntProperty("statusStg2") != 1)
                    {
                        if (getMap().countMonsters() == 0 && getMap().countItems() == 0)
                        {
                            if (canHold(4001045))
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_Found2nd));
                                await gainItem(4001045, 1);
                                await eim.giveEventPlayersExp(3500);
                                await clearStage(eim, 2);
                                eim.setProperty("statusStg2", "1");
                            }
                            else
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_Found2ndNoSlot));
                            }
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_Find2nd));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_WellDone));
                    }
                    break;

                case 920010400:
                    var stg3 = eim.getIntProperty("statusStg3");
                    if (stg3 == 0)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_MusicHint));
                    }
                    else if (stg3 == 1)
                    {
                        getMap().getReactorByName("stone3")?.forceHitReactor(1);
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_MusicCorrect));
                        await eim.giveEventPlayersExp(3500);
                        await clearStage(eim, 3);
                        eim.setProperty("statusStg3", "2");
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_ThankYou));
                    }
                    break;

                case 920010500:
                    var stg4 = eim.getIntProperty("statusStg4");
                    if (stg4 == 0)
                    {
                        var total = 3;
                        for (var i = 0; i < 2; i++)
                        {
                            var rnd = Random.Shared.Next(total + 1);
                            total -= rnd;
                            eim.setProperty("stage4_" + i, "" + rnd);
                        }
                        eim.setProperty("stage4_2", "" + total);
                        eim.setProperty("statusStg4", "1");
                    }

                    if (eim.getIntProperty("statusStg4") == 1)
                    {
                        var players = new int[3];
                        var sum = 0;
                        for (var i = 0; i < 3; i++)
                        {
                            players[i] = getMap().getNumPlayersInArea(i);
                            sum += players[i];
                        }

                        if (sum != 3)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_ExactlyThree));
                        }
                        else
                        {
                            var correct = 0;
                            for (var i = 0; i < 3; i++)
                            {
                                if (eim.getProperty("stage4_" + i) == "" + players[i])
                                    correct++;
                            }

                            if (correct == 3)
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_ComboRight));
                                getMap().getReactorByName("stone4")?.forceHitReactor(1);
                                await eim.giveEventPlayersExp(3500);
                                await clearStage(eim, 4);
                            }
                            else
                            {
                                await eim.showWrongEffect();
                                if (correct > 0)
                                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_OnePlatformRight));
                                else
                                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_AllPlatformsWrong));
                            }
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_FetchOthers));
                    }
                    break;

                case 920010600:
                    if (eim.getIntProperty("statusStg5") == 0)
                    {
                        if (!haveItem(4001052, 40))
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_Gather40));
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_GotAll5th));
                            await removeAll(4001052);
                            await gainItem(4001048, 1);
                            await eim.giveEventPlayersExp(3500);
                            await clearStage(eim, 5);
                            eim.setIntProperty("statusStg5", 1);
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_GotAllHere));
                    }
                    break;

                case 920010700:
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        var rnd1 = Random.Shared.Next(5);
                        var rnd2 = Random.Shared.Next(5);
                        while (rnd2 == rnd1)
                            rnd2 = Random.Shared.Next(5);

                        if (rnd1 > rnd2)
                            (rnd1, rnd2) = (rnd2, rnd1);

                        var comb = "";
                        for (var i = 0; i < rnd1; i++) comb += "0";
                        comb += "1";
                        for (var i = rnd1 + 1; i < rnd2; i++) comb += "0";
                        comb += "1";
                        for (var i = rnd2 + 1; i < 5; i++) comb += "0";

                        eim.setProperty("stage6_c", "" + comb);
                        eim.setProperty("statusStg6", "1");
                    }

                    if (eim.getIntProperty("statusStg6") == 1)
                    {
                        var comb = eim.getProperty("stage6_c")!;
                        var react = "";
                        var pushed = 0;
                        for (var i = 1; i <= 5; i++)
                        {
                            if (getMap().getReactorByName("" + i)?.getState() > 0)
                            {
                                react += "1";
                                pushed++;
                            }
                            else
                            {
                                react += "0";
                            }
                        }

                        if (pushed != 2)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_ExactlyTwoLevers));
                        }
                        else
                        {
                            var correct = 0;
                            var pshCorrect = 0;
                            for (var i = 0; i < 5; i++)
                            {
                                if (react[i] == comb[i])
                                {
                                    correct++;
                                    if (react[i] == '1')
                                        pshCorrect++;
                                }
                            }

                            if (correct == 5)
                            {
                                await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_LeverComboRight));
                                getMap().getReactorByName("stone6")?.forceHitReactor(1);
                                await eim.giveEventPlayersExp(3500);
                                await clearStage(eim, 6);
                            }
                            else
                            {
                                await eim.showWrongEffect();
                                if (pshCorrect >= 1)
                                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_OneLeverRight));
                                else
                                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_BothLeversWrong));
                            }
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_NicelyDone));
                    }
                    break;

                case 920010800:
                    await SayNext(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_DefeatPapaPixie));
                    break;

                case 920010900:
                    if (eim.getProperty("statusStg8") == "1")
                        await SayNext(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_JailIntro));
                    else
                        await SayNext(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_JailNoPieces));
                    break;

                case 920011000:
                    if (getMap().countMonsters() > 0)
                        await SayNext(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013001_HiddenRoom));
                    else
                        await warp(920011100, "st00");
                    break;
            }
        }

        private bool isStatueComplete()
        {
            for (var i = 1; i <= 6; i++)
            {
                if (getMap().getReactorByName("scar" + i)?.getState() < 1)
                    return false;
            }
            return true;
        }

        private static async Task clearStage(AbstractEventInstanceManager eim, int stage)
        {
            eim.setProperty("statusStg" + stage, "1");
            await eim.showClearEffect(true);
        }


        // Npc: 2013002 — Minerva the Goddess (reward NPC)
        public async Task party3_minerva()
        {
            var mapId = getMapId();

            if (mapId == 920010100)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013002_JailUnlocked)))
                    await warp(920011300, 0);
            }
            else if (mapId == 920011100)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013002_ReadyToExit)))
                    await warp(920011300, 0);
            }
            else if (mapId == 920011300)
            {
                var eim = getEventInstance();
                if (eim == null) return;

                await SayNext(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013002_ThanksRescue));

                var result = await eim.GiveClearReward(getPlayer());
                if (result != ClaimRewardResult.Success)
                    await SayOK(this.GetTalkMessage(ScriptMessage.OrbisPQ_2013002_NeedInventorySlot));
                else
                    await warp(200080101, 0);
            }
        }
    }
}
