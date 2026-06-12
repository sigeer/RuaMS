using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;

namespace Application.Plugin.Script.Events
{
    internal partial class NpcScript
    {
        // Npc: 2040034 
        public async Task party2_enter()
        {
            var em = GetEventManager(nameof(PQ_Ludi));

            var option = await AskMenu($"#e#b<组队任务：时空裂缝>\r\n#k#n{em.GetRequirementDescription(c)}\r\n\r\n由于上方有极其危险的生物，你无法再往上走。你想要和队友合作完成任务吗？如果是，请让你的#b队长#k和我交谈。",
                ["我想参加组队任务。", "我想了解更多详情。"]
            );
            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK("#e#b<组队任务：时空裂缝>#k#n\r\n#b#m220000000#!#k出现了时空裂缝！我们迫切需要勇敢的冒险家来击败入侵的怪物。请和一些可靠的盟友组队，拯救#m220000000#! 你必须通过击败怪物和解决谜题来通过各个阶段，最终击败#r#o9300012##k。");
                    break;
                default:
                    break;
            }

        }


        // Npc: 2040035, 2040036, 2040037, 2040038, 2040039, 2040040, 2040041, 2040042, 2040043, 2040044, 2040045 
        public async Task party2_play()
        {
            var eim = GetEventInstanceTrust();
            var stage = (int)Math.Floor((getMapId() - 922010100) / 100.0) + 1; ;
            var currentStatus = eim.ClearedMaps.GetValueOrDefault(getMapId(), StageStatus.NotStarted);
            switch (getNpc())
            {
                case 2040035:
                    await SayNext("恭喜你成功封印了次元裂缝！为了表彰你的辛勤工作，我有一份礼物送给你！拿去吧，这是你的奖品。");
                    if (eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        WarpOut();
                    }
                    else
                    {
                        await SayNext("看起来你的#r装备#k、#r消耗#k或#r其他#k背包中都没有空位。请腾出一些空间，然后再试一次。");
                    }
                    break;
                case 2040036:
                case 2040037:
                case 2040038:
                case 2040039:
                case 2040040:
                case 2040042:
                    Dictionary<int, (int Count, string Description)> required = new()
                    {
                        {2040036, (25,$"嗨。欢迎来到 #b第{stage}关#k。收集地图上散落的 25 个 #t4001022#，然后和我交谈。") },
                        {2040037,(15,$"嗨。欢迎来到 #b第{stage}关#k。收集地图上散落的 15 个 #t4001022#，然后和我交谈。") },
                        {2040038,(32,$"嗨。欢迎来到 #b第{stage}关#k。收集地图上散落的 32 个 #t4001022#，然后和我交谈。") },
                        {2040039,(6,$"嗨。欢迎来到 #b第{stage}关#k。在这个阶段，有几种生物隐藏在这座塔的内部阴影中。其中一些无法通过物理手段击败，因此需要使用魔法攻击来完成任务，而其他一些则相反。这次给我带来6个#t4001022#。") },
                        {2040040,(24,$"嗨。欢迎来到 #b第{stage}关#k。在这个阶段，每个人都必须合作。这里有6个传送门。一个被无法战胜的怪物守卫着，一个非常高。我希望你和你的队伍分别进入每一个，并打破里面的箱子。带回掉落物品——应该有24个。") },
                        {2040042,(3,$"嗨。欢迎来到 #b第{stage}关#k。需要靠远程攻击单位杀死三只老鼠，这将触发一些东西。接下来就是你自己去发现了！给我3张通行证！") },
                    };
                    var data = required[getNpc()];
                    if (isEventLeader())
                    {
                        if (!eim.ClearedMaps.TryGetValue(getMapId(), out var s) || s == StageStatus.NotStarted)
                        {           // preamble
                            await SayOK(data.Description);
                            eim.ClearedMaps[getMapId()] = StageStatus.Started;
                        }
                        else
                        {       // check stage completion
                            if (haveItem(4001022, data.Count))
                            {
                                await SayOK($"干得好！你已经收集了所有{data.Count}个#b#t4001022#。#k");
                                gainItem(4001022, -data.Count);

                                ClearLudiPQStage(eim, getMapId());
                            }
                            else
                            {
                                await SayNext($"抱歉，你没有{data.Count}个#b#t4001022#。#k");
                            }
                        }
                    }
                    else
                    {
                        await SayNext("请让你的#b队长#k来找我。");
                    }
                    break;
                case 2040041:
                    await SayOK("尝试找到正确的数字组合来达到顶部。");
                    break;
                case 2040043:
                    if (!eim.isEventLeader(getPlayer()))
                    {
                        await SayOK("跟随你的队长给出的指示来完成这个阶段。");
                        return;
                    }

                    if (currentStatus == StageStatus.NotStarted)
                    {
                        await SayNext($"嗨。欢迎来到 #b第{stage}关#k。在这个阶段，让你的队伍中的5名成员站在那些箱子上，以形成正确的组合来解锁下一个阶段。只有一个玩家应该留在所需的箱子上以确定组合。");
                        eim.ClearedMaps[getMapId()] = StageStatus.Started;
                        return;
                    }

                    var stgAreas = getMap().getAreas();

                    var em = eim.EventManager.Template as PQ_Ludi;

                    List<int> passedIndex = [];
                    var players = eim.getPlayers();
                    for (int i = 0; i < stgAreas.Count; i++)
                    {
                        if (players.Any(chr => stgAreas[i].Contains(chr.getPosition())))
                        {
                            passedIndex.Add(i);
                        }
                    }

                    if (passedIndex.Count != 5)
                    {
                        await SayNext("看起来你还没有找到5个箱子。请考虑不同的箱子组合。只允许站在箱子上的数量为5个，如果你移动箱子可能不算作答案，请记住这一点。继续加油！");
                        return;
                    }

                    var stgCombos = em.GetStage(eim, getMap());
                    if (passedIndex.SequenceEqual(stgCombos))
                    {
                        ClearLudiPQStage(eim, getMapId());
                        await SayNext("快点，去下一个阶段，传送门已经打开了！");
                    }
                    else
                    {
                        eim.showWrongEffect();
                    }
                    break;
                case 2040044:
                    if (!eim.isEventLeader(getPlayer()))
                    {
                        await SayOK("跟随你的队长给出的指示来完成这个阶段。");
                        return;
                    }

                    if (currentStatus == StageStatus.NotStarted)
                    {
                        await SayNext("嗨。欢迎来到#bBOSS阶段#k。在那个平台上杀死老鼠，揭示出阿利莎尔，并打败他！");
                        eim.ClearedMaps[getMapId()] = StageStatus.Started;
                        return;
                    }

                    if (haveItem(4001023, 1))
                    {
                        gainItem(4001023, -1);

                        ClearLudiPQStage(eim, getMapId());

                        eim.clearPQ();
                    }
                    else
                    {
                        await SayNext("请击败阿利莎并把他的#b#t4001023#带给我。#k");
                    }

                    break;
                case 2040045:
                    if (await AskYesNo("你想离开奖励阶段吗？"))
                    {
                        // warp(922011100, "st00");
                        WarpOut();
                    }
                    break;
                default:
                    break;
            }
            // TODO

        }

        static void ClearLudiPQStage(AbstractEventInstanceManager eim, int curMap)
        {
            eim.ClearedMaps[curMap] = StageStatus.Completed;
            eim.showClearEffect(true);

            eim.GiveStageClearRewardAll(curMap);
        }

        // Npc: 2040047 
        public async Task party2_out()
        {
            if (getMapId() == 922010000)
            {
                await SayNext("要离开这里，请沿着这条路走。");
                warp(221024500);
            }
            else
            {
                var eim = GetEventInstanceTrust();
                var outText = eim?.isEventCleared() ?? true
                    ? "你准备好离开这张地图了吗？"
                    : "一旦离开地图，若想再次尝试，必须重新开始整个任务。你确定要离开这张地图吗？";
                if (await AskYesNo(outText))
                {
                    WarpOut();
                }
            }
        }

    }
}
