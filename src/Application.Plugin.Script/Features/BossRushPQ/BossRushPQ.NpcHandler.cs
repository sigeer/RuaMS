using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;

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
                    await SayOK("你的队伍完成了如此惊人的壮举，走到了这一步，#b你已经打败了所有的boss#k，恭喜！现在我将给你们奖励，因为你们即将被传送出去…");
                }
                else
                {
                    await SayOK("在这个副本中#b打败所有的boss#k，恭喜你！现在你将获得与你在这里表现相匹配的奖品，我会将你传送出去。");
                }

                if (await eim.GiveClearReward(getPlayer(), 6) == ClaimRewardResult.BagFull)
                {
                    await SayOK("请提前在你的背包所有标签中安排一个空位。");
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
                        if (await AskYesNo("你的队伍准备好继续前进到下一阶段了吗？如果你认为已经完成了，就走过传送门，现在是时候了。现在，你们真的想要继续吗？"))
                        {
                            var restSpot = ((getMapId() - 1) % 5) + 1;
                            await eim.restartEventTimer(restSpot * 4 * 60000);
                            await eim.warpEventTeam(970030100 + eim.getIntProperty("lobby") + (500 * restSpot));
                        }
                    }
                    else
                    {
                        await SayOK("请等待您的队伍重新集合后再继续。");
                    }
                }
                else
                {
                    await SayOK("等待你的队长发出信号让我继续。如果你感觉不太好，想要退出，走过传送门，你将被传送出去，并且你会因为走到这一步而获得奖品。");
                }
            }
            else if (state == 1)
            {
                if (await AskYesNo("你希望放弃这个副本吗？"))
                {
                    await warp(970030000);
                }
            }
            else
            {
                var em = GetEventManager(BossRushPQFeature.EventName);
                var option = await AskMenu(
                    "#e#b<组队任务：首领突袭>\r\n#k#n" + em.Template.GetRequirementDescription(c) +
                    "\r\n\r\n你想要和队友合作完成远征任务，还是勇敢到足以独自完成？让你的#b队伍领袖#k与我交谈或者自己创建一个队伍。",
                    [
                        "我想参加组队任务。",
                        "我想了解更多详情。"
                    ]);

                if (option == 0)
                {
                    if (getParty() == null)
                    {
                        await SayOK("只有当你加入一个队伍时，你才能参加派对任务。");
                    }
                    else if (!isLeader())
                    {
                        await SayOK("你的队长必须与我交谈才能开始这个组队任务。");
                    }
                    else
                    {
                        var r = await em.StartInstance(getPlayer());
                        await SayOK(em.HandleCreateInstanceResult(r, c));
                    }
                }
                else
                {
                    await SayOK("#e#b<组队任务：Boss Rush>#k#n\r\n来自世界各地的勇敢冒险者来到这里，测试他们在战斗中的技能和能力，面对来自冒险岛的更强大的boss。与其他冒险者联手或者独自承担所有的压力并获得所有的荣耀，这取决于你。奖励根据冒险者们的进展程度给予，而额外奖品可能会随机给予队伍中的一名成员，所有这些都在远征结束时进行分配。\r\n\r\n这个任务还支持#b多个大厅，以匹配不同团队等级的玩家#k：如果你想更快地为你的团队设置boss rush，可以与等级较低的玩家组队。");
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
