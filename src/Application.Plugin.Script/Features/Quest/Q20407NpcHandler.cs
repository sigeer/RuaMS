using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Core.Server.life;
using System.Drawing;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    /// <summary>
    /// 任务 20407 相关脚本：blackWitch。
    /// 该任务在 NPC 与传送门两侧都有判定，故集中在此。
    /// </summary>
    internal sealed class Q20407NpcHandler : NpcScriptBase
    {
        public Q20407NpcHandler(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj)
        {
        }

        // Npc: 1104002 
        public async Task blackWitch()
        {
            if (!isQuestStarted(20407))
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Q20407_Unsure));
                return;
            }

            if (await SayAcceptDecline(this.GetTalkMessage(ScriptMessage.Q20407_JoinOffer)))
            {
                await this.Pink(this.GetTalkMessage(ScriptMessage.Q20407NpcHandler_EleanorLost));

                await getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9001010), new Point(850, 0));
                await getMap().destroyNPC(1104002);

            }
            else
            {
                await SayOK(this.GetTalkMessage(ScriptMessage.Q20407_Begone));
            }
        }
    }
}
