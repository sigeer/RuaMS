using Application.Resources.Messages;
using client.inventory;
using server;

namespace Application.Core.Server
{
    public class RewardStorage : AbstractStorage
    {
        public RewardStorage(IPlayer owner, int meso, Item[] items) : base(owner, 64, meso, items)
        {
        }

        public override bool StoreItemCheck(short slot, int itemId, short quantity)
        {
            Owner.Popup(nameof(ClientMessage.RewardStorage_CannotStore));
            return false;
        }

        public override bool StoreMeso(int meso)
        {
            Owner.Popup(nameof(ClientMessage.RewardStorage_CannotStore));
            return false;
        }
        public bool CanGetItemCount(int count)
        {
            return Slots >= Items.Count + count;
        }

        public bool PutItem(Item item)
        {
            if (!CanGetItemCount(1))
                return false;

            Items.Add(item);
            return true;
        }
    }
}
