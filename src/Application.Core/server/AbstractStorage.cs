using Application.Core.Channel.Net.Packets;
using Application.Core.Client.inventory;
using client.inventory;
using client.inventory.manipulator;
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

        protected virtual Task<bool> BaseCheck()
        {
            return Task.FromResult(true);
        }


        public virtual async Task<bool> StoreItemCheck(short slot, int itemId, short quantity)
        {
            if (!await BaseCheck())
            {
                return false;
            }

            if (quantity < 1)
            {
                await Owner.SendPacket(PacketCreator.enableActions());
                return false;
            }

            if (IsFull())
            {
                await Owner.SendPacket(StoragePacketCreator.getStorageError(0x11));
                return false;
            }

            return true;
        }
        public virtual async Task<bool> TakeOutItemCheck(Item item)
        {
            if (!await BaseCheck())
            {
                return false;
            }

            if (!Owner.CanHoldUniquesOnly(item.getItemId()))
            {
                await Owner.SendPacket(StoragePacketCreator.getStorageError(0x0C));
                return false;
            }

            if (!InventoryManipulator.checkSpace(Owner.Client, item.getItemId(), item.getQuantity(), item.getOwner()))
            {
                await Owner.SendPacket(StoragePacketCreator.getStorageError(0x0A));
                return false;
            }

            return true;
        }

        public virtual async Task<bool> TakeOutMesoCheck(int meso)
        {
            if (!await BaseCheck())
            {
                return false;
            }

            return meso <= Meso;
        }

        public virtual async Task<bool> StoreMesoCheck(int meso)
        {
            if (!await BaseCheck())
            {
                return false;
            }

            return Owner.getMeso() >= meso;
        }

        public virtual Task OnStoreSuccess(short slot, int itemId, short quantity)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnTakeOutSuccess(Item item)
        {
            return Task.CompletedTask;
        }

        public virtual async Task OpenStorage(int npcId)
        {
            Items.Sort((o1, o2) =>
            {
                return o1.getInventoryType().getType().CompareTo(o2.getInventoryType().getType());
            });

            foreach (var inv in EnumCache<InventoryType>.Values)
            {
                _typedItems[inv] = Items.ToList();
            }

            await Owner.SendPacket(StoragePacketCreator.getStorage(npcId, Slots, GetItems(), Meso));
            Owner.CurrentStorage = this;
        }

        public async Task UpdateMeso()
        {
            await Owner.SendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
        }

        public virtual async Task ArrangeItems()
        {
            if (!await BaseCheck())
            {
                return;
            }

            var sorter = new StorageSorter(this);
            sorter.Merge();
            Items = sorter.Sort();
            await Owner.SendPacket(StoragePacketCreator.arrangeStorage(Slots, Items));
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
