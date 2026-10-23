using Application.Core.Client;
using Application.Core.Server.maps;
using scripting.portal;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3314 相关脚本：timeQuest。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3314PortalHandler : PortalPlayerInteraction
    {
        public Q3314PortalHandler(IChannelClient c, IPortal portal) : base(c, portal)
        {
        }

        // Map: 270010100, 270010200, 270010300, 270010400, 270010500, 270020100 … (共 16 张) | pt: 7, 8
        public async Task<bool> timeQuest()
        {

            var mapid = getPlayer().getMapId();
            await playPortalSound();
            var map = (mapid - 270010000) / 100;
            //await Pink(map + " " + isQuestCompleted(3534));
            if (map < 5 && isQuestCompleted(3500 + map))
            {
                await warp(mapid + 10, "out00");
            }
            else if (map == 5 && isQuestCompleted(3502 + map))
            {
                await warp(270020000, "out00");
            }
            else if (map > 100 && map < 105 && isQuestCompleted(3407 + map))
            {
                await warp(mapid + 10, "out00");
            }
            else if (map == 105 && isQuestCompleted(3514))
            {
                await warp(270030000, "out00");
            }
            else if (map > 200 && map < 205 && isQuestCompleted(3314 + map))
            {
                await warp(mapid + 10, "out00");
            }
            else if (map == 205 && isQuestCompleted(3519))
            {
                await warp(270040000, "out00");
            }
            else if (map == 300 && (haveItem(4032002) || isQuestCompleted(3522)))
            {
                await warp(270040100, "out00");
            }
            else
            {
                if (map > 200)
                {
                    await Pink("随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    await warp(270030000, "in00");
                }
                else if (map > 100)
                {
                    await Pink("随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    await warp(270020000, "in00");
                }
                else
                {
                    await Pink("随着时间开始变得异常流动，你被传送回到了一个安全的空间。");
                    await warp(270010000, "in00");
                }
            }
            return true;
        }
    }
}
