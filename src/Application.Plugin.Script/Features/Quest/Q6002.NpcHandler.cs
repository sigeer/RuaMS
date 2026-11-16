using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                await SayOK(this.GetTalkMessage(ScriptMessage.Q6002_Head_ThankSaving));
            }
            else if (isQuestStarted(6002))
            {
                if (haveItem(4031507, 5) && haveItem(4031508, 5))
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Q6002_Head_ThankSaving));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.Q6002_Head_OnlyVery));
            }
        }
    }
}
