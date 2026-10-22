using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 9270047 — 马来西亚远征队（Scarga）入口
        public async Task MalaysiaBoss_GL()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(nameof(Exped_Scarga));
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK("您不符合与" + expedBoss + "战斗的条件！");
                return;
            }

            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();

            if (expedition == null)
            {
                var sel = await AskMenu("#e#b<远征：梦幻主题公园>\r\n#k#n"
                    + (em?.Template?.GetRequirementDescription(c) ?? "")
                    + "\r\n\r\n你想组建一个队伍来挑战 #r" + expedBoss + "#k 吗？\r\n#b#L1#让我们开始吧！#l\r\n#L2#不，我想再等一会儿...#l");
                if (sel == 1)
                {
                    expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
                    if (expedition != null)
                    {
                        await SayOK("有人已经主动成为了远征队的领袖。试着加入他们吧！");
                        return;
                    }

                    var r = await em.StartInstance(player);
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK("当然，并非每个人都能挑战" + expedBoss + "。");
                }
                return;
            }

            if (expedition.isLeader(player))
            {
                if (expedition.isInProgress())
                {
                    await SayOK("你的探险已经在进行中，对于那些仍在战斗中的人，让我们为那些勇敢的灵魂祈祷吧。");
                    return;
                }

                var sel = await AskMenu("你想要做什么？#b\r\n\r\n#L1#查看当前远征队员名单#l\r\n#L2#开始战斗！#l\r\n#L3#结束远征#l");
                if (sel == 1)
                {
                    var members = expedition.GetPlayerSortList();
                    if (members.Count == 1)
                    {
                        await SayOK("你是探险队中唯一的成员。");
                        return;
                    }

                    var text = "以下成员组成了你的远征队（点击成员名字可以将其踢出远征队）：\r\n";
                    text += "\r\n\t\t1." + members[0].Name;
                    for (var i = 1; i < members.Count; i++)
                    {
                        text += $"\r\n#b#L{i + 1}#{i + 1}. {members[i].Name}#l\n";
                    }

                    var kick = await AskMenu(text);
                    if (kick > 0)
                    {
                        var banned = members[kick - 1];
                        await expedition.ban(banned.Id);
                        await SayOK("你已经从远征中禁止了 " + banned.Name + "。");
                    }
                }
                else if (sel == 2)
                {
                    if (expedition.getPlayers().Count < expedition.EventManager.MinCount)
                    {
                        await SayOK("你的远征队至少需要有" + expedition.EventManager.MinCount + "名玩家注册。");
                        return;
                    }

                    await SayOK("祝你好运！阴森世界的守护者正在等待你们。");
                    await expedition.StartBattle();
                }
                else if (sel == 3)
                {
                    await player.getMap().LightBlue(expedition.getLeader().getName() + "远征结束了。");
                    await expedition.DisposeAsync();
                    await SayOK("这次探险已经结束。有时候最好的策略就是逃跑。");
                }
                return;
            }

            if (expedition.isRegistering())
            {
                if (expedition.contains(player))
                {
                    await SayOK("你已经注册了这次远征。请等待 #r" + expedition.getLeader().getName() + "#k 开始。");
                }
                else
                {
                    await SayOK(em.GetTemplate.HandleJoinInstanceResult(await em.GetTemplate.JoinMember(expedition, player), c));
                }
                return;
            }

            if (expedition.isInProgress())
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
                    await SayOK("另一支远征队正在挑战" + expedBoss + "，让我们为这些勇敢的灵魂祈祷吧。");
                }
            }
        }

        // Npc: 9201134 
        public async Task Malay_Warp()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                if (await AskYesNo("如果你现在离开，就无法返回了。你确定要离开吗？"))
                {
                    await warp(551030100, 2);
                }
            }
            else
            {
                await SayNext("你们打败了斯卡利昂和塔加！太棒了！把这个纪念品当作你们勇敢的奖励。");

                if (await eim.GiveClearReward(getPlayer()) == Core.scripting.Events.Abstraction.ClaimRewardResult.Success)
                {
                    await warp(551030100, 2);
                }
                else
                {
                    await SayOK(GetTalkMessage(nameof(ScriptTalk.Redeem_InventoryFull)));
                }
            }
        }
    }
}
