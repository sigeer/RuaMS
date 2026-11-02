using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Core.Scripting.Events;
using Application.Shared.Events;

namespace Application.Plugin.Script.Features.Exped_Showa
{
    /// <summary>昭和村远征相关 NPC。</summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }

        // Npc: 9120015 
        [ScriptName("To the Showa manor...")]
        public async Task s_To_the_Showa_manor()
        {
            var option = await AskMenu("你想从我这里得到什么？\r\n#L0##b收集一些有关藏身之处的信息。#l\r\n#L1#带我去藏身之处。#l\r\n#L2#什么都不要。#k");
            switch (option)
            {
                case 0:
                    await SayNext("我可以带你去藏身之处，但那个地方到处都是寻衅滋事的暴徒。你需要既非常强壮又勇敢才能进入那个地方。在藏身之处，你会找到控制这个地区所有其他头目的老大。到达藏身之处很容易，但那个地方顶层的房间每天只能进入一次。老大的房间不是一个可以胡闹的地方。我建议你不要在那里待得太久；一旦进去，你需要迅速处理好事情。老大本人是一个强大的对手，但在去见老大的路上你会遇到一些非常强大的敌人！这并不会容易。");
                    break;
                case 1:
                    await SayNext("Oh, the brave one. I've been awaiting your arrival. If these\r\nthugs are left unchecked, there's no telling what going to\r\nhappen in this neighborhood. Before that happens, I hope\r\nyou take care of all them and beat the boss, who resides\r\non the 5th floor. You'll need to be on alert at all times, since\r\nthe boss is too tough for even wisemen to handle.\r\nLooking at your eyes, however, I can see that eye of the\r\ntiger, the eyes that tell me you can do this. Let's go!");
                    break;

                case 2:
                    await SayOK("我很忙！如果你只是需要这个，就别打扰我！");
                    break;
                default:
                    break;
            }
        }

        // Npc: 9120200 
        public async Task con2()
        {
            if (await AskYesNo("你就在藏身处前面！什么？你想返回#m801000000#？"))
            {
                await warp(801000000);
            }
            else
            {
                await SayOK("如果你想回到#m801000000#，就来找我。");
            }
        }

        // Npc: 9120202 
        public async Task con3()
        {
            var eim = getEventInstance();
            if (eim == null || !eim.isEventCleared())
            {
                if (await AskYesNo("如果你现在离开，就无法返回了。你确定要离开吗？"))
                {
                    await warp(801040004, 1);
                }
            }
            else
            {
                await SayNext("你们做到了，干得漂亮！现在我们的城市摆脱了那些暴徒的暴政！作为城市的代表，请接受这份奖励，作为对你们努力的认可，我会把你们带回城里。");

                var result = await eim!.GiveClearReward(getPlayer());
                if (result != ClaimRewardResult.Success)
                {
                    await SayNext("请先在您的物品栏腾出空间…");
                }
                else
                {
                    await warp(801040101);
                }
            }
        }

        // Npc: 9120203 
        public async Task con4()
        {
            await SayNext("啊，Boss已经被打败了。这真是个高兴的日子！恭喜大家。沿着这条路返回城镇。");
            await warp(801000000);
        }

        // Npc: 9120201 
        public async Task s_dungeon()
        {
            var exped = ExpeditionType.SHOWA;
            var expedName = "Showa Gang";
            var expedMap = "Nightmarish Last Days";
            var expedItem = 4000138;

            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(Exped_ShowaFeature.EventName);
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
