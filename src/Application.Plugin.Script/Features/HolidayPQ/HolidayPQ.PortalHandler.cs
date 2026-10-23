using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.HolidayPQ
{
    /// <summary>
    /// HolidayPQ：in_xmas_party。
    /// </summary>
    internal sealed class HolidayPQPortalHandler : PortalPlayerInteraction
    {
        public HolidayPQPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        [ScriptName("08_xmas_out")]
        public async Task<bool> _08_xmas_out()
        {

            await playPortalSound();
            await warp(getMapId() - 2, 0);
            return true;
        }

        // Map: 209000000 | pt: 7
        public async Task<bool> in_xmas_party()
        {

            await openNpc(9209100);
            return false;
        }
    }
}
