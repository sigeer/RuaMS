using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.LudiMazePQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9103001 — Rolly，位于鲁塔比（220000000），任务入口
        public async Task party_ludimaze_enter()
        {
            var em = GetEventManager(LudiMazePQFeature.EventName);

            if (isUsingOldPqNpcStyle())
            {
                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                return;
            }

            var option = await AskMenu(this.GetTalkMessage(ScriptMessage.LudiMaze_EnterMenu, em.Template.GetRequirementDescription(c)));
            switch (option)
            {
                case 0:
                    if (getParty() == null)
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.LudiMaze_TryTaking));
                        break;
                    }

                    if (!isLeader())
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.LudiMaze_DecideTackle));
                        break;
                    }

                    await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
                    break;
                case 1:
                    await SayOK(this.GetTalkMessage(ScriptMessage.LudiMaze_PartyQuest));
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
                await SayOK(this.GetTalkMessage(ScriptMessage.LudiMaze_LetParty));
                return;
            }

            if (!eim.isEventTeamTogether())
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.LudiMaze_OneMore));
                return;
            }

            if (!haveItem(LUDIMAZE_COUPON, LUDIMAZE_MIN_COUPON))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.LudiMaze_NotEnough, LUDIMAZE_MIN_COUPON, LUDIMAZE_COUPON));
                return;
            }

            var qty = getItemQuantity(LUDIMAZE_COUPON);
            if (await AskYesNo(this.GetTalkMessage(ScriptMessage.LudiMaze_Gathered, qty, LUDIMAZE_COUPON)))
            {
                await removeAll(LUDIMAZE_COUPON);
                await eim.giveEventPlayersExp(50 * qty);
                await eim.clearPQ();
            }
        }

        // Npc: 9103002 — Rolly，位于迷宫出口（809050016），发放通关礼物
        public async Task party_ludimaze_success()
        {
            if (!await AskYesNo(this.GetTalkMessage(ScriptMessage.LudiMaze_PartyGave)))
                return;

            var eim = GetEventInstanceTrust();

            if (await eim.GiveClearReward(getPlayer()) != Core.scripting.Events.Abstraction.ClaimRewardResult.Success)
            {
                await SayNext(this.GetTalkMessage(ScriptMessage.LudiPQ_NoSpace));
                return;
            }

            await warp(809050017);
        }

        // Npc: 9103003 — 迷宫出口的向导（809050017），送回鲁塔比斯
        public async Task party_ludimaze_fail()
        {
            await SayNext(this.GetTalkMessage(ScriptMessage.LudiMaze_ComeWay));
            await warp(220000000);
        }

        const int LUDIMAZE_COUPON = 4001106;

        const int LUDIMAZE_MIN_COUPON = 30;
    }
}
