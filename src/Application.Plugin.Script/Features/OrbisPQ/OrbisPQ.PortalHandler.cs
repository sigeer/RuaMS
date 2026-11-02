using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.OrbisPQ
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        AbstractEventInstanceManager? GetEventInstance()
        {
            var eim = getEventInstance();
            return eim != null && eim.EventName == OrbisPQFeature.EventName ? eim : null;
        }

        // Map: 920010800 | pt: 7
        /// <summary>
        /// 进入花园：需要队长持有生命之根。
        /// </summary>
        public async Task<bool> party3_gardenin()
        {
            var eim = GetEventInstance();
            if (eim == null)
            {
                return false;
            }

            if (getPlayer().getParty() == null || !isEventLeader() || !haveItem(4001055, 1))
            {
                await Pink("Please get the leader in this portal, make sure you have the Root of Life.");
                return false;
            }

            await playPortalSound();
            await eim.warpEventTeam(OrbisPQFeature.GardenMap);
            return true;
        }

        // Map: 920010900 | pt: 8
        /// <summary>
        /// 第八阶段（击败 Pixie）后才开放的储藏室入口。
        /// </summary>
        public Task<bool> party3_jail1() => EnterIfStage8Cleared(920010910);

        // Map: 920010900 | pt: 8
        public Task<bool> party3_jail2() => EnterIfStage8Cleared(920010920);

        // Map: 920010900 | pt: 8
        public Task<bool> party3_jail3() => EnterIfStage8Cleared(920010930);

        // Map: 920010100 | pt: 8
        public Task<bool> party3_room8() => EnterIfStage8Cleared(920011000);

        async Task<bool> EnterIfStage8Cleared(int mapId)
        {
            var eim = GetEventInstance();
            if (eim == null || eim.getIntProperty("statusStg8") != 1)
            {
                await Pink("The storage is currently inaccessible, as the powers of the Pixies remains active within the tower.");
                return false;
            }

            await playPortalSound();
            await warp(mapId, 0);
            return true;
        }

        // Map: 920010600 | pt: 8
        /// <summary>
        /// 第四阶段：两位组合门，按随机组合判断当前柱位是否正确。
        /// </summary>
        public async Task<bool> party3_r4pt()
        {
            var eim = GetEventInstance();
            if (eim == null)
            {
                return false;
            }

            var comb = eim.getProperty("stage4_comb");
            if (comb == null)
            {
                var r = Math.Floor(Random.Shared.NextDouble() * 3) + 1;
                var s = Math.Floor(Random.Shared.NextDouble() * 3) + 1;
                comb = $"{r}{s}";
                eim.setProperty("stage4_comb", comb);
            }

            var pname = int.Parse(getPortal().getName().Substring(4, 6));
            var cname = int.Parse(comb);

            var secondPt = true;
            if (getPortal().getId() < 14)
            {
                cname = (int)Math.Floor(cname / 10.0);
                secondPt = false;
            }

            if (pname % 10 == cname % 10)
            {
                await playPortalSound();
                await TeleportPortal(secondPt ? 1 : getPortal().getId() + 3);
            }
            else
            {
                await playPortalSound();
                await TeleportPortal(2);
            }

            return true;
        }


        // Map: 920010700 | pt: 8
        /// <summary>
        /// 第六阶段：16 层随机组合，走错回到上一层平台。
        /// </summary>
        public async Task<bool> party3_r6pt()
        {
            var eim = GetEventInstance();
            if (eim == null)
            {
                return false;
            }

            var comb = eim.getProperty("stage6_comb");
            if (string.IsNullOrEmpty(comb))
            {
                var builder = new System.Text.StringBuilder("0");
                for (var i = 0; i < 16; i++)
                {
                    builder.Append(Math.Floor(Random.Shared.NextDouble() * 4) + 1);
                }

                comb = builder.ToString();
                eim.setProperty("stage6_comb", comb);
            }

            var portalId = int.Parse(getPortal().getName().Substring(2, 5));
            var pRow = (int)Math.Floor(portalId / 10.0);
            var pCol = portalId % 10;

            if (pCol == int.Parse(comb.Substring(pRow, 1)))
            {
                await TeleportPortal(pRow % 4 != 0 ? getPortal().getId() + 4 : pRow / 4);
            }
            else
            {
                pRow--;
                await TeleportPortal(pRow / 4.0 > 1 ? (int)(pRow / 4.0) : 5);
            }

            return true;
        }

        // Map: 920010910, 920010920, 920010930 | pt: 8
        public async Task<bool> party3_jailin()
        {
            var map = getMap();
            var mobcount = map.countMonster(9300044);

            if (mobcount > 0)
            {
                await Pink("请先使用控制杆清除所有威胁再继续前进");
                return false;
            }
            else
            {
                await playPortalSound();
                await warp(getMapId() + 2, 0);
                return true;
            }
        }

        // Map: 920010100 | pt: 7
        public async Task<bool> party3_room1()
        {

            await playPortalSound();
            await warp(920010200, 13);
            return true;
        }

        // Map: 920010100 | pt: 7
        public async Task<bool> party3_room2()
        {

            await playPortalSound();
            await warp(920010300, 1);
            return true;
        }

        // Map: 920010100 | pt: 7
        public async Task<bool> party3_room3()
        {

            await playPortalSound();
            await warp(920010400, 8);
            return true;
        }

        // Map: 920010100 | pt: 7
        public async Task<bool> party3_room4()
        {

            await playPortalSound();
            await warp(920010500, 3);
            return true;
        }

        // Map: 920010100 | pt: 8
        public async Task<bool> party3_room5()
        {

            await playPortalSound();
            await warp(920010600, 17);
            return true;
        }

        // Map: 920010100 | pt: 8
        public async Task<bool> party3_room6()
        {

            await playPortalSound();
            await warp(920010700, 23);
            return true;
        }

        // Map: 920010200, 920010300, 920010400, 920010500, 920010600, 920010700 … (共 7 张) | pt: 7, 8
        public async Task<bool> party3_roomout()
        {

            var exitPortal = 0;

            switch (getMapId())
            {
                case 920010200:
                    exitPortal = 4;
                    break;

                case 920010300:
                    exitPortal = 12;
                    break;

                case 920010400:
                    exitPortal = 5;
                    break;

                case 920010500:
                    exitPortal = 13;
                    break;

                case 920010600:
                    exitPortal = 15;
                    break;

                case 920010700:
                    exitPortal = 14;
                    break;

                case 920011000:
                    exitPortal = 16;
                    break;
            }

            await playPortalSound();
            await warp(920010100, exitPortal);
            return true;
        }
    }
}
