using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

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

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q3230_ReadyEnter)))
            {
                await TryStartSoloQuest(Q3230.QuestIdValue);
            }
        }

        // Npc: 2040028 
        public async Task ludi024()
        {
            var greeting = this.GetTalkMessage(ScriptMessage.Q3230_ThankFinding);
            if (isQuestStarted(Q3230.QuestIdValue))
            {
                if (haveItem(4031094))
                {
                    await completeQuest(Q3230.QuestIdValue);
                    await gainItem(4031094, -1);
                }
                else
                {
                    greeting = this.GetTalkMessage(ScriptMessage.Q3230_HaventFound);
                }
            }

            if (await AskYesNo(greeting))
            {
                await warp(221024400, 4);
            }
        }
    }
}
