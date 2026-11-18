using Application.Shared.Constants.Job;
using Application.Shared.Constants.Skill;
using Application.Shared.Quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 职业-战士相关
    /// </summary>
    internal partial class NpcScript
    {
        // Npc: 10202 
        public async Task infoSwordman()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_WarriorsPossess));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_SwordMan_LikeExperience)))
            {
                await lockUI();
                await warp(1020100, 0);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_WishExperience));
            }
        }

        // Npc: 1022000 
        public async Task fighter()
        {
            if (getJobId() == 0)
            {
                if (getLevel() >= 10 && canGetFirstJob(1))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_SeemFit));

                    if (canHold(1302077))
                    {
                        if (getJobId() == 0)
                        {
                            await changeJobById(JobId.WARRIOR);
                            await gainItem(1302077, 1);
                            await resetStats();
                        }
                        await SaySpeech([
                            this.GetTalkMessage(ScriptMessage.Job_SwordMan_MomentWalk),
                                this.GetTalkMessage(ScriptMessage.Job_SwordMan_RewardAdvancement),
                                this.GetTalkMessage(ScriptMessage.Job_SwordMan_ProperWarrior)
                            ]);
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_MakeSome));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_SwordMan_DontMeet));
                    return;
                }
                return;
            }
            else if (getLevel() >= 30 && getJob() == Job.WARRIOR)
            {
                var questId = QuestId.Get2ndJobQuest(Job.WARRIOR);
                var jobList = new Job[] { Job.FIGHTER, Job.PAGE, Job.SPEARMAN };
                if (haveItem(4031012))
                {
                    var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_SwordMan_VeryLike, jobList.Select(j => c.CurrentCulture.GetJobName(j)).Concat([this.GetTalkMessage(ScriptMessage.Job_Thief_IllChoose)])));
                    switch (option)
                    {
                        case 0:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_WarriorsWieldSwords));
                            break;
                        case 1:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_WarriorsWield));
                            break;
                        case 2:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_WarriorsWieldSpears));
                            break;
                        case 3:
                            var select = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_SwordMan_PleaseChoose, jobList.Select(j => c.CurrentCulture.GetJobName(j))));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_SwordMan_SureWant)))
                            {
                                await gainItem(4031012, -1);
                                await completeQuest(questId);
                                await changeJobById(job.Id);

                                await SaySpeech([
                                    this.GetTalkMessage(ScriptMessage.Job_SwordMan_Congratulations, jobStr),
                                    this.GetTalkMessage(ScriptMessage.Job_SwordMan_StartLearning, jobStr),
                                    this.GetTalkMessage(ScriptMessage.Job_SwordMan_HaveAlso),
                                    this.GetTalkMessage(ScriptMessage.Job_SwordMan_AlrightContinue)
                                    ]);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_PrepareNd));
                    if (!isQuestStarted(questId))
                        await startQuest(questId);

                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_SwordMan_PleaseDeliver)))
                    {
                        if (!haveItem(4031008))
                        {
                            if (canHold(4031008))
                            {
                                await gainItem(4031008, 1);
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_EtcFull));
                                return;
                            }
                        }

                    }
                }
                return;
            }
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.WARRIOR))
            {
                var questId = QuestId.Get3rdJobQuest(Job.WARRIOR);
                if (isQuestStarted(questId))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_SeemsReady));
                    }
                    else if (haveItem(4031059))
                    {
                        await gainItem(4031059, -1);
                        await gainItem(4031057, 1);

                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_HaveDefeated));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_SwordMan_FewDays));
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_CloneQuite));
                    }
                    return;
                }
            }
            await SayOK(GetDefault0());
        }

        // Npc: 1022105 
        public Task enter_warrior()
        {
            return EnterTranningMap(910220000);
        }



        // Npc: 1072000 
        public Task change_swordman() => Job2Enter(Job.WARRIOR, 4031008);

        // Npc: 1072004 
        public Task inside_swordman() => Job2Exit(Job.WARRIOR);

        // Npc: 2020008 
        public Task warrior3()
        {
            return Job3((int)JobStyle.WARRIOR,
                this.GetTalkMessage(ScriptMessage.Job_SwordMan_Perion), 1022000);
        }

        // Npc: 2081100 
        public async Task warrior4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.WARRIOR))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseDont));
                return;
            }
            else if (!isQuestCompleted(6904))
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
                        if (getJob() == Job.HERO)
                        {
                            await teachSkill(1121001, 0, 10, -1);
                            await teachSkill(1120004, 0, 10, -1);
                            await teachSkill(1121008, 0, 10, -1);
                        }
                        else if (getJob() == Job.PALADIN)
                        {
                            await teachSkill(1221001, 0, 10, -1);
                            await teachSkill(1220005, 0, 10, -1);
                            await teachSkill(1221009, 0, 10, -1);
                        }
                        else if (getJob() == Job.DARKKNIGHT)
                        {
                            await teachSkill(1321001, 0, 10, -1);
                            await teachSkill(1320005, 0, 10, -1);
                            await teachSkill(1320005, 0, 10, -1);
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
                // 正常应该通过任务学习
                await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_TeachArt));
                if (getJob() == Job.HERO)
                {
                    if (getPlayer().GetPlayerSkillLevel(Hero.ENRAGE) == 0)
                    {
                        await teachSkill(Hero.ENRAGE, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(Hero.GUARDIAN) == 0)
                    {
                        await teachSkill(Hero.GUARDIAN, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(Hero.STANCE) == 0)
                    {
                        await teachSkill(Hero.STANCE, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.PALADIN)
                {
                    if (getPlayer().GetPlayerSkillLevel(1221002) == 0)
                    {
                        await teachSkill(1221002, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(1221003) == 0)
                    {
                        await teachSkill(1221003, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(1221004) == 0)
                    {
                        await teachSkill(1221004, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.DARKKNIGHT)
                {
                    if (getPlayer().GetPlayerSkillLevel(1321002) == 0)
                    {
                        await teachSkill(1321002, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(1321008) == 0)
                    {
                        await teachSkill(1321008, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(1321009) == 0)
                    {
                        await teachSkill(1321009, 0, 10, -1);
                    }
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_DoneLeave));
            }
        }


    }
}
