using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                await SayOK(this.GetTalkMessage(ScriptMessage.Q23647_UmmHelp));
                return;
            }

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q23647_UmmHelpFind)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Q23647_TeeheheeThats));
                await gainItem(4031793, -1);
                await gainFame(-5);
                await setQuestProgress(23647, 1, 1);
            }
        }
    }
}
