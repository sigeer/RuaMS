using Application.Core.Client;
using Application.Core.Server.maps;
using Application.Resources.Messages;
using scripting.portal;

namespace Application.Plugin.Script.Features.Zakum
{
    /// <summary>
    /// Zakum：Zakum05。
    /// </summary>
    internal sealed class ZakumPortalHandler : PortalPlayerInteraction
    {
        public ZakumPortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 211042300 | pt: 7
        public async Task<bool> Zakum05()
        {
            var questStatus = getQuestStat(ZakumFeature.QuestId);
            if (questStatus == client.QuestStatus.Status.NOT_STARTED)
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Zakum_NeedMastersPermission));
                return false;
            }

            else if (questStatus == client.QuestStatus.Status.STARTED)
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Zakum_NeedAllTrials));
                return false;
            }

            if (!haveItem(4001017))
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Zakum_NeedFireEye));
                return false;
            }

            var react = getMap().getReactorById(2118002);
            if (react != null && react.getState() > 0)
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Zakum_EntranceLocked));
                return false;
            }

            await playPortalSound();
            await warp(211042400, "west00");
            return true;
        }
    }
}
