using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q6002NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q6002NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }


        public async Task tamepig_enter()
        {
            if (isQuestCompleted(6002))
            {
                await SayOK("谢谢你救了那只猪。");
            }
            else if (isQuestStarted(6002))
            {
                if (haveItem(4031507, 5) && haveItem(4031508, 5))
                {
                    await SayOK("谢谢你救了那只猪。");
                }
                else
                {
                    await TryStartSoloQuest(Q6002Feature.QuestIdValue, async () =>
                    {
                        await removeAll(4031507);
                        await removeAll(4031508);
                    });
                }
            }
            else
            {
                await SayOK("只有少数冒险者有资格保护守望者猪。");
            }
        }
    }
}
