using Application.Core.Channel.DataProviders;
using Application.Core.Game.Items;
using Application.Core.Models;
using Application.Shared.Constants.Item;
using Application.Templates.Item;
using Application.Utility.Tickables;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Players
{
    public class PlayerBag : IDisposable, ILoopTickable
    {
        readonly Inventory[] _dataSource;
        private bool disposedValue;
        Player Owner { get; }

        public PlayerBag(Player owner)
        {
            Owner = owner;
            var typeList = EnumCache<InventoryType>.Values;
            _dataSource = new Inventory[typeList.Length];

            for (int i = 0; i < typeList.Length; i++)
            {
                var type = typeList[i];
                byte b = DefaultConfigs.BagSize;
                if (type == InventoryType.CASH)
                {
                    b = DefaultConfigs.BagCashSize;
                }
                _dataSource[i] = new Inventory(owner, type, b);
            }
            _dataSource[InventoryType.CANHOLD.ordinal()] = new InventoryProof(owner);
        }

        public Inventory this[InventoryType type] => this[type.ordinal()];
        public Inventory this[int typeOrdinal] => _dataSource[typeOrdinal];

        public void SetValue(InventoryType type, Inventory inventory)
        {
            _dataSource[type.ordinal()] = inventory;
        }

        public Inventory[] GetValues()
        {
            return _dataSource;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    for (int i = 0; i < _dataSource.Length; i++)
                    {
                        _dataSource[i].dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~PlayerBag()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
        }

        void UnEquip(Inventory inv, Item invItem)
        {
            if (inv.getType() == InventoryType.EQUIPPED)
            {
                Owner.unequippedItem((Equip)invItem);
            }
            else
            {
                var petid = invItem.PetId;
                if (petid > -1)
                {
                    int petIdx = Owner.getPetIndex(petid);
                    if (petIdx > -1)
                    {
                        var pet = Owner.getPet(petIdx)!;
                        Owner.unequipPet(pet, true);
                    }
                }
            }
        }

        void RemoveItemInternal(Inventory inv, Item invItem, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            UnEquip(inv, invItem);

            bool allowZero = consume && ItemConstants.isRechargeable(invItem.getItemId());
            var removedCount = inv.removeItem(invItem.getPosition(), quantity, allowZero);
            if (inv.getType() != InventoryType.CANHOLD)
            {
                InventoryManipulator.AnnounceModifyInventory(Owner.Client, invItem, fromDrop, allowZero);
            }

            if (showMessage)
                Owner.sendPacket(PacketCreator.getShowItemGain(invItem.getItemId(), (short)-removedCount, true));
        }

        /// <summary>
        /// 移除type栏中slot上的物品quantity个
        /// </summary>
        /// <param name="type"></param>
        /// <param name="slot"></param>
        /// <param name="quantity"></param>
        /// <param name="fromDrop"></param>
        /// <param name="consume"></param>
        /// <returns></returns>
        public bool RemoveFromSlot(InventoryType type, short slot, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            Inventory inv = this[type];
            var item = inv.getItem(slot);
            if (item == null)
                return false;

            return RemoveFromItem(type, item, quantity, fromDrop, consume, showMessage);
        }

        /// <summary>
        /// 移除type栏中的物品
        /// </summary>
        /// <param name="type"></param>
        /// <param name="invItem">必须来自<see cref="Inventory"/></param>
        /// <param name="quantity"></param>
        /// <param name="fromDrop"></param>
        /// <param name="consume"></param>
        /// <returns></returns>
        public bool RemoveFromItem(InventoryType type, Item invItem, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            Inventory inv = this[type];
            if (invItem.getQuantity() < quantity)
                return false;

            if (!inv.Contains(invItem))
                return false;

            RemoveItemInternal(inv, invItem, quantity, fromDrop, consume, showMessage);

            return true;
        }

        public void RemoveFromInventory(InventoryType invType, int toRemoveCount = int.MaxValue, Func<Item, bool>? filter = null, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            var inv = this[invType];
            int slotLimit = inv.getSlotLimit();
            var type = inv.getType();

            List<ItemRemovedRecord> modifiedItems = [];
            for (short i = 0; i <= slotLimit; i++)
            {
                var item = inv.getItem((short)(type == InventoryType.EQUIPPED ? -i : i));
                if (item != null)
                {
                    if ((filter == null || filter(item)) && toRemoveCount > 0)
                    {
                        UnEquip(inv, item);

                        bool allowZero = consume && ItemConstants.isRechargeable(item.getItemId());
                        var removedCount = inv.removeItem(i, toRemoveCount >= short.MaxValue ? short.MaxValue : (short)toRemoveCount, allowZero);
                        toRemoveCount -= removedCount;

                        modifiedItems.Add(new ItemRemovedRecord(item, allowZero, removedCount));
                    }
                }
            }

            InventoryManipulator.AnnounceModifyInventory(Owner.Client, modifiedItems, fromDrop);

            if (showMessage)
            {
                var showData = modifiedItems.GroupBy(x => x.Item.getItemId()).ToDictionary(x => x.Key, x => x.Sum(x => x.RemovedCount));
                foreach (var data in showData)
                {
                    Owner.sendPacket(PacketCreator.getShowItemGain(data.Key, (short)-data.Value, true));
                }
            }
        }

        /// <summary>
        /// 移除背包中所有组队任务道具
        /// </summary>
        /// <param name="chr"></param>
        public void ClearPartyQuestItems()
        {
            InventoryType[] includedInv = [InventoryType.USE, InventoryType.ETC];
            BatchRemoveFromInventory(includedInv, item => (item.SourceTemplate as ItemTemplateBase)?.PartyQuest ?? false, showMessage: true);
        }

        /// <summary>
        /// 离线时移除
        /// </summary>
        public void ClearWhenLogout()
        {
            InventoryType[] includedInv = [InventoryType.EQUIP, InventoryType.EQUIPPED, InventoryType.SETUP, InventoryType.ETC];
            BatchRemoveFromInventory(includedInv, item => item.SourceTemplate.ExpireOnLogout, showMessage: true);
        }

        public void BatchRemoveFromInventory(InventoryType[] inventoryTypes, Func<Item, bool>? filter = null, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            foreach (var type in inventoryTypes)
            {
                RemoveFromInventory(type, int.MaxValue, filter, fromDrop, consume, showMessage);
            }
        }

        public long Next { get; private set; }

        public long Period { get; } = 60_000;

        public TickableStatus Status { get; private set; }

        public void OnTick(long now)
        {
            if (this.IsAvailable())
            {
                if (Next <= now)
                {
                    bool deletedCoupon = false;

                    List<Item> toberemove = new();
                    foreach (Inventory inv in GetValues())
                    {
                        foreach (Item item in inv.list())
                        {
                            long expiration = item.getExpiration();

                            if (expiration != -1 && (expiration < now))
                            {
                                if (item is Pet pet)
                                {
                                    Owner.unequipPet(pet, true);

                                    if (ItemConstants.isExpirablePet(item.getItemId()))
                                    {
                                        Owner.sendPacket(PacketCreator.itemExpired(item.getItemId()));
                                        toberemove.Add(item);
                                    }
                                    else
                                    {
                                        item.setExpiration(-1);
                                        Owner.forceUpdateItem(item);
                                    }
                                }
                                else
                                {
                                    Owner.sendPacket(PacketCreator.itemExpired(item.getItemId()));
                                    toberemove.Add(item);
                                    if (ItemConstants.isRateCoupon(item.getItemId()))
                                    {
                                        deletedCoupon = true;
                                    }
                                }

                                if ((item.getFlag() & ItemConstants.LOCK) == ItemConstants.LOCK)
                                {
                                    short lockObj = item.getFlag();
                                    lockObj &= ~(ItemConstants.LOCK);
                                    item.setFlag(lockObj); //Probably need a check, else people can make expiring items into permanent items...
                                    item.setExpiration(-1);
                                    Owner.forceUpdateItem(item);   //TEST :3
                                }
                            }
                        }

                        if (toberemove.Count > 0)
                        {
                            foreach (Item item in toberemove)
                            {
                                RemoveFromSlot(inv.getType(), item.getPosition(), item.getQuantity(), true);
                            }

                            ItemInformationProvider ii = ItemInformationProvider.getInstance();
                            foreach (Item item in toberemove)
                            {
                                var replace = ii.GetReplaceItemTemplate(item.getItemId());
                                if (replace != null)
                                {
                                    if (!string.IsNullOrEmpty(replace.Message))
                                    {
                                        Owner.Notice(replace.Message);
                                    }
                                    Owner.GainItem(replace.ItemId, 1,
                                        expires: replace.Period.GetExpirationFromMinutes());
                                }
                            }

                            toberemove.Clear();
                        }

                        if (deletedCoupon)
                        {
                            Owner.updateCouponRates();
                        }
                    }

                    Next = now + Period;
                }
            }
        }
    }
}
