/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Core.model;
using client.inventory.manipulator;
using constants.inventory;
using server;
using System.Collections;

namespace client.inventory;


/**
 * @author Matze, Ronan
 */
public class Inventory : IEnumerable<Item>
{
    ILogger log = LogFactory.GetLogger("Inventory");
    protected Dictionary<short, Item> inventory;
    protected InventoryType type;
    protected object lockObj = new object();

    protected IPlayer owner;
    protected byte slotLimit;
    protected bool isChecked = false;

    public Inventory(IPlayer mc, InventoryType type, byte slotLimit)
    {
        this.owner = mc;
        this.inventory = new();
        this.type = type;
        this.slotLimit = slotLimit;
    }

    public bool isExtendableInventory()
    { // not sure about cash, basing this on the previous one.
        return !(type.Equals(InventoryType.UNDEFINED) || type.Equals(InventoryType.EQUIPPED) || type.Equals(InventoryType.CASH));
    }

    public bool isEquipInventory()
    {
        return type.Equals(InventoryType.EQUIP) || type.Equals(InventoryType.EQUIPPED);
    }

    public byte getSlotLimit()
    {
        Monitor.Enter(lockObj);
        try
        {
            return slotLimit;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void setSlotLimit(int newLimit)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (newLimit < slotLimit)
            {
                List<short> toRemove = new();
                foreach (Item it in list())
                {
                    if (it.getPosition() > newLimit)
                    {
                        toRemove.Add(it.getPosition());
                    }
                }

                foreach (short slot in toRemove)
                {
                    removeSlot(slot);
                }
            }

            slotLimit = (byte)newLimit;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public IList<Item> list()
    {
        Monitor.Enter(lockObj);
        try
        {
            return inventory.Values.ToList();
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public Item? findById(int itemId)
    {
        return list().FirstOrDefault(x => x.getItemId() == itemId);
    }

    public Item? findByName(string name)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        foreach (Item item in list())
        {
            var itemName = ii.getName(item.getItemId());
            if (itemName == null)
            {
                log.Error("[CRITICAL] Item {ItemId} has no name", item.getItemId());
                continue;
            }

            if (name.Equals(itemName, StringComparison.OrdinalIgnoreCase))
            {
                return item;
            }
        }
        return null;
    }

    public int countById(int itemId)
    {
        int qty = 0;
        foreach (Item item in list())
        {
            if (item.getItemId() == itemId)
            {
                qty += item.getQuantity();
            }
        }
        return qty;
    }

    public int countNotOwnedById(int itemId)
    {
        int qty = 0;
        foreach (Item item in list())
        {
            if (item.getItemId() == itemId && item.getOwner().Equals(""))
            {
                qty += item.getQuantity();
            }
        }
        return qty;
    }

    public int freeSlotCountById(int itemId, int required)
    {
        List<Item> itemList = listById(itemId);
        int openSlot = 0;

        if (!ItemConstants.isRechargeable(itemId))
        {
            foreach (Item item in itemList)
            {
                required -= item.getQuantity();

                if (required >= 0)
                {
                    openSlot++;
                    if (required == 0)
                    {
                        return openSlot;
                    }
                }
                else
                {
                    return openSlot;
                }
            }
        }
        else
        {
            foreach (Item item in itemList)
            {
                required -= 1;

                if (required >= 0)
                {
                    openSlot++;
                    if (required == 0)
                    {
                        return openSlot;
                    }
                }
                else
                {
                    return openSlot;
                }
            }
        }

        return -1;
    }

    public List<Item> listById(int itemId)
    {
        List<Item> ret = new();
        foreach (Item item in list())
        {
            if (item.getItemId() == itemId)
            {
                ret.Add(item);
            }
        }

        if (ret.Count > 1)
        {
            ret.Sort((i1, i2) => i1.getPosition() - i2.getPosition());
        }

        return ret;
    }

    public List<Item> linkedListById(int itemId)
    {
        List<Item> ret = new();
        foreach (Item item in list())
        {
            if (item.getItemId() == itemId)
            {
                ret.Add(item);
            }
        }

        if (ret.Count > 1)
        {
            ret.Sort((i1, i2) => i1.getPosition() - i2.getPosition());
        }

        return ret;
    }

    public short addItem(Item item)
    {
        short slotId = addSlot(item);
        if (slotId == -1)
        {
            return -1;
        }
        item.setPosition(slotId);
        return slotId;
    }

    public void addItemFromDB(Item item)
    {
        if (item.getPosition() < 0 && !type.Equals(InventoryType.EQUIPPED))
        {
            return;
        }
        addSlotFromDB(item.getPosition(), item);
    }

    private static bool isSameOwner(Item source, Item target)
    {
        return source.getOwner().Equals(target.getOwner());
    }

    public void move(short sSlot, short dSlot, short slotMax)
    {
        Monitor.Enter(lockObj);
        try
        {
            Item? source = inventory.GetValueOrDefault(sSlot);
            Item? target = inventory.GetValueOrDefault(dSlot);
            if (source == null)
            {
                return;
            }
            if (target == null)
            {
                source.setPosition(dSlot);
                inventory.AddOrUpdate(dSlot, source);
                inventory.Remove(sSlot);
            }
            else if (target.getItemId() == source.getItemId() && !ItemConstants.isRechargeable(source.getItemId()) && isSameOwner(source, target))
            {
                if (type.getType() == InventoryType.EQUIP.getType() || type.getType() == InventoryType.CASH.getType())
                {
                    swap(target, source);
                }
                else if (source.getQuantity() + target.getQuantity() > slotMax)
                {
                    short rest = (short)((source.getQuantity() + target.getQuantity()) - slotMax);
                    source.setQuantity(rest);
                    target.setQuantity(slotMax);
                }
                else
                {
                    target.setQuantity((short)(source.getQuantity() + target.getQuantity()));
                    inventory.Remove(sSlot);
                }
            }
            else
            {
                swap(target, source);
            }
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    private void swap(Item source, Item target)
    {
        inventory.Remove(source.getPosition());
        inventory.Remove(target.getPosition());
        short swapPos = source.getPosition();
        source.setPosition(target.getPosition());
        target.setPosition(swapPos);
        inventory.AddOrUpdate(source.getPosition(), source);
        inventory.AddOrUpdate(target.getPosition(), target);
    }

    public Item? getItem(short slot)
    {
        Monitor.Enter(lockObj);
        try
        {
            return inventory.GetValueOrDefault(slot);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void removeItem(short slot)
    {
        removeItem(slot, 1, false);
    }

    public void removeItem(short slot, short quantity, bool allowZero)
    {
        var item = getItem(slot);
        if (item == null)
        {// TODO is it ok not to throw an exception here?
            return;
        }
        item.setQuantity((short)(item.getQuantity() - quantity));
        if (item.getQuantity() < 0)
        {
            item.setQuantity(0);
        }
        if (item.getQuantity() == 0 && !allowZero)
        {
            removeSlot(slot);
        }
    }

    public virtual short addSlot(Item item)
    {
        if (item == null)
        {
            return -1;
        }

        short slotId;
        Monitor.Enter(lockObj);
        try
        {
            slotId = getNextFreeSlot();
            if (slotId < 0)
            {
                return -1;
            }

            inventory.AddOrUpdate(slotId, item);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        if (ItemConstants.isRateCoupon(item.getItemId()))
        {
            // deadlocks with coupons rates found thanks to GabrielSin & Masterrulax
            ThreadManager.getInstance().newTask(() => owner.updateCouponRates());
        }

        return slotId;
    }

    public virtual void addSlotFromDB(short slot, Item item)
    {
        Monitor.Enter(lockObj);
        try
        {
            inventory.AddOrUpdate(slot, item);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        if (ItemConstants.isRateCoupon(item.getItemId()))
        {
            ThreadManager.getInstance().newTask(() => owner.updateCouponRates());
        }
    }

    public virtual void removeSlot(short slot)
    {
        Item? item;
        Monitor.Enter(lockObj);
        try
        {
            inventory.Remove(slot, out item);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }

        if (item != null && ItemConstants.isRateCoupon(item.getItemId()))
        {
            ThreadManager.getInstance().newTask(() => owner.updateCouponRates());
        }
    }

    public bool isFull()
    {
        Monitor.Enter(lockObj);
        try
        {
            return inventory.Count >= slotLimit;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public bool isFull(int margin)
    {
        Monitor.Enter(lockObj);
        try
        {
            //System.out.print("(" + inventory.Count + " " + margin + " <> " + slotLimit + ")");
            return inventory.Count + margin >= slotLimit;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public bool isFullAfterSomeItems(int margin, int used)
    {
        Monitor.Enter(lockObj);
        try
        {
            //System.out.print("(" + inventory.Count + " " + margin + " <> " + slotLimit + " -" + used + ")");
            return inventory.Count + margin >= slotLimit - used;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public short getNextFreeSlot()
    {
        if (isFull())
        {
            return -1;
        }

        Monitor.Enter(lockObj);
        try
        {
            for (short i = 1; i <= slotLimit; i++)
            {
                if (!inventory.ContainsKey(i))
                {
                    return i;
                }
            }
            return -1;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public short getNumFreeSlot()
    {
        if (isFull())
        {
            return 0;
        }

        Monitor.Enter(lockObj);
        try
        {
            short free = 0;
            for (short i = 1; i <= slotLimit; i++)
            {
                if (!inventory.ContainsKey(i))
                {
                    free++;
                }
            }
            return free;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    private static bool checkItemRestricted(List<ItemInventoryType> items)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        // thanks Shavit for noticing set creation that would be only effective in rare situations
        foreach (var p in items)
        {
            int itemid = p.Item.getItemId();
            if (ii.isPickupRestricted(itemid) && p.Item.getQuantity() > 1)
            {
                return false;
            }
        }

        return true;
    }

    public static bool checkSpot(IPlayer chr, Item item)
    {    // thanks Vcoc for noticing pshops not checking item stacks when taking item back
        return checkSpot(chr, Collections.singletonList(item));
    }

    public static bool checkSpot(IPlayer chr, List<Item> items)
    {
        List<ItemInventoryType> listItems = new();
        foreach (Item item in items)
        {
            listItems.Add(new(item, item.getInventoryType()));
        }

        return checkSpotsAndOwnership(chr, listItems);
    }

    public static bool checkSpots(IPlayer chr, List<ItemInventoryType> items)
    {
        return checkSpots(chr, items, false);
    }

    public static bool checkSpots(IPlayer chr, List<ItemInventoryType> items, bool useProofInv)
    {
        int invTypesSize = Enum.GetValues<InventoryType>().Length;
        List<int> zeroedList = new(invTypesSize);
        for (byte i = 0; i < invTypesSize; i++)
        {
            zeroedList.Add(0);
        }

        return checkSpots(chr, items, zeroedList, useProofInv);
    }

    public static bool checkSpots(IPlayer chr, List<ItemInventoryType> items, List<int> typesSlotsUsed, bool useProofInv)
    {
        // assumption: no "UNDEFINED" or "EQUIPPED" items shall be tested here, all counts are >= 0.

        if (!checkItemRestricted(items))
        {
            return false;
        }

        Dictionary<int, List<int>> rcvItems = new();
        Dictionary<int, sbyte> rcvTypes = new();

        foreach (var item in items)
        {
            int itemId = item.Item.getItemId();
            List<int>? qty = rcvItems.GetValueOrDefault(itemId);

            if (qty == null)
            {
                List<int> itemQtyList = new();
                itemQtyList.Add(item.Item.getQuantity());

                rcvItems.AddOrUpdate(itemId, itemQtyList);
                rcvTypes.AddOrUpdate(itemId, item.Type.getType());
            }
            else
            {
                if (!ItemConstants.isEquipment(itemId) && !ItemConstants.isRechargeable(itemId))
                {
                    qty.set(0, qty.get(0) + item.Item.getQuantity());
                }
                else
                {
                    qty.Add(item.Item.getQuantity());
                }
            }
        }

        var c = chr.getClient();
        foreach (var it in rcvItems)
        {
            int itemType = rcvTypes.GetValueOrDefault(it.Key) - 1;

            foreach (int itValue in it.Value)
            {
                int usedSlots = typesSlotsUsed.get(itemType);

                int result = InventoryManipulator.checkSpaceProgressively(c, it.Key, itValue, "", usedSlots, useProofInv);
                bool hasSpace = ((result % 2) != 0);

                if (!hasSpace)
                {
                    return false;
                }
                typesSlotsUsed.set(itemType, (result >> 1));
            }
        }

        return true;
    }

    private static long fnvHash32(string k)
    {
        uint FNV_32_INIT = 0x811c9dc5;
        uint FNV_32_PRIME = 0x01000193;

        uint rv = FNV_32_INIT;
        int len = k.Length;
        for (int i = 0; i < len; i++)
        {
            rv ^= k.ElementAt(i);
            rv *= FNV_32_PRIME;
        }

        return rv >= 0 ? rv : (2L * int.MaxValue) + rv;
    }

    private static long hashKey(int itemId, string owner)
    {
        return (itemId << 32) + fnvHash32(owner);
    }

    public static bool checkSpotsAndOwnership(IPlayer chr, List<ItemInventoryType> items)
    {
        return checkSpotsAndOwnership(chr, items, false);
    }

    public static bool checkSpotsAndOwnership(IPlayer chr, List<ItemInventoryType> items, bool useProofInv)
    {
        List<int> zeroedList = Enumerable.Repeat(0, 5).ToList();

        return checkSpotsAndOwnership(chr, items, zeroedList, useProofInv);
    }

    public static bool checkSpotsAndOwnership(IPlayer chr, List<ItemInventoryType> items, List<int> typesSlotsUsed, bool useProofInv)
    {
        //assumption: no "UNDEFINED" or "EQUIPPED" items shall be tested here, all counts are >= 0 and item list to be checked is a legal one.

        if (!checkItemRestricted(items))
        {
            return false;
        }

        Dictionary<long, GroupedItem> rcvInfo = new();

        foreach (var item in items)
        {
            long itemHash = hashKey(item.Item.getItemId(), item.Item.getOwner());
            var qty = rcvInfo.GetValueOrDefault(itemHash);

            if (qty == null)
            {
                rcvInfo.AddOrUpdate(itemHash, new GroupedItem(item, [item.Item.getQuantity()]));
            }
            else
            {
                // thanks BHB88 for pointing out an issue with rechargeable items being stacked on inventory check
                if (!ItemConstants.isEquipment(item.Item.getItemId()) && !ItemConstants.isRechargeable(item.Item.getItemId()))
                {
                    qty.GroupQuantity[0] += item.Item.getQuantity();
                }
                else
                {
                    qty.GroupQuantity.Add(item.Item.getQuantity());
                }
            }
        }

        var c = chr.getClient();
        foreach (var it in rcvInfo)
        {
            int itemId = (int)(it.Key >> 32);
            int itemType = it.Value.ItemInventoryType.Type.getType() - 1;

            foreach (int itValue in it.Value.GroupQuantity)
            {
                int usedSlots = typesSlotsUsed.get(itemType);

                //System.out.print("inserting " + itemId + " with type " + itemType + " qty " + it.getValue() + " owner '" + rcvOwners.get(it.Key) + "' current usedSlots:");
                //foreach(int i in typesSlotsUsed) System.out.print(" " + i);
                int result = InventoryManipulator.checkSpaceProgressively(c, itemId, itValue, it.Value.ItemInventoryType.Item.getOwner(), usedSlots, useProofInv);
                bool hasSpace = ((result % 2) != 0);
                //System.out.print(" -> hasSpace: " + hasSpace + " RESULT : " + result + "\n");

                if (!hasSpace)
                {
                    return false;
                }
                typesSlotsUsed.set(itemType, (result >> 1));
            }
        }

        return true;
    }

    public InventoryType getType()
    {
        return type;
    }

    public IEnumerator<Item> GetEnumerator()
    {
        return new InventoryEnumerator(list());
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Item? findByCashId(int cashId)
    {
        bool isRing = false;
        Equip equip = null;
        foreach (Item item in list())
        {
            if (item.getInventoryType().Equals(InventoryType.EQUIP))
            {
                equip = (Equip)item;
                isRing = equip.getRingId() > -1;
            }
            if ((item.getPetId() > -1 ? item.getPetId() : isRing ? equip.getRingId() : item.getCashId()) == cashId)
            {
                return item;
            }
        }

        return null;
    }

    public bool IsChecked()
    {
        Monitor.Enter(lockObj);
        try
        {
            return isChecked;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void SetChecked(bool yes)
    {
        Monitor.Enter(lockObj);
        try
        {
            isChecked = yes;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void lockInventory()
    {
        Monitor.Enter(lockObj);
    }

    public void unlockInventory()
    {
        Monitor.Exit(lockObj);
    }

    public void dispose()
    {

    }

}

public class InventoryEnumerator : IEnumerator<Item>
{
    int _currentIndex = -1;
    ICollection<Item> _items;

    public InventoryEnumerator(ICollection<Item> items)
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
