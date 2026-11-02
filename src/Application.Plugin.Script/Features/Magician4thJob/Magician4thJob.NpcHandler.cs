using Application.Core.Client;
using Application.Core.Game.Life;
using Application.Core.scripting.npc;

namespace Application.Plugin.Script.Features.Magician4thJob
{
    internal sealed class Magician4thJobNpcHandler : NpcScriptBase
    {
        public Magician4thJobNpcHandler(IChannelClient c, int npc, NPC? npcObject) : base(c, npc, npcObject)
        {
        }

        // Npc: 2012023 
        public async Task s4tornado()
        {
            // Q6230
            if (haveItem(4031476))
            {
                if (canHold(4031456, 1))
                {
                    await gainItem(4031476, -1);
                    await gainItem(4031456, 1);
                }
            }
        }

        // Npc: 2030014 
        public async Task s4freeze_item()
        {
            //Q6263
            if (haveItem(4031450, 1))
            {
                if (canHold(2280011, 1))
                {
                    await gainItem(2280011, 1);
                    await gainItem(4031450, -1);
                }
            }
        }

    }
}
