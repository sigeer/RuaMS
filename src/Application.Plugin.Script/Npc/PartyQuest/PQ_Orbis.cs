using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2013000 — Orbis PQ entrance (Wonky)
        public async Task party3_enter()
        {
            var em = GetEventManager(nameof(PQ_Orbis));
            if (em == null)
            {
                await SayOK("天空之城组队任务遇到了一个错误。");
                return;
            }

            if (getMapId() == 200080101)
            {
                if (isUsingOldPqNpcStyle())
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    return;
                }

                var sel = await AskMenu("#e#b<组队任务：女神之塔>\r\n#k#n\r\n你想要组建或加入一个团队来解决#b女神之塔#k的谜题吗？让你的#b队伍领袖#k与我交谈或者自己组建一个队伍。#b\r\n#L0#我想参加组队任务。\r\n#L1#我想了解更多细节。\r\n#L2#我想要领取奖励。#l");
                if (sel == 0)
                {
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                }
                else if (sel == 1)
                {
                    await SayOK("#e#b<女神之塔组队任务>#k#n\r\n我们的女神已经失踪了一段时间，有传言说她最后一次被看到是在女神之塔内。此外，我们的圣地已经被精灵们的压倒性力量夺取，这些生物最近一直在奥比斯的边缘徘徊。他们的领袖，皮克西爸爸，目前掌握着王位，可能知道她的下落，因此我们迫切需要找到一支由勇敢的英雄组成的队伍，冲进去夺回我们的圣地并拯救她。如果你的团队能够包含每个职业（战士，魔法师，弓箭手，飞侠和海盗），你们将得到我的祝福来帮助你们战斗。你们会帮助我们吗？");
                }
                else
                {
                    if (!haveItem(1082232) && haveItem(4001158, 10))
                    {
                        await gainItem(1082232, 1);
                        await gainItem(4001158, -10);
                    }
                    else
                    {
                        await SayOK("你要么已经拥有女神手镯，要么没有10个#t4001158#。");
                    }
                }
            }
            else
            {
                if (await AskYesNo("你打算退出这次救援任务吗？"))
                    await warp(920011200);
            }
        }


        // Npc: 2013001 — Orbis PQ stage manager (Chamberlain Eak)
        public async Task party3_play()
        {
            var eim = getEventInstance();
            var mapId = getMapId();

            if (mapId == 920011200)
            {
                await warp(200080101);
                return;
            }

            if (eim == null)
                return;

            if (!isEventLeader())
            {
                if (mapId == 920010000)
                    await warp(920010000, 2);
                else
                    await SayOK("我只想和你们的队长谈话！");
                return;
            }

            switch (mapId)
            {
                case 920010000:
                    if (eim.getIntProperty("statusStg0") != 1)
                    {
                        await eim.warpEventTeamToMapSpawnPoint(920010000, 2);
                        await eim.giveEventPlayersExp(3500);
                        await clearStage(eim, 0);
                        await SayNext("请救救#p2013002#，她被波波皮希困在封印中，他是我们塔楼的恐怖存在！他把#p2013002#雕像的所有部分都弄丢了，我们必须把它们全部找回来！哦，请原谅我，我是塔楼的帮佣易克。");
                    }
                    else
                    {
                        await warp(920010000, 2);
                    }
                    break;

                case 920010100:
                    if (isStatueComplete())
                    {
                        if (eim.getIntProperty("statusStg7") == 0)
                            await eim.warpEventTeam(920010800);
                        else if (eim.getIntProperty("statusStg8") == 0)
                            await SayOK("哦！你带来了#t4001055#！请把它放在雕像的底座上，让米涅瓦重生！");
                        else
                            await SayOK("谢谢你救了#p2013002#！请和她交谈…");
                    }
                    else
                    {
                        await SayOK("请拯救#p2013002#！收集她雕像的六块碎片，然后与我交谈以取回最后一块碎片！");
                    }
                    break;

                case 920010200:
                    if (!haveItem(4001050, 30))
                    {
                        await SayOK("收集这个阶段怪物身上的30个雕像碎片，然后请把它们带给我，这样我就可以把它们拼在一起！");
                    }
                    else
                    {
                        await SayOK("你已经找到了它们！这里是第一块雕像碎片。");
                        await removeAll(4001050);
                        await gainItem(4001044, 1);
                        await eim.giveEventPlayersExp(3500);
                        await clearStage(eim, 1);
                    }
                    break;

                case 920010300:
                    if (eim.getIntProperty("statusStg2") != 1)
                    {
                        if (getMap().countMonsters() == 0 && getMap().countItems() == 0)
                        {
                            if (canHold(4001045))
                            {
                                await SayOK("哦，我找到了第二块雕像碎片。拿去吧。");
                                await gainItem(4001045, 1);
                                await eim.giveEventPlayersExp(3500);
                                await clearStage(eim, 2);
                                eim.setProperty("statusStg2", "1");
                            }
                            else
                            {
                                await SayOK("我已经找到了第二块雕像碎片。在你的背包中腾出一个空位来拿它。");
                            }
                        }
                        else
                        {
                            await SayOK("在这个房间里找到隐藏的第二块雕像碎片。");
                        }
                    }
                    else
                    {
                        await SayOK("干得好。去找其他雕像碎片。");
                    }
                    break;

                case 920010400:
                    var stg3 = eim.getIntProperty("statusStg3");
                    if (stg3 == 0)
                    {
                        await SayOK("请找到本周的LP，并将其放在音乐播放器上。\r\n#v4001056# 星期日\r\n#v4001057# 星期一\r\n#v4001058# 星期二\r\n#v4001059# 星期三\r\n#v4001060# 星期四\r\n#v4001061# 星期五\r\n#v4001062# 星期六");
                    }
                    else if (stg3 == 1)
                    {
                        getMap().getReactorByName("stone3")?.forceHitReactor(1);
                        await SayOK("哦，这音乐... 它和环境非常搭配。做得好，一个箱子出现在场地上。从中取出雕像的一部分！");
                        await eim.giveEventPlayersExp(3500);
                        await clearStage(eim, 3);
                        eim.setProperty("statusStg3", "2");
                    }
                    else
                    {
                        await SayOK("非常感谢你！");
                    }
                    break;

                case 920010500:
                    var stg4 = eim.getIntProperty("statusStg4");
                    if (stg4 == 0)
                    {
                        var total = 3;
                        for (var i = 0; i < 2; i++)
                        {
                            var rnd = Random.Shared.Next(total + 1);
                            total -= rnd;
                            eim.setProperty("stage4_" + i, "" + rnd);
                        }
                        eim.setProperty("stage4_2", "" + total);
                        eim.setProperty("statusStg4", "1");
                    }

                    if (eim.getIntProperty("statusStg4") == 1)
                    {
                        var players = new int[3];
                        var sum = 0;
                        for (var i = 0; i < 3; i++)
                        {
                            players[i] = getMap().getNumPlayersInArea(i);
                            sum += players[i];
                        }

                        if (sum != 3)
                        {
                            await SayOK("这些平台上需要有确切的3名玩家。");
                        }
                        else
                        {
                            var correct = 0;
                            for (var i = 0; i < 3; i++)
                            {
                                if (eim.getProperty("stage4_" + i) == "" + players[i])
                                    correct++;
                            }

                            if (correct == 3)
                            {
                                await SayOK("你找到了正确的组合！地图顶部出现了一个宝箱，去拿取里面的雕像碎片吧！");
                                getMap().getReactorByName("stone4")?.forceHitReactor(1);
                                await eim.giveEventPlayersExp(3500);
                                await clearStage(eim, 4);
                            }
                            else
                            {
                                await eim.showWrongEffect();
                                if (correct > 0)
                                    await SayOK("一个平台上有正确数量的玩家。");
                                else
                                    await SayOK("所有的平台上都有错误的玩家数量。");
                            }
                        }
                    }
                    else
                    {
                        await SayOK("干得好！请去找其他碎片，拯救米内尔瓦！");
                    }
                    break;

                case 920010600:
                    if (eim.getIntProperty("statusStg5") == 0)
                    {
                        if (!haveItem(4001052, 40))
                        {
                            await SayOK("在这个阶段从怪物身上收集40个雕像碎片，然后请把它们带给我，这样我就可以把它们拼在一起！");
                        }
                        else
                        {
                            await SayOK("你已经找到了它们！这里是第五块雕像碎片。");
                            await removeAll(4001052);
                            await gainItem(4001048, 1);
                            await eim.giveEventPlayersExp(3500);
                            await clearStage(eim, 5);
                            eim.setIntProperty("statusStg5", 1);
                        }
                    }
                    else
                    {
                        await SayOK("你已经找到了所有的东西。去搜索塔的其他房间吧。");
                    }
                    break;

                case 920010700:
                    if (eim.getIntProperty("statusStg6") == 0)
                    {
                        var rnd1 = Random.Shared.Next(5);
                        var rnd2 = Random.Shared.Next(5);
                        while (rnd2 == rnd1)
                            rnd2 = Random.Shared.Next(5);

                        if (rnd1 > rnd2)
                            (rnd1, rnd2) = (rnd2, rnd1);

                        var comb = "";
                        for (var i = 0; i < rnd1; i++) comb += "0";
                        comb += "1";
                        for (var i = rnd1 + 1; i < rnd2; i++) comb += "0";
                        comb += "1";
                        for (var i = rnd2 + 1; i < 5; i++) comb += "0";

                        eim.setProperty("stage6_c", "" + comb);
                        eim.setProperty("statusStg6", "1");
                    }

                    if (eim.getIntProperty("statusStg6") == 1)
                    {
                        var comb = eim.getProperty("stage6_c")!;
                        var react = "";
                        var pushed = 0;
                        for (var i = 1; i <= 5; i++)
                        {
                            if (getMap().getReactorByName("" + i)?.getState() > 0)
                            {
                                react += "1";
                                pushed++;
                            }
                            else
                            {
                                react += "0";
                            }
                        }

                        if (pushed != 2)
                        {
                            await SayOK("地图顶部需要精确地推动两个杠杆。");
                        }
                        else
                        {
                            var correct = 0;
                            var pshCorrect = 0;
                            for (var i = 0; i < 5; i++)
                            {
                                if (react[i] == comb[i])
                                {
                                    correct++;
                                    if (react[i] == '1')
                                        pshCorrect++;
                                }
                            }

                            if (correct == 5)
                            {
                                await SayOK("你找到了正确的组合！从里面取出雕像碎片！");
                                getMap().getReactorByName("stone6")?.forceHitReactor(1);
                                await eim.giveEventPlayersExp(3500);
                                await clearStage(eim, 6);
                            }
                            else
                            {
                                await eim.showWrongEffect();
                                if (pshCorrect >= 1)
                                    await SayOK("其中一个推动的杠杆是正确的。");
                                else
                                    await SayOK("两个推杆都是错误的。");
                            }
                        }
                    }
                    else
                    {
                        await SayOK("干得漂亮！去看看其他的部分吧。");
                    }
                    break;

                case 920010800:
                    await SayNext("请找到一种方法来打败波波精灵！一旦你通过种植种子找到了黑暗尼芬死亡，你就找到了波波精灵！打败它，拿到生命之根来拯救米内尔瓦！！");
                    break;

                case 920010900:
                    if (eim.getProperty("statusStg8") == "1")
                        await SayNext("这是塔的监狱。你可能会在这里找到一些好东西，只要确保尽快解决前面的谜题。");
                    else
                        await SayNext("在那里你找不到任何雕像碎片。爬上梯子返回中心塔，然后到其他地方去搜索。一旦你救了米涅瓦，你可以回到这里拿下面的好东西。");
                    break;

                case 920011000:
                    if (getMap().countMonsters() > 0)
                        await SayNext("这是塔楼的隐藏房间。在清除了这个房间上的所有怪物之后，与我交谈以获得进入宝藏房间的权限，留下中央塔楼的通道。");
                    else
                        await warp(920011100, "st00");
                    break;
            }
        }

        private bool isStatueComplete()
        {
            for (var i = 1; i <= 6; i++)
            {
                if (getMap().getReactorByName("scar" + i)?.getState() < 1)
                    return false;
            }
            return true;
        }

        private static async Task clearStage(AbstractEventInstanceManager eim, int stage)
        {
            eim.setProperty("statusStg" + stage, "1");
            await eim.showClearEffect(true);
        }


        // Npc: 2013002 — Minerva the Goddess (reward NPC)
        public async Task party3_minerva()
        {
            var mapId = getMapId();

            if (mapId == 920010100)
            {
                if (await AskYesNo("我已经解除了阻止通往塔楼监狱储藏室的咒语。你可能会在那里找到一些好东西……或者，你可能想现在离开。你准备好离开了吗？"))
                    await warp(920011300, 0);
            }
            else if (mapId == 920011100)
            {
                if (await AskYesNo("所以，你准备好退出了吗？"))
                    await warp(920011300, 0);
            }
            else if (mapId == 920011300)
            {
                var eim = getEventInstance();
                if (eim == null) return;

                await SayNext("谢谢你不仅修复了雕像，还救出了我，米涅瓦，脱离困境。愿女神的祝福与你同在，直到最后……作为感激之情，请接受这份纪念品，以表彰你的勇敢。");

                var result = await eim.GiveClearReward(getPlayer());
                if (result != ClaimRewardResult.Success)
                    await SayOK("请先在您的背包中腾出空间。");
                else
                    await warp(200080101, 0);
            }
        }
    }
}
