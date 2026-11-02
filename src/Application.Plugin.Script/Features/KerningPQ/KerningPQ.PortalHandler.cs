using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.KerningPQ
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 103000800 | pt: 5
        public Task<bool> kpq0() => PassStage(KerningPQFeature.Stage2Map);

        // Map: 103000801 | pt: 5
        public Task<bool> kpq1() => PassStage(KerningPQFeature.Stage3Map);

        // Map: 103000802 | pt: 5
        public Task<bool> kpq2() => PassStage(KerningPQFeature.Stage4Map);

        // Map: 103000803 | pt: 5
        public Task<bool> kpq3() => PassStage(KerningPQFeature.Stage5Map);

        public Task<bool> kpq4() => PassStage(KerningPQFeature.BonusMap);

        /// <summary>
        /// 只有当前地图被标记为已通关，通往下一段的传送门才生效。
        /// </summary>
        async Task<bool> PassStage(int nextMapId)
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != KerningPQFeature.EventName)
            {
                return false;
            }

            if (!eim.ClearedMaps.ContainsKey(getMapId()))
            {
                await Pink("The portal is not opened yet.");
                return false;
            }

            await playPortalSound();
            var target = await eim.getMapInstance(nextMapId);
            await getPlayer().changeMap(target, target.getPortal("st00"));
            return true;
        }
    }
}
