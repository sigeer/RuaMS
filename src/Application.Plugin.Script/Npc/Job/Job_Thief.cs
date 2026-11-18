using Application.Shared.Constants.Job;
using Application.Shared.Quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 10203 
        public async Task infoRogue()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_ThievesPerfect));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_LikeExperience)))
            {
                await lockUI();
                await warp(1020400, 0);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_WishExperience));
            }
        }

        // Npc: 1052001 
        public async Task rogue()
        {
            if (getJob() == Job.BEGINNER)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_WantThief));
                if (getLevel() >= 10 && canGetFirstJob(4))
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_LookLike)))
                    {
                        if (canHold(2070000) && canHoldAll([1472061, 1332063]))
                        {
                            if (getJob() == Job.BEGINNER)
                            {
                                await changeJobById(JobId.THIEF);
                                await gainItem(2070015, 500);
                                await gainItem(1472061, 1);
                                await gainItem(1332063, 1);
                                await resetStats();

                                await SaySpeech([
                                this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightOutPart),
                                this.GetTalkMessage(ScriptMessage.Job_Thief_YouveGotten),
                                this.GetTalkMessage(ScriptMessage.Job_Thief_ReminderOnce)
                                ]);
                            }

                        }
                        else
                        {
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_MakeSome));
                        }
                    }

                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_TrainBit));
                    return;
                }
                return;
            }
            else if (getLevel() >= 30 && getJob() == Job.THIEF)
            {
                var questId = QuestId.Get2ndJobQuest(Job.THIEF);
                if (haveItem(4031012))
                {
                    var jobList = new Job[] { Job.ASSASSIN, Job.BANDIT };
                    var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightHave, jobList.Select(j => c.CurrentCulture.GetJobName(j)).Concat([this.GetTalkMessage(ScriptMessage.Job_Thief_IllChoose)])));
                    switch (option)
                    {
                        case 0:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_ThievesMaster));
                            break;
                        case 1:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_ThievesMasterDaggers));
                            break;
                        case 2:
                            var select = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_HaveMade, jobList.Select(j => c.CurrentCulture.GetJobName(j))));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_WantMake)))
                            {
                                if (getJob() == Job.THIEF)
                                {
                                    await gainItem(4031012, -1);
                                    await completeQuest(questId);
                                    await changeJobById(job.Id);

                                    await SaySpeech([
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightOut, jobStr),
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_HaveJust, jobStr),
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_HaveAlso),
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_NeedStrong, jobStr)
                                    ]);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_GoodDecision));
                    if (!isQuestStarted(questId))
                    {
                        await startQuest(questId);
                    }

                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseGet)))
                    {
                        if (!haveItem(4031011))
                        {
                            if (canHold(4031011))
                                await gainItem(4031011, 1);
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseMake));
                            }
                        }

                    }
                }
                return;
            }
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.THIEF))
            {
                var questId = QuestId.Get3rdJobQuest(Job.THIEF);
                if (isQuestStarted(questId))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_SeemsReady));
                    }
                    else if (haveItem(4031059))
                    {
                        await gainItem(4031059, -1);
                        await gainItem(4031057, 1);

                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_HaveDefeated));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_FewDays));
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_CloneQuite));
                    }
                    return;
                }
            }
            await SayOK(GetDefault0());
        }

        // Npc: 1052114 
        public Task enter_thief()
        {
            return EnterTranningMap(910310000);
        }


        // Npc: 1072003 
        public Task change_rogue() => Job2Enter(Job.THIEF, 4031011);
        // Npc: 1072007 
        public Task inside_rogue() => Job2Exit(Job.THIEF);
        // Npc: 2020011 
        public Task thief3()
        {
            return Job3((int)JobStyle.THIEF,
                this.GetTalkMessage(ScriptMessage.Job_Thief_KerningCity), 1052001);
        }


        // Npc: 2081400 
        public async Task thief4()
        {
            if (getLevel() < 120 || getJob().IsSameJobGroup(Job.THIEF))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseDont));
                return;
            }
            else if (!isQuestCompleted(6934))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_HaveYet));
                return;
            }
            else if (getJob().Rank == 3)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_MarvellousJob)))
                {
                    if (canHold(2280003, 1))
                    {
                        await changeJobById(getJobId() + 1);
                        if (getJob() == Job.NIGHTLORD)
                        {
                            await teachSkill(4120002, 0, 10, -1);
                            await teachSkill(4120005, 0, 10, -1);
                            await teachSkill(4121006, 0, 10, -1);
                        }
                        else if (getJob() == Job.SHADOWER)
                        {
                            await teachSkill(4220002, 0, 10, -1);
                            await teachSkill(4220005, 0, 10, -1);
                            await teachSkill(4221007, 0, 10, -1);
                        }
                        await gainItem(2280003, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseHave));
                    }
                }
            }
            else
            {
                await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_TeachArt));
                if (getJob() == Job.NIGHTLORD)
                {
                    if (getPlayer().GetPlayerSkillLevel(4121008) == 0)
                    {
                        await teachSkill(4121008, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(4121004) == 0)
                    {
                        await teachSkill(4121004, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.SHADOWER)
                {
                    if (getPlayer().GetPlayerSkillLevel(4221004) == 0)
                    {
                        await teachSkill(4221004, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(4221001) == 0)
                    {
                        await teachSkill(4221001, 0, 10, -1);
                    }
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_DoneLeave));
            }
        }
    }
}
