using Application.Core.Channel.Net.Packets;
using Application.Core.Client.inventory;
using Application.Resources.Messages;
using client.inventory;

namespace Application.Core.Server
{
    public class RewardStorage : AbstractStorage
    {
        public override ItemType StoreType => ItemType.ExtraStorage_Gachapon;
        public RewardStorage(Player owner, int meso, Item[] items) : base(owner, Limits.MaxStorageSlots, meso, items)
        {
        }

        public override async Task<bool> StoreItemCheck(short slot, int itemId, short quantity)
        {
            await Owner.Popup(nameof(ClientMessage.RewardStorage_OnlyAllowTakeOut));
            await Owner.SendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
            return false;
        }

        public override async Task<bool> StoreMesoCheck(int meso)
        {
            await Owner.Popup(nameof(ClientMessage.RewardStorage_OnlyAllowTakeOut));
            // 如果不返回STORAGE数据包，窗口会卡住
            await Owner.SendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
            return false;
        }

        public override async Task ArrangeItems()
        {
            await Owner.Popup(nameof(ClientMessage.RewardStorage_OnlyAllowTakeOut));
            await Owner.SendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
            return;
        }

        public async Task<bool> PutItem(Item item)
        {
            if (!CanGainItem(1))
                return false;

            await Owner.Notice(nameof(ClientMessage.RewardStorage_NewItem), Owner.Client.CurrentCulture.GetItemName(item.getItemId()) ?? item.getItemId().ToString());
            Items.Add(item);
            return true;
        }
    }
}
