using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 出口：击败傀儡师后才可离开。
    /// </summary>
    internal sealed class PuppeteerFeaturePortalHandler : PortalPlayerInteraction
    {
        public PuppeteerFeaturePortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 910510000 | pt: 7
        public async Task<bool> exit_puppeteer()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != PuppeteerFeature.EventName || !eim.isEventCleared())
            {
                await Pink("Defeat the Puppeteer before leaving.");
                return false;
            }

            await playPortalSound();
            await warp(105070300, 3);
            return true;
        }


        // Map: 105070300 | pt: 8
        public async Task<bool> enterDollcave()
        {

            if (isQuestCompleted(PuppeteerFeature.PuppeteerCygnus) || isQuestCompleted(21734))
            {
                // puppeteer defeated, newfound secret path
                await playPortalSound();
                await warp(105040201, 2);
                return true;
            }

            await openNpc(1063011);
            return false;
        }

        // Map: 105040201 | pt: 7
        public async Task<bool> enterDollWay()
        {

            if (isQuestCompleted(PuppeteerFeature.PuppeteerCygnus) || isQuestCompleted(21734))
            {
                // puppeteer defeated, newfound secret path
                await playPortalSound();
                await warp(105070300, 3);
                return true;
            }
            else if (isQuestStarted(21734))
            {
                await playPortalSound();
                await warp(910510100, 0);
                return true;
            }
            else
            {
                await Pink("一股不祥的力量阻止你通过此处。");
                return false;
            }
        }
    }
}
