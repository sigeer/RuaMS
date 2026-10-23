using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.TreasurePQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9220018 
        public async Task guyfawkes_ch()
        {
            var em = GetEventManager(TreasurePQFeature.EventName);
            var sel = await AskMenu("#e#b<组队任务：MV的巢穴>\r\n#k#n"
                + (em.Template?.GetRequirementDescription(c) ?? "")
                + "\r\n\r\n由于前方有极其危险的生物，你无法继续前进。你想要和队友合作完成任务吗？如果是的话，请让你的 #b队伍领袖#k 和我交谈。#b\r\n#L0#我想参与组队任务。\r\n#L1#我想了解更多详情。");

            if (sel == 0)
            {
                if (getParty() == null)
                {
                    await SayOK("只有当你加入一个队伍时，才能参加派对任务。");
                    return;
                }

                if (!isLeader())
                {
                    await SayOK("你的队长必须与我交谈才能开始这个组队任务。");
                    return;
                }

                await SayOK(em.HandleCreateInstanceResult(await em.StartInstance(getPlayer()), c));
            }
            else if (sel == 1)
            {
                await SayOK("#e#b<组队任务：MV的巢穴>#k#n\r\n带领你的队员前往 MV 的巢穴，找到并击败盘踞其中的怪物。");
            }
        }


    }
}
