using Application.Core.Channel;
using Application.Core.Channel.Net.Packets;
using Application.Core.Game.Items;
using Application.Templates;
using Application.Templates.Item.Cash;
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

        protected readonly SortedSet<TimedItemWrapper> _timedItems;

        protected AbstractInventory(Player owner, InventoryType type)
        {
            Owner = owner;
            this.Type = type;

            _timedItems = new();
        }

        public bool isExtendableInventory()
        {
            // not sure about cash, basing this on the previous one.
            return Type != InventoryType.UNDEFINED && Type != InventoryType.EQUIPPED && Type != InventoryType.CASH;
        }

        public bool isEquipInventory()
        {
            return Type == InventoryType.EQUIP || Type == InventoryType.EQUIPPED;
        }
        public InventoryType getType() => Type;

        #region Slots
        public abstract bool CanGainSlot(short slots);
        public abstract byte getSlotLimit();
        public abstract Task setSlotLimit(int newLimit);
        public abstract short getNextFreeSlot();

        public abstract short getNumFreeSlot();
        #endregion


        #region Query
        public abstract IEnumerable<Item> ListExsitedEnumerable();
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
        public abstract IEnumerable<InventoryItem> LoadAllItemEnumerable();

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
        List<Item> _tickToRemove = [];
        List<Item> _tickToUpdate = [];
        List<IInventoryOperationCommand> _tickToSync = [];
        List<ReplaceItemTemplate> _tickToReplace = [];
        public async Task OnTick(long now)
        {
            if (_timedItems.Count == 0)
            {
                return;
            }

            _tickToRemove.Clear();
            _tickToUpdate.Clear();
            _tickToSync.Clear();
            _tickToReplace.Clear();

            foreach (var p in _timedItems)
            {
                if (p.TickTime > now)
                {
                    // 未到时间不需要执行
                    break;
                }

                var item = p.Item;
                long expiration = item.getExpiration();

                if (expiration != -1 && (expiration < now))
                {
                    if ((item.getFlag() & ItemConstants.LOCK) == ItemConstants.LOCK)
                    {
                        short lockObj = item.getFlag();
                        lockObj &= ~(ItemConstants.LOCK);
                        item.setFlag(lockObj); //Probably need a check, else people can make expiring items into permanent items...
                        item.setExpiration(-1);

                        _tickToUpdate.Add(item);
                    }

                    else if (item is Pet pet)
                    {
                        pet.MapPet?.Recall(2);

                        if (pet.SourceTemplate.NoRevive)
                        {
                            _tickToRemove.Add(item);
                        }
                        else
                        {
                            item.setExpiration(-1);
                            _tickToUpdate.Add(item);
                        }
                    }
                    else
                    {
                        _tickToRemove.Add(item);
                    }


                }

                await OnTickItem(now, item, _tickToUpdate, _tickToRemove);
            }

            foreach (var item in _tickToUpdate)
            {
                _tickToSync.Add(new InventoryAdd(item.getInventoryType(), item, item.getPosition()));
            }


            foreach (var item in _tickToRemove)
            {
                var op = await removeSlot(item.getPosition());
                if (op != null)
                {
                    _tickToSync.Add(op);

                    if (item.SourceTemplate.Cash)
                    {
                        await Owner.SendPacket(MessagePacket.CashItemExpireMessage(item.getItemId()));
                    }

                    if (item.SourceTemplate.ReplaceItem != null)
                    {
                        _tickToReplace.Add(item.SourceTemplate.ReplaceItem);
                    }
                }
            }

            var toRemoveGeneral = _tickToRemove.Where(x => !x.SourceTemplate.Cash).Select(x => x.getItemId()).ToArray();
            if (toRemoveGeneral.Length > 0)
            {
                await Owner.SendPacket(MessagePacket.GeneralItemExpireMessage(toRemoveGeneral));
            }

            await Owner.SyncClientInventory(_tickToSync);

            foreach (var item in _tickToReplace)
            {
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(item.Message))
                    {
                        await Owner.Notice(item.Message);
                    }
                    await Owner.GainItem(item.ItemId, 1,
                          expires: item.Period.GetExpirationFromMinutes());
                }
            }

            if (getType() == InventoryType.CASH)
            {
                Owner.CalculateCoupon(now);
            }
        }

        protected virtual Task OnTickItem(long now, Item item, List<Item> toUpdate, List<Item> toRemove) => Task.CompletedTask;

        /// <summary>
        /// 加入背包
        /// </summary>
        /// <param name="position"></param>
        /// <param name="item"></param>
        /// <param name="fromLogin"></param>
        protected virtual Task OnItemEnter(short position, Item item, bool fromLogin)
        {
            if (item.getExpiration() != -1)
            {
                _timedItems.Add(new TimedItemWrapper(item, item.getExpiration()));
            }
            else if (item.SourceTemplate is CouponItemTemplate c && c.TimeRangeF.Length > 0)
            {
                // tick > now的不会触发
                _timedItems.Add(new TimedItemWrapper(item, 0));
            }
            return Task.CompletedTask;
        }
        /// <summary>
        /// 从背包移除
        /// </summary>
        /// <param name="item"></param>
        protected virtual Task OnItemLeave(Item item)
        {
            _timedItems.RemoveWhere(x => x.Item == item);
            return Task.CompletedTask;
        }

        public abstract Task PutItem(short position, Item item, bool fromLogin);

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
        public async Task<(IInventoryOperationCommand?, short actualRemoved)> removeItem(short slot, short quantity = 1, bool allowZero = false)
        {
            var item = getItem(slot);
            if (item == null)
            {
                // TODO is it ok not to throw an exception here?
                return (null, 0);
            }

            if (quantity < 0)
            {
                return (null, 0);
            }

            var original = item.getQuantity();
            var left = original - quantity;
            short actualRemoved;

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
                op = await removeSlot(slot);
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
            return (op, actualRemoved);
        }
        public abstract Task<IInventoryOperationCommand?> removeSlot(short slot);
        #endregion

        public abstract void Dispose();


        public abstract IEnumerator<Item> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
