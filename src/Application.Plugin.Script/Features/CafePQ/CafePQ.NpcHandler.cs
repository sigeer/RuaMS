using Application.Core.Client;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

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
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.CafePQ_GoingUse)))
            {
                if (getMeso() < 5000)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CafePQ_DontHave));
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
                                await SayNext(this.GetTalkMessage(ScriptMessage.CafePQ_TeamCollected));
                                await gainItem(4001007, couponsNeeded);
                                await eim.clearPQ();
                            }
                            else
                            {
                                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.CafePQ_NeedCouponsLeader, couponsNeeded)))
                                {
                                    await warp(193000000);
                                }
                            }
                        }
                        else
                        {
                            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.CafePQ_NeedCouponsMember, couponsNeeded)))
                            {
                                await warp(193000000);
                            }
                        }
                    }
                    else
                    {
                        if (await eim.GiveClearReward(getPlayer()) != ClaimRewardResult.Success)
                        {
                            await SayOK(this.GetTalkMessage(ScriptMessage.CafePQ_PleaseMake));
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
                var area = await AskMenu(this.GetTalkMessage(ScriptMessage.CafePQ_PickArea, levels.Select(x => x)));
                var pqArea = area + 1;
                var em = GetEventManager(CafePQFeature.EventNames[pqArea - 1]);
                var pqOption = await AskMenu(this.GetTalkMessage(ScriptMessage.CafePQ_EnterMenu, levels[area], em.Template.GetRequirementDescription(c)), [
                    this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                    this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
                ]);

                if (pqOption == 0)
                {
                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.CafePQ_PartyQuest));
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

            string[] levels = [this.GetTalkMessage(ScriptMessage.CafePQ_Tier), this.GetTalkMessage(ScriptMessage.CafePQ_Tier2), this.GetTalkMessage(ScriptMessage.CafePQ_Tier3), this.GetTalkMessage(ScriptMessage.CafePQ_Tier4), this.GetTalkMessage(ScriptMessage.CafePQ_Tier5), this.GetTalkMessage(ScriptMessage.CafePQ_Tier6)];

            var selection = await AskMenu(this.GetTalkMessage(ScriptMessage.CafePQ_InternetCafe),
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

            var sendStr = this.GetTalkMessage(ScriptMessage.CafePQ_RewardAtTier, levels[selection]);
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
