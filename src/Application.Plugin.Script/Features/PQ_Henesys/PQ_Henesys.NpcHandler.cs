using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using Application.Plugin.Script;

namespace Application.Plugin.Script.Features.PQ_Henesys
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1012112
        public async Task moonrabbit()
        {
            if (getMapId() == 100000200)
            {
                var em = GetEventManager(PQ_HenesysFeature.EventName);

                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.HenesysPQ_EnterMenu, em.Template.GetRequirementDescription(c), npc));
                switch (option)
                {
                    case 0:
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                        break;
                    case 1:
                        await SayOK(this.GetTalkMessage(ScriptMessage.HenesysPQ_Intro));
                        break;
                    case 2:
                        if (hasItem(4001101, 20) && canHold(1002798))
                        {
                            await gainItem(4001101, -20);
                            await gainItem(1002798, 1);

                            await SayNext(this.GetTalkMessage(ScriptMessage.Redeem_Success));
                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Redeem_NotEnough, "#t4001101#"));
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (getMapId() == 910010100)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.HenesysPQ_Complete)))
                {
                    var eim = GetEventInstanceTrust();
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(100000200);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Redeem_InventoryFull));
                    }
                    return;
                }
            }
            else if (getMapId() == 910010400)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AreYouReturningMap, this.GetTalkMessage(ScriptMessage.Henesys))))
                {
                    var eim = GetEventInstanceTrust();
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(100000200);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Redeem_InventoryFull));
                    }
                    return;
                }
            }
        }

        // Npc: 1012113
        public async Task moonrabbit_bonus()
        {
            if (getMapId() == 910010100)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.HenesysPQ_ImTommy, npc));

                if (isEventLeader())
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.HenesysPQ_ThinkAbout));
                    var eim = GetEventInstanceTrust();
                    await eim.startEventTimer(5 * 60000);
                    await eim.warpEventTeam(910010200);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.HenesysPQ_InteressedTell));
                }
            }
            else if (getMapId() == 910010200)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.HenesysPQ_LikeExit)))
                {
                    await warp(910010400);
                }
            }
            else if (getMapId() == 910010300)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.HenesysPQ_WarpedOut));
                await warp(100000200);
            }
        }

        // Npc: 1012114
        public async Task moonrabbit_tiger()
        {
            int option;
            if (isEventLeader())
            {
                option = await AskMenu(this.GetTalkMessage(ScriptMessage.HenesysPQ_TutorialTalk0, npc));
            }
            else
            {
                option = await AskMenu(this.GetTalkMessage(ScriptMessage.HenesysPQ_TutorialTalk1, npc));
            }
            switch (option)
            {
                case 0:
                    await SaySpeech([
                    this.GetTalkMessage(ScriptMessage.HenesysPQ_Tutorial0),
                    this.GetTalkMessage(ScriptMessage.HenesysPQ_Tutorial0),
                    this.GetTalkMessage(ScriptMessage.HenesysPQ_Tutorial1),
                    this.GetTalkMessage(ScriptMessage.HenesysPQ_Tutorial2),
                    this.GetTalkMessage(ScriptMessage.HenesysPQ_Tutorial3)]);
                    break;
                case 1:
                    if (haveItem(4001101, 10))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.HenesysPQ_CommitTask_Success));
                        if (haveItem(4001101, 10))
                        {
                            await gainItem(4001101, int.MinValue);

                            var eim = GetEventInstanceTrust();
                            eim.setProperty(1 + "stageclear", "true");
                            await LudiPQ.NpcHandler.ClearLudiPQStage(eim, getMapId());

                            var map = await eim.getMapInstance(getPlayer().getMapId());
                            await map.killAllMonstersNotFriendly();

                            await eim.clearPQ();
                            return;
                        }


                    }
                    await SayOK(this.GetTalkMessage(ScriptMessage.HenesysPQ_CommitTask_Fail));
                    break;
                case 2:
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.AreYouReturning)))
                    {
                        await warp(910010300);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.HenesysPQ_TutorialTalk2));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
