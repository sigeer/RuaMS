using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q3335NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q3335NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 2111003 
        public async Task snow_rose()
        {
            if (isQuestStarted(3335) && !haveItem(4031695, 1))
            {
                await TryStartSoloQuest(Q3335.QuestIdValue);
            }
            else
            {
                await SayOK(GetDefaultTalk(3335));
            }
        }
    }
}
