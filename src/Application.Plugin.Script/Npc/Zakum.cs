using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Shared.Items;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2030008
        public async Task Zakum00()
        {
            if (haveItem(4001109, 1))
            {
                warp(921100000, "out00");
                return;
            }

            var em = GetEventManager(nameof(PQ_Zakum));
            if (em == null)
            {
                await SayOK("扎昆组队任务遇到了一个错误。");
                return;
            }

            if (!(isQuestStarted(100200) || isQuestCompleted(100200)))
            {
                if (getLevel() >= em.MinLevel)
                {
                    await SayOK("小心，古老的力量并未被遗忘……如果你希望有朝一日击败#r扎昆#k，首先要获得#b长老会#k的批准，然后#b面对考验#k，只有这样你才有资格进行战斗。");
                }
                else
                {
                    await SayOK("小心，古老的力量并未被遗忘……");
                }
                return;
            }

            var selection = await AskMenu("#e#b<组队任务：扎昆组队任务>\r\n#k#n" + em.Template.GetRequirementDescription(c) + "\r\n\r\n小心，古老的力量并未被遗忘... #b\r\n#L0#进入未知的死亡矿井（第1阶段）#l\r\n#L1#面对熔岩之息（第2阶段）#l\r\n#L2#锻造#t4001017#（第3阶段）#l");

            if (selection == 0)
            {
                await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
            }
            else if (selection == 1)
            {
                if (haveItem(4031061) && !haveItem(4031062))
                {
                    if (await AskYesNo("你已经成功通过了第一阶段。你还有很长的路才能到达扎昆的祭台。所以，你想好挑战下一个阶段了吗？"))
                    {
                        warp(280020000, 0);
                    }
                }
                else
                {
                    if (haveItem(4031062))
                    {
                        await SayOK("你已经得到了#b熔岩之息#k，你不需要完成这个阶段。");
                    }
                    else
                    {
                        await SayOK("请先完成之前的试炼。");
                    }
                }
            }
            else
            {
                if (haveItem(4031061) && haveItem(4031062))
                {
                    if (!haveItem(4000082, 30))
                    {
                        await SayOK("你已经完成了试炼，但是还需要 #b#v4000082##t4000082# * 30#k 来锻造 #b5 个 #v4001017##t4001017##k。");
                    }
                    else
                    {
                        completeQuest(100201);
                        gainItem(4031061, -1);
                        gainItem(4031062, -1);
                        gainItem(4000082, -30);
                        gainItem(4001017, 5);
                        await SayOK("你 #r已经完成了试炼#k，从现在开始我批准你挑战扎昆。");
                    }
                }
                else
                {
                    await SayOK("你缺少一些必要的物品\r\n#b#v4031061##t4031061# * 1#k\r\n#b#v4031062##t4031062# * 1#k\r\n来锻造#b#v4001017##t4001017##k。");
                }
            }
        }


        // Npc: 2030010
        public async Task Zakum06()
        {
            if (getMapId() == 280030000)
            {
                if (!GetEventInstanceTrust().isEventCleared())
                {
                    if (await AskYesNo("如果你现在离开，你将不得不重新开始。你确定要离开吗？"))
                    {
                        warp(211042300);
                    }
                }
                else
                {
                    if (await AskYesNo("你们终于打败了扎昆，真是了不起的壮举！恭喜！你确定现在要离开吗？"))
                    {
                        warp(211042300);
                    }
                }
            }
            else
            {
                if (await AskYesNo("如果你现在离开，你将不得不重新开始。你确定要离开吗？"))
                {
                    warp(211042300);
                }
            }
        }


        // Npc: 2030011
        public async Task Zakum04()
        {
            await SayOK("下次见。");
            warp(211042300);
            removeAll(4001015);
            removeAll(4001016);
            removeAll(4001018);
        }


        // Npc: 2030013
        public async Task zakum_accept()
        {
            var expedItem = 4001017;
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(nameof(Battle_Zakum));
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);


            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK("您不符合与" + expedBoss + "战斗的条件！");
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
            if (expedition == null)
            {
                var selection = await AskMenu("#e#b<远征：" + expedBoss + ">\r\n#k#n" + em.Template.GetRequirementDescription(c) + "\r\n\r\n你想组建一个远征队来挑战 #r" + expedBoss + "#k 吗？\r\n#b#L1#让我们开始吧！#l\r\n#L2#不，我想再等一会儿...#l");

                if (selection == 1)
                {
                    if (!haveItem(expedItem))
                    {
                        await SayOK("作为远征队领袖，你必须携带#b#t" + expedItem + "##k在你的物品栏中，才能与" + expedBoss + "进行战斗！");
                        return;
                    }

                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK("有人已经主动成为了远征队的领袖。试着加入他们吧！");
                        return;
                    }

                    var r = em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK("当然，并非每个人都能挑战" + expedBoss + "。");
                }
            }
            else if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK("你的远征已经在进行中，对于那些仍在战斗中的人，让我们为那些勇敢的灵魂祈祷吧。");
                }
                else
                {
                    var list = "你想做什么？#b\r\n\r\n#L1#查看当前远征队成员#l\r\n#L2#开始战斗！#l\r\n#L3#退出远征队#l";
                    var selection = await AskMenu(list);

                    if (selection == 1)
                    {
                        var expedMembers = expedition.GetPlayerSortList();
                        var size = expedMembers.Count;
                        if (size == 1)
                        {
                            await SayOK("你是远征队中唯一的成员。");
                            return;
                        }
                        var text = "以下成员组成了你的远征队（点击成员名字可以将其踢出远征队）：\r\n";
                        text += "\r\n\t\t1." + expedMembers[0].Name;
                        for (var i = 1; i < size; i++)
                        {
                            text += "\r\n#b#L" + (i + 1) + "#" + (i + 1) + ". " + expedMembers[i].Name + "#l\n";
                        }
                        var kickSelection = await AskMenu(text);
                        if (kickSelection > 0)
                        {
                            var banned = expedMembers[kickSelection - 1];
                            expedition.ban(banned.Id);
                            await SayOK("你已经从远征中禁止了 " + banned.Name + "。");
                        }
                    }
                    else if (selection == 2)
                    {
                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK("你的远征队至少需要有" + expedition.EventManager.MinCount + "名玩家注册。");
                            return;
                        }

                        await SayOK($"远征队将开始，现在我将护送你前往 #b#m{expedition.EventManager.EntryMap}##k。");

                        expedition.StartBattle();
                    }
                    else if (selection == 3)
                    {
                        player.getMap().LightBlue(expedition.getLeader().getName() + "远征结束了。");
                        expedition.Dispose();
                        await SayOK("这次远征已经结束。有时候最好的策略就是逃跑。");
                    }
                }
            }
            else if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK("你已经注册了这次远征。请等待 #r" + expedition.getLeader().getName() + "#k 开始。");
                }
                else
                {
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(em.GetTemplate.JoinMember(expedition, getPlayer()), c));
                }
            }
            else if (expedition.isInProgress())
            {
                if (expedition.contains(player))
                {
                    if (expedition.getIntProperty("canJoin") == 1)
                    {
                        expedition.registerPlayer(player);
                    }
                    else
                    {
                        await SayOK("你的远征队已经开始对抗" + expedBoss + "的战斗。让我们为这些勇敢的灵魂祈祷。");
                    }
                }
                else
                {
                    await SayOK("另一支远征队正在挑战了" + expedBoss + "，让我们为这些勇敢的灵魂祈祷吧。");
                }
            }
        }
        // Npc: 2032002
        public async Task Zakum01()
        {
            var eim = getPlayer().getEventInstance();

            if (!eim.isEventCleared())
            {
                var selection = await AskMenu("...#b\r\n#L0#我在这里应该做什么？#l\r\n#L1#我带来了物品！#l\r\n#L2#我想要离开！#l");

                if (selection == 0)
                {
                    await SayOK("为了揭示扎昆的力量，你需要重新制造它的核心。在这个地牢的某个地方隐藏着#b一块 #v4001018##t4001018# #k，这是制作核心所需的材料之一。找到它，然后带给我。\r\n哦，你能帮我一个忙吗？\r\n这附近的石头下面也有一些#b #v4001015##t4001015# #k。如果你能找到#b30份#k，我会奖励你的努力。");
                }
                else if (selection == 1)
                {
                    if (!isEventLeader())
                    {
                        await SayOK("请让你们的队长把材料带给我，以完成这个考验。");
                        return;
                    }

                    if (!haveItem(4001018))
                    {
                        await SayOK("请找到#b #v4001018##t4001018# #k 再带来给我。");
                    }
                    else
                    {
                        var gotAllDocs = haveItem(4001015, 30);
                        if (!gotAllDocs)
                        {
                            if (await AskYesNo("所以，你带了#b #v4001018##t4001018# #k来了？我可以给你和你的每个队员#b一块#v4031061##t4031061##k，这应该足够制作扎昆的核心。确保你的整个队伍在继续之前有足够的背包空间。"))
                            {
                                gainItem(4001018, -1);
                                eim.giveEventPlayersExp(12000);
                                eim.clearPQ();
                            }
                        }
                        else
                        {
                            if (await AskYesNo("所以，你带来了#b #v4001018##t4001018# #k和#b #v4001015##t4001015# #k吗？我可以给你和你的每个队员#b一块#v4031061##t4031061##k，这应该足够制作扎昆的核心了。\r\n\r\n另外，既然你带来了#b #v4001015##t4001015# * 30#k，我还可以给你#b#v2030007##t2030007# * 5#k，可以随时带你到矿井入口。在继续之前，请确保你的整个队伍的背包有足够的空间。"))
                            {
                                gainItem(4001018, -1);
                                gainItem(4001015, -30);
                                eim.setProperty("gotDocuments", 1);
                                eim.giveEventPlayersExp(20000);
                                eim.clearPQ();
                            }
                        }
                    }
                }
                else if (selection == 2)
                {
                    if (await AskYesNo("你确定要退出吗？如果你是队伍的队长，你的队伍也将离开矿区。"))
                    {
                        warp(211042300);
                    }
                }
            }
            else
            {
                await SayNext("你完成了这次磨难，现在领取你的奖品。");

                if (eim.getIntProperty("gotDocuments") == 1)
                {
                    if (eim.gridCheck(getPlayer()) == -1)
                    {
                        if (CanHoldAll([new ItemQuantity(2030007, 5), new ItemQuantity(4031061, 1)]))
                        {
                            gainItem(2030007, 5);
                            gainItem(4031061, 1);
                            eim.gridInsert(getPlayer(), 1);
                        }
                        else
                        {
                            await SayOK("确保在继续之前你的背包有足够的空间。");
                        }
                    }
                    else
                    {
                        await SayOK("你已经领取了你的份额。你现在可以通过那边的传送门离开矿井了。");
                    }
                }
                else
                {
                    if (eim.gridCheck(getPlayer()) == -1)
                    {
                        if (canHold(4031061, 1))
                        {
                            gainItem(4031061, 1);
                            eim.gridInsert(getPlayer(), 1);
                        }
                        else
                        {
                            await SayOK("确保在继续之前你的背包有足够的空间。");
                        }
                    }
                    else
                    {
                        await SayOK("你已经领取了你的份额。你现在可以通过那边的传送门离开矿井了。");
                    }
                }
            }
        }


        // Npc: 2032003
        public async Task Zakum02()
        {
            await SayNext("恭喜你走到了这一步！好吧，我想我最好给你#b#t4031062##k。你确实赢得了它！");

            if (!canHold(4031062))
            {
                await SayOK("尝试释放一个空位来接收#b#t4031062##k。");
                return;
            }

            await SayNext("好了，你是时候离开了。");

            gainItem(4031062, 1);
            gainExp((int)(10000 * getPlayer().getExpRate()));
            warp(211042300);
        }
    }
}
