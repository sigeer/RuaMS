using Application.Core.Channel.Net.Packets;
using Application.Resources.Messages;
using client.inventory;

namespace Application.Core.Server
{
    public class RewardStorage : AbstractStorage
    {
        public RewardStorage(Player owner, int meso, Item[] items) : base(owner, Limits.MaxStorageSlots, meso, items)
        {
        }

        public override bool StoreItemCheck(short slot, int itemId, short quantity)
        {
            Owner.Popup(nameof(ClientMessage.RewardStorage_OnlyAllowTakeOut));
            Owner.sendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
            return false;
        }

        public override bool StoreMesoCheck(int meso)
        {
            Owner.Popup(nameof(ClientMessage.RewardStorage_OnlyAllowTakeOut));
            // 如果不返回STORAGE数据包，窗口会卡住
            Owner.sendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
            return false;
        }

        public override void ArrangeItems()
        {
            Owner.Popup(nameof(ClientMessage.RewardStorage_OnlyAllowTakeOut));
            Owner.sendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
            return;
        }

        public bool PutItem(Item item)
        {
            if (!CanGainItem(1))
                return false;

            Owner.Pink(nameof(ClientMessage.RewardStorage_NewItem), Owner.Client.CurrentCulture.GetItemName(item.getItemId()) ?? item.getItemId().ToString());
            Items.Add(item);
            return true;
        }
    }
}
