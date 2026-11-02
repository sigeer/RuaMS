using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.DavyJohn
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        public async Task davyJohn_enter()
        {
            var em = GetEventManager(DavyJohnFeature.EventName);

            var option = await AskMenu($"#e#b<组队任务：海盗船>\r\n#k#n{em.Template.GetRequirementDescription(c)}\r\n\r\n救命啊！我的儿子被绑在可怕的#r老海盗#k手中。我需要你的帮助... 你能组建或加入一个队伍来救他吗？请让你的#b队长#k与我交谈或者组建一个队伍。",
                ["我想参加组队任务。", "我想了解更多详情。"]
            );
            switch (option)
            {
                case 0:
                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK("#e#b<组队任务：海盗船>#k#n\r\n在这个组队任务中，你的任务是逐步穿过船舱，与途中的所有海盗和坏蛋战斗。当你到达#r老海盗#k时，根据之前阶段打开的宝箱数量，boss会变得更加强大，所以要保持警惕。如果打开了这些宝箱，将会给你的船员带来许多额外的奖励，值得一试！祝你好运。");
                    break;
                default:
                    break;
            }
        }

        public async Task davy_clear()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return;
            }

            switch (getMapId())
            {
                case DavyJohnFeature.BossMap:
                    if (isEventLeader())
                    {
                        await SayOK("多亏了你们的努力，我得救了！谢谢，伙计们！");
                        await eim.clearPQ();
                    }
                    else
                    {
                        await SayOK("多亏了你们的努力，我得救了！谢谢你们！在我给你们奖励之前，让你们的队长先和我说话...");
                    }
                    break;
                default:
                    await ShowExitMenu();
                    break;
            }
        }

        public async Task davyJohn_play()
        {
            var eim = getEventInstance();
            if (eim == null || eim.EventName != DavyJohnFeature.EventName)
            {
                return;
            }

            switch (getMapId())
            {
                case DavyJohnFeature.Stage1Map:
                    await SayNext("我们现在要前往海盗船！要进入船内，我们必须消灭所有守卫的怪物。");
                    break;
                case DavyJohnFeature.Stage2Map:
                    await TalkStage2(eim);
                    break;
                case DavyJohnFeature.Stage2PassageMap:
                case DavyJohnFeature.Stage3PassageMap:
                    await SayNext("要攻打海盗船，我们必须先消灭守卫。");
                    break;
                case DavyJohnFeature.Stage2BossMap:
                    await TalkSealedBossStage(eim, DavyJohnFeature.Stage2AProperty);
                    break;
                case DavyJohnFeature.Stage3BossMap:
                    await TalkSealedBossStage(eim, DavyJohnFeature.Stage3AProperty);
                    break;
                case DavyJohnFeature.Stage2CheckMap:
                case DavyJohnFeature.Stage3CheckMap:
                    await SayNext("他们是效忠于老海盗的凯丁和克鲁。随你处置。");
                    break;
                case DavyJohnFeature.Stage4Map:
                    await SayNext("我们必须使用骷髅钥匙关上门！");
                    break;
                case DavyJohnFeature.BossMap:
                    if (getMap().countMonsters() == 0)
                    {
                        await SayNext("谢谢你救了我们的领袖！我们欠你一份人情。");
                    }
                    else
                    {
                        await SayNext("打败所有的怪物！甚至是老海盗的手下！");
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 第二阶段：依次上缴 20 个 4001120 / 4001121 / 4001122。
        /// </summary>
        async Task TalkStage2(AbstractEventInstanceManager eim)
        {
            var stage = eim.getProperty(DavyJohnFeature.Stage2Property);
            if (stage == "0")
            {
                if (haveItem(4001120, 20))
                {
                    await gainItem(4001120, -20);
                    eim.setProperty(DavyJohnFeature.Stage2Property, "1");

                    await SayNext("太棒了！现在去收集20个#i4001121# #t4001121#给我。");
                }
                else
                {
                    await SayNext("我们现在要前往海盗船！为了进入，我们必须证明自己是高贵的海盗。给我猎取20个#i4001120# #t4001120#。");
                }
            }
            else if (stage == "1")
            {
                if (haveItem(4001121, 20))
                {
                    await gainItem(4001121, -20);
                    eim.setProperty(DavyJohnFeature.Stage2Property, "2");

                    await SayNext("太棒了！现在去收集20个#i4001122# #t4001122#给我。");
                }
                else
                {
                    await SayNext("我们现在要前往海盗船！为了进入，我们必须证明自己是高贵的海盗。给我猎取20个#i4001121# #t4001121#。");
                }
            }
            else if (stage == "2")
            {
                if (haveItem(4001122, 20))
                {
                    await gainItem(4001122, -20);
                    eim.setProperty(DavyJohnFeature.Stage2Property, "3");
                    eim.ClearedMaps[getMapId()] = StageStatus.Completed;
                    await eim.showClearEffect(getMapId());

                    await SayNext("太棒了！现在让我们走吧。");
                }
                else
                {
                    await SayNext("我们现在要前往海盗船！为了进入，我们必须证明自己是高贵的海盗。给我猎取20个#i4001122# #t4001122#。");
                }
            }
            else
            {
                await SayNext("下一个阶段已经开放。前进！");
            }
        }

        async Task TalkSealedBossStage(AbstractEventInstanceManager eim, string propertyKey)
        {
            if (getMap().countMonsters() == 0)
            {
                await SayNext("老海盗的宝箱出现了！如果你碰巧有一把钥匙，就把它放在宝箱旁边，揭示宝藏。这肯定会让他很生气。");
                if (eim.getProperty(propertyKey) == "0")
                {
                    await getMap().setReactorState();
                    eim.setProperty(propertyKey, "1");
                }
            }
            else
            {
                await SayNext("这些风铃花藏匿起来了。我们必须解救它们。");
            }
        }

        async Task ShowExitMenu()
        {
            var option = await AskMenu("谢谢你救了我！我能帮你什么忙吗？\r\n#b#L0#带我离开这里。\r\n#L1#兑换海盗船长帽。");
            if (option == 0)
            {
                if (!canHold(DavyJohnFeature.ClearRewardItem, 1))
                {
                    await SayOK("请在杂项栏中腾出空间。");
                    return;
                }
                await gainItem(DavyJohnFeature.ClearRewardItem, 1);
                await warp(DavyJohnFeature.RecruitMapId, 0);
                return;
            }

            await ExchangeHat();
        }

        /// <summary>
        /// 海盗船长帽兑换链：1002572 → 1002573 → 1003267，每次消耗 20 个纪念道具。
        /// </summary>
        async Task ExchangeHat()
        {
            if (haveItem(1003267, 1))
            {
                await SayOK("你有最好的帽子。");
                return;
            }

            int sourceItem;
            int targetItem;
            if (haveItem(1002573, 1))
            {
                sourceItem = 1002573;
                targetItem = 1003267;
            }
            else if (haveItem(1002572, 1))
            {
                sourceItem = 1002572;
                targetItem = 1002573;
            }
            else
            {
                sourceItem = 0;
                targetItem = 1002572;
            }

            if (!haveItem(DavyJohnFeature.ClearRewardItem, 20))
            {
                await SayOK($"你需要20个#t{DavyJohnFeature.ClearRewardItem}#来获得下一个帽子。");
                return;
            }

            if (!canHold(targetItem, 1))
            {
                await SayOK("在收到帽子之前，请在您的装备物品栏中腾出空间。");
                return;
            }

            if (sourceItem != 0)
            {
                await gainItem(sourceItem, -1);
            }
            await gainItem(DavyJohnFeature.ClearRewardItem, -20);
            await gainItem(targetItem, 1);
            await SayOK("我已经给你帽子了。");
        }
    }
}
