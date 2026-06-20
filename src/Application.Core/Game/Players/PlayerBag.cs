using Application.Core.Client.inventory;
using Application.Core.Models;
using Application.Templates.Item;
using Application.Utility.Tickables;
using client.inventory;
using tools;

namespace Application.Core.Game.Players
{
    public class PlayerBag : IDisposable, ILoopTickable
    {
        readonly AbstractInventory[] _dataSource;
        private bool disposedValue;
        Player Owner { get; }

        public PlayerBag(Player owner, int equipSlots, int useSlots, int setupSlots, int etcSlots)
        {
            Owner = owner;
            var typeList = EnumCache<InventoryType>.Values;
            _dataSource = new AbstractInventory[typeList.Length];

            for (int i = 0; i < typeList.Length; i++)
            {
                var type = typeList[i];
                switch (type)
                {
                    case InventoryType.EQUIP:
                        _dataSource[i] = new Inventory(owner, type, (byte)equipSlots);
                        break;
                    case InventoryType.USE:
                        _dataSource[i] = new Inventory(owner, type, (byte)useSlots);
                        break;
                    case InventoryType.SETUP:
                        _dataSource[i] = new Inventory(owner, type, (byte)setupSlots);
                        break;
                    case InventoryType.ETC:
                        _dataSource[i] = new Inventory(owner, type, (byte)etcSlots);
                        break;
                    case InventoryType.CASH:
                        _dataSource[i] = new Inventory(owner, type, DefaultConfigs.BagCashSize);
                        break;
                    case InventoryType.CANHOLD:
                        _dataSource[InventoryType.CANHOLD.ordinal()] = new InventoryProof(owner);
                        break;
                    case InventoryType.EQUIPPED:
                        _dataSource[InventoryType.EQUIPPED.ordinal()] = new InventoryEquipped(owner);
                        break;
                    default:
                        _dataSource[i] = new Inventory(owner, type, 0);
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

        async Task RemoveItemInternal(AbstractInventory inv, Item invItem, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            bool allowZero = consume && ItemConstants.isRechargeable(invItem.getItemId());
            var (removeRes, actualRemoved) = await inv.removeItem(invItem.getPosition(), quantity, allowZero);
            if (removeRes != null)
            {
                if (inv.getType() != InventoryType.CANHOLD)
                {
                    await Owner.SyncClientInventory(removeRes, fromDrop);
                }

                if (showMessage && actualRemoved > 0)
                    await Owner.SendPacket(PacketCreator.getShowItemGain(invItem.getItemId(), (short)-actualRemoved, true));
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
        public async Task<bool> TryRemoveFromSlot(InventoryType type, short slot, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            var inv = this[type];
            var item = inv.getItem(slot);
            if (item == null)
                return false;

            return await TryRemoveFromItem(type, item, quantity, fromDrop, consume, showMessage);
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
        public async Task<bool> TryRemoveFromItem(InventoryType type, Item invItem, short quantity = 1, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            var inv = this[type];
            if (invItem.getQuantity() < quantity)
                return false;

            if (!inv.Contains(invItem))
                return false;

            await RemoveItemInternal(inv, invItem, quantity, fromDrop, consume, showMessage);

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
        public async Task RemoveFromInventory(InventoryType invType, int toRemoveCount = int.MaxValue, Func<Item, bool>? filter = null, bool fromDrop = true, bool consume = false, bool showMessage = false)
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
                    var (removeRes, removedCount) = await inv.removeItem(p.Slot, toRemoveCount >= short.MaxValue ? short.MaxValue : (short)toRemoveCount, allowZero);
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
                await Owner.SyncClientInventory(ops, fromDrop);
            }

            if (showMessage)
            {
                var showData = modifiedItems.GroupBy(x => x.ItemId).ToDictionary(x => x.Key, x => x.Sum(x => x.RemovedCount));
                foreach (var data in showData)
                {
                    await Owner.SendPacket(PacketCreator.getShowItemGain(data.Key, (short)-data.Value, true));
                }
            }
        }

        /// <summary>
        /// 移除背包中所有组队任务道具
        /// </summary>
        /// <param name="chr"></param>
        public async Task ClearPartyQuestItems()
        {
            InventoryType[] includedInv = [InventoryType.USE, InventoryType.ETC];
            await BatchRemoveFromInventory(includedInv, item => (item.SourceTemplate as ItemTemplateBase)?.PartyQuest ?? false, showMessage: true);
        }

        /// <summary>
        /// 离线时移除
        /// </summary>
        public async Task ClearWhenLogout()
        {
            InventoryType[] includedInv = [InventoryType.EQUIP, InventoryType.EQUIPPED, InventoryType.SETUP, InventoryType.ETC];
            await BatchRemoveFromInventory(includedInv, item => item.SourceTemplate.ExpireOnLogout, showMessage: true);
        }

        public async Task BatchRemoveFromInventory(InventoryType[] inventoryTypes, Func<Item, bool>? filter = null, bool fromDrop = true, bool consume = false, bool showMessage = false)
        {
            foreach (var type in inventoryTypes)
            {
                await RemoveFromInventory(type, int.MaxValue, filter, fromDrop, consume, showMessage);
            }
        }

        public long Next { get; private set; }

        public long Period { get; } = 60_000;

        public TickableStatus Status { get; private set; }

        public async Task OnTick(long now)
        {
            if (this.IsAvailable())
            {
                if (Next <= now)
                {
                    foreach (var inv in GetValues())
                    {
                        await inv.OnTick(now);
                    }

                    Next = now + Period;
                }
            }
        }
    }
}
