using Application.Shared.Constants.Job;
using Application.Shared.Constants.Skill;
using Application.Shared.Quest;

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
            await SayNext("战士拥有巨大的力量和持久力来支持它，他们在近战战斗中表现最出色。普通攻击本身就很强大，再加上复杂的技能，这个职业非常适合进行爆发性攻击。");
            if (await AskYesNo("你想体验一下成为一个战士是什么感觉吗？"))
            {
                lockUI();
                warp(1020100, 0);
            }
            else
            {
                await SayNext("如果你想体验成为一个战士的感觉，再来找我吧。");
            }
        }

        // Npc: 1022000 
        public async Task fighter()
        {
            // TODO
            if (getJobId() == 0)
            {
                if (getLevel() >= 10 && canGetFirstJob(1))
                {
                    await SayNext("看来你符合条件，请注意：#r一旦转职就不能再更换职业了#k，如果还没想好就点#b结束对话#k。");

                    if (canHold(1302077))
                    {
                        if (getJobId() == 0)
                        {
                            changeJobById(JobId.WARRIOR);
                            gainItem(1302077, 1);
                            resetStats();
                        }
                        await SaySpeech([
                            "从此刻起，你正式踏上了战士之路。这不是一件容易的事情，但只要你有足够的勇气和信心，你将克服所有的困难。",
                                "你作为转职奖励，我给你每个背包都增加了4个格子，另外我还给了你一点 SP你可以用它来提升技能。",
                                "好了，你已经是一名合格的战士了！"
                            ]);
                    }
                    else
                    {
                        await SayNext("清理一下你的背包，然后回来找我说话。");
                    }
                }
                else
                {
                    await SayOK("你不满足成为#r战士#k的要求，继续努力吧。");
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
                    var option = await AskMenu("很好，那你接下来想", jobList.Select(j => $"请介绍一下 {c.CurrentCulture.GetJobName(j)}").Concat(["我要选择职业！"]));
                    switch (option)
                    {
                        case 0:
                            await SayNext("使用#r剑或斧#k的战士。\r\n\r\n剑客可以提高队伍的物理输出，完成第四次转职后拥有不俗的输出能力。");
                            break;
                        case 1:
                            await SayNext("使用#r剑或钝器#k的战士。\r\n\r\n准骑士可以降低怪物属性，完成第三次转职后拥有属性攻击能力。");
                            break;
                        case 2:
                            await SayNext("使用#r枪或矛#k的战士。\r\n\r\n枪战士可以提高队伍的生存能力，完成第三次转职后拥有不俗的输出能力。");
                            break;
                        case 3:
                            var select = await AskMenu("请选择你的转职方向", jobList.Select(j => $"{c.CurrentCulture.GetJobName(j)}"));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo($"你确定要转职为{jobStr}吗？一旦选择，就不能再更改了。"))
                            {
                                gainItem(4031012, -1);
                                completeQuest(questId);
                                changeJobById(job.Id);

                                await SaySpeech([
                                    $"恭喜你成为了一名{jobStr}！",
                                    $"现在你可以开始学习#b{jobStr}#k的技能了。此外，你的杂项物品栏也扩展了一行。你的最大生命值和魔法值也增加了。去检查一下，看看吧。",
                                    "我还给了你一点二转的 #bSP#k，打开技能栏学习二转技能。",
                                    "好了继续你的旅途吧。"
                                    ]);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    await SayNext("现在你可以准备第二次转职测试了。");
                    if (!isQuestStarted(questId))
                        startQuest(questId);

                    if (await AskYesNo("请把这封信带给#b#p1072000##k，他在#b#m102020300##k附近。"))
                    {
                        if (!haveItem(4031008))
                        {
                            if (canHold(4031008))
                            {
                                gainItem(4031008, 1);
                            }
                            else
                            {
                                await SayNext("#r其他栏满了 -> #r其他栏已满");
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
                        await SayNext("看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020008##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else if (haveItem(4031059))
                    {
                        gainItem(4031059, -1);
                        gainItem(4031057, 1);

                        await SayNext("你成功击败了我的分身并带回了#b#t4031059##k！看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020008##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else
                    {
                        await SayNext("我一直在等你！几天前，我从#b#p2020008##k听到了你的消息，你现在可以变得更强，但是你得通过我的考验。在蚂蚁广场有一个异界之门，里面有我的分身，你去击败它并带回#b#t4031059##k。");
                        await SayNext("我的分身相当强大，他会使用特殊技能，你要跟他一对一战斗，击败他并带回#b#t4031059##k，祝你好运！");
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
                "勇士部落", 1022000);
        }




        // Npc: 2081100 
        public async Task warrior4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.WARRIOR))
            {
                await SayOK("请不要现在打扰我，我正在集中精力。");
                return;
            }
            else if (!isQuestCompleted(6904))
            {
                await SayOK("你还没有通过我的考验。在你通过考验之前，我无法提升你的等级。");
                return;
            }
            else if (getJob().Rank == 3)
            {
                if (await AskYesNo("你通过了我的测试，做得非常出色。你准备好晋升到第四职业了吗？"))
                {
                    if (canHold(2280003, 1))
                    {
                        changeJobById(getJobId() + 1);
                        if (getJob() == Job.HERO)
                        {
                            teachSkill(1121001, 0, 10, -1);
                            teachSkill(1120004, 0, 10, -1);
                            teachSkill(1121008, 0, 10, -1);
                        }
                        else if (getJob() == Job.PALADIN)
                        {
                            teachSkill(1221001, 0, 10, -1);
                            teachSkill(1220005, 0, 10, -1);
                            teachSkill(1221009, 0, 10, -1);
                        }
                        else if (getJob() == Job.DARKKNIGHT)
                        {
                            teachSkill(1321001, 0, 10, -1);
                            teachSkill(1320005, 0, 10, -1);
                            teachSkill(1320005, 0, 10, -1);
                        }
                        gainItem(2280003, 1);
                    }
                    else
                    {
                        await SayOK("请在#b消耗栏#k留出一个空位，以便接收技能书。");
                    }
                }
            }
            else
            {
                await AskMenu("如果必要的话，我可以教你你职业的技能。\r\n#b#L0#教我我的职业技能。#l");
                if (getJob() == Job.HERO)
                {
                    if (getPlayer().getSkillLevel(Hero.ENRAGE) == 0)
                    {
                        teachSkill(Hero.ENRAGE, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(Hero.GUARDIAN) == 0)
                    {
                        teachSkill(Hero.GUARDIAN, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(Hero.STANCE) == 0)
                    {
                        teachSkill(Hero.STANCE, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.PALADIN)
                {
                    if (getPlayer().getSkillLevel(1221002) == 0)
                    {
                        teachSkill(1221002, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(1221003) == 0)
                    {
                        teachSkill(1221003, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(1221004) == 0)
                    {
                        teachSkill(1221004, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.DARKKNIGHT)
                {
                    if (getPlayer().getSkillLevel(1321002) == 0)
                    {
                        teachSkill(1321002, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(1321008) == 0)
                    {
                        teachSkill(1321008, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(1321009) == 0)
                    {
                        teachSkill(1321009, 0, 10, -1);
                    }
                }
                await SayOK("好了");
            }
        }


    }
}
