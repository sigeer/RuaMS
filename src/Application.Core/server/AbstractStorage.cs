using Application.Core.Channel.Net.Packets;
using client.inventory;
using client.inventory.manipulator;
using server;
using tools;

namespace Application.Core.Server
{
    public abstract class AbstractStorage
    {
        protected AbstractStorage(IPlayer owner, byte slots, int meso, IEnumerable<Item> items)
        {
            Meso = meso;
            Slots = slots;
            Owner = owner;

            Items = new List<Item>(slots);
            Items.AddRange(items);
        }

        public int Meso { get; set; }
        public byte Slots { get; set; }

        public IPlayer Owner { get; }
        public List<Item> Items { get; protected set; }

        protected Lock lockObj = new Lock();

        public bool IsFull()
        {
            lock (lockObj)
            {
                return Items.Count >= Slots;
            }
        }

        public Item? GetItemBySlot(sbyte slot)
        {
            lockObj.Enter();
            try
            {
                return Items.ElementAtOrDefault(slot);
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public List<Item> GetItems()
        {
            lockObj.Enter();
            try
            {
                return Items.ToList();
            }
            finally
            {
                lockObj.Exit();
            }
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

        public virtual bool TakeOutMeso(int meso)
        {
            return meso <= Meso;
        }

        public virtual bool StoreMeso(int meso)
        {
            return Owner.getMeso() >= meso;
        }

        public virtual void OnStoreSuccess(short slot, int itemId, short quantity)
        {

        }

        public virtual void OnTakeOutSuccess(Item item)
        {

        }

        public void OpenStorage(int npcId)
        {
            lockObj.Enter();
            try
            {
                Items.Sort((o1, o2) =>
                {
                    if (o1.getInventoryType().getType() < o2.getInventoryType().getType())
                    {
                        return -1;
                    }
                    else if (o1.getInventoryType() == o2.getInventoryType())
                    {
                        return 0;
                    }
                    return 1;
                });


                Owner.sendPacket(StoragePacketCreator.getStorage(npcId, Slots, GetItems(), Meso));
                Owner.CurrentStorage = this;
            }
            finally
            {
                lockObj.Exit();
            }
        }

        public void UpdateMeso()
        {
            Owner.sendPacket(StoragePacketCreator.mesoStorage(Slots, Meso));
        }

        public void ArrangeItems()
        {
            lockObj.Enter();
            try
            {
                StorageInventory msi = new StorageInventory(Owner.Client, Items);
                msi.mergeItems();
                Items = msi.sortItems();
                Owner.sendPacket(StoragePacketCreator.arrangeStorage(Slots, Items));
            }
            finally
            {
                lockObj.Exit();
            }
        }

    }
}
