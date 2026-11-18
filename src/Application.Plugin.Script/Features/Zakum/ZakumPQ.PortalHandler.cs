using Application.Resources.Messages;
using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Zakum
{
    /// <summary>
    /// 试炼出口：完成试炼并领取战利品（退出网格）后才放行。
    /// </summary>
    internal sealed class ZakumPQPortalHandler : PortalPlayerInteraction
    {
        public ZakumPQPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 280010000 | pt: 7
        public async Task<bool> Zakum03()
        {
            var eim = getEventInstance();
            if (eim == null || (eim.EventName != ZakumPQ.PartyQuestEventName))
            {
                return false;
            }

            if (!eim.isEventCleared())
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.ZakumPQPortalHandler_PartyHas));
                return false;
            }

            if (eim.gridCheck(getPlayer()) == -1)
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.ZakumPQPortalHandler_HaveClaimed));
                return false;
            }

            await playPortalSound();
            await warp(ZakumPQ.ExitMap);
            return true;
        }
    }
}
