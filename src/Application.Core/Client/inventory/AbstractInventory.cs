using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using client.inventory;
using System.Collections;
using System.Diagnostics;
using ZLinq;

namespace Application.Core.Client.inventory
{
    public abstract class AbstractInventory : IEnumerable<Item>, IDisposable
    {
        public Player Owner { get; }
        public InventoryType Type { get; }

        protected AbstractInventory(Player owner, InventoryType type)
        {
            Owner = owner;
            this.Type = type;
        }

        public bool isExtendableInventory()
        {
            // not sure about cash, basing this on the previous one.
            return !(Type.Equals(InventoryType.UNDEFINED) || Type.Equals(InventoryType.EQUIPPED) || Type.Equals(InventoryType.CASH));
        }

        public bool isEquipInventory()
        {
            return Type.Equals(InventoryType.EQUIP) || Type.Equals(InventoryType.EQUIPPED);
        }
        public InventoryType getType() => Type;

        #region Slots
        public abstract bool CanGainSlot(short slots);
        public abstract byte getSlotLimit();
        public abstract void setSlotLimit(int newLimit);
        public abstract short getNextFreeSlot();

        public abstract short getNumFreeSlot();
        #endregion


        #region Query
        protected abstract IEnumerable<Item> ListExsitedEnumerable();
        /// <summary>
        /// 加载所有物品（不含插槽信息）
        /// </summary>
        /// <returns></returns>
        public abstract List<Item> list();
        /// <summary>
        /// 加载所有物品及其插槽
        /// </summary>
        /// <returns></returns>
        public abstract List<InventoryItem> LoadAllItem();

        public abstract Item? getItem(short slot);
        public bool HasItem(int itemId)
        {
            return ListExsitedEnumerable().Any(x => x.getItemId() == itemId);
        }
        public List<Item> listById(int itemId)
        {
            return ListExsitedEnumerable().Where(x => x.getItemId() == itemId).OrderBy(x => x.getPosition()).ToList();
        }

        public int countById(int itemId)
        {
            return ListExsitedEnumerable().Where(x => x.getItemId() == itemId).Sum(x => x.getQuantity());
        }
        public int countNotOwnedById(int itemId)
        {
            return ListExsitedEnumerable().Count(x => x.getItemId() == itemId && string.IsNullOrEmpty(x.getOwner()));
        }

        public Item? findByCashId(long cashId)
        {
            return ListExsitedEnumerable().FirstOrDefault(x => x.getCashId() == cashId);
        }

        public Item? findById(int itemId)
        {
            return ListExsitedEnumerable().FirstOrDefault(x => x.getItemId() == itemId);
        }
        public Item? findByName(string name)
        {
            return ListExsitedEnumerable().FirstOrDefault(x => name.Equals(ClientCulture.SystemCulture.GetItemName(x.getItemId()), StringComparison.OrdinalIgnoreCase));
        }

        #endregion


        #region Management
        public abstract void PutItem(short position, Item item);

        public abstract void SwapFromMove(short sSlot, short dSlot);

        public abstract void RemoveFromMove(short slot);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="actualRemoved">实际移除的数量</param>
        /// <param name="quantity"></param>
        /// <param name="allowZero"></param>
        /// <returns></returns>
        public IInventoryOperationCommand? removeItem(short slot, out short actualRemoved, short quantity = 1, bool allowZero = false)
        {
            actualRemoved = 0;
            var item = getItem(slot);
            if (item == null)
            {
                // TODO is it ok not to throw an exception here?
                return null;
            }

            if (quantity < 0)
            {
                return null;
            }

            var original = item.getQuantity();
            var left = original - quantity;

            if (left <= 0)
            {
                item.setQuantity(0);
                actualRemoved = original;
            }
            else
            {
                item.setQuantity((short)left);
                actualRemoved = quantity;
            }

            IInventoryOperationCommand? op = null;
            if (left <= 0 && !allowZero)
                op = removeSlot(slot);
            else
            {
                op = new InventoryUpdateQuantity(item.getInventoryType(), slot, item.getQuantity());
            }

            Activity.Current?.AddEvent(
                    new ActivityEvent(
                        "RemoveItem",
                        tags: new ActivityTagsCollection
                        {
                            ["Inventory"] = getType(),
                            ["Slot"] = slot,
                            ["RemoveCount"] = quantity,
                            ["ActualRemoved"] = actualRemoved
                        }));
            return op;
        }
        public abstract IInventoryOperationCommand? removeSlot(short slot);
        #endregion

        public abstract void Dispose();


        public abstract IEnumerator<Item> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
