using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.RescueGaga
{
    /// <summary>
    /// 四个掉落点的出口门行为一致。
    /// </summary>
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 922240000 | pt: 9
        public Task<bool> Spacegaga_out0() => Fall();

        // Map: 922240000 | pt: 9
        public Task<bool> Spacegaga_out1() => Fall();

        // Map: 922240000 | pt: 9
        public Task<bool> Spacegaga_out2() => Fall();

        // Map: 922240000 | pt: 9
        public Task<bool> Spacegaga_out3() => Fall();

        /// <summary>
        /// 记录掉落次数：未超限时重新进入当前地图，超限后送回出口。
        /// </summary>
        async Task<bool> Fall()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != RescueGagaFeature.EventName)
            {
                return false;
            }

            var falls = eim.getIntProperty("falls");
            if (falls < RescueGagaFeature.MaxFalls)
            {
                eim.setIntProperty("falls", falls + 1);
                await playPortalSound();
                await warp(getPlayer().getMapId(), 0);
                return true;
            }

            await playPortalSound();
            await warp(RescueGagaFeature.EXIT_MAP, 0);
            return true;
        }

        // Map: 922240000 | pt: 9
        public async Task<bool> gaga_success()
        {

            await playPortalSound();
            await warp(922240100 + (getPlayer().getMapId() - 922240000), 0);
            return true;
        }

        // Map: 922240200 | pt: 9
        public async Task<bool> space_return()
        {

            await playPortalSound();
            await warp(getPlayer().getSavedLocation("EVENT"));
            return true;
        }
    }
}
