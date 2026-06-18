using Application.Shared.Constants.Job;
using Application.Shared.Quest;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {

        // Npc: 10203 
        public async Task infoRogue()
        {
            await SayNext("飞侠是幸运、灵巧和力量的完美结合，擅长对无助的敌人进行突袭攻击。高水平的闪避能力和速度使得飞侠能够从各个角度攻击敌人。");
            if (await AskYesNo("想体验一下飞侠是什么感觉嘛？"))
            {
                await lockUI();
                await warp(1020400, 0);
            }
            else
            {
                await SayNext("如果你想体验成为一名飞侠的感觉，再来找我吧。");
            }
        }

        // Npc: 1052001 
        public async Task rogue()
        {
            if (getJob() == Job.BEGINNER)
            {
                await SayNext("想成为一个#r飞侠#k吗？有一些标准要达到，毕竟我们不是谁都可以接纳的……#b所以你的等级至少10级，至少" + getFirstJobStatRequirement(4) + "。让我们看看。");
                if (getLevel() >= 10 && canGetFirstJob(4))
                {
                    if (await AskYesNo("哦...！你看起来就很鸡贼，确实像是我们团队的一员... 你需要再多一点邪恶的心思，你觉得怎么样？想成为飞侠吗？ "))
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
                                "好的，从现在开始，你就是我们的一员了！你将在...过着流浪者的生活，但要耐心等待，很快你就会过上好日子。好了，虽然不多，但我会传授给你一些我的能力... 哈啊啊啊！！",
                                "你现在变得更强大了。而且你的所有物品栏都增加了槽位。确切地说，是一整行。去亲自看看吧。我刚给了你一点点#bSP#k。当你在屏幕左下角打开#b技能#k菜单时，可以使用SP学习技能。不过要注意一点：你不能一次性全部提升。在学习了一些技能之后，你还可以获得一些特定的技能。",
                                "但是要记住，技能并不是一切。作为一个魔法师，你的属性应该支持你的技能。魔法师主要使用智力作为他们的主属性，幸运作为次要属性。如果提升属性很困难，就使用#b自动分配#k。",
                                "现在提醒一下。一旦你做出选择，就不能改变主意，试图选择另一条道路。现在去吧，做一个骄傲的飞侠。"
                                ]);
                            }

                        }
                        else
                        {
                            await SayNext("清理一下你的背包，然后回来找我说话。");
                        }
                    }

                }
                else
                {
                    await SayOK("再多训练一会儿，直到你达到基本要求，我就可以教你成为一名#r魔法师#k的方法。");
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
                    var option = await AskMenu("好的，当你做出决定后，点击底部的[我会选择我的职业]。", jobList.Select(j => $"请介绍一下 {c.CurrentCulture.GetJobName(j)}").Concat(["我要选择职业！"]));
                    switch (option)
                    {
                        case 0:
                            await SayNext("擅长使用#r拳套#k的刺客。\r\n\r\n#b刺客#k是远程攻击者。他们非常节约金币，并且有很高的伤害潜力，但是比飞侠花费更多。");
                            break;
                        case 1:
                            await SayNext("擅长使用匕首的侠客。\r\n\r\n#b侠客#k是快速的近战攻击者，在二转职业中非常强大。他们不像刺客那样效率高，也没有远程攻击的优势，但在原始力量方面弥补了这一点。");
                            break;
                        case 2:
                            var select = await AskMenu("现在... 你决定好了吗？请选择你想要在二转时选择的职业。", jobList.Select(j => $"{c.CurrentCulture.GetJobName(j)}"));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo("所以你第二次转职想要选择成为#b" + jobStr + "#k吗？你知道一旦在这里做出选择，就无法在改变了，对吧？"))
                            {
                                if (getJob() == Job.THIEF)
                                {
                                    await gainItem(4031012, -1);
                                    await completeQuest(questId);
                                    await changeJobById(job.Id);

                                    await SaySpeech([
                                    $"好了，从现在开始你就是{jobStr}了。{jobStr}是聪明的一群人，拥有令人难以置信的魔法能力，能够轻松地穿透怪物的心灵和心理结构……请每天都训练自己，我会帮助你变得比你现在更强大。",
                                    $"我刚刚给了你一本书，上面列出了你作为一个{jobStr}可以获得的技能清单。此外，你的杂项物品栏也扩展了一行。你的最大生命值和魔法值也增加了。去检查一下，看看吧。",
                                    "我也给了你一点 #bSP#k。打开左下角的 #b技能菜单#k。你可以提升新获得的二级技能。不过要注意，你不能一次性提升它们。有些技能只有在学会其他技能后才能使用。记得要记住这一点。",
                                    jobStr + " 你需要坚强。但若将自身的力量发泄在弱者身上，这并不是正确的方法。将自己所拥有的力量用在正确的事情上，这是比变得更强更为重要的课题。好了！相信你不断自我修炼，过不久就会再与我相见的，我期待那天的到来。"
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
                    await SayNext("做得好。你看起来很强壮，但我需要看看你是否真的足够强大来通过测试，这不是一个困难的测试，所以你会做得很好。拿着我的信先……确保你不要丢了它！");
                    if (!isQuestStarted(questId))
                    {
                        await startQuest(questId);
                    }

                    if (await AskYesNo("请把这封信送到废弃都市附近的#b#p1072003##k。他正在代替我担任教练的工作。把信交给他，他会代替我来测试你。祝你好运。"))
                    {
                        if (!haveItem(4031011))
                        {
                            if (canHold(4031011))
                                await gainItem(4031011, 1);
                            else
                            {
                                await SayNext("请在你的背包中腾出一些空间。");
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
                        await SayNext("看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020011##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else if (haveItem(4031059))
                    {
                        await gainItem(4031059, -1);
                        await gainItem(4031057, 1);

                        await SayNext("你成功击败了我的分身并带回了#b#t4031059##k！看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020011##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else
                    {
                        await SayNext("我一直在等你！几天前，我从#b#p2020011##k听到了你的消息，你现在可以变得更强，但是你得通过我的考验。在猴子沼泽深处有一个异界之门，里面有我的分身，你去击败它并带回#b#t4031059##k。");
                        await SayNext("我的分身相当强大，他会使用特殊技能，你要跟他一对一战斗，击败他并带回#b#t4031059##k，祝你好运！");
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
                "废弃都市", 1052001);
        }


        // Npc: 2081400 
        public async Task thief4()
        {
            if (getLevel() < 120 || getJob().IsSameJobGroup(Job.THIEF))
            {
                await SayOK("请不要现在打扰我，我正在集中精力。");
                return;
            }
            else if (!isQuestCompleted(6934))
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
                        await SayOK("请在#b使用#k的物品栏中留出一个空位，以便接收技能书。");
                    }
                }
            }
            else
            {
                await AskMenu("如果必要的话，我可以教你你职业的技能。\r\n#b#L0#教我我的职业技能。#l");
                if (getJob() == Job.NIGHTLORD)
                {
                    if (getPlayer().getSkillLevel(4121008) == 0)
                    {
                        await teachSkill(4121008, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(4121004) == 0)
                    {
                        await teachSkill(4121004, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.SHADOWER)
                {
                    if (getPlayer().getSkillLevel(4221004) == 0)
                    {
                        await teachSkill(4221004, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(4221001) == 0)
                    {
                        await teachSkill(4221001, 0, 10, -1);
                    }
                }
                await SayOK("好了");
            }
        }
    }
}
