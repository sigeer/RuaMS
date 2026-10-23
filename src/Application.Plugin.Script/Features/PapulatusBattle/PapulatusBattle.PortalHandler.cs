using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.PapulatusBattle
{
    internal sealed class PortalHandler : PortalPlayerInteraction
    {
        public PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 220080000 | pt: 8
        public async Task<bool> Populatus00()
        {

            if (!((isQuestStarted(6361) && haveItem(4031870, 1)) || (isQuestCompleted(6361) && !isQuestCompleted(6363))))
            {
                var em = GetEventManager(PapulatusBattleFeature.EventName);

                var r = await em.StartInstance(getPlayer());
                if (r == CreateInstanceResult.Success)
                {
                    await playPortalSound();
                    return true;
                }
                else
                {
                    await Pink(em.HandleCreateInstanceResult(r, c));
                    return false;
                }
            }
            else
            {
                await playPortalSound();
                await warp(922020300, 0);
                return true;
            }
        }
    }
}
