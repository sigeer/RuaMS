using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.RockSpirit
{
    internal sealed class RockSpiritPortalHandler : PortalPlayerInteraction
    {
        public RockSpiritPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 103040420, 103040450 | pt: 7
        public async Task<bool> Depart_goBack00()
        {

            await playPortalSound();
            await warp(getPlayer().getMap().getId() - 10, "left00");
            return true;
        }

        // Map: 103040420, 103040430, 103040450, 103040460 | pt: 7
        public async Task<bool> Depart_goBack01()
        {

            await playPortalSound();
            await warp(getPlayer().getMap().getId() - 10, "left01");
            return true;
        }

        // Map: 103040410, 103040440 | pt: 7
        public async Task<bool> Depart_goFoward0()
        {

            var mapid = getPlayer().getMap().getId();

            if (mapid == 103040410 && isQuestCompleted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right00");
                return true;
            }
            else if (mapid == 103040420 && isQuestCompleted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right00");
                return true;
            }
            else if (mapid == 103040410 && isQuestStarted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right00");
                return true;
            }
            else if (mapid == 103040420 && isQuestStarted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right00");
                return true;
            }
            else
            {
                if (mapid == 103040440 || mapid == 103040450)
                {
                    await playPortalSound();
                    await warp(mapid + 10, "right00");
                    return true;
                }
                await Pink("You cannot access this area.");
                return false;
            }
        }

        // Map: 103040410, 103040420, 103040440, 103040450 | pt: 7
        public async Task<bool> Depart_goFoward1()
        {

            var mapid = getPlayer().getMap().getId();

            if (mapid == 103040410 && isQuestCompleted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right01");
                return true;
            }
            else if (mapid == 103040420 && isQuestCompleted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right01");
                return true;
            }
            else if (mapid == 103040410 && isQuestStarted(2287))
            {
                await playPortalSound();
                await warp(103040420, "right01");
                return true;
            }
            else if (mapid == 103040420 && isQuestStarted(2288))
            {
                await playPortalSound();
                await warp(103040430, "right01");
                return true;
            }
            else
            {
                if (mapid == 103040440 || mapid == 103040450)
                {
                    await playPortalSound();
                    await warp(mapid + 10, "right01");
                    return true;
                }
                await Pink("You cannot access this area.");
                return false;
            }
        }

        public async Task<bool> Depart_TopFloor()
        {

            await openNpc(1052125); //It is actually suppose to open the npc, because it leads to a boss map
            return true;
        }

        // Map: 103040400 | pt: 7
        public async Task<bool> Depart_topOut()
        {
            await playPortalSound();
            await warp(103040300, 1);
            return true;
        }
    }
}
