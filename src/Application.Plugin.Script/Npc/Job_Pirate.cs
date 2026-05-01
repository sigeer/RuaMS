using Application.Core.Game.Maps.Mists;
using Application.Core.scripting.Infrastructure;
using Application.Shared.Constants.Job;
using Application.Shared.Quest;
using static Application.Templates.Quest.QuestAct;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 10204 
        public async Task infoPirate()
        {
            await SayNext("海盗拥有出色的灵巧和力量，他们利用枪支进行远程攻击，同时在近战战斗中利用自己的力量。枪手使用基于元素的子弹来增加伤害，而搏击者则可以变身为不同的形态以达到最大效果。");
            if (await AskYesNo("你想体验一下成为一个海盗是什么感觉吗？"))
            {
                lockUI();
                warp(1020500, 0);
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
        public async Task inside_pirate()
        {
            int currentMapId = getPlayer().getMapId();

            if (currentMapId == 108000502 || currentMapId == 108000501)
            {
                var item = currentMapId == 108000502 ? 4031856 : 4031857;
                if (!haveItem(item, 15))
                {
                    var option = await AskMenu($"你还没有给我带来15个#b#t{item}##k。我期待你的进展，伙计！\r\n#b#L1#我想离开#l");
                    removeAll(item);
                    warp(120000101, 0);
                }
                else
                {
                    await SayNext($"哇，你给我带来了15个#b#t{item}##k！恭喜你。让我现在把你传送出去。");
                    warp(120000101, 0);
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
                await SayNext("想成为一个#r海盗#k吗？有一些标准需要满足，因为我们不能接受每个人... #b你的等级至少应该是10级，具有" + getFirstJobStatRequirement(5) + "。让我们看看。");
                if (getLevel() >= 10 && canGetFirstJob(5))
                {
                    if (await AskYesNo("哦...！你看起来就像是我们团队的一员... 你只需要一点邪恶的心思，然后... 是的... 那么，你觉得怎么样？想成为海盗吗？"))
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
                return;
            }
            else if (getLevel() >= 30 && getJob() == Job.PIRATE)
            {
                if (isQuestCompleted(2191) || isQuestCompleted(2192))
                {
                    await SayNext("我看到你做得很好。我会允许你迈出漫长道路上的下一步。");
                    var jobList = new Job[] { Job.BRAWLER, Job.GUNSLINGER };
                    var option = await AskMenu("好的，当你做出决定后，点击底部的[我会选择我的职业]。", jobList.Select(j => $"请介绍一下 {c.CurrentCulture.GetJobName(j)}").Concat(["我要选择职业！"]));
                    switch (option)
                    {
                        case 0:
                            await SayNext("掌握#r指节#k的海盗。\r\n\r\n#b拳手#k是近战、近距离拳击手，造成大量伤害并拥有高生命值。携带#r螺旋冲击#k，可以一次对多个目标造成巨大伤害。#r橡木桶#k使人能够在艰难战斗中进行侦察或伪装，为面对危险时提供可能的逃生路线。");
                            break;
                        case 1:
                            await SayNext("掌握#r枪械#k的海盗。\r\n\r\n#b枪手#k更快速且远程攻击者。通过#r翅膀#k技能，枪手可以在空中盘旋，比普通跳跃更长、更持久。#r空白射击#k可以使附近多个目标陷入眩晕状态。");
                            break;
                        case 2:
                            var select = await AskMenu("现在... 你决定好了吗？请选择你想要在二转时选择的职业。", jobList.Select(j => $"{c.CurrentCulture.GetJobName(j)}"));
                            var job = jobList[select];
                            var jobStr = c.CurrentCulture.GetJobName(job);
                            if (await AskYesNo("所以你第二次转职想要选择成为#b" + jobStr + "#k吗？你知道一旦在这里做出选择，就无法在改变了，对吧？"))
                            {
                                var requiredQuest = job == Job.BRAWLER ? 2191 : 2192;
                                if (!isQuestCompleted(requiredQuest))
                                {
                                    await SayOK($"你需要先完成任务 《{c.CurrentCulture.GetQuestName(requiredQuest)}》");
                                    return;
                                }

                                if (getJob() == Job.PIRATE)
                                {
                                    changeJobById(job.Id);

                                    await SaySpeech([
                                        job == Job.BRAWLER
                                        ? "从现在开始，你是一个#b拳手#k。拳手用他们的拳头统治世界……这意味着他们需要比其他人更多地锻炼身体。如果你在训练中遇到任何困难，我会很乐意帮助你。"
                                        : "从现在开始，你是一名#b枪手#k。枪手以其类似狙击手的远程攻击和使用枪支作为主要武器而闻名。你应该继续训练，真正掌握你的技能。如果你在训练中遇到困难，我会在这里帮助你。",
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
                    await SayNext("你取得的进步令人惊讶。");
                    await SayNext("做得好。你看起来很强壮，但我需要看看你是否真的足够强大来通过测试，这不是一个困难的测试，所以你会做得很好。");
                    if (await AskYesNo("你现在想要参加测试吗？"))
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
                            await SayOK("你还没有接到转职任务！");
                            return;
                        }

                        var em = GetSoloQuestEventManager(jobQuest);
                        var r = em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
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
            else if (isQuestStarted(6330) || isQuestStarted(6370))
            {
                var questId = isQuestStarted(6330) ? 6330 : 6370;
                var qProgress = getQuestProgressInt(questId, questId + 1);
                if (qProgress == 0)
                {
                    await SayNext("你准备好了吗？现在试着忍受我的攻击2分钟。我不会手下留情的。祝你好运，因为你会需要的。");
                    var em = GetSoloQuestEventManager(questId);
                    var r =  em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else if (getEventInstance() != null)
                {
                    await SayNext("做的不错。我们到外面讨论一下吧！");
                    WarpOut();
                }
                return;
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
        public async Task pirate4()
        {
            if (getLevel() < 120 || !getJob().IsSameJobGroup(Job.PIRATE))
            {
                await SayOK("请不要现在打扰我，我正在集中精力。");
                return;
            }
            
            if (!isQuestCompleted(6944))
            {
                await SayOK("你还没有通过我的考验。在你通过考验之前，我无法提升你的等级。");
                return;
            }
            
            if (getJob().Rank == 3)
            {
                if (await AskYesNo("你通过了我的测试，做得非常出色。你准备好晋升到第四职业了吗？"))
                {
                    if (canHold(2280003, 1))
                    {
                        int jobId = getJob().Id;
                        changeJobById(jobId + 1);
                        
                        jobId = getJob().Id;
                        if (jobId == 512)
                        {
                            teachSkill(5121001, 0, 10, -1);
                            teachSkill(5121002, 0, 10, -1);
                            teachSkill(5121007, 0, 10, -1);
                            teachSkill(5121009, 0, 10, -1);
                        }
                        else if (jobId == 522)
                        {
                            teachSkill(5220001, 0, 10, -1);
                            teachSkill(5220002, 0, 10, -1);
                            teachSkill(5221004, 0, 10, -1);
                            teachSkill(5220011, 0, 10, -1);
                        }
                        
                        gainItem(2280003, 1);
                    }
                    else
                    {
                        await SayOK("请在#b使用#k的物品栏中留出一个空位，以便接收技能书。");
                    }
                }
            }
            else
            {
                var option = await AskMenu("如果必要的话，我可以教你你的职业技能。\r\n#b#L0#教我我的职业技能。#l");
                if (option == 0)
                {
                    int jobId = getJob().Id;
                    if (jobId == 512)
                    {
                        if (getPlayer().getSkillLevel(5121003) == 0)
                        {
                            teachSkill(5121003, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5121004) == 0)
                        {
                            teachSkill(5121004, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5121005) == 0)
                        {
                            teachSkill(5121005, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5121010) == 0)
                        {
                            teachSkill(5121010, 0, 10, -1);
                        }
                    }
                    else if (jobId == 522)
                    {
                        if (getPlayer().getSkillLevel(5221006) == 0)
                        {
                            teachSkill(5221006, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5221007) == 0)
                        {
                            teachSkill(5221007, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5221008) == 0)
                        {
                            teachSkill(5221008, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5221009) == 0)
                        {
                            teachSkill(5221009, 0, 10, -1);
                        }
                        if (getPlayer().getSkillLevel(5221003) == 0)
                        {
                            teachSkill(5221003, 0, 10, -1);
                        }
                    }
                    
                    await SayOK("事情已经完成。现在离开我。");
                }
            }
        }
    }
}
