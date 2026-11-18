using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2175NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q2175NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 1092007 
        public async Task nautil_black()
        {
            if (TryGetDefaultTalk(out var msg, Q2175.QuestIdValue))
            {
                await SayOK(msg);
                return;
            }

            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q2175_WantWarped)))
            {
                await TryStartSoloQuest(Q2175.QuestIdValue);
            }

        }
    }
}
