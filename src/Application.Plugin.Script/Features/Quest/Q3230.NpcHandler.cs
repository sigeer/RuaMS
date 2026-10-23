using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3230NpcHandler : NpcScriptBase
    {
        public Q3230NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2040002
        /// <summary>
        /// 玩偶屋入口 NPC。
        /// </summary>
        public async Task ludi023()
        {
            if (TryGetDefaultTalk(out var msg, Q3230.QuestIdValue))
            {
                await SayOK(msg);
                return;
            }

            if (await AskYesNo("你准备好进入玩偶屋了吗？"))
            {
                await TryStartSoloQuest(Q3230.QuestIdValue);
            }
        }

        // Npc: 2040028 
        public async Task ludi024()
        {
            var greeting = "感谢你找到了钟摆。你准备好返回玩具塔了吗？";
            if (isQuestStarted(Q3230.QuestIdValue))
            {
                if (haveItem(4031094))
                {
                    await completeQuest(Q3230.QuestIdValue);
                    await gainItem(4031094, -1);
                }
                else
                {
                    greeting = "你还没有找到那个钟摆。你想回到玩具塔吗？";
                }
            }

            if (await AskYesNo(greeting))
            {
                await warp(221024400, 4);
            }
        }
    }
}
