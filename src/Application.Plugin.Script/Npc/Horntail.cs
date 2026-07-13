using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Shared.Events;
using Application.Shared.GameProps;
using Application.Utility.Configs;
using server.expeditions;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2081005 — Keroben (Horntail Cave Entrance)
        public async Task hontale_keroben()
        {
            var isTransformed = getPlayer().getBuffSource(BuffStat.MORPH) == 2210003;
            if (!isTransformed && !haveItem(4001086))
            {
                await SayOK(GetDefault0());
                return;
            }

            var sel = await AskMenu("欢迎来到生命之洞穴 - 入口！你想要进去和 #r暗黑龙王#k 战斗吗？如果你想要和他战斗，你可能需要一些 #b#v2000005##k，这样如果你被 #r暗黑龙王#k 击中了，你就可以恢复一些HP。\r\n#L1#我想花100,000金币购买10个！#l\r\n#L2#不用了，让我进去吧！#l");

            if (sel == 1)
            {
                if (getMeso() < 100000)
                {
                    await SayOK("抱歉，你没有足够的金币来购买它们！");
                    return;
                }

                if (!canHold(2000005))
                {
                    await SayOK("抱歉，你的背包里没有空位来存放这个物品！");
                    return;
                }

                await gainMeso(-100000);
                await gainItem(2000005, 10);
                await SayOK("谢谢购买这个药水。记得好好使用它！");
            }
            else if (sel == 2)
            {
                if (getLevel() > 99)
                {
                    await warp(240050000, 0);
                }
                else
                {
                    await SayOK("对不起，您需要至少达到100级或以上才能进入。");
                }
            }
        }


        // Npc: 2083000 — The Encrypted Slate
        public async Task hontale_enterToE()
        {
            if (haveItem(4001086))
            {
                if (await AskYesNo("你现在想要进入 #b#m240050400##k 吗？"))
                    await warp(240050400);
            }
            else if (YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS && canBypassHTPQ())
            {
                if (await AskYesNo("你现在想要进入 #b#m240050400##k 吗？"))
                    await warp(240050400);
            }
            else
            {
                await SayOK("那些没有#r#t4001086##k的人必须在挑战#b暗黑龙王#k之前证明他们的勇气。");
            }
        }

        private bool canBypassHTPQ()
        {
            return haveItem(4001083) && haveItem(4001084) && haveItem(4001085);
        }


        // Npc: 2083001 — Mark of the Squad (Horntail PQ)
        public async Task hontale_enter1()
        {
            if (getMapId() == 240050000)
            {
                var em = GetEventManager(nameof(PQ_Horntail));
                if (em == null)
                {
                    await SayOK("霍恩尾巴洞窟遭遇了一个错误。");
                    return;
                }

                if (isUsingOldPqNpcStyle())
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    return;
                }

                var sel = await AskMenu("#e#b<组队任务：暴君蛋龙试炼场>\r\n#k#n" + em.Template.GetRequirementDescription(c) + "\r\n\r\n这是通往暴君蛋龙巢穴的路径。如果你想面对他，你和你的队伍将在前方的试炼场上接受考验。#b\r\n#L0#让我们通过试炼场。\r\n#L2#我想听更多细节。");

                if (sel == 0)
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
                else
                {
                    await SayOK("#e#b<组队任务：暴君之地>#k#n\r\n作为暴君巢穴的守门人，我只会允许值得的人进入。即使是对于那些人来说，内部的路径也像迷宫一样，充满了分支和考验。然而，那些擅长与团队BOSS战斗的人有更好的机会站在我们的领袖面前，尽管我们这种人也有机会。");
                }
            }
            else
            {
                await hontale_enter1_inquest();
            }
        }

        private async Task hontale_enter1_inquest()
        {
            if (!isEventLeader())
            {
                await SayOK("只有你的队伍领袖才能与日程表进行交互。");
                return;
            }

            var mapId = getMapId();
            if (mapId == 240050100)
            {
                if (haveItem(4001087) && haveItem(4001088) && haveItem(4001089) && haveItem(4001090) && haveItem(4001091))
                {
                    await gainItem(4001087, -1);
                    await gainItem(4001088, -1);
                    await gainItem(4001089, -1);
                    await gainItem(4001090, -1);
                    await gainItem(4001091, -1);

                    await getEventInstance()!.warpEventTeam(240050200);
                }
                else
                {
                    await SayOK("你没有所有需要的钥匙来继续前进。");
                }
            }
            else if (mapId == 240050300 || mapId == 240050310)
            {
                if (haveItem(4001092, 1) && haveItem(4001093, 6))
                {
                    await gainItem(4001092, -1);
                    await gainItem(4001093, -6);
                    await getEventInstance()!.clearPQ();
                }
                else
                {
                    await SayOK("检查一下你是否带着" + (mapId == 240050300 ? "" : "所有") + "6把红钥匙和1把蓝钥匙。");
                }
            }
        }


        // Npc: 2083002 — Crystal of Roots (Horntail Exit)
        public async Task hontale_out()
        {
            if (!await AskYesNo("你想要离开吗？"))
                return;

            if (getMapId() > 240050400)
                await warp(240050600);
            else
                await warp(240040700, "out00");
        }


        // Npc: 2083004 — Mark of the Squad (Horntail Expedition)
        public async Task hontale_accept()
        {
            var player = getPlayer();
            var em = GetEventManager<ExpeditionEventManager>(nameof(Battle_Horntail));
            var expedition = em.GetOnlyEventInstanceManager<ExpeditionEventInstanceManager>();
            var expedBoss = c.CurrentCulture.GetMobName(em.GetTemplate.BossId);

            if (player.getLevel() < em.MinLevel || player.getLevel() > em.MaxLevel)
            {
                await SayOK("您不符合与" + expedBoss + "战斗的条件！");
                return;
            }

            if (expedition == null)
            {
                var sel = await AskMenu("#e#b<远征：Horntail>\r\n#k#n" + (em.Template.GetRequirementDescription(c) ?? "") + "\r\n\r\n你想组建一个远征队来挑战 #r" + expedBoss + "#k 吗？\r\n#b#L1#让我们开始吧！#l\r\n#L2#不，我想再等一会儿...#l");

                if (sel == 1)
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
                        text += "\r\n\t\t1." + expedMembers[0].Name;
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
                    else if (sel == 2)
                    {
                        var size = expedition.getPlayers().Count;
                        if (size < expedition.EventManager.MinCount)
                        {
                            await SayOK("你的远征队至少需要有" + expedition.EventManager.MinCount + "名玩家注册。");
                            return;
                        }

                        await SayOK("祝你好运！整个里弗尔都指望你了。");

                        await expedition.StartBattle();
                    }
                    else if (sel == 3)
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
    }
}
