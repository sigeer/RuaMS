using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.npc;

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
                if (await AskYesNo("你准备好离开这个地方了吗？"))
                {
                    await warp(541010110, 0);
                }
            }
            else
            {
                if (await AskYesNo("你已经打败了拉塔尼卡船长，干得好！你准备好离开这个地方了吗？"))
                {
                    if (await eim.GiveClearReward(getPlayer()) == ClaimRewardResult.Success)
                    {
                        await warp(541010110, 0);
                    }
                    else
                    {
                        await SayOK("请在你的背包中腾出一个空间来接收战利品。");
                    }
                    return;
                }
            }
        }

    }
}
