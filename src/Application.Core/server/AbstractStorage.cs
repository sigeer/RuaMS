using Application.Core.Channel.Net.Packets;
using client.inventory;
using client.inventory.manipulator;
using server;
using tools;
using ZLinq;

namespace Application.Core.Server
{
    public abstract class AbstractStorage
    {
        protected AbstractStorage(Player owner, byte slots, int meso, IEnumerable<Item> items)
        {
            Meso = meso;
            Slots = slots;
            Owner = owner;

            Items = new List<Item>(slots);
            Items.AddRange(items);

            _typedItems = new();
        }

        public int Meso { get; set; }
        public byte Slots { get; set; }

        public Player Owner { get; }
        protected List<Item> Items { get; set; }
        protected Dictionary<InventoryType, List<Item>> _typedItems;

        public bool IsFull()
        {
            return Items.Count >= Slots;
        }

        public bool CanGainItem(int count)
        {
            return Items.Count + count <= Slots;
        }

        public bool CanGainSlots(int slots)
        {
            return slots + Slots <= Limits.MaxStorageSlots;
        }

        public Item? GetItemBySlot(int slot)
        {
            return Items.ElementAtOrDefault(slot);
        }

        public Item? GetItemByTypedSlot(InventoryType type, int typedSlot)
        {
            return _typedItems.GetValueOrDefault(type)?.ElementAt(typedSlot);
        }

        public List<Item> GetItems()
        {
            return Items.ToList();
        }


        public virtual bool StoreItemCheck(short slot, int itemId, short quantity)
        {
            if (quantity < 1)
            {
                Owner.sendPacket(PacketCreator.enableActions());
                return false;
            }

            if (IsFull())
            {
                Owner.sendPacket(StoragePacketCreator.getStorageError(0x11));
                return false;
            }

            return true;
        }
        public virtual bool TakeOutItemCheck(Item item)
        {
            if (!Owner.CanHoldUniquesOnly(item.getItemId()))
            {
                Owner.sendPacket(StoragePacketCreator.getStorageError(0x0C));
                return false;
            }

            if (!InventoryManipulator.checkSpace(Owner.Client, item.getItemId(), item.getQuantity(), item.getOwner()))
            {
                Owner.sendPacket(StoragePacketCreator.getStorageError(0x0A));
                return false;
            }

            return true;
        }

        public virtual bool TakeOutMesoCheck(int meso)
        {
            return meso <= Meso;
        }

        public virtual bool StoreMesoCheck(int meso)
        {
            return Owner.getMeso() >= meso;
        }

        public virtual void OnStoreSuccess(short slot, int itemId, short quantity)
        {

        }

        public virtual void OnTakeOutSuccess(Item item)
        {

        }

        public virtual void OpenStorage(int npcId)
        {
            Items.Sort((o1, o2) =>
            {
                return o1.getInventoryType().getType().CompareTo(o2.getInventoryType().getType());
            });

            foreach (var inv in EnumCache<InventoryType>.Values)
            {
                _typedItems[inv] = Items.ToList();
            }

            Owner.sendPacket(StoragePacketCreator.getStorage(npcId, Slots, GetItems(), Meso));
            Owner.CurrentStorage = this;
        }

        public void UpdateMeso()
        {
            Owner.sendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
        }

        public virtual void ArrangeItems()
        {
            StorageInventory msi = new StorageInventory(Owner.Client, Items);
            msi.mergeItems();
            Items = msi.sortItems();
            Owner.sendPacket(StoragePacketCreator.arrangeStorage(Slots, Items));
        }

        public void AddItem(Item item)
        {
            Items.Add(item);
            _typedItems[item.getInventoryType()] = Items.AsValueEnumerable().Where(x => x.getInventoryType() == item.getInventoryType()).ToList();
        }

        public bool RemoveItem(Item item)
        {
            if (Items.Remove(item))
            {
                _typedItems[item.getInventoryType()] = Items.AsValueEnumerable().Where(x => x.getInventoryType() == item.getInventoryType()).ToList();
                return true;
            }
            return false;
        }

        public List<Item> GetTypedItems(InventoryType type) => _typedItems.GetValueOrDefault(type, []);
    }
}
