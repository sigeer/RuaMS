using Application.Core.Client;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21747NpcHandler : Application.Core.scripting.npc.NpcScriptBase
    {
        public Q21747NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 2091009 
        public async Task enterShadow()
        {
            if (isQuestStarted(Q21747Feature.QuestIdValue) && getQuestProgressInt(Q21747Feature.QuestIdValue, 9300351) == 0)
            {
                var inputText = await AskText("封印神殿的入口... #b暗号#k!");
                if (inputText == "道可道非常道")
                {
                    await TryStartSoloQuest(Q21747Feature.QuestIdValue);
                }
                else
                {
                    await SayOK("#r错误！");
                }
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }
    }
}
