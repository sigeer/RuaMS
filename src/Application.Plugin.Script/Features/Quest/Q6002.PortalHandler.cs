using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q6002PortalHandler : PortalPlayerInteraction
    {
        public Q6002PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 923010100 | pt: 7
        public async Task<bool> tamepig_out2()
        {

            if (!(haveItem(4031507, 5) && haveItem(4031508, 5) && isQuestStarted(6002)))
            {
                await removeAll(4031507);
                await removeAll(4031508);
            }

            var pCount = getPlayer().countItem(4031507);
            var rCount = getPlayer().countItem(4031508);

            if (pCount > 5)
            {
                await gainItem(4031507, -1 * (pCount - 5));
            }
            if (rCount > 5)
            {
                await gainItem(4031508, -1 * (rCount - 5));
            }

            await playPortalSound();
            await warp(230000003, "out00");
            return true;
        }

        // Npc: 2060005 
    }
}
