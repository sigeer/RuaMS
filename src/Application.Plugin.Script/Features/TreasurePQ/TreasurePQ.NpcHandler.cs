using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.TreasurePQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9220018 
        public async Task guyfawkes_ch()
        {
            var em = GetEventManager(TreasurePQFeature.EventName);
            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.TreasurePQ_PartyQuest, (em.Template?.GetRequirementDescription(c) ?? "")), [
                    this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                    this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
                ]);

            if (sel == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else if (sel == 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.TreasurePQ_PartyQuestMvs));
            }
        }


    }
}
