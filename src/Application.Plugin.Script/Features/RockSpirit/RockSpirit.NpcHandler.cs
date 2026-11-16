using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.RockSpirit
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1052125 
        public async Task Depart_topFloorIn()
        {

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.RockSpirit_HoldAccess));
            switch (option)
            {
                case 0:
                    if (isQuestStarted(2286) || isQuestStarted(2287) || isQuestStarted(2288))
                    {
                        var em = GetEventManager(RockSpiritFeature.EventName);
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                        return;
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.RockSpirit_HearAnything));
                    }
                    break;
                case 1:
                    if (isQuestCompleted(2290))
                    {
                        if (getPlayer().getLevel() > 50)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.RockSpirit_VipArea));
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.RockSpirit_VipAreaOnly));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.RockSpirit_VipHa));
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
