using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.TDBattle
{
    /// <summary>
    /// 时空裂缝副本入口：按地图号算出关卡序号，开对应的 <c>TD_BattleN</c> 副本。
    /// </summary>
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 240070202, 240070302, 240070402, 240070502, 240070602 | pt: 7
        public async Task<bool> TD_Boss_enter()
        {
            var stage = (int)((Math.Floor(getMapId() / 100.0)) % 10) - 1;
            return await TryStartEvent(GetEventManager(TDBattleFeature.EventNames[stage - 1]));
        }
    }
}
