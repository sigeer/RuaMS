using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;
using Application.Resources.Messages;
using Application.Shared.Constants.Map;

namespace Application.Plugin.Script.Features.KerningPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        /// <summary>
        /// 入口 NPC：创建或加入副本。
        /// </summary>
        public async Task party1_enter()
        {
            var em = GetEventManager(KerningPQFeature.EventName);
            var option = await AskMenu(GetTalkMessage(nameof(ScriptTalk.KerningPQ_Description), em.Template.GetRequirementDescription(getClient())),
                [GetTalkMessage(nameof(ScriptTalk.PartyQuest_Participate)), GetTalkMessage(nameof(ScriptTalk.PartyQuest_Intro))
            ]);

            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK(GetTalkMessage(nameof(ScriptTalk.KerningPQ_Intro)));
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 关卡 NPC：第一阶段交通行证，第 2~4 阶段核对位置，第五阶段收尾。
        /// </summary>
        public async Task party1_play()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != KerningPQFeature.EventName || eim.EventManager.Template is not Event em)
            {
                return;
            }

            var curMap = getMapId();
            var stage = curMap - KerningPQFeature.Stage1Map + 1;

            if (eim.ClearedMaps.TryGetValue(curMap, out var s) && s == StageStatus.Completed)
            {
                if (stage < 5)
                {
                    await SayNext("请赶紧前往下一个阶段，传送门已经打开了！");
                }
                else
                {
                    await SayNext("太棒了！你通过了所有的关卡来到了这一点。这是为了你出色的表现而给予的小奖品。在接受之前，请确保你的使用和其他物品栏有空位可用。");

                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(KerningPQFeature.BonusMap, "st00");
                    }
                    else
                    {
                        await SayNext("背包已满，或者你已经领取了奖励！");
                    }
                }
                return;
            }

            if (curMap == KerningPQFeature.Stage1Map)
            {
                await PlayStage1(eim, em);
            }
            else if (curMap == KerningPQFeature.Stage2Map || curMap == KerningPQFeature.Stage3Map)
            {
                await CheckStage2_3_4(eim, em, curMap);
            }
            else if (curMap == KerningPQFeature.Stage4Map)
            {
                await PlayStage4(eim);
            }
        }

        async Task PlayStage1(AbstractEventInstanceManager eim, Event em)
        {
            var stage1 = em.GetStage1();
            if (isEventLeader())
            {
                var numpasses = eim.getPlayerCount() - 1;     // minus leader

                if (hasItem(KerningPQFeature.PassItem, numpasses))
                {
                    await ClearStage(eim, KerningPQFeature.Stage1Map);
                    eim.gridClear();
                    await gainItem(KerningPQFeature.PassItem, -numpasses);

                    await SayNext("你收集了" + numpasses + "张通行证！恭喜你通过了这个关卡！我会制作一个传送你到下一个关卡的传送门。到那里有时间限制，所以请赶快。祝你们好运！");
                }
                else
                {
                    await SayNext("对不起，但你的通行证数量不够。你需要给我正确数量的通行证；应该是你队伍成员数量减去队长的数量，在这种情况下需要 "
                        + numpasses + " 张通行证来通过这个关卡。告诉你的队伍成员解决问题，收集通行证，然后交给你。");
                }
                return;
            }

            var data = eim.gridCheck(getPlayer());
            if (data == 0)
            {
                await SayNext("谢谢你带来了#t4001007#。请把#t4001008#交给你的队长。");
            }
            else if (data == -1)
            {
                data = Random.Shared.Next(stage1.Quest.Length) + 1;   // 从 1 开始计数
                eim.gridInsert(getPlayer(), data);
                await SayNext(stage1.Quest[data - 1]);
            }
            else
            {
                var answer = stage1.Answer[data - 1];
                if (itemQuantity(KerningPQFeature.TicketItem) == answer)
                {
                    await gainItem(KerningPQFeature.TicketItem, -answer);
                    await gainItem(KerningPQFeature.PassItem, 1);
                    eim.gridInsert(getPlayer(), 0);

                    await SayNext("这是正确的答案！为此，你刚刚获得了一个#b通行证#k。请将它交给队伍的队长。");
                }
                else
                {
                    await SayNext("对不起，但那不是正确的答案！\r\n" + stage1.Quest[eim.gridCheck(getPlayer()) - 1]);
                }
            }
        }

        async Task PlayStage4(AbstractEventInstanceManager eim)
        {
            if (!eim.isEventLeader(getPlayer()))
            {
                await SayNext("欢迎来到第五个也是最后一个阶段。在地图上四处走动，你将能够找到一些Boss怪物。打败它们，收集#b通行证#k，并将它们#b交给你的队长#k。完成后，回到我这里领取你的奖励。");
                return;
            }

            if (!haveItem(KerningPQFeature.PassItem, KerningPQFeature.BonusPassCount))
            {
                await SayNext("你好。欢迎来到第五个也是最后一个阶段。在地图上四处走动，你会找到一些Boss怪物。打败它们，收集#b通行证#k，然后把它们交给我。一旦你获得了通行证，你的队伍队长会收集它们，然后在收集齐#b通行证#k后再把它们交给我。这些怪物可能对你来说很熟悉，但它们可能比你想象的要强大，所以请小心。祝你好运！");
                return;
            }

            await SayNext("这是通往最后的奖励阶段的传送门。这个阶段让你更容易地击败普通怪物。你将有一定的时间来尽可能多地狩猎，但你可以随时通过NPC中途离开这个阶段。再次恭喜你通过了所有的阶段。让你的队伍跟我对话，他们可以通过到达奖励阶段来领取奖品。保重……");
            await gainItem(KerningPQFeature.PassItem, -KerningPQFeature.BonusPassCount);

            await ClearStage(eim, KerningPQFeature.Stage4Map);
            await eim.clearPQ();
        }

        async Task CheckStage2_3_4(AbstractEventInstanceManager eim, Event em, int mapId)
        {
            var stgAreas = getMap().getAreas().ToArray();

            if (!eim.isEventLeader(getPlayer()))
            {
                await SayOK("跟随你的队长给出的指示来完成这个阶段。");
                return;
            }

            HashSet<int> passedIndex = [];
            var players = eim.getPlayers();
            for (int i = 0; i < stgAreas.Length; i++)
            {
                var area = stgAreas[i];
                if (players.Any(chr => area.Contains(chr.getPosition())))
                {
                    passedIndex.Add(i);
                }
            }

            if (passedIndex.Count != 3)
            {
                var msg = mapId switch
                {
                    KerningPQFeature.Stage2Map => "嗨。欢迎来到第二阶段。在我旁边，你会看到一些绳子，在这些绳子中，有#b3个与传送你到下一阶段的传送门相连#k。你只需要让#b3名队伍找到正确的绳子然后挂在上面#k\r\n但是，如果你挂得太低，这不算作答案；请确保靠近绳子中间位置才算作正确答案。此外，你的队伍只允许有3名成员挂在绳子上，队伍的队长必须#b双击我来检查答案是否正确#k。现在，寻找正确的绳子挂上去吧！",
                    KerningPQFeature.Stage3Map => "嗨。欢迎来到第三阶段。在我旁边，你会看到一些平台。在这些平台中，有#b3个与传送你到下一阶段的传送门相连#k。你只需要让#b3个队员找到正确的平台站上去#k\r\n但是，如果你站得太靠边，是不行的；请确保靠近平台的中间位置才算作正确答案。此外，你的队伍只允许有3名成员站在平台上。一旦他们在上面，队伍的队长必须#b双击我来检查答案是否正确#k。现在，寻找正确的平台吧！",
                    _ => "嗨。欢迎来到第四阶段。在我旁边，你会看到一些木桶。在这些木桶中，有#b3个与传送你到下一阶段的传送门相连#k。你只需要让#b3个队员找到正确的木桶站上去#k\r\n但是，如果你站得太靠边，这不算作答案；请站在木桶的中间才算作正确答案。此外，你的队伍只允许有3名成员站在木桶上。队伍的队长必须#b双击我来检查答案是否正确#k。现在，寻找正确的木桶站上去吧！",
                };
                await SayNext(msg);
                return;
            }

            var stgCombos = em.GetStage(eim, getMap());
            if (passedIndex.SetEquals(stgCombos))
            {
                await ClearStage(eim, getMapId());
                await SayNext("请赶紧前往下一个阶段，传送门已经打开了！");
            }
            else
            {
                await eim.showWrongEffect();
            }
        }

        /// <summary>
        /// 出口 NPC：在奖励地图直接返回城镇，其他地图询问后离开。
        /// </summary>
        public async Task party1_out()
        {
            if (getMapId() == KerningPQFeature.ExitMapId)
            {
                await SayNext("要返回城市，请沿着这条路走。");
                await warp(MapId.KERNING_CITY);
                return;
            }

            var eim = getEventInstance();
            var outText = eim?.isEventCleared() ?? true
                ? "你准备好离开这张地图了吗？"
                : "一旦离开地图，若想再次尝试，必须重新开始整个任务。你确定要离开这张地图吗？";
            if (await AskYesNo(outText))
            {
                await WarpOut();
            }
        }

        static async Task ClearStage(AbstractEventInstanceManager eim, int curMap)
        {
            eim.ClearedMaps[curMap] = StageStatus.Completed;
            await eim.showClearEffect(true);

            await eim.GiveStageClearRewardAll(curMap);
        }
    }
}
