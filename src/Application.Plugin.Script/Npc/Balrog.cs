using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 1061014 
        public async Task balog_accept()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>("Battle_Balrog");
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
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK("有人已经主动成为了远征队的领袖。试着加入他们吧！");
                        return;
                    }

                    var r = await em.StartInstance(getPlayer());
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
                        var expedMembers = expedition.getPlayers();
                        var size = expedMembers.Count;
                        if (size == 1)
                        {
                            await SayOK("你是远征队中唯一的成员。");
                            return;
                        }
                        var text = "以下成员组成了你的远征队（点击成员名字可以将其踢出远征队）：\r\n";
                        text += "\r\n\t\t1." + expedition.getLeader().getName();
                        for (var i = 1; i < size; i++)
                        {
                            text += "\r\n#b#L" + (i + 1) + "#" + (i + 1) + ". " + expedMembers[i].Name + "#l\n";
                        }
                        var kickSelection = await AskMenu(text);
                        if (kickSelection > 0)
                        {
                            var banned = expedMembers[kickSelection - 1];
                            await expedition.ban(banned.Id);
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

                        await expedition.StartBattle();
                    }
                    else if (selection == 3)
                    {
                        await player.getMap().LightBlue(expedition.getLeader().getName() + "远征结束了。");
                        await expedition.DisposeAsync();
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
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(await em.GetTemplate.JoinMember(expedition, getPlayer()), c));
                }
            }
            else if (expedition.isInProgress())
            {
                if (expedition.contains(player))
                {
                    if (expedition.getIntProperty("canJoin") == 1)
                    {
                        await expedition.registerPlayer(player);
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


        // Npc: 1061016 
        public async Task balog_scroll()
        {
            int[] items = [2040728, 2040729, 2040730, 2040731, 2040732, 2040733, 2040734, 2040735, 2040736, 2040737, 2040738, 2040739];
            await AskMenu("你好，#h #。我可以交换你的#t4001261#。\r\n\r\n#r#L1#兑换物品#l#k");
            var option = await AskMenu("可兑换物品", items.Select(i => $"#i{i}# #z{i}#"));
            if (!canHold(items[option], 1))
            {
                await SayOK("请腾出空间");
            }
            else if (!haveItemWithId(4001261))
            {
                await SayOK("你没有足够的#t4001261#。");
            }
            else
            {
                await gainItem(4001261, -1);
                await gainItem(items[option], 1);
                await SayOK("谢谢您的兑换。");
            }
        }


        // Npc: 1061018 
        public async Task balog_InOut()
        {
            var eim = GetEventInstanceTrust();
            if (eim.isEventCleared())
            {
                await SayOK("哇！你打败了蝙蝠怪。");

                await warp(eim.EventManager.ExitMap, 0);
            }
            else
            {
                if (await AskYesNo(getMap().getAllPlayers().Count > 1 ? "你真的要离开这场战斗，让你的同伴们去死吗？" : "逃跑吧，懦夫。"))
                {
                    await eim.exitPlayer(getPlayer());
                }
            }
        }
    }
}
