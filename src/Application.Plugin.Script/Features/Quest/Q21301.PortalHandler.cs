using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21301PortalHandler : PortalPlayerInteraction
    {
        public Q21301PortalHandler(IChannelClient c, Application.Core.Server.maps.IPortal portal)
            : base(c, portal)
        {
        }

        // Map: 140020200 | pt: 7
        public async Task<bool> enterPort()
        {
            if (isQuestStarted(Q21301.QuestIdValue) && getQuestProgressInt(Q21301.QuestIdValue, 9001013) == 0)
            {
                var em = GetSoloQuestEventManager(Q21301.QuestIdValue);
                var r = await em.StartInstance(getPlayer());
                if (r != CreateInstanceResult.Success)
                {
                    await Pink("门从另一边被堵住了。有人已经和小偷乌鸦打起来了？");
                    return false;
                }

                await playPortalSound();
                return true;
            }
            else
            {
                await playPortalSound();
                await warp(140020300, 1);
            }
            return true;
        }
    }
}
