using Application.Core.Client.inventory;

namespace client.inventory;


public class InventoryEquipped : AbstractInventory
{
    /// <summary>
    /// Slot（从1开始） - Item
    /// </summary>
    protected Dictionary<short, Equip> inventory;

    public InventoryEquipped(Player mc) : base(mc, InventoryType.EQUIPPED)
    {
        this.inventory = new();
    }

    #region Slots
    public override bool CanGainSlot(short slots)
    {
        return false;
    }

    public override byte getSlotLimit()
    {
        return byte.MinValue;
    }

    public override void setSlotLimit(int newLimit)
    {
    }

    public override short getNextFreeSlot()
    {
        return -1;
    }

    public override short getNumFreeSlot()
    {
        return 0;
    }
    #endregion


    public override List<Item> list()
    {
        return ListExsitedEnumerable().ToList();
    }

    protected override IEnumerable<Item> ListExsitedEnumerable()
    {
        return inventory.Values;
    }

    public override List<InventoryItem> LoadAllItem()
    {
        return LoadAllItemEnumerable().ToList();
    }

    public override IEnumerable<InventoryItem> LoadAllItemEnumerable()
    {
        return inventory.Select(kw => new InventoryItem(kw.Key, kw.Value));
    }

    void SetItemPosition(Equip item, short pos)
    {
        item.setPosition(pos);
        inventory[pos] = item;
        item.PlayerInventory = this;
    }

    /// <summary>
    /// 初始化插入
    /// </summary>
    /// <param name="position"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public override void PutItem(short position, Item item)
    {
        if (position >= 0 || item is not Equip eqp)
        {
            return;
        }

        SetItemPosition(eqp, position);

        Owner.equippedItem(eqp);
    }

    public Equip? Equip(short slot, Equip newEqp)
    {
        var oldEquip = getItem(slot);

        SetItemPosition(newEqp, slot);

        if (oldEquip == null)
        {
            Owner.equippedItem(newEqp, true);
        }
        else
        {
            if (oldEquip.NeedRecalcEffect(newEqp))
            {
                Owner.unequippedItem(oldEquip);
                Owner.equippedItem(newEqp, true);
            }

        }
        return oldEquip;
    }

    public override void RemoveFromMove(short slot)
    {
        inventory.Remove(slot);
    }

    public override void SwapFromMove(short sSlot, short dSlot)
    {
        var dItem = inventory.GetValueOrDefault(dSlot);
        var sItem = inventory.GetValueOrDefault(sSlot);
        if (dItem != null)
        {
            SetItemPosition(dItem, sSlot);
        }
        else
        {
            RemoveFromMove(sSlot);
        }

        if (sItem != null)
        {
            SetItemPosition(sItem, dSlot);
        }
        else
        {
            RemoveFromMove(dSlot);
        }
    }
    public override Equip? getItem(short slot)
    {
        return inventory.GetValueOrDefault(slot);
    }

    /// <summary>
    /// 是否装备了<paramref name="slot"/>
    /// </summary>
    /// <param name="slot">槽位 <see cref="EquipSlot"/></param>
    /// <returns></returns>
    public bool HasEquipped(short slot)
    {
        return inventory.ContainsKey(slot);
    }

    public override IInventoryOperationCommand? removeSlot(short slot)
    {
        if (inventory.Remove(slot, out var item))
        {
            Owner.unequippedItem(item);

            return new InventoryRemove(item!.getInventoryType(), slot);
        }
        return null;
    }

    public override IEnumerator<Item> GetEnumerator()
    {
        return new InventoryEnumerator(list());
    }

    public bool IsChecked { get; set; }

    public override void Dispose()
    {
        IsChecked = false;
        inventory.Clear();
    }
}

public class InventoryEnumerator : IEnumerator<Item>
{
    int _currentIndex = -1;
    List<Item> _items;

    public InventoryEnumerator(List<Item> items)
    {
        _items = items;
    }

    public Item Current => _items.ElementAt(_currentIndex);

    object System.Collections.IEnumerator.Current => _items.ElementAt(_currentIndex);

    public bool MoveNext()
    {
        return ++_currentIndex < _items.Count;
    }

    public void Reset()
    {
        _currentIndex = -1;
    }

    public void Dispose()
    {
        Reset();
    }
}
