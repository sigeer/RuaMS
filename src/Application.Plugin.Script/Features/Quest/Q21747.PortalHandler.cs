using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21747PortalHandler : PortalPlayerInteraction
    {
        public Q21747PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 925020000 | pt: 8
        public async Task<bool> enterBackStreet()
        {

            if (isQuestActive(21747) || isQuestActive(21744) && isQuestCompleted(21745))
            {
                await playPortalSound();
                await warp(925040000, 0);
                return true;
            }
            else
            {
                await Pink("You don't have permission to access this area.");
                return false;
            }
        }
    }
}
