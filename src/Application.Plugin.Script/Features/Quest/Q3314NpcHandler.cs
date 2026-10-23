using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.GameProps;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3314 相关脚本：alcadno_potion。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3314NpcHandler : NpcScriptBase
    {
        public Q3314NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2111015 
        public async Task alcadno_potion()
        {
            if (isQuestStarted(3314) && !haveItem(2022198, 1) && getPlayer().getBuffSource(BuffStat.HPREC) != 2022198)
            {
                if (canHold(2022198, 1))
                {
                    await gainItem(2022198, 1);
                    await SayOK("你拿了桌子上放着的药丸。");
                }
                else
                {
                    await SayOK("你没有可用的消耗槽来获取#t2022198#。");
                }
            }
        }
    }
}
