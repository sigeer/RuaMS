using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Shared.Constants.Map;
using Application.Shared.MapObjects;
using Application.Utility;
using Application.Utility.Configs;

namespace Application.Plugin.Script.Features.Dojo
{
    /// <summary>
    /// 武陵道场（Mu Lung Dojo）：NPC 2091005 报名、2091006 公告板。
    /// 关卡推进与奖励结算在传送门脚本里。
    /// </summary>
    internal sealed class DojoNpcHandler : NpcScriptBase
    {
        public DojoNpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 2091005
        public async Task dojang_enter()
        {
            var belts = new[] { 1132000, 1132001, 1132002, 1132003, 1132004 };
            var belt_level = new[] { 25, 35, 45, 60, 75 };
            var belt_points = YamlConfig.config.server.USE_FAST_DOJO_UPGRADE
                ? new[] { 10, 90, 200, 460, 850 }
                : new[] { 200, 1800, 4000, 9200, 17000 };

            int selectedMenu = -1;

            if (DojoFeature.IsRestingSpot(getMapId()))
            {
                var eim = GetEventInstanceTrust();
                string text = "我很惊讶你能走到这一步！但从这里开始就没那么容易了。你想继续挑战吗？\r\n\r\n#b#L0#我想继续#l\r\n#L1#我想离开#l\r\n";
                if (eim.EventName == DojoFeature.SoloEventName)
                {
                    text += "#L2#我想记录我到目前为止的分数#l";
                }
                var selection = await AskMenu(text);
                selectedMenu = selection;

                if (selectedMenu == 0)
                {
                    if (eim.EventName == DojoFeature.PartyEventName)
                    {
                        if (!isPartyLeader())
                        {
                            await SayOK("你不是队长！如果你想继续，让你们的队长来找我。");
                            return;
                        }
                    }
                    else
                    {
                        getPlayer().setDojoStage(0);
                    }

                    var nextStage = (getMapId() + 100);
                    await eim.warpEventTeam(nextStage);
                }
                else if (selectedMenu == 1)
                {
                    if (await AskYesNo("所以，你要放弃了吗？你真的要离开吗？"))
                    {
                        await warp(DojoFeature.ExitMap, "st00");
                    }
                }
                else if (selectedMenu == 2)
                {
                    if (await AskYesNo("如果你记录下你的分数，下次可以从上次离开的地方开始。这不是很方便吗？你想记录下当前的分数吗？"))
                    {
                        var currentStg = DojoFeature.GetStage(getMapId());
                        if (getPlayer().getDojoStage() == currentStg)
                        {
                            await SayOK("我已记录下你的分数。下次挑战时，你可以从这里开始。");
                        }
                        else
                        {
                            await SayNext("我已记录下你的分数。下次挑战时，你可以从这里开始。注意，如果你选择#r继续挑战道馆，你的记录将被抹去#k。");
                            getPlayer().setDojoStage(currentStg);
                        }
                    }
                    else
                    {
                        await SayNext("你觉得你还能再走得更高吗？祝你好运！");
                    }
                }
            }
            else if (getLevel() >= 25)
            {
                if (getMapId() == DojoFeature.RecruitMap)
                {
                    var selection = await AskMenu("我们的师傅是武陵最强大的人，你想挑战他？好吧，但你以后会后悔的。\r\n\r\n#b#L0#我想独自挑战他。#l\r\n#L1#我想组队挑战他。#l\r\n\r\n#L2#我想获得一条腰带。#l\r\n#L3#我想重置我的修炼点数。#l\r\n#L4#我想获得一枚勋章。#l\r\n#L5#什么是武陵道场？#l");
                    selectedMenu = selection;

                    if (selectedMenu == 0)
                    {
                        if (!getPlayer().FinishedDojoTutorial)
                        {
                            if (await AskYesNo("嘿！你！这是你第一次来吗？嗯，我的主人不会随便见任何人。他很忙。看你的样子，我觉得他不会理你。哈！但是，今天是你的幸运日……我告诉你吧，如果你能打败我，我就让你见我的主人。你觉得怎么样？"))
                            {
                                var em = GetEventManager(DojoFeature.PreEventName);
                                var r = await em.StartInstance(getPlayer());
                                await SayOK(em.HandleCreateInstanceResult(r, c));
                            }
                            else
                            {
                                await SayNext("哈哈！你这样的心，想要给谁留下好印象呢？\r\n还是回到你应该去的地方吧！");
                            }
                        }
                        else
                        {
                            var storedStage = getPlayer().getDojoStage();
                            if (storedStage > 0)
                            {
                                var stageWarp = DojoFeature.GetMobStage(storedStage);

                                if (!await AskYesNo($"上次你独自挑战时，你通过了#b{stageWarp}#k关。我现在可以带你去那里。你想去那里吗？（选择#r否#k来删除这个记录。）"))
                                {
                                    getPlayer().setDojoStage(0);
                                }
                            }
                            var em = GetEventManager(DojoFeature.SoloEventName);
                            var r = await em.StartInstance(getPlayer());
                            await SayOK(em.HandleCreateInstanceResult(r, c));
                        }
                    }
                    else if (selectedMenu == 1)
                    {
                        var em = GetEventManager(DojoFeature.PartyEventName);
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                    }
                    else if (selectedMenu == 2)
                    {
                        if (!canHold(belts[0]))
                        {
                            await SayNext("在尝试领取腰带之前，请确保你的装备栏有足够的空间！");
                            return;
                        }

                        var belt_on_inventory = new bool[belts.Length];
                        for (var i = 0; i < belts.Length; i++)
                        {
                            belt_on_inventory[i] = haveItemWithId(belts[i], true);
                        }

                        var selStr = $"你有 #b{getPlayer().getDojoPoints()}#k 修炼点数。师傅更喜欢有天赋的人。如果你获得的点数超过平均水平，你可以根据你的分数获得一条腰带。\r\n";
                        for (var i = 0; i < belts.Length; i++)
                        {
                            if (belt_on_inventory[i])
                            {
                                selStr += $"\r\n#L{i}##i{belts[i]}# #t{belts[i]}# (已拥有)";
                            }
                            else
                            {
                                selStr += $"\r\n#L{i}##i{belts[i]}# #t{belts[i]}#";
                            }
                        }

                        var beltSelection = await AskMenu(selStr);
                        var belt = belts[beltSelection];
                        var level = belt_level[beltSelection];
                        var points = belt_points[beltSelection];
                        var oldbelt = beltSelection > 0 ? belts[beltSelection - 1] : -1;
                        var haveOldbelt = oldbelt == -1 || haveItemWithId(oldbelt, false);

                        if (beltSelection > 0 && !belt_on_inventory[beltSelection - 1])
                        {
                            await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                        }
                        else if (getPlayer().getDojoPoints() >= points)
                        {
                            if (beltSelection > 0 && !haveOldbelt)
                            {
                                await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                            }
                            else if (getLevel() > level)
                            {
                                if (beltSelection > 0)
                                {
                                    await gainItem(oldbelt, -1);
                                }
                                await gainItem(belt, 1);
                                getPlayer().setDojoPoints(getPlayer().getDojoPoints() - points);
                                await SayNext($"这里有一个 #i{belt}# #b#t{belt}##k。你已经证明了自己的勇气，可以在道场排名中晋升。干得好！");
                            }
                            else
                            {
                                await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                            }
                        }
                        else
                        {
                            await SendBeltRequirements(belt, oldbelt, haveOldbelt, level, points);
                        }
                    }
                    else if (selectedMenu == 3)
                    {
                        if (await AskYesNo("你知道如果你重置你的修炼点数，它会返回到0，对吧？不过，这并不总是件坏事。如果你在重置后能够重新开始赚取修炼点数，你就可以再次获得腰带。你现在想要重置你的修炼点数吗？"))
                        {
                            getPlayer().setDojoPoints(0);
                            await SayNext("好了！你所有的修炼点数都已经重置了。把它看作一个新的开始，努力训练吧！");
                        }
                        else
                        {
                            await SayNext("你需要冷静一下吗？深呼吸后再回来。");
                        }
                    }
                    else if (selectedMenu == 4)
                    {
                        if (getPlayer().getVanquisherStage() <= 0)
                        {
                            if (await AskYesNo($"你还没有尝试过勋章吗？如果你在勇士部落道场中打败某种类型的怪物#b100次#k，你就可以获得一个称号，叫做#b#t{1142033 + getPlayer().getVanquisherStage()}##k。看起来你甚至还没有获得#b#t{1142033 + getPlayer().getVanquisherStage()}##k... 你想尝试一下#b#t{1142033 + getPlayer().getVanquisherStage()}##k吗？"))
                            {
                                getPlayer().setVanquisherStage(1);
                            }
                            else
                            {
                                await SayNext("如果你不想的话，没关系。");
                            }
                        }
                        else
                        {
                            if (getPlayer().getDojoStage() > 37)
                            {
                                await SayNext("你已经完成了所有勋章挑战。");
                            }
                            else if (getPlayer().getVanquisherKills() < 100)
                            {
                                await SayNext($"你仍然需要 #b{100 - getPlayer().getVanquisherKills()}#k 才能获得 #b#t{1142032 + getPlayer().getVanquisherStage()}##k。请再努力一点。提醒一下，只有在武陵道场由我们的大师召唤的怪物才算数。哦，还要确保你不是在打怪后就退出！#r如果你打败怪物后没有进入下一关，就不算胜利#k。");
                            }
                            else
                            {
                                await SayNext($"你已经获得了#b#t{1142032 + getPlayer().getVanquisherStage()}##k。");
                                await gainItem(1142033 + getPlayer().getVanquisherStage(), 1);
                                getPlayer().setVanquisherStage(getPlayer().getVanquisherStage() + 1);
                                getPlayer().setVanquisherKills(0);
                            }
                        }
                    }
                    else if (selectedMenu == 5)
                    {
                        await SayNext("我们的师傅是武陵最强大的人。他建造的地方叫做武陵道场，一栋有38层楼高的建筑！你可以在每一层上训练自己。当然，对于你这个级别的人来说，要到达顶层会很困难。");
                    }
                }
                else
                {
                    if (await AskYesNo("所以，你要放弃？你真的要走吗？"))
                    {
                        await warp(DojoFeature.ExitMap, 0);
                        await Pink("你能下定决心吗？");
                    }
                    else
                    {
                        await SayNext("别再改变主意了！很快，你会哭着求我回去的。");
                    }
                }
            }
            else
            {
                await SayOK("嘿！你在嘲笑我的主人吗？你以为你是谁来挑战他？这太可笑了！你至少应该是 #b25#k 级。");
            }
        }

        private async Task SendBeltRequirements(int belt, int oldbelt, bool haveOldbelt, int level, int points)
        {
            var beltReqStr = oldbelt != -1 ? " 你必须拥有 #i" + oldbelt + "#," : "";
            var pointsLeftStr = points - getPlayer().getDojoPoints() > 0 ? " 你还需要 #r" + (points - getPlayer().getDojoPoints()) + "#k 修炼点数" : "";
            var beltLeftStr = !haveOldbelt ? " 你必须确保所需腰带未装备且位于你的装备栏中" : "";
            var conjStr = pointsLeftStr.Length > 0 && beltLeftStr.Length > 0 ? " and" : "";

            await SayNext("为了获得 #i" + belt + "# #b#t" + belt + "##k," + beltReqStr + " 你至少需要达到等级 #b" + level + "#k 并且至少需要获得 #b" + points + " 修炼点数#k。\r\n\r\n如果你想获得这条腰带，" + beltLeftStr + conjStr + pointsLeftStr + "。");
        }


        // Npc: 2091006
        public async Task dojang_move()
        {
            var selection = await AskMenu("#e< 注意 >#n\r\n如果有人有勇气挑战武陵道场，请来武陵道场。 - 武公 -\r\n\r\n\r\n#b#L0#挑战武陵道场。#l\r\n#L1#更详细地阅读通知。#l");

            if (selection == 1)
            {
                await SaySpeech([
                    "#e< 注意：接受挑战！ >#n\r\n我的名字是武公，武陵道场的主人。自古以来，我一直在武陵修炼，直到我的技能达到了巅峰。从今天开始，我将接受所有对武陵道场的申请者。武陵道场的权利将只赋予最强大的人。\r\n如果有人希望向我学习，随时来挑战吧！如果有人希望挑战我，也欢迎。我会让你充分意识到自己的弱点。",
                    "PS：你可以独自挑战我。但如果你没有那种勇气，尽管叫上你所有的朋友。"
                    ]);
            }
            else
            {
                if (await AskYesNo("（当我把手放在公告板上时，一股神秘的能量开始包围着我。）\r\n\r\n你想去武陵道场吗？"))
                {
                    getPlayer().SaveLocation(SavedLocationType.MIRROR);
                    await warp(925020000, 4);
                }
                else
                {
                    await SayOK("当我把手从公告板上拿开时，覆盖在我身上的神秘能量也消失了。");
                }
            }
        }
    }
}
