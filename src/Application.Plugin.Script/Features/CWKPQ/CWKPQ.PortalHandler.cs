using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.CWKPQ
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 610030100 | pt: 8
        public Task<bool> glpqPortal0() => PassIf(eim => eim.getIntProperty("glpq1") != 0, 610030200, "This path is currently blocked.");

        // Map: 610030200 | pt: 8
        public Task<bool> glpqPortal1() => PassIf(eim => eim.getIntProperty("glpq2") != 0, 610030300, "The portal is not opened yet!");

        // Map: 610030300 | pt: 8
        public Task<bool> glpqPortal3() => PassIf(eim => eim.getIntProperty("glpq3") >= 5 && eim.getIntProperty("glpq3_p") >= 5, 610030400, "The portal is not opened yet.");

        // Map: 610030400 | pt: 8
        public Task<bool> glpqPortal4() => PassIf(eim => eim.getIntProperty("glpq4") >= 5, 610030500, "The portal is not opened yet.");

        // Map: 610030500 | pt: 8
        public Task<bool> glpqPortal5() => PassIf(eim => eim.getIntProperty("glpq5") >= 5, 610030600, "The portal is not opened yet.");

        // Map: 610030600 | pt: 8
        public Task<bool> glpqPortal6() => PassIf(eim => eim.getIntProperty("glpq6") >= 3, 610030700, "The portal is not opened yet.");

        // Map: 610030700 | pt: 8
        public async Task<bool> glpqPortal7()
        {
            await playPortalSound();
            await warp(610030800, 0);
            return true;
        }

        // Map: 610030500 | pt: 8
        public Task<bool> glpqPortal00() => EnterJobRoom(1, 610030510, "Only warriors may enter this portal.");

        // Map: 610030500 | pt: 8
        public Task<bool> glpqPortal01() => EnterJobRoom(3, 610030540, "Only bowmen may enter this portal.");

        // Map: 610030500 | pt: 8
        public Task<bool> glpqPortal02() => EnterJobRoom(2, 610030521, "Only mages may enter this portal.");

        // Map: 610030500 | pt: 8
        public Task<bool> glpqPortal03() => EnterJobRoom(4, 610030530, "Only thieves may enter this portal.");

        // Map: 610030500 | pt: 8
        public Task<bool> glpqPortal04() => EnterJobRoom(5, 610030550, "Only pirates may enter this portal.");

        // Map: 610030300 | pt: 8
        /// <summary>
        /// 每名队员穿过一次即计数；凑满 5 人且 5 个技能石都激活后才标记通关。
        /// </summary>
        public async Task<bool> glpqPortal2()
        {
            var eim = getEventInstance();
            if (eim == null) return false;

            await playPortalSound();
            await warp(610030300, 0);

            if (eim.getIntProperty("glpq3") >= 5 && eim.getIntProperty("glpq3_p") >= 5)
            {
                await LightBlue("The portal at the bottom has already been opened! Proceed there!");
                return true;
            }

            if (eim.getIntProperty("glpq3_p") >= 5)
            {
                await mapMessage(6, "Not all Sigils have been activated yet. Make sure they have all been activated to proceed to the next stage.");
                return true;
            }

            eim.setIntProperty("glpq3_p", eim.getIntProperty("glpq3_p") + 1);
            if (eim.getIntProperty("glpq3") >= 5 && eim.getIntProperty("glpq3_p") >= 5)
            {
                await mapMessage(6, "The Antellion grants you access to the next portal! Proceed!");
                await eim.showClearEffect(610030300, "3pt", 2);
                await eim.GiveStageClearRewardAll(3);
            }
            else
            {
                await mapMessage(6, "An adventurer has passed through! " + (5 - eim.getIntProperty("glpq3_p")) + " to go.");
            }

            return true;
        }

        // Map: 610030100 | pt: 8
        /// <summary>
        /// 击碎封印装置后开启隐藏通道。
        /// </summary>
        public async Task<bool> glpqPortalDummy()
        {
            var react = getMap().getReactorByName("mob0");
            if (react == null)
            {
                return false;
            }

            var eim = getEventInstance();
            if (eim == null) return false;

            if (react.getState() >= 1)
            {
                await eim.dropMessage(5, "The portal apparatus is malfunctional, due to the last transportation. The finding another way through.");
                return false;
            }

            await react.forceHitReactor(1);
            eim.setIntProperty("glpq1", 1);

            await eim.dropMessage(5, "A strange force starts being emitted from the portal apparatus, showing a hidden path once blocked now open.");
            await playPortalSound();
            await warp(610030100, 0);

            await eim.showClearEffect();
            await eim.GiveStageClearRewardAll(1);
            return true;
        }

        async Task<bool> EnterJobRoom(int jobNiche, int mapId, string denied)
        {
            if (getPlayer().getJob().GetJobNiche() != jobNiche)
            {
                await Pink(denied);
                return false;
            }

            await playPortalSound();
            await warp(mapId, 0);
            return true;
        }

        async Task<bool> PassIf(Func<AbstractEventInstanceManager, bool> condition, int mapId, string denied)
        {
            var eim = getEventInstance();
            if (eim == null || !condition(eim))
            {
                await Pink(denied);
                return false;
            }

            await playPortalSound();
            await warp(mapId, 0);
            return true;
        }
    }
}
