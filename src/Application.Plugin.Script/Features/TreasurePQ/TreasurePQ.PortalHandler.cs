using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.TreasurePQ
{
    /// <summary>
    /// 隧道出口：第一阶段密道谜题解开后才开放。
    /// </summary>
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 674030000 | pt: 8
        public async Task<bool> guyfawkes0_esc()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != TreasurePQFeature.EventName || eim.getIntProperty("statusStg1") != 1)
            {
                await Pink("The tunnel is currently blocked.");
                return false;
            }

            await playPortalSound();
            await warp(674030200, 0);
            return true;
        }
    }
}
