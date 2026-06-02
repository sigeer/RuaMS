using Application.Core.Channel.DataProviders;
using Application.Core.Server;
using client.inventory;

namespace Application.Core.Client.inventory
{
    public abstract class InventorySorter
    {
        protected abstract Item? GetItem(short slot);
        protected abstract void Swap(short sSlot, short dSlot);
        protected abstract void RemoveSlot(short slot);

        protected abstract short GetSize();

        protected abstract short GetSlotMax(int itemId);

        protected abstract short GetNextFreeSlot();
        protected abstract List<Item?> GetAllItems();

        /// <summary>
        /// 合并
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<IInventoryOperationCommand> Merge()
        {
            List<IInventoryOperationCommand> allOperation = [];

            Item? srcItem, dstItem;

            for (short dst = 1; dst <= GetSize(); dst++)
            {
                dstItem = GetItem(dst);
                if (dstItem == null)
                {
                    continue;
                }

                for (short src = (short)(dst + 1); src <= GetSize(); src++)
                {
                    srcItem = GetItem(src);
                    if (srcItem == null)
                    {
                        continue;
                    }

                    if (dstItem.getItemId() != srcItem.getItemId())
                    {
                        continue;
                    }

                    if (dstItem.getQuantity() == GetSlotMax(dstItem.getItemId()))
                    {
                        break;
                    }

                    allOperation.AddRange(Move(src, dst));
                }
            }


            bool sorted = false;
            while (!sorted)
            {
                short freeSlot = GetNextFreeSlot();

                if (freeSlot != -1)
                {
                    short itemSlot = -1;
                    for (short i = (short)(freeSlot + 1); i <= GetSize(); i = (short)(i + 1))
                    {
                        if (GetItem(i) != null)
                        {
                            itemSlot = i;
                            break;
                        }
                    }
                    if (itemSlot > 0)
                    {
                        allOperation.AddRange(Move(itemSlot, freeSlot));
                    }
                    else
                    {
                        sorted = true;
                    }
                }
                else
                {
                    sorted = true;
                }
            }
            return allOperation;
        }

        public List<IInventoryOperationCommand> Move(short sSlot, short dSlot)
        {
            Item? source = GetItem(sSlot);
            Item? target = GetItem(dSlot);
            if (source == null)
            {
                return [];
            }

            var type = source.getInventoryType();
            if (target == null)
            {
                Swap(sSlot, dSlot);

                return [new InventoryMove(type, sSlot, dSlot)];
            }
            else if (target.getItemId() == source.getItemId() && !ItemConstants.isRechargeable(source.getItemId()) && isSameOwner(source, target))
            {
                var slotMax = GetSlotMax(target.getItemId());

                if (type.getType() == InventoryType.EQUIP.getType() || type.getType() == InventoryType.CASH.getType())
                {
                    Swap(dSlot, sSlot);
                    return [new InventoryMove(type, sSlot, dSlot)];
                }
                else if (source.getQuantity() + target.getQuantity() > slotMax)
                {
                    short rest = (short)((source.getQuantity() + target.getQuantity()) - slotMax);
                    source.setQuantity(rest);
                    target.setQuantity(slotMax);

                    return [new InventoryUpdateQuantity(type, sSlot, rest), new InventoryUpdateQuantity(type, dSlot, slotMax)];
                }
                else
                {
                    var nextQuantity = (short)(source.getQuantity() + target.getQuantity());
                    target.setQuantity(nextQuantity);

                    RemoveSlot(sSlot);

                    return [new InventoryRemove(type, sSlot), new InventoryUpdateQuantity(type, dSlot, nextQuantity)];
                }
            }
            else
            {
                Swap(dSlot, sSlot);
                return [new InventoryMove(type, sSlot, dSlot)];
            }
        }


        private static bool isSameOwner(Item source, Item target)
        {
            return source.getOwner().Equals(target.getOwner());
        }

        protected List<Item?> SortInternal(List<Item?> inventoryItems, int sort, int thenSort)
        {
            if (inventoryItems.Count == 0)
                return inventoryItems;

            // 分离 null 和非 null
            var items = inventoryItems.OfType<Item>().ToList();
            var nullItems = inventoryItems.Where(x => x == null).ToList();

            if (items.Count == 0)
                return inventoryItems; // 全为 null

            // 对非 null 部分排序
            IOrderedEnumerable<Item>? sorted = null;
            // 注意：这里需要将 Item? 转换为 Item，因为已经过滤了 null

            var ii = ItemInformationProvider.getInstance();
            if (sort == 1)
                sorted = items.OrderBy(x => x.getQuantity());
            else if (sort == 2)
                sorted = items.OrderBy(x => ii.getName(x.getItemId()));
            else if (sort == 3)
                sorted = items.OrderBy(x => ((Equip)x).getLevel());
            else
                sorted = items.OrderBy(x => x.GetSortKey()); // 默认排序

            if (thenSort == 1)
                sorted = sorted.ThenBy(x => x.getQuantity());
            else if (thenSort == 2)
                sorted = sorted.ThenBy(x => ii.getName(x.getItemId()));
            else if (thenSort == 3)
                sorted = sorted.ThenBy(x => ((Equip)x).getLevel());

            var result = sorted.ToList<Item?>(); // 转为 List<Item?>
            result.AddRange(nullItems);
            return result;
        }

        /// <summary>
        /// 根据初始数组和目标数组，生成交换步骤序列。
        /// 要求：两个数组的元素类型 T 应正确实现 Equals 和 GetHashCode，
        /// 且元素具有唯一性（或可以区分），否则结果可能出错。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="initial">初始数组</param>
        /// <param name="final">目标数组</param>
        /// <returns>交换步骤列表，每个元素为 (位置A, 位置B)</returns>
        public static List<(int, int)> GetSwapSteps<T>(IList<T> initial, IList<T> final)
        {
            if (initial.Count != final.Count)
                throw new ArgumentException("数组长度必须相同");

            T[] work = new T[initial.Count];
            initial.CopyTo(work, 0);

            var comparer = EqualityComparer<T>.Default;
            var steps = new List<(int, int)>();

            for (int i = 0; i < work.Length; i++)
            {
                if (!comparer.Equals(work[i], final[i]))
                {
                    // 查找 final[i] 在当前 work 中的位置
                    int j = i + 1;
                    for (; j < work.Length; j++)
                    {
                        if (comparer.Equals(work[j], final[i]))
                            break;
                    }
                    if (j == work.Length)
                        throw new InvalidOperationException("无法找到匹配的元素");

                    // 交换
                    Swap(work, i, j);
                    steps.Add((i, j));
                }
            }

            return steps;
        }

        private static void Swap<T>(T[] array, int i, int j)
        {
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

    }

    public class BagInventorySorter : InventorySorter
    {
        Inventory _inv;

        public BagInventorySorter(Inventory inv)
        {
            _inv = inv;
        }

        protected override short GetSize() => _inv.getSlotLimit();

        protected override short GetSlotMax(int itemId) => ItemInformationProvider.getInstance().getSlotMax(_inv.Owner.Client, itemId);

        protected override Item? GetItem(short slot)
        {
            return _inv.getItem(slot);
        }

        protected override void RemoveSlot(short slot)
        {
            _inv.RemoveFromMove(slot);
        }

        protected override short GetNextFreeSlot() => _inv.getNextFreeSlot();

        protected override void Swap(short sSlot, short dSlot)
        {
            _inv.SwapFromMove(sSlot, dSlot);
        }

        protected override List<Item?> GetAllItems()
        {
            return _inv.LoadAllSlot();
        }

        public List<InventoryMove> Sort()
        {
            var originalItems = GetAllItems();

            int invTypeCriteria = (_inv.getType() == InventoryType.EQUIP) ? 3 : 1;
            int sortCriteria = YamlConfig.config.server.USE_ITEM_SORT_BY_NAME ? 2 : 0;
            var final = SortInternal(originalItems, sortCriteria, invTypeCriteria);

            for (int i = 0; i < final.Count; i++)
            {
                _inv.SetItemPosition(final[i], Inventory.MapClientSlot((short)i));
            }

            var steps = GetSwapSteps(originalItems, final);
            return steps.Select(x => new InventoryMove(_inv.getType(), Inventory.MapClientSlot((short)x.Item1), Inventory.MapClientSlot((short)x.Item2))).ToList();
        }
    }

    public class StorageSorter : InventorySorter
    {
        AbstractStorage _storage;
        Item?[] _items;

        public StorageSorter(AbstractStorage storage)
        {
            _storage = storage;

            _items = _storage.GetItems().ToArray();
        }

        protected override Item? GetItem(short slot)
        {
            return _items[slot];
        }

        protected override short GetNextFreeSlot()
        {
            for (short i = 0; i < _items.Length; i++)
            {
                if (_items[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        protected override short GetSize()
        {
            return (short)_items.Length;
        }

        protected override short GetSlotMax(int itemId) => ItemInformationProvider.getInstance().getSlotMax(_storage.Owner.Client, itemId);

        protected override void RemoveSlot(short slot)
        {
            _items[slot] = null;
        }

        protected override void Swap(short sSlot, short dSlot)
        {
            (_items[sSlot], _items[dSlot]) = (_items[dSlot], _items[sSlot]);
        }

        protected override List<Item?> GetAllItems()
        {
            return _items.ToList();
        }

        public List<Item> Sort()
        {
            var originalItems = GetAllItems();

            int invTypeCriteria = 1;
            int sortCriteria = YamlConfig.config.server.USE_ITEM_SORT_BY_NAME ? 2 : 0;
            return SortInternal(originalItems, sortCriteria, invTypeCriteria).OfType<Item>().ToList();
        }
    }
}
