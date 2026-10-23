using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 23647 相关脚本：foxLaidy。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q23647NpcHandler : NpcScriptBase
    {
        public Q23647NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2071012 
        public async Task foxLaidy()
        {
            if (getQuestProgressInt(23647, 1) != 0)
            {
                await SayOK(GetDefault0());
                return;
            }

            if (!haveItem(4031793, 1))
            {
                await SayOK("嗯...嘿...你能帮我找到我在树林里丢失的一块柔软而闪亮的银色毛皮吗？我需要它，我需要它，我非常非常需要它！");
                return;
            }

            if (await AskYesNo("嘿... 嗯... 你能帮我找到我在树林里丢失的一块#b柔软而闪亮的银色毛皮#k吗？我需要它，我需要它，我非常非常需要它！... 哦，你找到了它！！！你会把它给我吗？"))
            {
                await SayNext("嘿嘿嘿~这是你从我这里拿走的报酬，你值得拥有。");
                await gainItem(4031793, -1);
                await gainFame(-5);
                await setQuestProgress(23647, 1, 1);
            }
        }
    }
}
