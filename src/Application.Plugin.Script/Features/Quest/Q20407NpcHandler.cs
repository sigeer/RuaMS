using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;
using Application.Core.Server.life;
using System.Drawing;

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
                await SayOK("... Knight, you still #bseem unsure to face this fight#k, don't you? There's no grace in challenging someone when they are still not mentally ready for the battle. Talk your peace to that big clumsy bird of yours, maybe it'll put some guts on you.");
                return;
            }

            if (await SayAcceptDecline("Hahahahaha! This place's Empress is already under my domain, that's surely a great advance on the #bBlack Wings#k' overthrow towards Maple World... And you, there? Still wants to face us? Or, better yet, since you seem strong enough to be quite a supplementary reinforcement at our service, #rwill you meet our expectations and fancy joining us#k since there's nothing more you can do?"))
            {
                await Pink("Eleanor: Oh, lost the Empress and still challenging us? Now you've done it! Prepare yourself!!!");

                await getMap().spawnMonsterOnGroundBelow(LifeFactory.Instance.GetMonsterTrust(9001010), new Point(850, 0));
                await getMap().destroyNPC(1104002);

            }
            else
            {
                await SayOK("Heh, cowards have no place on the #rBlack Mage's#k army. Begone!");
            }
        }
    }
}
