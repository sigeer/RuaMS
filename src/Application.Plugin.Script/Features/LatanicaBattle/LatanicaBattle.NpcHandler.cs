using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;
using Application.Plugin.Script;
using Application.Resources.Messages;

namespace Application.Plugin.Script.Features.Quest
{
    internal sealed class LatanicaBattleNpcHandler : NpcScriptBase
    {
        public LatanicaBattleNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }


        // Npc: 9270033 
        public async Task captinsg01()
        {
            var eim = GetEventInstanceTrust();
            if (!eim.isEventCleared())
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Latanica_ReadyLeave)))
                {
                    await warp(541010110, 0);
                }
            }
            else
            {
                if (await AskYesNo(this.GetTalkMessage(ScriptMessage.Latanica_HaveDefeated)))
                {
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(541010110, 0);
                    }
                    else
                    {
                        await SayOK(this.GetTalkMessage(ScriptMessage.Latanica_PleaseMake));
                    }
                    return;
                }
            }
        }

    }
}
