using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.YaoSengPQ
{
    internal sealed class NpcHandler : NpcScriptBase
    {
        public NpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 9310039 
        public async Task q8535s()
        {
            const int recruitMap = 702070400;
            const short questId = 8534;

            if (getMapId() != recruitMap)
                return;

            if (questId > 0 && !isQuestCompleted(questId))
            {
                await SayOK("...");
                return;
            }

            var em = GetEventManager(YaoSengPQFeature.EventName);

            var sel = await AskMenu("#e#b<组队任务> 少林密室#n\r\n"
                + (em.Template?.GetRequirementDescription(c) ?? "")
                + "#k\r\n\r\n你和你的队伍成员一起完成任务怎么样？\r\n在这里，你会遇到障碍和问题，如果没有出色的团队合作，你是无法完成的。\r\n如果你想尝试，请告诉你的#b队长#k来找我谈谈。#b\r\n"
                + "#L0#我想参加组队任务。#l\r\n"
                + "#L1#我想了解更多细节。#l");

            if (sel == 0)
            {
                if (getParty() == null)
                {
                    await SayOK("只有当你加入一个队伍时，你才能参加组队任务。");
                    return;
                }

                if (!isLeader())
                {
                    await SayOK("必须由你的队长与我交谈才能开始这个组队任务。");
                    return;
                }

                var r = await em.StartInstance(getPlayer(), 1);
                if (r != CreateInstanceResult.Success)
                {
                    await SayOK("另一个队伍已经进入了该频道的#r组队任务#k。请尝试其他频道，或者等待当前队伍完成。");
                }
            }
            else if (sel == 1)
            {
                await SayOK("#e#b<组队任务> 少林密室#k#n\r\n带领你的队员到#e#b#m702060000##n#k进行调查并解决问题的源头。");
            }
        }

        // Npc: 9310044 
        public async Task outshaolinBoss()
        {
            var mapID_out = 702070400;
            if (await AskYesNo($"你要离开#b#e#m{getMapId()}##k#n 回到 #b#e#m{mapID_out}##k#n 吗？"))
            {
                await warp(mapID_out);
            }
        }
    }
}
