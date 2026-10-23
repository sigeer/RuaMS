using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.PQ_Henesys
{
    /// <summary>
    /// PQ_Henesys：enterAchter。
    /// </summary>
    internal sealed class PQ_HenesysPortalHandler : PortalPlayerInteraction
    {
        public PQ_HenesysPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 100000200 | pt: 7
        public async Task<bool> enterAchter()
        {

            await playPortalSound();
            await warp(100000201, "out02");
            await playPortalSound();
            return true;
        }
    }
}
