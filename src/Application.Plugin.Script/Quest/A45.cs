
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Quest
{

    // Masteria
    internal partial class QuestScript
    {
        // Quest: 8219 
        public async Task q8219s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_TimeKid)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_CurrentlyWandering));
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }
        }
        // Quest: 8219 
        public async Task q8219e()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.A45_CameBrother));
            await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeemsHelped));
            if (canHold(3992040, 1))
            {
                await forceCompleteQuest();
                await gainItem(3992040, 1);
                await gainExp(175000);
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_DontHave));
            }
        }
        // Quest: 8221 
        public async Task q8221s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_ItsAbout)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_NeedHave));
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }

        }
        // Quest: 8223 
        public async Task q8223s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_JackSent)))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_GreatMission));
                await forceStartQuest();
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }
        }
        // Quest: 8224 
        public async Task q8224s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_TravelerCome, npc)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A45_NeedHunt));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }
        }
        // Quest: 8225 
        public async Task q8225s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_PartnerMake)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A45_VeryProve));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }
        }
        // Quest: 8226 
        public async Task q8226s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_PartTeam)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A45_NextMission));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }
        }
        // Quest: 8227 
        public async Task q8227s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_BuddyNice)))
            {
                if (canHold(4032032, 1))
                {
                    await gainItem(4032032, 1);
                    await SayOK(this.GetTalkMessage(ScriptMessage.A45_VeryIm));
                    await forceStartQuest();
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A45_TheresSlot));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_ComeCity));
            }
        }
        // Quest: 8227 
        public async Task q8227e()
        {
            if (haveItem(4032032, 1))
            {
                await gainItem(4032032, -1);
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A45_BroughtLetter));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_DontBrought));
            }
        }
        // Quest: 8228 
        public async Task q8228s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_HmThats)))
            {
                if (canHold(4032032, 1))
                {
                    await gainItem(4032032, 1);
                    await forceStartQuest();

                    await SayOK(this.GetTalkMessage(ScriptMessage.A45_VeryIm));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A45_TheresSlot));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_ComeCity));
            }
        }
        // Quest: 8228 
        public async Task q8228e()
        {
            if (haveItem(4032032, 1))
            {
                await gainItem(4032032, -1);
                await forceCompleteQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A45_NativeWorld));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_ImAfraid));
            }
        }
        // Quest: 8229 
        public async Task q8229s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_KnewRely)))
            {
                if (haveItem(4032018, 1))
                {
                    await forceStartQuest();
                }
                else if (canHold(4032018, 1))
                {
                    await gainItem(4032018, 1);
                    await forceStartQuest();
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.A45_OyNeed));
                }
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_ComeCity));
            }
        }
        // Quest: 8229 
        public async Task q8229e()
        {
            if (haveItem(4032018, 1))
            {
                await gainItem(4032018, -1);
                await gainExp(50000);

                await forceCompleteQuest();
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_BroughtNicely));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_WhatsWrong));
            }
        }
        // Quest: 8230 
        public async Task q8230s()
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_TravelerNeed)))
            {
                await forceStartQuest();

                await SayOK(this.GetTalkMessage(ScriptMessage.A45_ThatsThing));

            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_SeeAround));
            }
        }
        // Quest: 8230 
        public async Task q8230e()
        {
            if (haveItem(3992041))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_AhAccomplish));
                await forceCompleteQuest();
            }
            else if (getQuestStatus(8223) == 2)
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_CompletedMission));
            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_FolksBack));
            }
        }

        async Task q8231_8238(string target, int requiredItem, int count)
        {
            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.A45_TravelerNeedHelp) + target + this.GetTalkMessage(ScriptMessage.A45_Text)))
            {
                await forceStartQuest();

                var reqs = this.GetTalkMessage(ScriptMessage.A45_ItemAmountFormat, count, requiredItem);
                await SayOK(this.GetTalkMessage(ScriptMessage.A45_VeryGet) + reqs + this.GetTalkMessage(ScriptMessage.A45_AsapNlc));

            }
        }

        // Quest: 8231 
        public Task q8231s() => q8231_8238(this.GetTalkMessage(ScriptMessage.A45_Leprechauns), 4032031, 30);
        // Quest: 8232 
        public Task q8232s() => q8231_8238("#o9400583#", 4032031, 30);

        // Quest: 8233 
        public Task q8233s() => q8231_8238("#o9400580#", 4032011, 30);

        // Quest: 8234 
        public Task q8234s() => q8231_8238("#o9400580#", 4032011, 30);

        // Quest: 8235 
        public Task q8235s() => q8231_8238("#o9400549#", 4031903, 1);

        // Quest: 8236 
        public Task q8236s() => q8231_8238("#o9400549#", 4031903, 1);

        // Quest: 8237 
        public Task q8237s() => q8231_8238("#o9400575#", 4032013, 1);

        // Quest: 8238 
        public Task q8238s() => q8231_8238("#o9400575#", 4032013, 1);

    }
}