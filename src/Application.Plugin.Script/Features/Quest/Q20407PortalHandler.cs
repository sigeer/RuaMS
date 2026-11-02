using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 20407 相关脚本：enterWitch, outDarkEreb。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q20407PortalHandler : PortalPlayerInteraction
    {
        public Q20407PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 240040510 | pt: 8
        public async Task<bool> enterWitch()
        {

            if (isQuestCompleted(20404))
            {
                int warpMap;

                if (isQuestCompleted(20407))
                {
                    warpMap = 924010200;
                }
                else if (isQuestCompleted(20406))
                {
                    warpMap = 924010100;
                }
                else
                {
                    warpMap = 924010000;
                }

                await playPortalSound();
                await warp(warpMap, 1);
                return true;

            }
            else
            {
                await Pink("I shouldn't go here.. it's creepy!");
                return false;
            }
        }
        // Map: 913030000 | pt: 7
        public async Task<bool> outDarkEreb()
        {
            var warpMap = isQuestCompleted(20407) ? 924010200 : 924010100;

            await playPortalSound();
            await warp(warpMap, 0);
            return true;
        }
    }
}
