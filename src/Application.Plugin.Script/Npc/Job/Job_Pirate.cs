using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants.Job;
using Application.Shared.Quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 10204 
        public async Task infoPirate()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_PiratesBlessed));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Pirate_LikeExperience)))
            {
                await lockUI();
                await warp(1020500, 0);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_WishExperience));
            }
        }

        // Npc: 1095002 
        public Task enter_pirate()
        {
            return EnterTranningMap(912030000);
        }
        // Npc: 1072008 
        public async Task inside_pirate()
        {
            int currentMapId = getPlayer().getMapId();

            if (currentMapId == 108000502 || currentMapId == 108000501)
            {
                var item = currentMapId == 108000502 ? 4031856 : 4031857;
                if (!haveItem(item, 15))
                {
                    var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Pirate_HaventBrought));
                    await removeAll(item);
                    await warp(120000101, 0);
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_WowHave, item));
                    await warp(120000101, 0);
                }
            }
            else
            {
                throw new ConversationDiffMapException();
            }
        }

        // Npc: 1090000 
        public async Task kairinT()
        {
            if (getJob() == Job.BEGINNER)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_WantPirate, getFirstJobStatRequirement(5)));
                if (getLevel() >= 10 && canGetFirstJob(5))
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Pirate_LookLike)))
                    {
                        if (canHold(2330000) && canHoldAll([1482000, 1492000]))
                        {
                            if (getJob() == Job.BEGINNER)
                            {
                                await changeJobById(JobId.PIRATE);
                                await gainItem(1482000, 1);
                                await gainItem(1492000, 1);
                                await resetStats();

                                await SaySpeech([
                                this.GetTalkMessage(ScriptMessage.Job_Pirate_AlrightOut),
                                this.GetTalkMessage(ScriptMessage.Job_Thief_YouveGotten),
                                this.GetTalkMessage(ScriptMessage.Job_Pirate_ReminderOnce)
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
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_Pirate_TrainBit));
                    return;
                }
                return;
            }
            else if (getLevel() >= 30 && getJob() == Job.PIRATE)
            {
                if (isQuestCompleted(2191) || isQuestCompleted(2192))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_SeeHave));
                    var jobList = new Job[] { Job.BRAWLER, Job.GUNSLINGER };
                    var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightHave, jobList.Select(j => c.CurrentCulture.GetJobName(j)).Concat([this.GetTalkMessage(ScriptMessage.Job_Thief_IllChoose)])));
                    switch (option)
                    {
                        case 0:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_PiratesMaster));
                            break;
                        case 1:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_PiratesMasterGuns));
                            break;
                        case 2:
                            var select = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Pirate_AlrightHave, jobList.Select(j => c.CurrentCulture.GetJobName(j))));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_WantMake, jobStr)))
                            {
                                var requiredQuest = job == Job.BRAWLER ? 2191 : 2192;
                                if (!isQuestCompleted(requiredQuest))
                                {
                                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_Pirate_FirstNeed, c.CurrentCulture.GetQuestName(requiredQuest)));
                                    return;
                                }

                                if (getJob() == Job.PIRATE)
                                {
                                    await changeJobById(job.Id);

                                    await SaySpeech([
                                        job == Job.BRAWLER
                                        ? this.GetTalkMessage(ScriptMessage.Job_Pirate_OutBrawler)
                                        : this.GetTalkMessage(ScriptMessage.Job_Pirate_OutGunslinger),
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_HaveJust),
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_HaveAlso),
                                    this.GetTalkMessage(ScriptMessage.Job_Pirate_NeedStrong, jobStr)
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
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_ProgressHave));
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_GoodDecision));
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Pirate_LikeTake)))
                    {
                        var jobQuest = 0;
                        if (isQuestStarted(2191))
                        {
                            jobQuest = 2191;
                        }
                        else if (isQuestStarted(2192))
                        {
                            jobQuest = 2192;
                        }
                        else
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.Job_Pirate_HaventAccepted));
                            return;
                        }

                        await TryStartSoloQuest(jobQuest);
                    }
                }
                return;

            }
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.PIRATE))
            {
                var questId = QuestId.Get3rdJobQuest(Job.PIRATE);
                if (isQuestStarted(questId))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_SeemsReady));
                    }
                    else if (haveItem(4031059))
                    {
                        await gainItem(4031059, -1);
                        await gainItem(4031057, 1);

                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_HaveDefeated));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_FewDays));
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_CloneQuite));
                    }
                    return;
                }
            }
            else if (isQuestStarted(6330) || isQuestStarted(6370))
            {
                var questId = isQuestStarted(6330) ? 6330 : 6370;
                var qProgress = getQuestProgressInt(questId, questId + 1);
                if (qProgress == 0)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_YoureReady));
                    await TryStartSoloQuest(questId);
                }
                else if (getEventInstance() != null)
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Pirate_BadAll));
                    await WarpOut();
                }
                return;
            }

            await SayOK(GetDefault0());
        }

        // Npc: 2020013 
        public Task pirate3()
        {
            return Job3((int)JobStyle.PIRATE,
                this.GetTalkMessage(ScriptMessage.Job_Pirate_NautilusHarbor), 1090000);
        }


        // Npc: 2081500 
        public async Task pirate4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.PIRATE))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseDont));
                return;
            }

            if (!isQuestCompleted(6944))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_HaveYet));
                return;
            }

            if (getJob().Rank == 3)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Thief_MarvellousJob)))
                {
                    if (canHold(2280003, 1))
                    {
                        int jobId = getJob().Id;
                        await changeJobById(jobId + 1);

                        jobId = getJob().Id;
                        if (jobId == 512)
                        {
                            await teachSkill(5121001, 0, 10, -1);
                            await teachSkill(5121002, 0, 10, -1);
                            await teachSkill(5121007, 0, 10, -1);
                            await teachSkill(5121009, 0, 10, -1);
                        }
                        else if (jobId == 522)
                        {
                            await teachSkill(5220001, 0, 10, -1);
                            await teachSkill(5220002, 0, 10, -1);
                            await teachSkill(5221004, 0, 10, -1);
                            await teachSkill(5220011, 0, 10, -1);
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
                var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_TeachArt));
                if (option == 0)
                {
                    int jobId = getJob().Id;
                    if (jobId == 512)
                    {
                        if (getPlayer().GetPlayerSkillLevel(5121003) == 0)
                        {
                            await teachSkill(5121003, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5121004) == 0)
                        {
                            await teachSkill(5121004, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5121005) == 0)
                        {
                            await teachSkill(5121005, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5121010) == 0)
                        {
                            await teachSkill(5121010, 0, 10, -1);
                        }
                    }
                    else if (jobId == 522)
                    {
                        if (getPlayer().GetPlayerSkillLevel(5221006) == 0)
                        {
                            await teachSkill(5221006, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5221007) == 0)
                        {
                            await teachSkill(5221007, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5221008) == 0)
                        {
                            await teachSkill(5221008, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5221009) == 0)
                        {
                            await teachSkill(5221009, 0, 10, -1);
                        }
                        if (getPlayer().GetPlayerSkillLevel(5221003) == 0)
                        {
                            await teachSkill(5221003, 0, 10, -1);
                        }
                    }

                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_DoneLeave));
                }
            }
        }
    }
}
