using Application.Shared.Constants.Job;
using Application.Shared.Quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 10201 
        public async Task infoMagician()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_MagiciansArmed));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Magician_LikeExperience)))
            {
                await lockUI();
                await warp(1020300, 0);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_WishExperience));
            }
        }

        // Npc: 1032001 
        public async Task magician()
        {
            if (getJob() == Job.BEGINNER)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_WantMagician) + getFirstJobStatRequirement(2) + this.GetTalkMessage(ScriptMessage.Job_Magician_LetsSee));
                if (getLevel() >= 8 && canGetFirstJob(2))
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Magician_LookLike)))
                    {
                        if (canHold(1372043) && canHold(2070000))
                        {
                            if (getJob() == Job.BEGINNER)
                            {
                                await changeJobById(JobId.MAGICIAN);
                                await gainItem(1372043, 1);
                                await resetStats();

                                await SaySpeech([
                                this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightOutPart),
                                this.GetTalkMessage(ScriptMessage.Job_Thief_YouveGotten),
                                this.GetTalkMessage(ScriptMessage.Job_Magician_RememberSkills),
                                this.GetTalkMessage(ScriptMessage.Job_Magician_OneMore),
                                this.GetTalkMessage(ScriptMessage.Job_Magician_AllTeach)
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_Magician_TrainBit));
                }
                return;
            }
            else if (getLevel() >= 30 && getJob() == Job.MAGICIAN)
            {
                var questId = QuestId.Get2ndJobQuest(Job.MAGICIAN);
                if (haveItem(4031012))
                {
                    var jobList = new Job[] { Job.FP_WIZARD, Job.IL_WIZARD, Job.CLERIC };
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_HahaKnew));
                    var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightHave, jobList.Select(j => c.CurrentCulture.GetJobName(j)).Concat([this.GetTalkMessage(ScriptMessage.Job_Thief_IllChoose)])));
                    switch (option)
                    {
                        case 0:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_MagiciansMaster));
                            break;
                        case 1:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_MagiciansMasterIce));
                            break;
                        case 2:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_MagiciansMasterHoly));
                            break;
                        case 3:
                            var select = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Magician_HaveMade, jobList.Select(j => c.CurrentCulture.GetJobName(j))));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_WantMake)))
                            {
                                if (getJob() == Job.MAGICIAN)
                                {
                                    await gainItem(4031012, -1);
                                    await completeQuest(questId);
                                    await changeJobById(job.Id);

                                    await SaySpeech([
                                    this.GetTalkMessage(ScriptMessage.Job_Magician_AlrightYoure, jobStr),
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

                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Magician_PleaseGet)))
                    {
                        if (!haveItem(4031009))
                        {
                            if (canHold(4031009))
                                await gainItem(4031009, 1);
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseMake));
                            }
                        }

                    }
                }
                return;
            }
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.MAGICIAN))
            {
                var questId = QuestId.Get3rdJobQuest(Job.MAGICIAN);
                if (isQuestStarted(questId))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_SeemsReady));
                    }
                    else if (haveItem(4031059))
                    {
                        await gainItem(4031059, -1);
                        await gainItem(4031057, 1);

                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_HaveDefeated));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Magician_FewDays));
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_CloneQuite));
                    }
                    return;
                }
            }
            await SayOK(GetDefault0());
        }

        // Npc: 1032114 
        public Task enter_magicion()
        {
            return EnterTranningMap(910120000);
        }

        // Npc: 1072001 
        public Task change_magician() => Job2Enter(Job.MAGICIAN, 4031009);

        // Npc: 1072005 
        public Task inside_magician() => Job2Exit(Job.MAGICIAN);

        // Npc: 2020009 
        public Task wizard3()
        {
            return Job3((int)JobStyle.MAGICIAN,
                this.GetTalkMessage(ScriptMessage.Job_Magician_Ellinia), 1032001);
        }


        // Npc: 2081200 
        public async Task magician4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.MAGICIAN))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseDont));
                return;
            }
            else if (!isQuestCompleted(6914))
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
                        if (getJob() == Job.FP_ARCHMAGE)
                        {
                            await teachSkill(2121001, 0, 10, -1);
                            await teachSkill(2121002, 0, 10, -1);
                            await teachSkill(2121006, 0, 10, -1);
                        }
                        else if (getJob() == Job.IL_ARCHMAGE)
                        {
                            await teachSkill(2221001, 0, 10, -1);
                            await teachSkill(2221002, 0, 10, -1);
                            await teachSkill(2221006, 0, 10, -1);
                        }
                        else if (getJob() == Job.BISHOP)
                        {
                            await teachSkill(2321001, 0, 10, -1);
                            await teachSkill(2321002, 0, 10, -1);
                            await teachSkill(2321006, 0, 10, -1);
                        }
                        await gainItem(2280003, 1);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Job_Magician_PleaseHave));
                    }
                }
            }
            else
            {
                await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_TeachArt));
                if (getJob() == Job.FP_ARCHMAGE)
                {
                    if (getPlayer().GetPlayerSkillLevel(2121007) == 0)
                    {
                        await teachSkill(2121007, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(2121005) == 0)
                    {
                        await teachSkill(2121005, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(2121003) == 0)
                    {
                        await teachSkill(2121003, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.IL_ARCHMAGE)
                {
                    if (getPlayer().GetPlayerSkillLevel(2221007) == 0)
                    {
                        await teachSkill(2221007, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(2221005) == 0)
                    {
                        await teachSkill(2221005, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(2221003) == 0)
                    {
                        await teachSkill(2221003, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.BISHOP)
                {
                    if (getPlayer().GetPlayerSkillLevel(2321008) == 0)
                    {
                        await teachSkill(2321008, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(2321006) == 0)
                    {
                        await teachSkill(2321006, 0, 10, -1);
                    }
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_DoneLeave));
            }
        }

    }
}
