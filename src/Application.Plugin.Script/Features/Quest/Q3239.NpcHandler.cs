using Application.Core.Client;

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
                if (await AskYesNo("你准备好离开这个地方了吗？"))
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

                if (await AskYesNo("你准备好进入#b#m922000000##k了吗？"))
                {
                    await TryStartSoloQuest(Q3239.QuestIdValue);
                }
            }

        }
    }
}
