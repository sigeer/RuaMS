using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.NineSpirit
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 240040610 | pt: 7
        public async Task<bool> dragonNest()
        {

            if (isQuestCompleted(3706))
            {
                await playPortalSound();
                await warp(240040612, "out00");
                return true;
            }
            else if (isQuestStarted(100203) || getPlayer().haveItem(4001094))
            {
                var em = GetEventManager(NineSpiritFeature.EventName);
                if (await em.StartInstance(getPlayer()) != Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await Pink("There is currently someone in this map, come back later.");
                    return false;
                }
                else
                {
                    await playPortalSound();
                    return true;
                }
            }
            else
            {
                await Pink("A strange force is blocking you from entering.");
                return false;
            }
        }
    }
}
