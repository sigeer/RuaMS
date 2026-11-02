using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.LudiPQ
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 922010100 | pt: 5
        public Task<bool> lpq0() => PassStage(922010200);

        // Map: 922010200 | pt: 5
        public Task<bool> lpq1() => PassStage(922010300);

        // Map: 922010300 | pt: 5
        public Task<bool> lpq2() => PassStage(922010400);

        // Map: 922010400 | pt: 5
        public Task<bool> lpq3() => PassStage(922010500);

        // Map: 922010500 | pt: 5
        public Task<bool> lpq4() => PassStage(922010600);

        // Map: 922010600 | pt: 2
        public Task<bool> lpq5() => PassStage(922010700);

        // Map: 922010700 | pt: 5
        public Task<bool> lpq6() => PassStage(922010800);

        // Map: 922010800 | pt: 5
        public Task<bool> lpq7() => PassStage(922010900);

        /// <summary>
        /// 只有当前地图被标记为已通关，传送门才生效。
        /// </summary>
        async Task<bool> PassStage(int nextMapId)
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != LudiPQFeature.EventName)
            {
                return false;
            }

            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await Pink("Some seal is blocking this door.");
                return false;
            }

            await playPortalSound();
            var target = await eim.getMapInstance(nextMapId);
            await getPlayer().changeMap(target, target.getPortal("st00"));
            return true;
        }
    }
}
