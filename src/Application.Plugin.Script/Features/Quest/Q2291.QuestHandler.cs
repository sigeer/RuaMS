using Application.Core.Client;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q2291QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q2291QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 2291 
        public async Task q2291e()
        {
            if (!haveItem(4032521, 10))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Q2291_DidntGet));
                return;
            }
            else
            {

                await SayNext(this.GetTalkMessage(ScriptMessage.Q2291_GotGreat));

                var em = GetSoloQuestEventManager(Q2291.QuestIdValue);
                if (await em.StartInstance(getPlayer()) != Core.scripting.Events.Abstraction.CreateInstanceResult.Success)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Q2291_UhLooks));
                    return;
                }

                await gainItem(4032521, -10);
                await forceCompleteQuest();
            }
        }
    }
}
