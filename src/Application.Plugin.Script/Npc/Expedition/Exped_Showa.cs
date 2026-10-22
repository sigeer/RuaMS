using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Shared.Events;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9120201 
        public async Task s_dungeon()
        {
            var exped = ExpeditionType.SHOWA;
            var expedName = "Showa Gang";
            var expedMap = "Nightmarish Last Days";
            var expedItem = 4000138;

            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(nameof(Exped_Showa));
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK("您不符合与" + expedBoss + "战斗的条件！");
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu("#e#b<远征：" + expedName + ">\r\n#k#n" + (em.Template?.GetRequirementDescription(c) ?? "") + "\r\n\r\n你想组建一个队伍来挑战 #r" + expedBoss + "#k 吗？\r\n#b#L1#让我们开始吧！#l\r\n#L2#不，我想再等一会儿...#l");
                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK("有人已经主动成为了远征队的领袖。试着加入他们吧！");
                        return;
                    }

                    if (!haveItem(expedItem))
                    {
                        await SayOK("作为远征队领袖，你必须在你的物品栏中拥有一件#b#t" + expedItem + "##k，以对抗" + expedBoss + "！");
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
                    await SayOK("你的探险已经在进行中，对于那些仍在战斗中的人，让我们为那些勇敢的灵魂祈祷吧。");
                }
                else
                {
                    var list = "你想做什么？#b\r\n\r\n#L1#查看当前远征队成员#l\r\n#L2#开始战斗！#l\r\n#L3#退出远征队#l";
                    var sel = await AskMenu(list);

                    if (sel == 1)
                    {
                        var expedMembers = expedition.GetPlayerSortList();
                        var size = expedMembers.Count;
                        if (size == 1)
                        {
                            await SayOK("你是探险队中唯一的成员。");
                            return;
                        }

                        var text = "以下成员组成了你的探险队（点击成员名字可以将其踢出探险队）：\r\n";
                        text += "\r\n\t\t1." + expedition.getLeader().getName();
                        for (var i = 1; i < size; i++)
                        {
                            text += "\r\n#b#L" + (i + 1) + "#" + (i + 1) + ". " + expedMembers[i].Name + "#l\n";
                        }

                        var kickSel = await AskMenu(text);
                        if (kickSel > 0)
                        {
                            var banned = expedMembers[kickSel - 1];
                            await expedition.ban(banned.Id);
                            await SayOK("你已经从远征中禁止了 " + banned.Name + "。");
                        }
                    }
                    else if (sel == 2)
                    {
                        if (!haveItem(expedItem))
                        {
                            await SayOK("作为远征队领袖，你必须在你的物品栏中拥有一件#b#t" + expedItem + "##k，以对抗" + expedBoss + "！");
                            return;
                        }

                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK("你的远征队至少需要有" + expedition.EventManager.MinCount + "名玩家注册。");
                            return;
                        }

                        await SayOK("探险队将开始，现在将由护送你前往 #b" + expedMap + "#k。");

                        await expedition.StartBattle();
                    }
                    else if (sel == 3)
                    {
                        await player.getMap().LightBlue(expedition.getLeader().getName() + "探险结束了。");
                        await expedition.DisposeAsync();
                        await SayOK("这次探险已经结束。有时候最好的策略就是逃跑。");
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
                    if (em != null)
                    {
                        var eim = em.getInstance(expedName + player.getClient().getChannel());
                        if (eim != null && eim.getIntProperty("canJoin") == 1)
                        {
                            await eim.registerPlayer(player);
                        }
                        else
                        {
                            await SayOK("你的远征队已经开始对抗" + expedBoss + "的战斗。让我们为这些勇敢的灵魂祈祷。");
                        }
                    }
                    else
                    {
                        await SayOK("无法找到战斗实例。");
                    }
                }
                else
                {
                    await SayOK("另一支探险队已经主动挑战了" + expedBoss + "，让我们为这些勇敢的灵魂祈祷吧。");
                }
            }
        }
    }
}
