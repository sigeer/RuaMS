using Application.Core.Game.Life;
using scripting.npc;

namespace Application.Core.scripting.npc
{
    public class NpcScriptBase : NPCConversationManager
    {
        protected NPC? _npcObj;
        public NpcScriptBase(IChannelClient c, int npc, NPC? npcObj) : base(c, npc, npcObj?.getObjectId() ?? -1, null)
        {
            _npcObj = npcObj;
        }
    }
}
