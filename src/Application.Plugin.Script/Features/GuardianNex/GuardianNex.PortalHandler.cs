using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.GuardianNex
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 240070000 | pt: 8
        public async Task<bool> TD_neo_inTree()
        {

            var nex = GetEventManager(GuardianNexFeature.EventName);
            int[] quests = [3719, 3724, 3730, 3736, 3742, 3748];
            int[] mobs = [7120100, 7120101, 7120102, 8120100, 8120101, 8140510];

            for (var i = 0; i < quests.Length; i++)
            {
                if (isQuestActive(quests[i]))
                {
                    if (getQuestProgressInt(quests[i], mobs[i]) != 0)
                    {
                        await this.Pink(this.GetTalkMessage(ScriptMessage.GuardianNexPortalHandler_AlreadyFaced));
                        return false;
                    }

                    if (await nex.StartInstance(getPlayer(), lobbyId: i) != CreateInstanceResult.Success)
                    {
                        await this.Pink(this.GetTalkMessage(ScriptMessage.GuardianNexPortalHandler_SomeoneAlready));
                        return false;
                    }
                    else
                    {
                        await playPortalSound();
                        return true;
                    }
                }
            }

            await this.Pink(this.GetTalkMessage(ScriptMessage.PortalScript_MysteriousForce));
            return false;
        }

        // Map: 240070000 | pt: 8
        public async Task<bool> TD_chat_enter()
        {

            await openNpc(2083006);
            return false;
        }
    }
}
