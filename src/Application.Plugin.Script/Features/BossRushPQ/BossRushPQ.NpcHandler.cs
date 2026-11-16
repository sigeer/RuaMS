using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.BossRushPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9000037 
        public async Task Raid_solo()
        {
            bool onRestingSpot() => getMapId() >= 970030001 && getMapId() <= 970030010;
            bool isFinalBossDone() => getMapId() >= 970032700 && getMapId() < 970032800 && getMap().countMonsters() == 0;

            int state;
            if (getMapId() >= 970030001 && getMapId() <= 970042711)
            {
                if (!onRestingSpot())
                {
                    state = isFinalBossDone() ? 3 : 1;
                }
                else
                {
                    state = 2;
                }
            }
            else
            {
                state = 0;
            }

            if (state == 3)
            {
                var eim = getEventInstance();
                if (eim.getProperty("clear") == null)
                {
                    await eim.clearPQ();
                    eim.setProperty("clear", "true");
                }

                if (isEventLeader())
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.BossRush_PartyCompleted));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.BossRush_DefeatingAll));
                }

                if (await eim.GiveClearReward(getPlayer(), 6) == ClaimRewardResult.BagFull)
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.BossRush_PleaseArrange));
                    return;
                }

                await warp(970030000);
            }
            else if (state == 2)
            {
                if (isEventLeader())
                {
                    var eim = getPlayer().getEventInstance();
                    if (eim != null && eim.isEventTeamTogether())
                    {
                        if (await AskYesNo(this.GetTalkMessage(ScriptMessage.BossRush_PartyReady)))
                        {
                            var restSpot = ((getMapId() - 1) % 5) + 1;
                            await eim.restartEventTimer(restSpot * 4 * 60000);
                            await eim.warpEventTeam(970030100 + eim.getIntProperty("lobby") + (500 * restSpot));
                        }
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.BossRush_PleaseWait));
                    }
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.BossRush_WaitParty));
                }
            }
            else if (state == 1)
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.BossRush_WishAbandon)))
                {
                    await warp(970030000);
                }
            }
            else
            {
                var em = GetEventManager(BossRushPQFeature.EventName);
                var option = await AskMenu(
                    this.GetTalkMessage(ScriptMessage.BossRush_PartyQuestBoss, em.Template.GetRequirementDescription(c)),
                    [
                        this.GetTalkMessage(ScriptMessage.PartyQuest_Start),
                        this.GetTalkMessage(ScriptMessage.PartyQuest_MoreInfo)
                    ]);

                if (option == 0)
                {
                    var r = await em.StartInstance(getPlayer());
                    await SayOK(em.HandleCreateInstanceResult(r, c));
                }
                else
                {
                    await SayOK(this.GetTalkMessage(ScriptMessage.BossRush_PartyQuest));
                }
            }
        }


        // Npc: 9000038 
        public async Task Raid_party()
        {
            int[] itemSet_lv6 = [3010061, 1122018, 1122005, 1022088, 1402013, 1032030, 1032070, 1102046, 2330004, 2041013, 2041016, 2041019, 2041022, 2049100, 2049003, 2020012, 2020013, 2020014, 2020015, 2022029, 2022045, 2022068, 2022069, 2022179, 2022180, 4004000, 4004001, 4004002, 4004003, 4004004, 4003000];
            int[] itemQty_lv6 = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 25, 25, 25, 25, 25, 25, 25, 25, 4, 4, 12, 12, 12, 12, 12, 25];
            int[] itemSet_lv5 = [3010063, 1122018, 1122005, 1022088, 1402013, 1032030, 1032070, 1102046, 2330004, 2041013, 2041016, 2041019, 2041022, 2049100, 2049003, 2020012, 2020013, 2020014, 2020015, 2022029, 2022045, 2022068, 2022069, 2022179, 2022180, 4004000, 4004001, 4004002, 4004003, 4004004, 4003000];
            int[] itemQty_lv5 = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 15, 15, 15, 15, 15, 15, 15, 15, 2, 2, 8, 8, 8, 8, 8, 12];
            int[] itemSet_lv4 = [1122001, 1122006, 1022103, 1442065, 1032042, 1032021, 1102168, 2070005, 2040025, 2040029, 2040301, 2040413, 2040701, 2040817, 2002028, 2020009, 2020010, 2020011, 2022004, 2022005, 2022025, 2022027, 2022048, 2022049, 4020000, 4020001, 4020002, 4020003, 4020004, 4020005, 4020006, 4020007, 4020008, 4003000];
            int[] itemQty_lv4 = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 5, 45, 45, 45, 45, 45, 45, 45, 45, 45, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8];
            int[] itemSet_lv3 = [1122002, 1022088, 1012076, 1402029, 1032041, 1032044, 1102167, 2070011, 2040026, 2040030, 2040302, 2040412, 2040702, 2040818, 2002028, 2020009, 2020010, 2020011, 2022004, 2022005, 2022025, 2022027, 2022048, 2022049, 4010000, 4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007, 4003000];
            int[] itemQty_lv3 = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 20, 20, 20, 20, 20, 20, 20, 20, 20, 5, 5, 5, 5, 5, 5, 5, 5, 5];
            int[] itemSet_lv2 = [1122003, 1012077, 1012079, 1432014, 1032059, 1032002, 1102191, 2330002, 2040001, 2040311, 2040401, 2040601, 2040824, 2040901, 2010000, 2010001, 2010002, 2010003, 2010004, 2020001, 2020002, 2020003, 2022020, 2022022, 4020000, 4020001, 4020002, 4020003, 4020004, 4020005, 4020006, 4020007, 4020008, 4003000];
            int[] itemQty_lv2 = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 60, 60, 60, 60, 60, 60, 60, 60, 60, 60, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3];
            int[] itemSet_lv1 = [1122004, 1012078, 1432008, 1432009, 1032040, 1032009, 1102166, 2070001, 2040002, 2040310, 2040400, 2040600, 2040825, 2040902, 2010000, 2010001, 2010002, 2010003, 2010004, 2020001, 2020002, 2020003, 2022020, 2022022, 4010000, 4010001, 4010002, 4010003, 4010004, 4010005, 4010006, 4010007, 4003000];
            int[] itemQty_lv1 = [1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 25, 25, 25, 25, 25, 25, 25, 25, 25, 25, 2, 2, 2, 2, 2, 2, 2, 2, 2];

            string[] levels = ["#m970030001#", "#m970030002#", "#m970030003#", "#m970030004#", "#m970030005#", "Final stage"];

            var sendStr = "The #bBoss Rush Party Quest#k rewards players accordingly to how far the team went on the boss huntings. Take note that each player #bcan only claim a reward if they leave through a portal inside a Resting Spot#k. Challenging stronger bosses will require the team to commit to more fightings until the next Resting Spot is reached, or until the final boss is defeated.\r\n\r\nThe possible rewards for those leaving in the selected Resting Spot are depicted here:\r\n\r\n";
            var selection = await AskMenu(sendStr, levels.Select((x, i) => x));

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

            var resultStr = $"The following items are being awarded at #b{levels[selection]}#k:\r\n\r\n";
            for (var i = 0; i < lvTarget.Length; i++)
            {
                resultStr += $"#i{lvTarget[i]}# #t{lvTarget[i]}#";
                if (lvQty[i] > 1)
                {
                    resultStr += $" ({lvQty[i]})";
                }
                resultStr += "\r\n";
            }

            await SayOK(resultStr);
        }

    }
}
