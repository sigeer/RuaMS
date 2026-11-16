using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Pirate4thJobFeature
{
    internal sealed class Pirate4thJobFeatureNpcHandler : NpcScriptBase
    {
        public Pirate4thJobFeatureNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 1092019 
        public async Task s4strike()
        {
            if (!isQuestStarted(6400))
            {
                await SayOK(GetDefaultTalk(6400));
            }
            else
            {
                var seagullProgress = getQuestProgressInt(6400, 1);
                if (seagullProgress == 0)
                {
                    string[] seagullQuestion = [
                    "One day, I went to the ocean and caught 62 Octopi for dinner. But then some kid came by and gave me 10 Octopi as a gift! How many Octopi do I have then, in total?"
                    ];
                    string[] seagullAnswer = ["72"];
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pirate4_IllGive));
                    for (int i = 0; i < seagullQuestion.Length; i++)
                    {
                        var inputAnswer = await AskText(seagullQuestion[i]);
                        if (inputAnswer == seagullAnswer[i])
                        {
                            continue;
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Pirate4_HmmThats));
                            return;
                        }
                    }
                    await setQuestProgress(6400, 1, 1);
                    await SayNext(this.GetTalkMessage(ScriptMessage.Pirate4_CantBelieve));
                    return;
                }
                else if (seagullProgress == 1)
                {
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Pirate4_LetsGoOnto),
                        this.GetTalkMessage(ScriptMessage.Pirate4_ImGoing),
                        this.GetTalkMessage(ScriptMessage.Pirate4_AnywayOnly)
                        ]);
                    var em = GetEventManager(Pirate4thJobFeature.StrikeEventName);
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pirate4_OhhhhImpressive));
                }

            }
        }

        // Npc: 1022103 
        public Task s4strike_statue()
        {
            // TODO
            return Task.CompletedTask;
        }

        #region mind
        // Npc: 2095000 
        public async Task s4mind()
        {
            if (getMapId() != 925010400)
            {
                var em = GetEventManager(Pirate4thJobFeature.MindEventName);

                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Pirate4_EnterMenu, em.Template.GetRequirementDescription(c)), [
                    this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                    this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
                ]);

                if (option == 0)
                {
                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Pirate4_PartyQuest));
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pirate4_MissionSucceeded)))
                {
                    await warp(120000104);
                }
            }
        }

        // Npc: 1092008 
        public async Task s4mind_in()
        {
            if (!isQuestStarted(6410))
            {
                await SayOK(GetDefaultTalk(6410));
                return;
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pirate4_LetsGo)))
                {
                    await warp(925010000, 0);
                }
            }
        }

        // Npc: 1095000 
        public async Task s4mind_out()
        {
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Pirate4_SomeWay)))
            {
                await warp(120000104);
            }
        }
        #endregion
    }
}
