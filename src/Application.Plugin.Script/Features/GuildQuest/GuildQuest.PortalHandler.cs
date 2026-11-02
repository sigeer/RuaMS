using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.GuildQuest
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 990000700 | pt: 8
        /// <summary>
        /// 离开挑战楼层时按玩家记录的楼层索引回到对应出口。
        /// </summary>
        public async Task<bool> guild1F00()
        {
            int[] backPortals = [6, 8, 9, 11];
            var eim = getEventInstance();
            if (eim == null || eim.EventName != GuildQuestFeature.EventName)
            {
                return false;
            }

            var idx = eim.gridCheck(getPlayer());
            await playPortalSound();
            await warp(990000600, backPortals[idx]);
            return true;
        }

        // Map: 990000611 | pt: 8
        public Task<bool> guild1F01() => EnterFloor(0);

        // Map: 990000620 | pt: 8
        public Task<bool> guild1F02() => EnterFloor(1);

        // Map: 990000631 | pt: 8
        public Task<bool> guild1F03() => EnterFloor(3);

        // Map: 990000641 | pt: 8
        public Task<bool> guild1F04() => EnterFloor(2);

        /// <summary>
        /// 记录玩家进入的楼层编号（1F01/02/03/04 依次对应 0/1/3/2）。
        /// </summary>
        async Task<bool> EnterFloor(int slot)
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != GuildQuestFeature.EventName)
            {
                return false;
            }

            eim.gridInsert(getPlayer(), slot);
            await playPortalSound();
            await warp(990000700, "st00");
            return true;
        }

        // Map: 990000000 | pt: 8
        /// <summary>
        /// 集合口：等所有公会成员到齐（entryTimestamp 到期）才放行。
        /// </summary>
        [ScriptTag(["GuildQuest"])]
        public async Task<bool> guildwaitingenter()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != GuildQuestFeature.EventName)
            {
                return false;
            }

            var entryTime = long.Parse(eim.getProperty("entryTimestamp"));
            var timeNow = c.CurrentServer.Node.getCurrentTime();
            var timeLeft = Math.Ceiling((entryTime - timeNow) / 1000.0);

            if (timeLeft > 0)
            {
                await Pink("The portal will open in about " + timeLeft + " seconds.");
                return false;
            }

            await playPortalSound();
            await warp(990000100, 0);
            return true;
        }

        // Map: 990000000 | pt: 7
        public async Task<bool> guildwaitingexit()
        {
            await playPortalSound();
            await warp(101030104);
            return true;
        }

        // Map: 990000800 | pt: 8
        /// <summary>
        /// BOSS 房间入口：门由反应堆 kinggate 打开后才能通过。
        /// </summary>
        public Task<bool> kinggate_open() => EnterBossRoom(1, "This door is closed.");

        // Map: 990000800 | pt: 8
        /// <summary>
        /// BOSS 房间的另一侧入口。
        /// </summary>
        public Task<bool> kinggate2_open() => EnterBossRoom(2, "This crack appears to be blocked off by the door nearby.");

        async Task<bool> EnterBossRoom(int portalId, string denied)
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != GuildQuestFeature.EventName)
            {
                return false;
            }

            if (getPlayer().getMap().getReactorByName("kinggate")?.getState() != 1)
            {
                await Pink(denied);
                return false;
            }

            await playPortalSound();
            await warp(990000900, portalId);

            if (eim.getProperty("boss") == "true")
            {
                await changeMusic("Bgm10/Eregos");
            }

            return true;
        }

        // Map: 990000700 | pt: 8
        public async Task<bool> ghostgate_open()
        {

            if (getPlayer().getMap().getReactorByName("ghostgate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000800, 0);
                return true;
            }
            else
            {
                await Pink("This way forward is not open yet.");
                return false;
            }
        }

        // Map: 990000430 | pt: 8
        public async Task<bool> metalgate_open()
        {

            if (getPlayer().getMap().getReactorByName("metalgate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000431, 0);
                return true;
            }
            await Pink("This way forward is not open yet.");
            return false;
        }

        // Map: 990000610 | pt: 8
        public async Task<bool> secretgate1_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate1")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000611, 1);
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }

        // Map: 990000630 | pt: 8
        public async Task<bool> secretgate2_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate2")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000631, 1);
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }

        // Map: 990000640 | pt: 8
        public async Task<bool> secretgate3_open()
        {

            if (getPlayer().getMap().getReactorByName("secretgate3")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000641, 1);
                return true;
            }
            else
            {
                await Pink("This door is closed.");
                return false;
            }
        }

        // Map: 990000400 | pt: 8
        public async Task<bool> speargate_open()
        {

            if (getPlayer().getMap().getReactorByName("speargate")?.getState() == 4)
            {
                await playPortalSound();
                await warp(990000401, 0);
                return true;
            }
            else
            {
                await Pink("This way forward is not open yet.");
                return false;
            }
        }

        // Map: 990000300 | pt: 8
        public async Task<bool> statuegate_open()
        {

            if (getPlayer().getMap().getReactorByName("statuegate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000301, 0);
                return true;
            }
            else
            {
                await Pink("The gate is closed.");
                return false;
            }
        }

        // Map: 990000400 | pt: 8
        public async Task<bool> stonegate_open()
        {

            if (getPlayer().getMap().getReactorByName("stonegate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000430, 0);
                return true;
            }
            else
            {
                await Pink("The door is still blocked.");
                return false;
            }
        }

        // Map: 990000600 | pt: 8
        public async Task<bool> under30gate()
        {

            if (getPlayer().getLevel() <= 30)
            {
                await playPortalSound();
                await warp(990000640, 1);
                return true;
            }
            else
            {
                await Pink("You cannot proceed past this point.");
                return false;
            }
        }

        // Map: 990000500 | pt: 8
        public async Task<bool> watergate_open()
        {

            if (getPlayer().getMap().getReactorByName("watergate")?.getState() == 1)
            {
                await playPortalSound();
                await warp(990000600, 1);
                return true;
            }
            else
            {
                await Pink("This way forward is not open yet.");
            }
            return false;
        }
    }
}
