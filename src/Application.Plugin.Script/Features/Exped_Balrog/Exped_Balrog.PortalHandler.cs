using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Exped_Balrog
{
    /// <summary>
    /// Exped_Balrog：balog_summon。
    /// </summary>
    internal sealed class Exped_BalrogPortalHandler : PortalPlayerInteraction
    {
        public Exped_BalrogPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 105090200 | pt: 7
        public async Task<bool> balogTemple()
        {
            await playPortalSound();
            await warp(105100000, 2);
            return true;
        }

        // Map: 105100301, 105100401 | pt: 7
        public async Task<bool> balog_end()
        {
            if (!canHold(4001261, 1))
            {
                await Pink("请给装备栏腾出至少1个空格子。");
                return false;
            }
            await gainItem(4001261, 1);
            await playPortalSound();
            await warp(105100100, 0);
            return true;
        }

    }
}
