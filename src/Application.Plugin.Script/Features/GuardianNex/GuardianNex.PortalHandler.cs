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
                        await Pink("You already faced Nex. Complete your mission.");
                        return false;
                    }

                    if (await nex.StartInstance(getPlayer(), lobbyId: i) != CreateInstanceResult.Success)
                    {
                        await Pink("Someone is already challenging Nex. Wait for them to finish before you enter.");
                        return false;
                    }
                    else
                    {
                        await playPortalSound();
                        return true;
                    }
                }
            }

            await Pink("A mysterious force won't let you in.");
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
