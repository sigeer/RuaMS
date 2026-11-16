using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.ElementalBattle
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2041023 
        public async Task s4efreet()
        {
            if (!(isQuestCompleted(6316) && (isQuestStarted(6225) || isQuestStarted(6315))))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.ElemBattle_SeemsHave));
                return;
            }

            var em = GetEventManager(ElementalBattleFeature.EventName);
            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.ElemBattle_EnterMenu, em.Template.GetRequirementDescription(c)), [
                this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
            ]);

            if (option == 0)
            {
                var r = await em.StartInstance(getPlayer());
                await SayOK(em.HandleCreateInstanceResult(r, c));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.ElemBattle_PartyQuest));
            }
        }
    }
}
