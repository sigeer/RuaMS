using Acornima;
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

    public override IEnumerable<Item> ListExsitedEnumerable()
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
    public override void PutItem(short position, Item item, bool fromLogin)
    {
        if (position >= 0 || item is not Equip eqp)
        {
            return;
        }

        SetItemPosition(eqp, position);
        OnItemEnter(position, item, fromLogin);
    }

    public Equip? Equip(short slot, Equip newEqp)
    {
        var oldEquip = getItem(slot);
        if (oldEquip != null && oldEquip.NeedRecalcEffect(newEqp))
        {
            removeSlot(slot);
        }
        SetItemPosition(newEqp, slot);
        OnItemEnter(slot, newEqp, false);
        return oldEquip;
    }

    protected override void OnItemEnter(short position, Item item, bool fromLogin)
    {
        if (item is not client.inventory.Equip equip)
        {
            return;
        }
        int itemid = equip.getItemId();

        if (itemid == ItemId.PENDANT_OF_THE_SPIRIT)
        {
            _timedItems.Add(new TimedItemWrapper(item, 0));
        }

        Owner.getRingById(equip.getRingId())?.equip();

        if (!fromLogin)
        {
            // 登录后进入地图时会广播
            var petIndex = EquipSlot.PetsNameTag.IndexOf(equip.getPosition());
            if (petIndex != -1)
            {
                Owner.getPet(petIndex)?.BroadcastNameChanged();
            }
        }

        base.OnItemEnter(position, item, fromLogin);
    }

    protected override void OnItemLeave(Item item)
    {
        if (item is not client.inventory.Equip equip)
        {
            return;
        }
        int itemid = equip.getItemId();

        if (itemid == ItemId.PENDANT_OF_THE_SPIRIT)
        {
            Owner.CalculateSpiritPendant(Owner.Client.CurrentServer.Node.getCurrentTime(), false);
        }

        Owner.getRingById(equip.getRingId())?.unequip();

        var petIndex = EquipSlot.PetsNameTag.IndexOf(equip.getPosition());
        if (petIndex != -1)
        {
            Owner.getPet(petIndex)?.BroadcastNameChanged();
        }
        base.OnItemLeave(item);
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
            OnItemLeave(item);

            return new InventoryRemove(item!.getInventoryType(), slot);
        }
        return null;
    }

    public override IEnumerator<Item> GetEnumerator()
    {
        foreach (var item in inventory.Values)
            if (item != null)
                yield return item;
    }

    public bool IsChecked { get; set; }

    public override void Dispose()
    {
        IsChecked = false;
        inventory.Clear();
    }

    protected override void OnTickItem(long now, Item item, List<Item> toUpdate, List<Item> toRemove)
    {
        if (item.getItemId() == ItemId.PENDANT_OF_THE_SPIRIT)
        {
            Owner.CalculateSpiritPendant(now, true);
        }
    }
}

public struct InventoryEnumerator : IEnumerator<Item>
{
    private readonly List<Item> _items;
    private int _currentIndex;

    public InventoryEnumerator(List<Item> items)
    {
        _items = items;
        _currentIndex = -1;
    }

    public Item Current => _items[_currentIndex];

    object System.Collections.IEnumerator.Current => _items[_currentIndex];

    public bool MoveNext() => ++_currentIndex < _items.Count;

    public void Reset() => _currentIndex = -1;

    public void Dispose() { }
}
