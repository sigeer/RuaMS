using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Exped_PinkBean
{
    /// <summary>
    /// Exped_PinkBean：Pinkin、PPinkOut。
    /// </summary>
    internal sealed class Exped_PinkBeanPortalHandler : PortalPlayerInteraction
    {
        public Exped_PinkBeanPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 270050200 | pt: 7
        public async Task<bool> Pinkin()
        {

            await playPortalSound();
            await warp(270050100);
            return true;
        }
        // Map: 270050300 | pt: 7
        public async Task<bool> PPinkOut()
        {

            await playPortalSound();
            await warp(270050000);
            return true;
        }
    }
}
