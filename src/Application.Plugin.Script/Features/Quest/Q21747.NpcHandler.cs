using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
                var inputText = await AskText(this.GetTalkMessage(ScriptMessage.Q21747Npc_EntranceSealed));
                if (inputText == this.GetTalkMessage(ScriptMessage.Q21747Npc_ActionsSpeak))
                {
                    await TryStartSoloQuest(Q21747Feature.QuestIdValue);
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Npc2103008_Error));
                }
            }
            else
            {
                await SayOK(GetDefault0());
            }
        }
    }
}
