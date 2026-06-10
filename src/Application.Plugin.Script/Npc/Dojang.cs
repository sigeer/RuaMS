using Application.Shared.Constants.Map;
using Application.Utility.Configs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script.Npc
{
    internal partial class NpcScript
    {
        // Npc: 2091005
        public async Task dojang_enter()
        {
            var belts = new[] { 1132000, 1132001, 1132002, 1132003, 1132004 };
            var belt_level = new[] { 25, 35, 45, 60, 75 };
            var belt_points = YamlConfig.config.server.USE_FAST_DOJO_UPGRADE 
                ? new[] { 10, 90, 200, 460, 850 } 
                : new[] { 200, 1800, 4000, 9200, 17000 };

            var belt_on_inventory = new bool[belts.Length];
            for (var i = 0; i < belts.Length; i++)
            {
                belt_on_inventory[i] = haveItemWithId(belts[i], true);
            }

            int selectedMenu = -1;
            int dojoWarp = 0;

            bool isRestingSpot(int mapId) => (mapId / 100 % 100) % 6 == 0 && mapId != 925020001;

            if (isRestingSpot(getMapId()))
            {
                string text = "我很惊讶你能走到这一步！但从这里开始不会那么容易。你仍然想要挑战吗？\r\n\r\n#b#L0#我想继续#l\r\n#L1#我想离开#l\r\n";
                if (!MapId.isPartyDojo(getMapId()))
                {
                    text += "#L2#我想记录我到目前为止的分数#l";
                }
                var selection = await AskMenu(text);
                selectedMenu = selection;

                if (selectedMenu == 0)
                {
                    var hasParty = getParty() != null;
                    bool firstEnter = false;
                    var avDojo = getClient().getChannelServer().lookupPartyDojo(getParty());
                    
                    if (avDojo < 0)
                    {
                        if (hasParty)
                        {
                            if (!isPartyLeader())
                            {
                                await SayOK("你不是队长！如果你想继续，让你们的队长来找我谈话。");
                                return;
                            }
                            if (!CheckTeamMemberLevelRange(35))
                            {
                                await SayOK("你的队伍成员等级范围太广，无法进入。请确保你的所有队伍成员等级相差不超过#r35级#k。");
                                return;
                            }
                        }
                        avDojo = getClient().getChannelServer().ingressDojo(hasParty, getParty(), (getMapId() / 100) % 100);
                        firstEnter = true;
                    }

                    if (avDojo < 0)
                    {
                        if (avDojo == -1)
                        {
                            await SayOK("所有道馆都已经被使用了。请等一会儿再试。");
                        }
                        else
                        {
                            await SayOK("你的队伍已经注册了道馆。等待注册时间结束后再次进入。");
                        }
                    }
                    else
                    {
                        var baseStg = hasParty ? 925030000 : 925020000;
                        var nextStg = (getMapId() + 100) / 100 % 100;
                        var dojoWarpMap = baseStg + (nextStg * 100) + avDojo;
                        
                        if (firstEnter)
                        {
                            getClient().getChannelServer().resetDojoMap(dojoWarpMap);
                        }
                        
                        getPlayer().setDojoStage(0);
                        if (!hasParty || !isLeader())
                        {
                            warp(dojoWarpMap, 0);
                        }
                        else
                        {
                            warpParty(dojoWarpMap, 0);
                        }
                    }
                }
                else if (selectedMenu == 1)
                {
                    if (await AskYesNo("所以，你要放弃了吗？你真的要离开吗？"))
                    {
                        warp(925020002, "st00");
                    }
                }
                else if (selectedMenu == 2)
                {
                    if (await AskYesNo("如果你记录下你的分数，下次可以从上次离开的地方开始。这不是很方便吗？你想记录下当前的分数吗？"))
                    {
                        if (getPlayer().getDojoStage() == getMapId() / 100 % 100)
                        {
                            await SayOK("你的分数已经被记录下来。下次挑战道馆时，你可以回到这个点。");
                        }
                        else
                        {
                            await SayNext("我已记录下你的分数。如果你告诉我下次你再次挑战时，你将能够从上次离开的地方开始。请注意，如果你选择继续挑战道馆，你的#r记录将被抹去#k，所以请谨慎选择。");
                            getPlayer().setDojoStage(getMapId() / 100 % 100);
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
                if (getMapId() == 925020001)
                {
                    var selection = await AskMenu("我的主人是武陵最强大的人，你想挑战他？好吧，但你以后会后悔的。\r\n\r\n#b#L0#我想独自挑战他。#l\r\n#L1#我想组队挑战他。#l\r\n\r\n#L2#我想获得一条腰带。#l\r\n#L3#我想重置我的训练点数。#l\r\n#L4#我想获得一枚勋章。#l\r\n#L5#什么是武陵道场？#l");
                    selectedMenu = selection;

                    if (selectedMenu == 0)
                    {
                        if (!getPlayer().hasEntered(getMapId()) && !getPlayer().FinishedDojoTutorial)
                        {
                            if (await AskYesNo("嘿！你！这是你第一次来吗？嗯，我的主人不会随便见任何人。他很忙。看你的样子，我觉得他不会理你。哈！但是，今天是你的幸运日……我告诉你吧，如果你能打败我，我就让你见我的主人。你觉得怎么样？"))
                            {
                                var avDojo = getClient().getChannelServer().ingressDojo(true, 0);
                                if (avDojo < 0)
                                {
                                    if (avDojo == -1)
                                    {
                                        await SayOK("所有道馆都已经被使用了。请稍等一会再尝试。");
                                    }
                                    else
                                    {
                                        await SayOK("你的队伍可能已经在使用道馆，或者你的队伍在道馆的预定时间还没有结束。请等待他们完成后再进入。");
                                    }
                                }
                                else
                                {
                                    getClient().getChannelServer().getMapFactory().getMap(925020010 + avDojo).resetMapObjects();
                                    resetDojoEnergy();
                                    warp(925020010 + avDojo, 0);
                                }
                            }
                            else
                            {
                                await SayNext("哈哈！你这样的心，想要给谁留下好印象呢？\r\n还是回到你应该去的地方吧！");
                            }
                        }
                        else if (getPlayer().getDojoStage() > 0)
                        {
                            dojoWarp = getPlayer().getDojoStage();
                            getPlayer().setDojoStage(0);
                            var stageWarp = (dojoWarp / 6) * 5;
                            
                            if (await AskYesNo($"上次你独自挑战时，你一直走到了第#b{stageWarp}#k关。我现在可以带你去那里。你想去那里吗？（选择#rNo#k来删除这个记录。）"))
                            {
                                var avDojo = getClient().getChannelServer().ingressDojo(false, dojoWarp);
                                if (avDojo < 0)
                                {
                                    if (avDojo == -1)
                                    {
                                        await SayOK("所有道馆都已经被使用了。请等一会儿再试。");
                                    }
                                    else
                                    {
                                        await SayOK("你的队伍可能已经在使用道馆，或者你的队伍在道馆的允许时间还没有结束。请等待他们完成后再进入。");
                                    }
                                    getPlayer().setDojoStage(dojoWarp);
                                }
                                else
                                {
                                    var warpDojoMap = 925020000 + (dojoWarp + 1) * 100 + avDojo;
                                    getClient().getChannelServer().resetDojoMap(warpDojoMap);
                                    resetDojoEnergy();
                                    warp(warpDojoMap, 0);
                                }
                            }
                        }
                        else
                        {
                            var avDojo = getClient().getChannelServer().ingressDojo(false, dojoWarp);
                            if (avDojo < 0)
                            {
                                if (avDojo == -1)
                                {
                                    await SayOK("所有道馆都已经被使用了。请等一会儿再试。");
                                }
                                else
                                {
                                    await SayOK("你的队伍可能已经在使用道馆，或者你的队伍在道馆的允许时间还没有结束。请等待他们完成后再进入。");
                                }
                                getPlayer().setDojoStage(dojoWarp);
                            }
                            else
                            {
                                var warpDojoMap = 925020000 + (dojoWarp + 1) * 100 + avDojo;
                                getClient().getChannelServer().resetDojoMap(warpDojoMap);
                                resetDojoEnergy();
                                warp(warpDojoMap, 0);
                            }
                        }
                    }
                    else if (selectedMenu == 1)
                    {
                        var party = getParty();
                        if (party == null)
                        {
                            await SayNext("你以为你要去哪里？你甚至不是队伍的领袖！去告诉你的队伍领袖来找我谈话。");
                            return;
                        }
                        if (party.getLeaderId() != getPlayer().getId())
                        {
                            await SayNext("你以为你要去哪里？你甚至不是队伍的领袖！去告诉你的队伍领袖来找我谈话。");
                            return;
                        }
                        if (!CheckTeamMemberLevelRange(30))
                        {
                            await SayNext("你的队伍成员等级范围太广，无法进入。请确保你的所有队伍成员等级相差不超过#r30级#k。");
                            return;
                        }

                        var avDojo = getClient().getChannelServer().ingressDojo(true, getParty(), 0);
                        if (avDojo < 0)
                        {
                            if (avDojo == -1)
                            {
                                await SayOK("所有道馆都已经被使用了。请稍等一会再尝试。");
                            }
                            else
                            {
                                await SayOK("你的队伍可能已经在使用道馆，或者你的队伍在道馆的预定时间还没有结束。请等待他们完成后再进入。");
                            }
                        }
                        else
                        {
                            getClient().getChannelServer().resetDojoMap(925030100 + avDojo);
                            resetPartyDojoEnergy();
                            warpParty(925030100 + avDojo);
                        }
                    }
                    else if (selectedMenu == 2)
                    {
                        if (!canHold(belts[0]))
                        {
                            await SayNext("在尝试领取腰带之前，请确保你的装备栏有足够的空间！");
                            return;
                        }

                        var selStr = $"你有 #b{getPlayer().getDojoPoints()}#k 训练点数。师傅更喜欢有天赋的人。如果你获得的点数超过平均水平，你可以根据你的分数获得一条腰带。\r\n";
                        for (var i = 0; i < belts.Length; i++)
                        {
                            if (belt_on_inventory[i])
                            {
                                selStr += $"\r\n#L{i}##i{belts[i]}# #t{belts[i]}# (Already on inventory)";
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
                                    gainItem(oldbelt, -1);
                                }
                                gainItem(belt, 1);
                                getPlayer().setDojoPoints(getPlayer().getDojoPoints() - points);
                                await SayNext($"这里有一个 #i{belt}# #b#t{belt}##k。你已经证明了自己的勇气，可以在道馆排名中晋升。干得好！");
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
                        if (await AskYesNo("你知道如果你重置你的训练点数，它会返回到0，对吧？不过，这并不总是件坏事。如果你在重置后能够重新开始赚取训练点数，你就可以再次获得腰带。你现在想要重置你的训练点数吗？"))
                        {
                            getPlayer().setDojoPoints(0);
                            await SayNext("好了！你所有的训练点数都已经重置了。把它看作一个新的开始，努力训练吧！");
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
                            if (await AskYesNo($"你还没有尝试过勋章吗？如果你在勇士部落道场中打败某种类型的怪物#b100次#k，你就可以获得一个称号，叫做#b#t{1142033 + getPlayer().getVanquisherStage()}##k。看起来你甚至还没有获得#b#t{1142033 + getPlayer().getVanquisherStage()}##k... 你想尝试一下#b#t{1142033 +getPlayer().getVanquisherStage()}##k吗？"))
                            {
                                if (getPlayer().getDojoStage() > 37)
                                {
                                    await SayNext("你已经完成了所有勋章挑战。");
                                }
                                else if (getPlayer().getVanquisherKills() < 100 && getPlayer().getVanquisherStage() > 0)
                                {
                                    await SayNext($"你仍然需要 #b{100 - getPlayer().getVanquisherKills()}#k 才能获得 #b#t{1142032 + getPlayer().getVanquisherStage()}##k。请再努力一点。提醒一下，只有在武陵道场由我们的大师召唤的怪物才算数。哦，还要确保你不是在打怪后就退出！#r如果你打败怪物后没有进入下一关，就不算胜利#k。");
                                }
                                else if (getPlayer().getVanquisherStage() <= 0)
                                {
                                    getPlayer().setVanquisherStage(1);
                                }
                                else
                                {
                                    await SayNext($"你已经获得了#b#t{1142032 + getPlayer().getVanquisherStage()}##k。");
                                    gainItem(1142033 + getPlayer().getVanquisherStage(), 1);
                                    getPlayer().setVanquisherStage(getPlayer().getVanquisherStage() + 1);
                                    getPlayer().setVanquisherKills(0);
                                }
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
                                gainItem(1142033 + getPlayer().getVanquisherStage(), 1);
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
                    if (await AskYesNo("什么，你要放弃了吗？你只需要达到下一个级别！你真的想要放弃并离开吗？"))
                    {
                        var dojoMapId = getMapId();
                        warp(925020002, 0);
                        Pink("Can you make up your mind please?");
                        getClient().getChannelServer().freeDojoSectionIfEmpty(dojoMapId);
                    }
                    else
                    {
                        await SayNext("别再改变主意了！很快，你会哭着求我回去的。");
                    }
                }
            }
            else
            {
                await SayOK("嘿！你在嘲笑我的主人吗？你以为你是谁来挑战他？这太可笑了！你至少应该是 #b25#k 级别。");
            }
        }

        private async Task SendBeltRequirements(int belt, int oldbelt, bool haveOldbelt, int level, int points)
        {
            var beltReqStr = oldbelt != -1 ? " you must have the #i" + oldbelt + "# belt in your inventory," : "";
            var pointsLeftStr = points - getPlayer().getDojoPoints() > 0 ? " you need #r" + (points - getPlayer().getDojoPoints()) + "#k more training points" : "";
            var beltLeftStr = !haveOldbelt ? " you must have the needed belt unequipped and available in your EQP inventory" : "";
            var conjStr = pointsLeftStr.Length > 0 && beltLeftStr.Length > 0 ? " and" : "";

            await SayNext("为了获得 #i" + belt + "# #b#t" + belt + "##k," + beltReqStr + " 你至少需要达到等级 #b" + level + "#k 并且至少需要获得 #b" + points + " 训练点数#k。\r\n\r\n如果你想获得这条腰带，" + beltLeftStr + conjStr + pointsLeftStr + "。");
        }


        // Npc: 2091006
        public async Task dojang_move()
        {
            var selection = await AskMenu("#e< 注意 >#n\r\n如果有人有勇气挑战武陵道场，请来武陵道场。 - 武功 -\r\n\r\n\r\n#b#L0#挑战武陵道场。#l\r\n#L1#更详细地阅读通知。#l");
            
            if (selection == 1)
            {
                await SayNext("#e< 注意：接受挑战！ >#n\r\n我的名字是慕容，慕龙道场的主人。自古以来，我一直在慕龙修炼，直到我的技能达到了巅峰。从今天开始，我将接受所有对慕龙道场的申请者。慕龙道场的权利将只赋予最强大的人。\r\n如果有人希望向我学习，随时来挑战吧！如果有人希望挑战我，也欢迎。我会让你充分意识到自己的弱点。");
                await SayNext("PS：你可以独自挑战我。但如果你没有那种勇气，尽管叫上你所有的朋友。");
            }
            else
            {
                if (await AskYesNo("（当我把手放在公告板上时，一股神秘的能量开始包围着我。）\r\n\r\n你想去勇士部落道场吗？"))
                {
                    getPlayer().saveLocation("MIRROR");
                    warp(925020000, 4);
                }
                else
                {
                    await SayOK("当我把手从公告板上拿开时，覆盖在我身上的神秘能量也消失了。");
                }
            }
        }
    }
}
