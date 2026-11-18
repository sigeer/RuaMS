using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.YaoSengPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9310039 
        public async Task q8535s()
        {
            const int recruitMap = 702070400;
            const short questId = 8534;

            if (getMapId() != recruitMap)
                return;

            if (questId > 0 && !isQuestCompleted(questId))
            {
                await SayOK("...");
                return;
            }

            var em = GetEventManager(YaoSengPQFeature.EventName);

            var sel = await AskMenu(this.GetTalkMessage(ScriptMessage.YaoSeng_PartyQuest,  (em.Template?.GetRequirementDescription(c) ?? "")), [
                this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
            ]);

            if (sel == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer(), 1), c));
            }
            else if (sel == 1)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.YaoSeng_PartyQuestShaolin));
            }
        }

        // Npc: 9310044 
        public async Task outshaolinBoss()
        {
            var mapID_out = 702070400;
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.WuGong_LeavePrompt, getMapId(), mapID_out)))
            {
                await warp(mapID_out);
            }
        }
    }
}
