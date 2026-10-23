using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 7770 相关脚本：magatia_dark0。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q7770PortalHandler : PortalPlayerInteraction
    {
        public Q7770PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 261000000 | pt: 8
        public async Task<bool> magatia_dark0()
        {

            if (isQuestCompleted(7770))
            {
                await playPortalSound();
                await warp(926130000, "out00");
                return true;
            }
            else
            {
                await Pink("This pipe seems too dark to venture inside.");
                return false;
            }
        }
    }
}
