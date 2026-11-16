using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Items;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Zakum
{
    internal sealed class ZakumPQNpcHandler : NpcScriptBase
    {
        public ZakumPQNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2030008
        public async Task Zakum00()
        {
            if (haveItem(4001109, 1))
            {
                await warp(921100000, "out00");
                return;
            }

            var em = GetEventManager(ZakumPQ.PartyQuestEventName);
            if (em == null)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_PQError));
                return;
            }

            if (IsQuestNotStarted(ZakumFeature.QuestId))
            {
                if (getLevel() >= em.MinLevel)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_Elder_Intro));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_Elder_Beware));
                }
                return;
            }

            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Zakum_StageMenu, em.Template.GetRequirementDescription(c)));

            if (selection == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else if (selection == 1)
            {
                if (haveItem(4031061) && !haveItem(4031062))
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_ConfirmFirstStage)))
                    {
                        await warp(280020000, 0);
                    }
                }
                else
                {
                    if (haveItem(4031062))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyHaveBreath));
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_CompleteEarlierTrials));
                    }
                }
            }
            else
            {
                if (haveItem(4031061) && haveItem(4031062))
                {
                    if (!haveItem(4000082, 30))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedFireOre30));
                    }
                    else
                    {
                        await completeQuest(ZakumFeature.QuestId);
                        await gainItem(4031061, -1);
                        await gainItem(4031062, -1);
                        await gainItem(4000082, -30);
                        await gainItem(4001017, 5);
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_TrialsCompleted));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_LackItems));
                }
            }
        }


        // Npc: 2030010
        public async Task Zakum06()
        {
            if (getMapId() == 280030000)
            {
                if (!GetEventInstanceTrust().isEventCleared())
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_LeaveRestart)))
                    {
                        await warp(ZakumPQ.ExitMap);
                    }
                }
                else
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_LeaveAfterVictory)))
                    {
                        await warp(ZakumPQ.ExitMap);
                    }
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_LeaveRestart)))
                {
                    await warp(ZakumPQ.ExitMap);
                }
            }
        }


        // Npc: 2030011
        public async Task Zakum04()
        {
            await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_SeeYouNextTime));
            await warp(ZakumPQ.ExitMap);
            await removeAll(4001015);
            await removeAll(4001016);
            await removeAll(4001018);
        }


        // Npc: 2032002
        public async Task Zakum01()
        {
            var eim = getPlayer().getEventInstance();

            if (!eim.isEventCleared())
            {
                var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.Zakum_MineMenu));

                if (selection == 0)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_FindFireOre));
                }
                else if (selection == 1)
                {
                    if (!isEventLeader())
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_LeaderBringMaterials));
                        return;
                    }

                    if (!haveItem(4001018))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_BringFireOre));
                    }
                    else
                    {
                        var gotAllDocs = haveItem(4001015, 30);
                        if (!gotAllDocs)
                        {
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_ConfirmWithOre)))
                            {
                                await gainItem(4001018, -1);
                                await eim.giveEventPlayersExp(12000);
                                await eim.clearPQ();
                            }
                        }
                        else
                        {
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_ConfirmWithOreAndDocs)))
                            {
                                await gainItem(4001018, -1);
                                await gainItem(4001015, -30);
                                eim.setProperty("gotDocuments", 1);
                                await eim.giveEventPlayersExp(20000);
                                await eim.clearPQ();
                            }
                        }
                    }
                }
                else if (selection == 2)
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Zakum_ConfirmExit)))
                    {
                        await warp(ZakumPQ.ExitMap);
                    }
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Zakum_ReceivePrize));

                if (eim.getIntProperty("gotDocuments") == 1)
                {
                    if (eim.gridCheck(getPlayer()) == -1)
                    {
                        if (CanHoldAll([new ItemQuantity(2030007, 5), new ItemQuantity(4031061, 1)]))
                        {
                            await gainItem(2030007, 5);
                            await gainItem(4031061, 1);
                            eim.gridInsert(getPlayer(), 1);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedInventoryRoom));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyReceived));
                    }
                }
                else
                {
                    if (eim.gridCheck(getPlayer()) == -1)
                    {
                        if (canHold(4031061, 1))
                        {
                            await gainItem(4031061, 1);
                            eim.gridInsert(getPlayer(), 1);
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_NeedInventoryRoom));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_AlreadyReceived));
                    }
                }
            }
        }


        // Npc: 2032003
        public async Task Zakum02()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Zakum_GetBreathOfFire));

            if (!canHold(4031062))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Zakum_FreeSlotForItem));
                return;
            }

            await SayNext(this.GetTalkMessage(ScriptMessage.Zakum_TimeToLeave));

            await gainItem(4031062, 1);
            await gainExp((int)(10000 * getPlayer().getExpRate()));
            await warp(ZakumPQ.ExitMap);
        }
    }
}
