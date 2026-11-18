using Application.Core.Client;
using Application.Core.scripting.quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.MushroomCastle
{
    internal sealed class QuestHandler : QuestScriptBase
    {
        public QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc) : base(c, quest, npc)
        {
        }

        // Quest: 2333 
        public async Task q2333s()
        {
            var eim = GetEventInstanceTrust();

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.MushQ_PleaseHelp)))
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.MushQ_PrimeMinister));
                await forceStartQuest();


                var template = eim.EventManager.GetTemplate as Event;
                await template.SpawnBoss(eim);
            }
        }
        // Quest: 2333 
        public async Task q2333e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.MushQ_HurrayDefeated));

            await gainExp(15000);
            await forceCompleteQuest();
        }

        // Quest: 2335 
        public async Task q2335s()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.MushQ_EndMinions));
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.MushQ_IveHeard)))
            {
                if (canHold(4032405))
                {
                    await gainItem(4032405, 1);
                    await forceStartQuest();

                    await SayNext(this.GetTalkMessage(ScriptMessage.MushQ_OneLast));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.MushQ_PleaseHave));
                }
            }
        }

    }
}
