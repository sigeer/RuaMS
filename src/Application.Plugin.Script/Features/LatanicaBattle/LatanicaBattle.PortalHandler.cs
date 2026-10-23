using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.LatanicaBattle
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 541010060 | pt: 7
        public async Task<bool> captinsg00()
        {

            if (!haveItem(4000381))
            {
                await Pink("You do not have White Essence.");
                return false;
            }
            else
            {
                var em = GetEventManager(LatanicaBattleFeature.EventName);
                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await playPortalSound();
                    return true;
                }
                else
                {
                    await Pink(em.HandleCreateInstanceResult(r, c));
                    return false;
                }
            }
        }
    }
}
