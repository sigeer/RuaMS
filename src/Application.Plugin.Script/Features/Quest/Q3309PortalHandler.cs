using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3309 相关脚本：magatia_alc0。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3309PortalHandler : PortalPlayerInteraction
    {
        public Q3309PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 261020600 | pt: 7
        public async Task<bool> magatia_alc0()
        {

            if (!isQuestStarted(3309) || haveItem(4031708, 1))
            {
                await playPortalSound();
                await warp(261020700, "down00");
            }
            else
            {
                await playPortalSound();
                await warp(926120000, "out00");
            }

            return true;
        }
    }
}
