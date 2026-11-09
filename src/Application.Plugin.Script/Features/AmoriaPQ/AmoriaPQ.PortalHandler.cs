using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.AmoriaPQ
{
    internal sealed class AmoriaPQPortalHandler : PortalPlayerInteraction
    {
        public AmoriaPQPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        public async Task<bool> apq00()
        {

            await playPortalSound();
            await warp(670010300, 0);
            return true;
        }

        public async Task<bool> apq01()
        {

            await playPortalSound();
            await warp(670010301, 0);
            return true;
        }

        public async Task<bool> apq02()
        {

            await playPortalSound();
            await warp(670010302, 0);
            return true;
        }

        public async Task<bool> apq1()
        {

            await playPortalSound();
            await warp(670010400, 0);
            return true;
        }

        public async Task<bool> apq2()
        {

            await playPortalSound();
            await warp(670010500, 0);
            return true;
        }

        public async Task<bool> apq3()
        {

            await playPortalSound();
            await warp(670010600, 0);
            return true;
        }

        // Map: 670010300, 670010301, 670010302, 670010400, 670010500 | pt: 5
        public async Task<bool> apqClosed()
        {

            await Pink("大门还没有打开。");
            return false;
        }

        // Map: 670010600 | pt: 8
        public async Task<bool> apqDoor()
        {

            var name = getPortal().getName().Substring(2, 4);
            var gate = getPlayer().getMap().getReactorByName("gate" + name);
            if (gate != null && gate.getState() == 4)
            {
                await playPortalSound();
                await warp(670010600, "gt" + name + "PIB");
                return true;
            }
            else
            {
                await Pink("大门还没有打开。");
                return false;
            }
        }
    }
}
