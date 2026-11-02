using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.MonsterCarnival
{
    /// <summary>
    /// MonsterCarnival：mc_out。
    /// </summary>
    internal sealed class MonsterCarnivalPortalHandler : PortalPlayerInteraction
    {
        public MonsterCarnivalPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 980000000, 980030000 | pt: 7
        public async Task<bool> mc_out()
        {

            var returnMap = getPlayer().getSavedLocation("MONSTER_CARNIVAL");
            if (returnMap < 0)
            {
                returnMap = 102000000; // Just Incase there is no saved location.
            }
            var target = await getPlayer().getClient().getChannelServer().getMapFactory().getMap(returnMap);
            await getPlayer().changeMap(target);
            await playPortalSound();
            return true;
        }

        // Map: 980031200 | pt: 7
        public async Task<bool> MC2revive()
        {

            await playPortalSound();
            await WarpReturn();
            return true;
        }

        // Map: 980000102 | pt: 7
        public async Task<bool> MCrevive1()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }

        // Map: 980000202 | pt: 7
        public async Task<bool> MCrevive2()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }

        // Map: 980000302 | pt: 7
        public async Task<bool> MCrevive3()
        {

            var portal = 0;
            switch (getPlayer().getTeam())
            {
                case 0:
                    portal = 4;
                    break;
                case 1:
                    portal = 3;
                    break;
            }
            await WarpReturn(portal);
            await playPortalSound();
            return true;
        }

        // Map: 980000402 | pt: 7
        public async Task<bool> MCrevive4()
        {

            var portal = 0;
            switch (getPlayer().getTeam())
            {
                case 0:
                    portal = 4;
                    break;
                case 1:
                    portal = 3;
                    break;
            }
            await WarpReturn(portal);
            await playPortalSound();
            return true;
        }

        // Map: 980000502 | pt: 7
        public async Task<bool> MCrevive5()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }

        // Map: 980000602 | pt: 7
        public async Task<bool> MCrevive6()
        {

            await WarpReturn();
            await playPortalSound();
            return true;
        }
    }
}
