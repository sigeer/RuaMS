using client.inventory;
using scripting.npc;

namespace Application.Core.scripting.item
{
    public class ItemScriptBase : NPCConversationManager
    {
        protected Item _item;
        public ItemScriptBase(IChannelClient c, Item item, int npcId) : base(c, npcId, -1, null)
        {
            _item = item;
        }
    }
}
