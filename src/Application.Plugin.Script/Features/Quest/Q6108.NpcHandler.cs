using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q6108NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q6108NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 1061012 
        public async Task s4snipe()
        {
            if (TryGetDefaultTalk(out var msg, Q6108.QuestIdValue))
            {
                await SayOK(msg);
                return;
            }

            await TryStartSoloQuest(Q6108.QuestIdValue);
        }
    }
}
