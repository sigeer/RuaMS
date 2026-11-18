using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 7770 相关脚本：yurete2_dead。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q7770NpcHandler : NpcScriptBase
    {
        public Q7770NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2112011 
        public async Task yurete2_dead()
        {
            await AskMenu(
                this.GetTalkMessage(ScriptMessage.Q7770_Defeated),
                [this.GetTalkMessage(ScriptMessage.Q7770_CheerUp)]
                );
            await SayNext(this.GetTalkMessage(ScriptMessage.Q7770_Forgiven));
            if (!isQuestCompleted(7770))
            {
                await completeQuest(7770);
            }
            await warp(926110600, 0);
        }
    }
}
