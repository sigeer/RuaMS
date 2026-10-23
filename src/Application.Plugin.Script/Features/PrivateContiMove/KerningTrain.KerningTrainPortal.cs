using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.PrivateContiMove
{
    internal sealed class KerningTrainPortal : PortalPlayerInteraction
    {
        public KerningTrainPortal(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 103000310 | pt: 7
        public async Task<bool> Depart_ToKerning()
        {
            var contiMove = GetEventManager(KerningTrainFeature.EventName);
            if (await contiMove.StartInstance(getPlayer()) != CreateInstanceResult.Success)
            {
                await Pink("The passenger wagon is already full. Try again a bit later.");
                return false;
            }

            await playPortalSound();
            return true;
        }
    }
}
