using Application.Core.Client;
using Application.Core.scripting.Events.Instances;
using Application.Core.Server.maps;
using Application.Resources.Messages;
using scripting.portal;

namespace Application.Plugin.Script.Features.EllinPQ
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        AbstractEventInstanceManager? GetEventInstance()
        {
            var eim = getEventInstance();
            return eim != null && eim.EventName == EllinPQFeature.EventName ? eim : null;
        }

        // Map: 930000600 | pt: 7
        /// <summary>
        /// 通关后才能离场，且只有队长能带队进入奖励地图。
        /// </summary>
        public async Task<bool> party6_out()
        {
            var eim = GetEventInstance();
            if (eim == null || !eim.isEventCleared())
            {
                await Pink(nameof(ClientMessage.EllinPQ_NeedDefeatBossFirst));
                return false;
            }

            if (!isEventLeader())
            {
                await Pink(nameof(ClientMessage.Tip_WaitForLeaderEnterPortal));
                return false;
            }

            await playPortalSound();
            await eim.warpEventTeam(EllinPQFeature.ClearMapId);
            return true;
        }

        // Map: 930000800 | pt: 7
        /// <summary>
        /// 奖励关出口：交还独占道具，若弹簧机关已被触发则补发一枚碎片。
        /// </summary>
        public async Task<bool> party6_stage800()
        {
            var eim = GetEventInstance();
            if (eim != null)
            {
                await eim.dropExclusiveItems(getPlayer());

                var spring = getMap().getReactorById(3008000);
                if (spring != null && spring.getState() > 0)
                {
                    if (!canHold(4001198, 1))
                    {
                        await Pink("Tip_CheckEtcSizeBeforeEnterPortal");
                        return false;
                    }

                    await gainItem(4001198, 1);
                }
            }

            await playPortalSound();
            await warp(300030100, 0);
            return true;
        }

        // Map: 930000000, 930000010, 930000100, 930000200 | pt: 7
        [ScriptTag(["EllinPQ"])]
        public async Task<bool> party6_stage()
        {

            switch (getMapId())
            {
                case 930000000:
                    await playPortalSound();
                    await warp(930000100, 0);
                    return true;
                case 930000100:
                    if (getMap().countMonsters() == 0)
                    {
                        await playPortalSound();
                        await warp(930000200, 0);
                        return true;
                    }
                    else
                    {
                        await Pink(nameof(ClientMessage.Tip_EliminateAllMonster));
                        return false;
                    }
                case 930000200:
                    if (getMap().getReactorByName("spine") != null && getMap().getReactorByName("spine")?.getState() < 4)
                    {
                        await Pink(nameof(ClientMessage.EllinPQ_SpineBlockWay));
                        return false;
                    }
                    else
                    {
                        await playPortalSound();
                        await warp(930000300, 0); //assuming they cant get past reactor without it being gone
                        return true;
                    }
                default:
                    await Pink(nameof(ClientMessage.Tip_UnknownPortal));
                    return false;
            }
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage501()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "02st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage502()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "03st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage503()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "04st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage504()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "05st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage505()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "06st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage506()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "07st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage507()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "08st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage508()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "09st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage509()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "10st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage510()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "11st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage511()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "12st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage512()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "13st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage513()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "14st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage514()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "15st");
            }

            return true;
        }

        // Map: 930000300 | pt: 7
        public async Task<bool> party6_stage515()
        {

            if (Random.Shared.NextDouble() < 0.1)
            {
                await playPortalSound();
                await warp(930000300, "16st");
            }
            else
            {
                await playPortalSound();
                await warp(930000300, "01st");
            }

            return true;
        }
    }
}
