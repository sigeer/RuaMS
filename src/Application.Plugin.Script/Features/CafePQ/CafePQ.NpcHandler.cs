using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.CafePQ
{
    /// <summary>
    /// 咖啡厅组队任务的场内 NPC（1052013）：招募图报名、副本内收取优惠券/发奖。
    /// </summary>
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, Application.Core.Game.Life.NPC? npcObject)
            : base(c, npc, npcObject)
        {
        }
        // Npc: 1052012 
        public async Task go_pc()
        {
            if (await AskYesNo("那么，你打算使用网吧吗？在那里使用空间是需要付费的，费用是#b5,000金币#k。你要进入网吧吗？"))
            {
                if (getMeso() < 5000)
                {
                    await SayOK("哦，你没有钱，对吧？抱歉，我不能让你进去。");
                }
                else
                {
                    await gainMeso(-5000);
                    await warp(193000000, "out00");
                }
            }
        }

        // Npc: 1052013 
        public async Task go_pcmap()
        {
            if (getMapId() != 193000000)
            {
                var eim = getEventInstance();

                if (eim != null)
                {
                    if (!eim.isEventCleared())
                    {
                        var couponsNeeded = eim.getIntProperty("couponsNeeded");

                        if (isEventLeader())
                        {
                            if (haveItem(4001007, couponsNeeded))
                            {
                                await SayNext("你的团队收集了所有需要的优惠券，干得好！");
                                await gainItem(4001007, couponsNeeded);
                                await eim.clearPQ();
                            }
                            else
                            {
                                if (await AskYesNo($"你的团队必须收集#r{couponsNeeded}#k张优惠券才能完成这个副本。当你手头有足够的数量时，和我交谈...或者你想#b现在退出#k？请注意，如果你现在退出，#r你的团队也将被迫退出#k。"))
                                {
                                    await warp(193000000);
                                }
                            }
                        }
                        else
                        {
                            if (await AskYesNo($"你的队伍必须收集#r{couponsNeeded}#k张优惠券才能完成这个副本。让你的队长带着正确数量的优惠券来找我谈谈……或者你想要#b现在退出#k吗？请注意，如果你现在退出，你的队伍#r可能会人手不足#k以继续进行这个副本。"))
                            {
                                await warp(193000000);
                            }
                        }
                    }
                    else
                    {
                        if (await eim.GiveClearReward(getPlayer()) != ClaimRewardResult.Success)
                        {
                            await SayOK("请在您的ETC库存中腾出一个空间来接收奖品。");
                        }
                        else
                        {
                            await warp(193000000);
                        }
                    }
                }
            }
            else
            {
                string[] levels = ["#m190000000#", "#m191000000#", "#m192000000#", "#m195000000#", "#m196000000#", "#m197000000#"];
                var area = await AskMenu("高级道路是一个聚集了各种类型怪物，刷经验、刷装备的理想场所。#p1052014# 选择您想要面对的区域：", levels.Select(x => x));

                var pqArea = area + 1;
                var em = GetEventManager(CafePQFeature.EventNames[pqArea - 1]);
                var pqOption = await AskMenu($"#e#b<组队任务：高级之路 - {levels[area]}>\r\n#k#n" + em.Template.GetRequirementDescription(c) + "\r\n\r\n#p1052014# 的操作方式与普通的不同。它们不使用金币或扭蛋券，而是使用 #r橡皮擦#k，可以通过完成高级之路上的任务获得。要前往那里，你必须找到队友并参加一个组队任务。当组队并准备好后，让你的 #b队长#k 与我交谈。", [
                    "我想参加组队任务。",
                    "我想了解更多详情。"
                ]);

                if (pqOption == 0)
                {
                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK("#e#b<组队任务：高级之路>#k#n\r\n在前方的地图上，你将面对许多普通等级的怪物。击败它们，收集所有需要的优惠券交给我。然后所有成员将获得一个橡皮擦，与所面对的等级相对应。在机器上插入#b多个相同的橡皮擦或多种不同的橡皮擦#k，以获得更大奖品的机会。");
                }
            }
        }

        // Npc: 1052015 
        public async Task mouse()
        {
            int[] itemSet_lv6 = [1442046, 1432018, 1102146, 1102145, 2022094, 2022544, 2022123, 2022310, 2040727, 2041058, 2040817, 4000030, 4003005, 4003000, 4011007, 4021009, 4011008, 3010098];
            int[] itemQty_lv6 = [1, 1, 1, 1, 35, 15, 20, 20, 1, 1, 1, 30, 30, 30, 1, 1, 3, 1];
            int[] itemSet_lv5 = [1382015, 1382016, 1442044, 1382035, 2022310, 2022068, 2022069, 2022190, 2022047, 2040727, 2040924, 2040501, 4000030, 4003005, 4003000, 4011003, 4011006, 4021004, 3010099];
            int[] itemQty_lv5 = [1, 1, 1, 1, 20, 40, 40, 30, 30, 1, 1, 1, 20, 20, 25, 3, 2, 3, 1];
            int[] itemSet_lv4 = [1332029, 1472027, 1462032, 1492019, 2022045, 2022048, 2022094, 2022123, 2022058, 2041304, 2041019, 2040826, 2040758, 4000030, 4003005, 4003000, 4010007, 4011003, 4021003, 3010016, 3010017];
            int[] itemQty_lv4 = [1, 1, 1, 1, 45, 40, 25, 20, 60, 1, 1, 1, 1, 10, 10, 20, 5, 1, 1, 1, 1];
            int[] itemSet_lv3 = [1302058, 1372008, 1422030, 1422031, 1022082, 2022279, 2022120, 2001001, 2001002, 2022071, 2022189, 2040914, 2041001, 2041041, 2041308, 4031203, 4000030, 4003005, 4003000, 4010004, 4010006, 4020000, 4020006, 3010002, 3010003];
            int[] itemQty_lv3 = [1, 1, 1, 1, 1, 65, 40, 40, 40, 25, 25, 1, 1, 1, 1, 10, 7, 10, 8, 5, 5, 5, 5, 1, 1];
            int[] itemSet_lv2 = [1022073, 1012098, 1012101, 1012102, 1012103, 2022055, 2022056, 2022103, 2020029, 2020032, 2020031, 2022191, 2022016, 2043300, 2043110, 2043800, 2041001, 2040903, 4031203, 4000021, 4003005, 4003000, 4003001, 4010000, 4010001, 4010003, 4010004, 4020004, 3010004, 3010005];
            int[] itemQty_lv2 = [1, 1, 1, 1, 1, 40, 40, 40, 40, 60, 60, 60, 60, 1, 1, 1, 1, 1, 4, 6, 7, 5, 2, 4, 4, 3, 3, 4, 1, 1];
            int[] itemSet_lv1 = [1302021, 1302024, 1302033, 1082150, 1002419, 2022053, 2022054, 2020032, 2022057, 2022096, 2022097, 2022192, 2020030, 2010005, 2022041, 2030000, 2040100, 2040004, 2040207, 2048004, 4031203, 4000021, 4003005, 4003000, 4003001, 4010000, 4010001, 4010002, 4010005, 4020004];
            int[] itemQty_lv1 = [1, 1, 1, 1, 1, 20, 20, 20, 20, 20, 25, 25, 25, 50, 50, 12, 1, 1, 1, 1, 3, 4, 2, 2, 1, 2, 2, 2, 2, 2];

            string[] levels = ["第1层", "第2层", "第3层", "第4层", "第5层", "第6层"];

            var selection = await AskMenu("这里是#b网咖派对任务#k，奖励玩家与票类似的#b怪物橡皮#k，可以用在自动售货机上兑换奖品。通过选择不同的#r层级#k，进一步获得更好的奖品。\r\n\r\n这里描述了每个级别可能的奖励:\r\n",
                levels.Select(x => x));

            int[] lvTarget, lvQty;
            switch (selection)
            {
                case 0: lvTarget = itemSet_lv1; lvQty = itemQty_lv1; break;
                case 1: lvTarget = itemSet_lv2; lvQty = itemQty_lv2; break;
                case 2: lvTarget = itemSet_lv3; lvQty = itemQty_lv3; break;
                case 3: lvTarget = itemSet_lv4; lvQty = itemQty_lv4; break;
                case 4: lvTarget = itemSet_lv5; lvQty = itemQty_lv5; break;
                default: lvTarget = itemSet_lv6; lvQty = itemQty_lv6; break;
            }

            var sendStr = $"下列物品将在#b{levels[selection]}#k层获得:\r\n\r\n";
            for (var i = 0; i < lvTarget.Length; i++)
            {
                sendStr += $"#i{lvTarget[i]}# #t{lvTarget[i]}#";
                if (lvQty[i] > 1)
                {
                    sendStr += $" ({lvQty[i]})";
                }
                sendStr += "\r\n";
            }

            await SayNext(sendStr);
        }
    }
}
