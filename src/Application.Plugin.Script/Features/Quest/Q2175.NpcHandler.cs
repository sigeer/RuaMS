using Application.Core.Client;

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

            if (await AskYesNo("你想要被传送到黑魔法师的弟子那里吗？"))
            {
                await TryStartSoloQuest(Q2175.QuestIdValue);
            }

        }
    }
}
