using Application.Shared.Constants.Job;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 10204 
        public async Task infoPirate()
        {
            await SayNext("海盗拥有出色的灵巧和力量，他们利用枪支进行远程攻击，同时在近战战斗中利用自己的力量。枪手使用基于元素的子弹来增加伤害，而搏击者则可以变身为不同的形态以达到最大效果。");
            if (await SayYesNo("你想体验一下成为一个海盗是什么感觉吗？"))
            {
                lockUI();
                warp(1020500, 0);
                dispose();
            }
            else
            {
                await SayNext("如果你想体验成为一个海盗的感觉，再来找我吧。");
            }
        }

        // Npc: 1095002 
        public Task enter_pirate()
        {
            return EnterTranningMap(912030000);
        }
        // Npc: 1072008 
        public Task inside_pirate()
        {
            // TODO
            return Task.CompletedTask;
        }

        // Npc: 1090000 
        public async Task kairinT()
        {
            // TODO
            if (getJob() == Job.BEGINNER)
            {
                await SayNext("想成为一个#r海盗#k吗？有一些标准需要满足，因为我们不能接受每个人... #b你的等级至少应该是10级，具有" + getFirstJobStatRequirement(2) + "。让我们看看。");
                if (getLevel() >= 10 && canGetFirstJob(2))
                {
                    if (await SayYesNo("哦...！你看起来就像是我们团队的一员... 你只需要一点邪恶的心思，然后... 是的... 那么，你觉得怎么样？想成为海盗吗？"))
                    {
                        if (canHold(2330000) && canHoldAll([1482000, 1492000]))
                        {
                            if (getJob() == Job.BEGINNER)
                            {
                                changeJobById(JobId.PIRATE);
                                gainItem(1482000, 1);
                                gainItem(1492000, 1);
                                resetStats();

                                await SaySpeech([
                                "好的，从现在开始，你就是我们的一员了！你将在...过着流浪者的生活，但要耐心等待，很快你就会过上好日子。好了，虽然不多，但我会传授给你一些我的能力... 哈啊啊啊！！",
                                "你现在变得更强大了。而且你的所有物品栏都增加了槽位。确切地说，是一整行。去亲自看看吧。我刚给了你一点点#bSP#k。当你在屏幕左下角打开#b技能#k菜单时，可以使用SP学习技能。不过要注意一点：你不能一次性全部提升。在学习了一些技能之后，你还可以获得一些特定的技能。",
                                "现在提醒一下。一旦你做出选择，就不能改变主意，试图选择另一条道路。现在去吧，做一个骄傲的海盗。"
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
                    await SayOK("再多训练一会儿，直到你达到基本要求，我就可以教你成为一名#r海盗#k的方法。");
                    return;
                }
            }
            else if (getLevel() >= 30 && getJob() == Job.PIRATE)
            {
                if (haveItem(4031012))
                {
                    var jobList = new Job[] { Job.BRAWLER, Job.GUNSLINGER };
                    await SayNext("哈哈...我知道你会轻松通过那个测试的。不过，在那之前...你需要选择给你的两条路中的一条。这对你来说会是一个艰难的决定，但是...如果有任何问题需要问，请尽管问吧。");
                    var option = await SayOption("好的，当你做出决定后，点击底部的[我会选择我的职业]。", jobList.Select(j => $"请介绍一下 {c.CurrentCulture.GetJobName(j)}").Concat(["我要选择职业！"]));
                    switch (option)
                    {
                        case 0:
                            await SayNext("掌握#r指节#k的海盗。\r\n\r\n#b格斗家#k是近战、近距离拳击手，造成大量伤害并拥有高生命值。携带#r螺旋冲击#k，可以一次对多个目标造成巨大伤害。#r橡木桶#k使人能够在艰难战斗中进行侦察或伪装，为面对危险时提供可能的逃生路线。");
                            break;
                        case 1:
                            await SayNext("掌握#r枪械#k的海盗。\r\n\r\n#b枪手#k更快速且远程攻击者。通过#r翅膀#k技能，枪手可以在空中盘旋，比普通跳跃更长、更持久。#r空白射击#k可以使附近多个目标陷入眩晕状态。");
                            break;
                        case 2:
                            var select = await SayOption("现在... 你决定好了吗？请选择你想要在二转时选择的职业。", jobList.Select(j => $"{c.CurrentCulture.GetJobName(j)}"));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await SayYesNo("所以你第二次转职想要选择成为#b" + jobStr + "#k吗？你知道一旦在这里做出选择，就无法在改变了，对吧？"))
                            {
                                if (getJob() == Job.PIRATE)
                                {
                                    gainItem(4031012, -1);
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
            else if (getLevel() >= 70 && getJob().Rank == 2 && getJob().IsSameJobGroup(Job.PIRATE))
            {
                if (isQuestStarted(-53001))
                {
                    if (haveItem(4031057))
                    {
                        await SayNext("看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020013##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else if (haveItem(4031059))
                    {
                        gainItem(4031059, -1);
                        gainItem(4031057, 1);

                        await SayNext("你成功击败了我的分身并带回了#b#t4031059##k！看来你已经准备好第三次转职了，把#b#t4031057##k带给#b#p2020013##k，他会帮助你进行第三次转职的，祝你好运！");
                    }
                    else
                    {
                        await SayNext("我一直在等你！几天前，我从#b#p2020013##k听到了你的消息，你现在可以变得更强，但是你得通过我的考验。在火独眼兽洞穴深处有一个异界之门，里面有我的分身，你去击败它并带回#b#t4031059##k。");
                        await SayNext("我的分身相当强大，他会使用特殊技能，你要跟他一对一战斗，击败他并带回#b#t4031059##k，祝你好运！");
                    }
                    return;
                }
            }
            await SayOK(GetDefault0());
        }

        // Npc: 2020013 
        public Task pirate3()
        {
            return Job3((int)JobStyle.PIRATE,
                "诺特勒斯号", 1090000);
        }


        // Npc: 2081500 
        public Task pirate4()
        {
            // TODO
            return Task.CompletedTask;
        }
    }
}
