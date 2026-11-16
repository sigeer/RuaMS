using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.EllinPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2133000 
        public async Task party6_entry()
        {
            var em = GetEventManager(EllinPQFeature.EventName);
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.EllinPQ_PartyQuest) + em.Template.GetRequirementDescription(c) + this.GetTalkMessage(ScriptMessage.EllinPQ_LikeAssemble),
                [
                this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo),
                this.GetTalkMessage(ScriptMessage.EllinPQ_LikeClaim)]);

            switch (option)
            {
                case 0:
                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                    break;
                default:
                    break;
            }
        }


        // Npc: 2133001 
        public async Task party6_elin()
        {
            var eim = GetEventInstanceTrust();

            var curStage = eim.ClearedMaps.GetValueOrDefault(getMapId(), StageStatus.NotStarted);
            if (getMapId() == 930000000)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.EllinPQ_StageIntro));
                return;
            }
            else if (getMapId() == 930000100)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.EllinPQ_ClearContaminated));
                return;
            }

            else if (getMapId() == 930000300)
            {
                if (curStage == StageStatus.NotStarted)
                {
                    await eim.showClearEffect(getMapId());
                    eim.ClearedMaps[getMapId()] = StageStatus.Completed;
                }

                await SayNext(this.GetTalkMessage(ScriptMessage.EllinPQ_GreatFound));
                await eim.warpEventTeam(930000400);
            }
            else if (getMapId() == 930000400)
            {
                if (haveItem(4001169, 20))
                {
                    if (isEventLeader())
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.EllinPQ_HaveBrought));

                        if (haveItem(4001169, 20) && isEventLeader())
                        {
                            eim.ClearedMaps[getMapId()] = StageStatus.Completed;
                            await gainItem(4001169, -20);
                            await eim.warpEventTeam(930000500);
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.EllinPQ_HaveBroughtYoure));
                        return;
                    }
                }
                else if (!haveItem(2270004))
                {
                    if (canHold(2270004, 10))
                    {
                        await gainItem(2270004, 10);
                        await SayOK(this.GetTalkMessage(ScriptMessage.EllinPQ_TakeFirst));
                        return;

                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.EllinPQ_PleaseMake));
                        return;
                    }
                }
                else
                {

                }
            }
        }


        // Npc: 2133002 
        public async Task party6_giveUp()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.EllinPQ_LikeExit)))
            {
                await WarpOut();
            }
        }
        // Npc: 2133004 
        public async Task party6_spra()
        {
            if (!haveItem(4001163) || !isEventLeader())
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.EllinPQ_LetParty)))
                {
                    await WarpOut();
                }
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.EllinPQ_GreatHave));
                await GetEventInstanceTrust().warpEventTeam(930000600);
            }
        }
    }
}
