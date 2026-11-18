using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3239NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q3239NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 2040003 
        public async Task ludi020()
        {
            if (getMapId() == 922000000)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Latanica_ReadyLeave)))
                {
                    await WarpOut();
                }
            }
            else
            {
                if (TryGetDefaultTalk(out var msg, Q3239.QuestIdValue))
                {
                    await SayOK(msg);
                    return;
                }

                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q3239Npc_ReadyToEnter, 922000000)))
                {
                    await TryStartSoloQuest(Q3239.QuestIdValue);
                }
            }

        }
    }
}
