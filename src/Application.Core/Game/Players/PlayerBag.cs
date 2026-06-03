using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.Core.Game.Items;
using Application.Core.Models;
using Application.Templates.Item;
using Application.Utility.Tickables;
using client.inventory;
using System.Runtime.ConstrainedExecution;
using tools;

namespace Application.Core.Game.Players
{
    public class PlayerBag : IDisposable, ILoopTickable
    {
        readonly AbstractInventory[] _dataSource;
        private bool disposedValue;
        Player Owner { get; }

        public PlayerBag(Player owner)
        {
            Owner = owner;
            var typeList = EnumCache<InventoryType>.Values;
            _dataSource = new AbstractInventory[typeList.Length];

            for (int i = 0; i < typeList.Length; i++)
            {
                byte b = DefaultConfigs.BagSize;
                var type = typeList[i];
                switch (type)
                {
                    case InventoryType.EQUIP:
                        b = (byte)Owner.Equipslots;
                        _dataSource[i] = new Inventory(owner, type, b);
                        break;
                    case InventoryType.USE:
                        b = (byte)Owner.Useslots;
                        _dataSource[i] = new Inventory(owner, type, b);
                        break;
                    case InventoryType.SETUP:
                        b = (byte)Owner.Setupslots;
                        _dataSource[i] = new Inventory(owner, type, b);
                        break;
                    case InventoryType.ETC:
                        b = (byte)Owner.Etcslots;
                        _dataSource[i] = new Inventory(owner, type, b);
                        break;
                    case InventoryType.CASH:
                        b = DefaultConfigs.BagCashSize;
                        _dataSource[i] = new Inventory(owner, type, b);
                        break;
                    case InventoryType.CANHOLD:
                        _dataSource[InventoryType.CANHOLD.ordinal()] = new InventoryProof(owner);
                        break;
                    case InventoryType.EQUIPPED:
                        _dataSource[InventoryType.EQUIPPED.ordinal()] = new InventoryEquipped(owner);
                        break;
                    default:
                        b = byte.MaxValue;
                        _dataSource[i] = new Inventory(owner, type, b);
                        break;
                }
            }
        }

        public AbstractInventory this[InventoryType type] => this[type.ordinal()];
        public AbstractInventory this[int typeOrdinal] => _dataSource[typeOrdinal];


        public AbstractInventory[] GetValues()
        {
            return _dataSource.Where((x, i) => i != InventoryType.CANHOLD.ordinal() && i != InventoryType.UNDEFINED.ordinal()).ToArray();
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
                        _dataSource[i].Dispose();
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

        void RemoveItemInternal(AbstractInventory inv, Item invItem, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            bool allowZero = consume && ItemConstants.isRechargeable(invItem.getItemId());
            var removeRes = inv.removeItem(invItem.getPosition(), out var actualRemoved, quantity, allowZero);
            if (removeRes != null)
            {
                if (inv.getType() != InventoryType.CANHOLD)
                {
                    Owner.SyncClientInventory(removeRes, fromDrop);
                }

                if (showMessage && actualRemoved > 0)
                    Owner.sendPacket(PacketCreator.getShowItemGain(invItem.getItemId(), (short)-actualRemoved, true));
            }
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
        public bool TryRemoveFromSlot(InventoryType type, short slot, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            var inv = this[type];
            var item = inv.getItem(slot);
            if (item == null)
                return false;

            return TryRemoveFromItem(type, item, quantity, fromDrop, consume, showMessage);
        }

        /// <summary>
        /// 移除type栏中的物品，数量不足则移除失败
        /// </summary>
        /// <param name="type"></param>
        /// <param name="invItem">必须来自<see cref="Inventory"/></param>
        /// <param name="quantity"></param>
        /// <param name="fromDrop"></param>
        /// <param name="consume"></param>
        /// <returns></returns>
        public bool TryRemoveFromItem(InventoryType type, Item invItem, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            var inv = this[type];
            if (invItem.getQuantity() < quantity)
                return false;

            if (!inv.Contains(invItem))
                return false;

            RemoveItemInternal(inv, invItem, quantity, fromDrop, consume, showMessage);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="invType"></param>
        /// <param name="toRemoveCount">须为正数</param>
        /// <param name="filter"></param>
        /// <param name="fromDrop"></param>
        /// <param name="consume"></param>
        /// <param name="showMessage"></param>
        public void RemoveFromInventory(InventoryType invType, int toRemoveCount = int.MaxValue, Func<Item, bool>? filter = null, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            var inv = this[invType];

            List<ItemRemovedRecord> modifiedItems = [];
            List<IInventoryOperationCommand> ops = [];

            foreach (var p in inv.LoadAllItem())
            {
                if (toRemoveCount <= 0)
                    return;

                if (filter == null || filter(p.Item))
                {
                    bool allowZero = consume && ItemConstants.isRechargeable(p.Item.getItemId());
                    var removeRes = inv.removeItem(p.Slot, out var removedCount, toRemoveCount >= short.MaxValue ? short.MaxValue : (short)toRemoveCount, allowZero);
                    if (removeRes != null)
                    {
                        ops.Add(removeRes);

                        toRemoveCount -= removedCount;
                        modifiedItems.Add(new ItemRemovedRecord(p.Item.getItemId(), removedCount));
                    }
                }
            }

            if (invType != InventoryType.CANHOLD)
            {
                Owner.SyncClientInventory(ops, fromDrop);
            }

            if (showMessage)
            {
                var showData = modifiedItems.GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.Sum(x => x.RemovedCount));
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
                    foreach (var inv in GetValues())
                    {
                        foreach (Item item in inv.list())
                        {
                            long expiration = item.getExpiration();

                            if (expiration != -1 && (expiration < now))
                            {
                                if (item is Pet pet)
                                {
                                    pet.MapPet?.Recall(2);

                                    if (pet.SourceTemplate.NoRevive)
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

                            if (inv.getType() == InventoryType.EQUIPPED && item.getItemId() == ItemId.PENDANT_OF_THE_SPIRIT)
                            {
                                Owner.CalculateSpiritPendant(now, false);
                            }
                        }

                        if (toberemove.Count > 0)
                        {
                            foreach (Item item in toberemove)
                            {
                                TryRemoveFromSlot(inv.getType(), item.getPosition(), item.getQuantity(), true);
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
