using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 23647 相关脚本：foxLaidy_map。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q23647PortalHandler : PortalPlayerInteraction
    {
        public Q23647PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 222010300 | pt: 7
        public async Task<bool> foxLaidy_map()
        {

            if (!(isQuestStarted(3647) && haveItem(4031793, 1)))
            {
                await playPortalSound();
                await warp(222010200, "east00");
            }
            else
            {
                if (!isQuestStarted(23647))
                {
                    await forceStartQuest(23647);
                }
                await playPortalSound();
                await warp(922220000, "east00");
            }

            return true;
        }
    }
}
