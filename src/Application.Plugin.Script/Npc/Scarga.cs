using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 9270047 
        public Task MalaysiaBoss_GL()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 9201134 
        public async Task Malay_Warp()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                if (await SayYesNo("如果你现在离开，就无法返回了。你确定要离开吗？"))
                {
                    warp(551030100, 2);
                }
            }
            else
            {
                await SayNext("你们打败了斯卡利昂和塔加！太棒了！把这个纪念品当作你们勇敢的奖励。");

                if (!eim.giveEventReward(getPlayer()))
                {
                    await SayNext("请先在你的背包里腾出空间！");
                }
                else
                {
                    warp(551030100, 2);
                }
            }
        }
    }
}
