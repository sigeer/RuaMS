using Application.Shared.Constants.Job;
using Application.Shared.Quest;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 10200 
        public async Task infoArcher()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_BowmenBlessed));
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Archer_LikeExperience)))
            {
                await lockUI();
                await warp(1020300, 0);
            }
            else
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_WishExperience));
            }
        }

        // Npc: 1012100 
        public async Task bowman()
        {
            if (getJobId() == 0)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_DecidedBecome) + getFirstJobStatRequirement(3) + this.GetTalkMessage(ScriptMessage.Job_Archer_LetsSee));
                if (getLevel() >= 10 && canGetFirstJob(3))
                {
                    await SaySpeech([
                        this.GetTalkMessage(ScriptMessage.Job_Archer_DecidedBecome) + getFirstJobStatRequirement(3) + this.GetTalkMessage(ScriptMessage.Job_Archer_LetsSee),
                        this.GetTalkMessage(ScriptMessage.Job_Archer_ImportantFinal)
                        ], current: 1);

                    if (canHold(1452051) && canHold(2070000))
                    {
                        if (getJobId() == 0)
                        {
                            await changeJobById(300);
                            await gainItem(1452051, 1);
                            await gainItem(2060000, 1000);
                            await resetStats();
                        }
                        await SaySpeech([
                            this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightOutPart),
                                this.GetTalkMessage(ScriptMessage.Job_Thief_YouveGotten),
                                this.GetTalkMessage(ScriptMessage.Job_Archer_ReminderOnce)
                            ]);
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_MakeSome));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.Job_Archer_TrainBit));
                }
                return;
            }
            else if (getLevel() >= 30 && getJob() == Job.BOWMAN)
            {
                var questId = QuestId.Get2ndJobQuest(Job.BOWMAN);
                if (haveItem(4031012))
                {
                    await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_HahaKnew));
                    var option = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Thief_AlrightHave), [
                        this.GetTalkMessage(ScriptMessage.Job_Archer_PleaseExplain),
                        this.GetTalkMessage(ScriptMessage.Job_Archer_PleaseExplainBeing),
                        this.GetTalkMessage(ScriptMessage.Job_Archer_IllChoose)
                        ]);
                    switch (option)
                    {
                        case 0:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_ArchersMaster));
                            break;
                        case 1:
                            await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_ArchersMasterCrossbows));
                            break;
                        case 2:
                            var select = await AskMenu(this.GetTalkMessage(ScriptMessage.Job_Archer_HaveMade));
                            var job = 300 + select * 10;
                            var jobStr = (job == 310 ? this.GetTalkMessage(ScriptMessage.Job_Archer_Hunter) : this.GetTalkMessage(ScriptMessage.Job_Archer_Crossbowman));
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Archer_WantMake)))
                            {
                                await gainItem(4031012, -1);
                                await forceCompleteQuest(questId);
                                if (getJobId() != job)
                                    await changeJobById(job);

                                await SaySpeech([
                                    this.GetTalkMessage(ScriptMessage.Job_Archer_AlrightYoure, jobStr),
                                    this.GetTalkMessage(ScriptMessage.Job_Archer_HaveJust),
                                    this.GetTalkMessage(ScriptMessage.Job_Thief_HaveAlso),
                                    this.GetTalkMessage(ScriptMessage.Job_Archer_NeedStrong, jobStr)
                                    ]);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Job_Archer_HmmmHave)))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_GoodDecision));
                        if (!haveItem(4031010))
                        {
                            if (canHold(4031010))
                            {
                                await gainItem(4031010, 1);
                            }
                            else
                            {
                                await SayNext(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseMake));
                                return;
                            }
                        }
                        if (!isQuestStarted(questId))
                        {
                            await startQuest(questId);
                        }

                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_PleaseGet));
                    }
                }
                return;
            }
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.BOWMAN))
            {
                var questId = QuestId.Get3rdJobQuest(Job.BOWMAN);
                if (isQuestStarted(questId))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_SeemsReady));
                    }
                    else if (haveItem(4031059))
                    {
                        await gainItem(4031059, -1);
                        await gainItem(4031057, 1);

                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_HaveDefeated));
                    }
                    else
                    {
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_FewDays));
                        await SayNext(this.GetTalkMessage(ScriptMessage.Job_Archer_CloneQuite));
                    }
                    return;
                }
            }
            await SayOK(GetDefault0());
        }

        // Npc: 1012119 
        public Task enter_archer()
        {
            return EnterTranningMap(910060000);
        }

        // Npc: 1072002 
        public Task change_archer() => Job2Enter(Job.BOWMAN, 4031010);

        // Npc: 1072006 
        public Task inside_archer() => Job2Exit(Job.BOWMAN);

        // Npc: 2020010 
        public Task bowman3()
        {
            return Job3((int)JobStyle.BOWMAN,
                this.GetTalkMessage(ScriptMessage.Henesys), 1012100);
        }


        // Npc: 2081300 
        public async Task archer4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.BOWMAN))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_PleaseDont));
                return;
            }
            else if (!isQuestCompleted(6924))
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
                        if (getJob() == Job.BOWMASTER)
                        {
                            await teachSkill(3121002, 0, 10, -1);
                            await teachSkill(3121005, 0, 10, -1);
                            await teachSkill(3121007, 0, 10, -1);
                        }
                        else if (getJob() == Job.MARKSMAN)
                        {
                            await teachSkill(3221002, 0, 10, -1);
                            await teachSkill(3221004, 0, 10, -1);
                            await teachSkill(3221006, 0, 10, -1);
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
                if (getJob() == Job.BOWMASTER)
                {
                    if (getPlayer().GetPlayerSkillLevel(3121008) == 0)
                    {
                        await teachSkill(3121008, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(3121006) == 0)
                    {
                        await teachSkill(3121006, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(3121004) == 0)
                    {
                        await teachSkill(3121004, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.MARKSMAN)
                {
                    if (getPlayer().GetPlayerSkillLevel(3221007) == 0)
                    {
                        await teachSkill(3221007, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(3221005) == 0)
                    {
                        await teachSkill(3221005, 0, 10, -1);
                    }
                    if (getPlayer().GetPlayerSkillLevel(3221001) == 0)
                    {
                        await teachSkill(3221001, 0, 10, -1);
                    }
                }
                await SayOK(this.GetTalkMessage(ScriptMessage.Job_Thief_DoneLeave));
            }
        }
    }
}
