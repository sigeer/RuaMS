using Application.Core.Client;
using Application.Shared.Constants.Job;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class Q21401QuestHandler : Application.Core.scripting.quest.QuestScriptBase
    {
        public Q21401QuestHandler(IChannelClient c, Application.Core.Server.quest.Quest quest, int npc)
            : base(c, quest, npc)
        {
        }

        // Quest: 21401 
        public async Task q21401s()
        {
            await SaySpeech([
                this.GetTalkMessage(ScriptMessage.Q21401_LookLike),
                this.GetTalkMessage(ScriptMessage.Q21401_WhileTrapped),
                this.GetTalkMessage(ScriptMessage.Q21401_SinceAwoke)
                ]);
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q21401_PleaseAran)))
            {
                await TryStartSoloQuest(getQuest(), forceStartQuest);
            }
        }

        // Quest: 21401 
        public async Task q21401e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Q21401_ThankAran));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Q21401_AnywayJust)))
            {
                if (!isQuestCompleted(21401))
                {
                    if (!canHold(2280003, 1))
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Q21401_WowEquip));
                        return;
                    }

                    await gainItem(2280003, 1);
                    await changeJobById(JobId.ARAN4);

                    await completeQuest();
                }
                await SayNext(this.GetTalkMessage(ScriptMessage.Q21401_AllDormant));
            }
        }
    }
}
