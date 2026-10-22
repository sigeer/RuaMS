using Application.Core.Scripting.Events;
using Application.Plugin.Script.Events;

namespace Application.Plugin.Script.Npc
{
    /// <summary>
    /// 鲁塔比迷宫组队任务（LudiMazePQ）相关 NPC。
    /// </summary>
    internal partial class NpcScript
    {
        const int LUDIMAZE_COUPON = 4001106;
        const int LUDIMAZE_MIN_COUPON = 30;

        // Npc: 9103001 — Rolly，位于鲁塔比（220000000），任务入口
        public async Task party_ludimaze_enter()
        {
            var em = GetEventManager(nameof(LudiMazePQ));

            if (isUsingOldPqNpcStyle())
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                return;
            }

            var option = await AskMenu($"#e#b<组队任务：鲁塔比迷宫>\r\n#k#n{em.Template.GetRequirementDescription(c)}\r\n\r\n这是通往鲁塔比迷宫的入口。尽情享受吧！\r\n#b#L0#进入鲁塔比迷宫#l\r\n#L1#什么是鲁塔比迷宫？#l");
            switch (option)
            {
                case 0:
                    if (getParty() == null)
                    {
                        await SayOK("尝试与你的队伍一起挑战迷宫任务。");
                        break;
                    }

                    if (!isLeader())
                    {
                        await SayOK("如果你决定去挑战它，请让你的队长通知我！");
                        break;
                    }

                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK("#e#b<组队任务：鲁塔比迷宫>#k#n\r\n这个迷宫适用于所有3人或更多成员的队伍，所有参与者必须在51级至70级之间。你将有15分钟的时间来逃离迷宫。在房间的中心，将设置一个传送门，将你传送到另一个房间。这些传送门将把你传送到其他房间，你会（希望）找到出口。皮埃特里将在出口处等待，所以你只需要和他交谈，他就会放你出去。打破房间里的所有箱子，箱子里的怪物会掉落一个优惠券。逃离迷宫后，你将根据收集的优惠券获得经验奖励。此外，如果队长拥有至少30张优惠券，那么将为队伍提供特别的礼物。如果你无法在规定的15分钟内逃离迷宫，你将在迷宫中获得0经验。");
                    break;
                default:
                    break;
            }
        }

        // Npc: 9103000 — Pierre，位于迷宫终点（809050015），按优惠券发放经验
        public async Task party_ludimaze_goal()
        {
            var eim = GetEventInstanceTrust();

            if (!isEventLeader())
            {
                await SayOK("让你的队长和我交谈，结束这个任务。");
                return;
            }

            if (!eim.isEventTeamTogether())
            {
                await SayOK("一个或多个队员尚未到达，请等待他们先到达这里。");
                return;
            }

            if (!haveItem(LUDIMAZE_COUPON, LUDIMAZE_MIN_COUPON))
            {
                await SayOK($"你的队伍还不能完成这个组队任务，因为你手头上还没有达到最低要求的{LUDIMAZE_MIN_COUPON}个#t{LUDIMAZE_COUPON}#。");
                return;
            }

            var qty = getItemQuantity(LUDIMAZE_COUPON);
            if (await AskYesNo($"太棒了！你从这次冒险中获得了{qty}个#t{LUDIMAZE_COUPON}#，现在你的队伍将从这次行动中获得公平的经验值。你准备好离开了吗？"))
            {
                await removeAll(LUDIMAZE_COUPON);
                await eim.giveEventPlayersExp(50 * qty);
                await eim.clearPQ();
            }
        }

        // Npc: 9103002 — Rolly，位于迷宫出口（809050016），发放通关礼物
        public async Task party_ludimaze_success()
        {
            if (!await AskYesNo("你的队伍付出了非常出色的努力，至少收集了30张优惠券。为此，我为你们每个人准备了一份礼物。领取礼物后，你们将被送回到鲁塔比斯。现在，你们想要立刻领取礼物吗？"))
                return;

            var eim = GetEventInstanceTrust();

            if (await eim.GiveClearReward(getPlayer()) != Core.scripting.Events.Abstraction.ClaimRewardResult.Success)
            {
                await SayNext("看起来你的#r装备#k、#r消耗#k或#r其他#k背包中都没有空位。请腾出一些空间，然后再试一次。");
                return;
            }

            await warp(809050017);
        }

        // Npc: 9103003 — 迷宫出口的向导（809050017），送回鲁塔比斯
        public async Task party_ludimaze_fail()
        {
            await SayNext("走这边回到鲁塔比。");
            await warp(220000000);
        }
    }
}
