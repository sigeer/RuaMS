using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;
using Application.Resources.Messages;
using Application.Shared.Constants.Map;
using Application.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Plugin.Script
{
    internal partial class NpcScript
    {
        // Npc: 9020000 
        public async Task party1_enter()
        {
            var em = GetEventManager<PQ_Kerning>(nameof(PQ_Kerning));
            var option = await SayOption(
        GetTalkMessage(nameof(ScriptTalk.KerningPQ_Description), em.GetRequirementDescription(getClient())), [
            GetTalkMessage(nameof(ScriptTalk.PartyQuest_Participate)), GetTalkMessage(nameof(ScriptTalk.PartyQuest_Intro))
            ]);

            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK(GetTalkMessage(nameof(ScriptTalk.KerningPQ_Intro)));
                    break;
                default:
                    break;
            }
        }


        // Npc: 9020001 
        public async Task party1_play()
        {
            var eim = getEventInstance();
            var curMap = getMapId();
            var stage = curMap - 103000800 + 1;
            if (eim.getProperty(stage.ToString() + "stageclear") != null)
            {
                if (stage < 5)
                {
                    await SayNext("请赶紧前往下一个阶段，传送门已经打开了！");
                }
                else
                {
                    await SayNext("太棒了！你通过了所有的关卡来到了这一点。这是为了你出色的表现而给予的小奖品。在接受之前，请确保你的使用和其他物品栏有空位可用。");
                }
            }
            else if (curMap == 103000800)
            {
                string[] stage1Questions = [
                    "收集与#b战士#n首次转职所需最低等级相同数量的#b通行证#n。",
                    "收集与#b战士#n首次转职所需最低力量（STR）相同数量的#b通行证#n。",
                    "收集与#b魔法师#n首次转职所需最低智力（INT）相同数量的#b通行证#n。",
                    "收集与#b弓箭手#n首次转职所需最低敏捷（DEX）相同数量的#b通行证#n。",
                    "收集与#b飞侠#n首次转职所需最低敏捷（DEX）相同数量的#b通行证#n。",
                    "收集与二次转职所需最低等级相同数量的#b通行证#n。",
                    "收集与#b魔法师#n首次转职所需最低等级相同数量的#b通行证#n。"
                ];
                int[] stage1Answers = [10, 35, 20, 25, 25, 30, 8];
                // stage 1
                if (isEventLeader())
                {
                    var numpasses = eim.getPlayerCount() - 1;     // minus leader

                    if (hasItem(4001008, numpasses))
                    {
                        clearStage(stage, eim, curMap);
                        eim.gridClear();
                        gainItem(4001008, -numpasses);

                        await SayNext("你收集了" + numpasses + "张通行证！恭喜你通过了这个关卡！我会制作一个传送你到下一个关卡的传送门。到那里有时间限制，所以请赶快。祝你们好运！");
                    }
                    else
                    {
                        await SayNext("对不起，但你的通行证数量不够。你需要给我正确数量的通行证；应该是你队伍成员数量减去队长的数量，在这种情况下需要 " 
                            + numpasses + " 张通行证来通过这个关卡。告诉你的队伍成员解决问题，收集通行证，然后交给你。");
                    }
                }
                else
                {
                    var data = eim.gridCheck(getPlayer());

                    if (data == 0)
                    {
                        await SayNext("谢谢你带来了#t4001007#。请把#t4001008#交给你的队长。");
                    }
                    else if (data == -1)
                    {
                        data = Random.Shared.Next(stage1Questions.Length) + 1;   //data will be counted from 1
                        eim.gridInsert(getPlayer(), data);

                        var question = stage1Questions[data - 1];
                        await SayNext(question);
                    }
                    else
                    {
                        var answer = stage1Answers[data - 1];

                        if (itemQuantity(4001007) == answer)
                        {
                            gainItem(4001007, -answer);
                            gainItem(4001008, 1);
                            eim.gridInsert(getPlayer(), 0);

                            await SayNext("这是正确的答案！为此，你刚刚获得了一个#b通行证#k。请将它交给队伍的队长。");
                        }
                        else
                        {
                            var question = stage1Questions[eim.gridCheck(getPlayer()) - 1];
                            await SayNext("对不起，但那不是正确的答案！\r\n" + question);
                        }
                    }
                }
            }
            else if (curMap == 103000801)
            {   // stage 2
                var stgProperty = "stg2Property";
                var stgAreas = getMap().getAreas().ToArray();

                var r = eim.getProperty(stgProperty);

                var nextStgId = 103000802;

                if (!eim.isEventLeader(getPlayer()))
                {
                    await SayOK("跟随你的队长给出的指示来完成这个阶段。");
                    return;
                }
                else if (r == null)
                {
                    await SayNext("嗨。欢迎来到第二阶段。在我旁边，你会看到一些绳子，在这些绳子中，有#b3个与传送你到下一阶段的传送门相连#k。你只需要让#b3名队伍成员找到正确的绳子然后挂在上面#k\r\n但是，如果你挂得太低，这不算作答案；请确保靠近绳子中间位置才算作正确答案。此外，你的队伍只允许有3名成员挂在绳子上，队伍的队长必须#b双击我来检查答案是否正确#k。现在，寻找正确的绳子挂上去吧！");
                    eim.setProperty(stgProperty, string.Join(',', Randomizer.Take(3, 4)));
                }
                else
                {
                    var stgCombos = r.Split(',').Select(int.Parse).ToArray();

                    var players = eim.getPlayers();
                    for (int i = 0; i < stgAreas.Length; i++)
                    {
                        if (stgCombos.Contains(i) ^ players.Any(chr => stgAreas[i].Contains(chr.getPosition())) )
                        {
                            eim.showWrongEffect();
                            return;
                        }
                    }
                    clearStage(stage, eim, curMap);
                    await SayNext("请赶紧前往下一个阶段，传送门已经打开了！");
                }
            }
            else if (curMap == 103000802)
            {
                var stgProperty = "stg3Property";
                var stgAreas = getMap().getAreas().ToArray();

                var r = eim.getProperty(stgProperty);

                var nextStgId = 103000803;

                if (!eim.isEventLeader(getPlayer()))
                {
                    await SayOK("跟随你的队长给出的指示来完成这个阶段。");
                }
                else if (r == null)
                {
                    await SayNext("嗨。欢迎来到第三阶段。在我旁边，你会看到一些平台。在这些平台中，#b3个与传送你到下一阶段的传送门相连#k。你只需要让#b3个队员找到正确的平台站上去#k\r\n但是，如果你站得太靠边，是不行的；请确保靠近平台的中间位置才算作正确答案。此外，你的队伍只允许有3名成员站在平台上。一旦他们在上面，队伍的队长必须#b双击我来检查答案是否正确#k。现在，寻找正确的平台吧！");
                    eim.setProperty(stgProperty, string.Join(',', Randomizer.Take(3, 5)));
                }
                else
                {
                    var stgCombos = r.Split(',').Select(int.Parse).ToArray();

                    var players = eim.getPlayers();
                    for (int i = 0; i < stgAreas.Length; i++)
                    {
                        if (stgCombos.Contains(i) ^ players.Any(chr => stgAreas[i].Contains(chr.getPosition())))
                        {
                            eim.showWrongEffect();
                            return;
                        }
                    }

                    clearStage(stage, eim, curMap);
                    await SayNext("请赶紧前往下一个阶段，传送门已经打开了！");
                }

                dispose();
            }
            else if (curMap == 103000803)
            {
                var stgProperty = "stg4Property";
                var stgAreas = getMap().getAreas().ToArray();

                var r = eim.getProperty(stgProperty);

                var nextStgId = 103000804;

                if (!eim.isEventLeader(getPlayer()))
                {
                    await SayOK("跟随你的队长给出的指示来完成这个阶段。");
                }
                else if (eim.getProperty(stgProperty) == null)
                {
                    eim.setProperty(stgProperty, string.Join(',', Randomizer.Take(3, 6)));
                    await SayNext("嗨。欢迎来到第四阶段。在我旁边，你会看到一些木桶。在这些木桶中，#b3个与传送你到下一阶段的传送门相连#k。你只需要让#b3个队员找到正确的木桶站上去#k\r\n但是，如果你站得太靠边，这不算作答案；请站在木桶的中间才算作正确答案。此外，你的队伍只允许有3名成员站在木桶上。队伍的队长必须#b双击我来检查答案是否正确#k。现在，寻找正确的木桶站上去吧！");
                }
                else
                {
                    var stgCombos = r.Split(',').Select(int.Parse).ToArray();

                    var players = eim.getPlayers();
                    for (int i = 0; i < stgAreas.Length; i++)
                    {
                        if (stgCombos.Contains(i) ^ players.Any(chr => stgAreas[i].Contains(chr.getPosition())))
                        {
                            eim.showWrongEffect();
                            return;
                        }
                    }

                    clearStage(stage, eim, curMap);
                    await SayNext("请赶紧前往下一个阶段，传送门已经打开了！");
                }
            }
            else if (curMap == 103000804)
            {
                if (eim.isEventLeader(getPlayer()))
                {
                    if (haveItem(4001008, 10))
                    {
                        await SayNext("这是通往最后的奖励阶段的传送门。这个阶段让你更容易地击败普通怪物。你将有一定的时间来尽可能多地狩猎，但你可以随时通过NPC中途离开这个阶段。再次恭喜你通过了所有的阶段。让你的队伍跟我对话，他们可以通过到达奖励阶段来领取奖品。保重……");
                        gainItem(4001008, -10);

                        clearStage(stage, eim, curMap);
                        eim.clearPQ();
                    }
                    else
                    {
                        await SayNext("你好。欢迎来到第五个也是最后一个阶段。在地图上四处走动，你会找到一些Boss怪物。打败它们，收集#b通行证#k，然后把它们交给我。一旦你获得了通行证，你的队伍队长会收集它们，然后在收集齐#b通行证#k后再把它们交给我。这些怪物可能对你来说很熟悉，但它们可能比你想象的要强大，所以请小心。祝你好运！");
                    }
                }
                else
                {
                    await SayNext("欢迎来到第五个也是最后一个阶段。在地图上四处走动，你将能够找到一些Boss怪物。打败它们，收集#b通行证#k，并将它们#b交给你的队长#k。完成后，回到我这里领取你的奖励。");
                }
            }
        }

        void clearStage(int stage, AbstractEventInstanceManager eim,int curMap)
        {
            eim.setProperty(stage + "stageclear", "true");
            eim.showClearEffect(true);

            eim.linkToNextStage(stage, "kpq", curMap);  //opens the portal to the next map
        }


        // Npc: 9020002 
        public async Task party1_out()
        {
            if (getMapId() == 103000890)
            {
                await SayNext("返回城市的路上，请沿着这条路走。");
                warp(MapId.KERNING_CITY);
            }
            else
            {
                var outText = "Once you leave the map, you'll have to restart the whole quest if you want to try it again.  Do you still want to leave this map?";
                if (getMapId() == 103000805)
                {
                    outText = "Are you ready to leave this map?";
                }
                if (await SayYesNo(outText))
                {
                    warp(103000890);
                }
            }
        }
    }
}
