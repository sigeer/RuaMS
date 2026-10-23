using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3360 相关脚本：secretDoor。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3360PortalHandler : PortalPlayerInteraction
    {
        public Q3360PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 261010000, 261020200 | pt: 8
        public async Task<bool> secretDoor()
        {

            if (isQuestCompleted(3360))
            {
                await playPortalSound();
                await warp(261030000, "sp_" + ((getMapId() == 261010000) ? "jenu" : "alca"));
                return true;
            }
            else if (isQuestStarted(3360))
            {
                await openNpc(2111024);
                return false;
            }
            else
            {
                await Pink("门锁住了.");
                return false;
            }
        }
    }
}
