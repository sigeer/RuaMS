using Application.Shared.Constants.Job;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {

        // Npc: 10201 
        public async Task infoMagician()
        {
            await SayNext("法师装备着华丽的基于元素的法术和辅助整个队伍的次要魔法。在二转职业之后，基于元素的魔法将对相克元素的敌人造成大量伤害。");
            if (await SayYesNo("你想体验一下成为一个魔法师是什么感觉吗？"))
            {
                lockUI();
                warp(1020300, 0);
                dispose();
            }
            else
            {
                await SayNext("如果你想体验成为一个魔法师的感觉，再来找我吧。");
            }
        }

        // Npc: 1032001 
        public async Task magician()
        {
            // TODO
            if (getJob() == Job.BEGINNER)
            {
                await SayNext("想成为一个#r魔法师#k吗？有一些标准需要满足，因为我们不能接受每个人... #b你的等级至少应该是8#k，至少有" + getFirstJobStatRequirement(2) + "。让我们看看。");
                if (getLevel() >= 8 && canGetFirstJob(2))
                {
                    if (await SayYesNo("哦...！你看起来就像是我们团队的一员... 你只需要一点邪恶的心思，然后... 是的... 那么，你觉得怎么样？想成为魔法师吗？"))
                    {
                        if (canHold(1372043) && canHold(2070000))
                        {
                            if (getJob() == Job.BEGINNER)
                            {
                                changeJobById(JobId.MAGICIAN);
                                gainItem(1372043, 1);
                                resetStats();

                                await SaySpeech([
                                "好的，从现在开始，你就是我们的一员了！你将在...过着流浪者的生活，但要耐心等待，很快你就会过上好日子。好了，虽然不多，但我会传授给你一些我的能力... 哈啊啊啊！！",
                                "你现在变得更强大了。而且你的所有物品栏都增加了槽位。确切地说，是一整行。去亲自看看吧。我刚给了你一点点#bSP#k。当你在屏幕左下角打开#b技能#k菜单时，可以使用SP学习技能。不过要注意一点：你不能一次性全部提升。在学习了一些技能之后，你还可以获得一些特定的技能。",
                                "但是要记住，技能并不是一切。作为一个魔法师，你的属性应该支持你的技能。魔法师主要使用智力作为他们的主属性，幸运作为次要属性。如果提升属性很困难，就使用#b自动分配#k。",
                                "现在，我要再警告你一句。如果你从现在开始在战斗中失败，你将失去一部分总经验值。要特别注意这一点，因为你的生命值比大多数人都要少。",
                                "这是我能教你的全部了。祝你在旅途中好运，年轻的魔法师。"
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
            }
            else if (getLevel() >= 30 && getJob() == Job.MAGICIAN)
            {
                if (haveItem(4031012))
                {
                    var jobList = new Job[] { Job.FP_WIZARD, Job.IL_WIZARD, Job.CLERIC };
                    await SayNext("哈哈...我知道你会轻松通过那个测试的。我承认，你是一个很棒的魔法师。我会让你比现在强大得多。不过，在那之前...你需要选择给你的两条路中的一条。这对你来说会是一个艰难的决定，但是...如果有任何问题需要问，请尽管问吧。");
                    var option = await SayOption("好的，当你做出决定后，点击底部的[我会选择我的职业]。", jobList.Select(j => $"请介绍一下 {c.CurrentCulture.GetJobName(j)}").Concat(["我要选择职业！"]));
                    switch (option)
                    {
                        case 0:
                            await SayNext("掌握#r火/毒系魔法#k的魔法师。\r\n\r\n#b火/毒法师#k是一个活跃的职业，能够造成魔法元素伤害。这些技能使他们在面对对其元素弱点的敌人时具有显著优势。通过他们的技能#r冥想#k和#r减速#k，#b火/毒法师#k可以增加他们的魔法攻击并减少对手的移动能力。#b火/毒法师#k拥有强大的火焰箭攻击和毒素攻击。");
                            break;
                        case 1:
                            await SayNext("掌握#r冰/雷系魔法#k的魔法师。\r\n\r\n#b冰/雷法师#k是一个活跃的职业，能够造成魔法元素伤害。这些技能使他们在面对对其元素弱点的敌人时具有显著优势。通过他们的技能#r冥想#k和#r减速#k，#b冰/雷法师#k可以增加他们的魔法攻击力并减少对手的移动能力。#b冰/雷法师#k拥有冰冻攻击和闪电攻击。");
                            break;
                        case 2:
                            await SayNext("掌握#r神圣魔法#k的魔法师。\r\n\r\n#b牧师#k是一个强大的支持职业，必定会被任何队伍接受。这是因为他们有能力#r治疗#k自己和队伍中的其他成员。使用#r祝福#k，#b牧师#k可以增强属性并减少所受的伤害。如果你发现生存困难，这个职业是值得一试的。#b牧师#k对不死怪物尤其有效。");
                            break;
                        case 3:
                            var select = await SayOption("现在... 你决定好了吗？请选择你想要在二转时选择的职业。", jobList.Select(j => $"{c.CurrentCulture.GetJobName(j)}"));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await SayYesNo("所以你第二次转职想要选择成为#b" + jobStr + "#k吗？你知道一旦在这里做出选择，就无法在改变了，对吧？"))
                            {
                                if (getJob() == Job.MAGICIAN)
                                {
                                    gainItem(4031012, -1);
                                    completeQuest(100008);
                                    changeJobById(job.Id);

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
                    if (!isQuestStarted(100006))
                    {
                        startQuest(100006);
                    }

                    if (await SayYesNo("请将这封信交给#b#p1072001##k，他在魔法密林附近的#b#m101020000##k。他正在代替我担任教练的工作。把信交给他，他会代替我测试你。祝你好运。"))
                    {
                        if (!haveItem(4031009))
                        {
                            if (canHold(4031009))
                                gainItem(4031009, 1);
                            else
                            {
                                await SayNext("请在你的背包中腾出一些空间。");
                            }
                        }

                    }
                }
            }
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.MAGICIAN))
            {
                if (isQuestStarted(-23001))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext("看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020009##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else if (haveItem(4031059))
                    {
                        gainItem(4031059, -1);
                        gainItem(4031057, 1);

                        await SayNext("你成功击败了我的分身并带回了#b#t4031059##k！看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020009##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else
                    {
                        await SayNext("我一直在等你！几天前，我从#b#p2020009##k听到了你的消息，你现在可以变得更强，但是你得通过我的考验。在巫婆森林深处有一个异界之门，里面有我的分身，你去击败它并带回#b#t4031059##k。");
                        await SayNext("我的分身相当强大，他会使用特殊技能，你要跟他一对一战斗，击败他并带回#b#t4031059##k，祝你好运！");
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
        public async Task change_magician()
        {
            if (isQuestCompleted(100007))
            {
                await SayOK("你真是一个真正的英雄！");
                return;
            }
            else if (isQuestCompleted(100006))
            {
                await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你已经通过了测试。祝你好运。");
                warp(108000200, 0);
            }
            else if (isQuestStarted(100006))
            {
                await SaySpeech([
                    "嗯...这绝对是#b#t4031009##k的来信...所以你来到这里是为了接受测试，进行魔法师的第二次职业转职。好吧，我来给你解释一下测试。不要太担心，它并不是那么复杂。",
                    "我会把你送到一个隐藏的地图。你会看到一些平常不会见到的怪物。它们看起来和普通的怪物一样，但态度完全不同。它们既不会提升你的经验等级，也不会给你提供物品。",
                    "你将能够在打倒这些怪物时获得一种名为#b#t4031013##k的大理石。这是一种由它们邪恶的心灵制成的特殊大理石。收集30个，然后去找我的一个同事谈谈。这就是你通过考验的方法。",
                    ]);
                if (await SayYesNo("一旦你进去，就不能离开，直到完成你的任务。如果你死了，你的经验等级会下降...所以你最好做好准备...那么，你现在想去吗？"))
                {
                    await SayNext("好的，我会让你进去！打败里面的怪物，收集30个#t4031013#，然后和我里面的一位同事交谈。他会给你#b#t4031012##k，证明你已经通过了测试。祝你好运。");
                    warp(108000200, 0);
                    completeQuest(100006);
                    startQuest(100007);
                    gainItem(4031008, -1);
                }
            }
            else
            {
                await SayOK("一旦你准备好了，我可以告诉你路线。");
            }
        }

        // Npc: 1072005 
        public async Task inside_magician()
        {
            if (haveItem(4031013, 30))
            {
                await SayOK("哦哦哦.. 你收集了所有30个#t4031013#！！这应该很困难.. 简直不可思议！好吧。你通过了测试，为此，我会奖励你 #b#t4031012##k。拿着它回到艾琳尼亚去吧。");
                removeAll(4031013);
                completeQuest(100007);
                startQuest(100008);
                gainItem(4031012);
            }
            else
            {
                await SayOption("你需要收集 #b30 个 #t4031013##k。祝你好运。", ["我想离开"]);
            }
            warp(101020000, 9);
        }

        // Npc: 2020009 
        public Task wizard3()
        {
            return Job3((int)JobStyle.MAGICIAN, 
                "魔法密林", 1032001);
        }


        // Npc: 2081200 
        public async Task magician4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.MAGICIAN))
            {
                await SayOK("请不要现在打扰我，我正在集中精力。");
                return;
            }
            else if (!isQuestCompleted(6914))
            {
                await SayOK("你还没有通过我的考验。在你通过考验之前，我无法提升你的等级。");
                return;
            }
            else if (getJob().Rank == 3)
            {
                if (await SayYesNo("你通过了我的测试，做得非常出色。你准备好晋升到第四职业了吗？"))
                {
                    if (canHold(2280003, 1))
                    {
                        changeJobById(getJobId() + 1);
                        if (getJob() == Job.FP_ARCHMAGE)
                        {
                            teachSkill(2121001, 0, 10, -1);
                            teachSkill(2121002, 0, 10, -1);
                            teachSkill(2121006, 0, 10, -1);
                        }
                        else if (getJob() == Job.IL_ARCHMAGE)
                        {
                            teachSkill(2221001, 0, 10, -1);
                            teachSkill(2221002, 0, 10, -1);
                            teachSkill(2221006, 0, 10, -1);
                        }
                        else if (getJob() == Job.BISHOP)
                        {
                            teachSkill(2321001, 0, 10, -1);
                            teachSkill(2321002, 0, 10, -1);
                            teachSkill(2321006, 0, 10, -1);
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
                await SayOption("如果必要的话，我可以教你你职业的技能。\r\n#b#L0#教我我的职业技能。#l");
                if (getJob() == Job.FP_ARCHMAGE)
                {
                    if (getPlayer().getSkillLevel(2121007) == 0)
                    {
                        teachSkill(2121007, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(2121005) == 0)
                    {
                        teachSkill(2121005, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(2121003) == 0)
                    {
                        teachSkill(2121003, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.IL_ARCHMAGE)
                {
                    if (getPlayer().getSkillLevel(2221007) == 0)
                    {
                        teachSkill(2221007, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(2221005) == 0)
                    {
                        teachSkill(2221005, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(2221003) == 0)
                    {
                        teachSkill(2221003, 0, 10, -1);
                    }
                }
                else if (getJob() == Job.BISHOP)
                {
                    if (getPlayer().getSkillLevel(2321008) == 0)
                    {
                        teachSkill(2321008, 0, 10, -1);
                    }
                    if (getPlayer().getSkillLevel(2321006) == 0)
                    {
                        teachSkill(2321006, 0, 10, -1);
                    }
                }
                await SayOK("好了");
            }
        }

    }
}
