using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 3309 相关脚本：magatia_dark1。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q3309NpcHandler : NpcScriptBase
    {
        public Q3309NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2111010 
        public async Task magatia_dark1()
        {
            if (isQuestStarted(3309) && !haveItem(4031708, 1))
            {
                if (canHold(4031708, 1))
                {
                    await gainItem(4031708, 1);
                }
                else
                {
                    await SayOK("有一个ETC槽位可用，可以获取Alcadno的秘密文件。");
                }
            }

        }
    }
}
