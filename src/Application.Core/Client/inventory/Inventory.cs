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


using Application.Core.Channel.DataProviders;
using Application.Core.Client.inventory;
using Application.Core.Game.Items;
using Application.Core.model;
using Application.Templates.Item.Cash;
using client.inventory.manipulator;
using System.Diagnostics;
using ZLinq;

namespace client.inventory;


/**
 * @author Matze, Ronan
 */
public class Inventory : AbstractInventory
{
    /// <summary>
    /// Slot（从1开始） - Item
    /// </summary>
    protected Item?[] inventory;

    public Inventory(Player mc, InventoryType type, byte slotLimit) : base(mc, type)
    {
        this.inventory = new Item?[slotLimit];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serverSlot"></param>
    /// <returns>Client Slot</returns>
    public static short MapClientSlot(short serverSlot) => ++serverSlot;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientSlot"></param>
    /// <returns>Server Slot</returns>
    public static short MapServerSlot(short clientSlot) => --clientSlot;

    public override bool CanGainSlot(short slots)
    {
        slots += getSlotLimit();
        return slots <= DefaultConfigs.BagMaxSize;
    }

    public override byte getSlotLimit()
    {
        return (byte)inventory.Length;
    }

    public override async Task setSlotLimit(int newLimit)
    {
        if (newLimit < inventory.Length)
        {
            for (int i = newLimit + 1; i <= inventory.Length; i++)
            {
                await removeSlot((short)i);
            }
        }

        Array.Resize(ref inventory, newLimit);
    }

    public bool isFull()
    {
        return !inventory.Any(x => x == null);
    }

    public bool isFull(int margin)
    {
        //System.out.print("(" + inventory.Count + " " + margin + " <> " + slotLimit + ")");
        return inventory.Count(x => x != null) + margin >= inventory.Length;
    }

    public bool isFullAfterSomeItems(int margin, int used)
    {
        //System.out.print("(" + inventory.Count + " " + margin + " <> " + slotLimit + " -" + used + ")");
        return inventory.Count(x => x != null) + margin >= inventory.Length - used;
    }

    public override short getNextFreeSlot()
    {
        var freeSlot = (short)Array.IndexOf(inventory, null);
        if (freeSlot == -1)
        {
            return -1;
        }

        return MapClientSlot(freeSlot);
    }

    public override short getNumFreeSlot()
    {
        return (short)inventory.Count(x => x == null);
    }

    public override List<Item> list()
    {
        return ListExsitedEnumerable().ToList();
    }

    public override IEnumerable<Item> ListExsitedEnumerable()
    {
        return inventory.OfType<Item>();
    }

    public override List<InventoryItem> LoadAllItem()
    {
        return LoadAllItemEnumerable().ToList();
    }

    public override IEnumerable<InventoryItem> LoadAllItemEnumerable()
    {
        for (short i = 0; i < inventory.Length; i++)
        {
            var item = inventory[i];
            if (item != null)
            {
                yield return new(MapClientSlot(i), item);
            }
        }
    }
    public List<Item?> LoadAllSlot()
    {
        return inventory.ToList();
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="pos">Client Slot</param>
    public void SetItemPosition(Item? item, short pos)
    {
        if (item != null)
        {
            item.setPosition(pos);
            item.PlayerInventory = this;
        }
        inventory[MapServerSlot(pos)] = item;
    }

    public async Task<InventoryAdd?> AddItem(Item item)
    {
        var clientSlot = getNextFreeSlot();
        if (clientSlot == -1)
        {
            return null;
        }

        Activity.Current?.AddEvent(
        new ActivityEvent(
            "AddItem",
            tags: new ActivityTagsCollection
            {
                ["Inventory"] = getType(),
                ["Slot"] = clientSlot,
                ["Item.Id"] = item.getItemId(),
                ["Item.Quantity"] = item.getQuantity()
            }));

        SetItemPosition(item, clientSlot);

        await OnItemEnter(clientSlot, item, false);

        return new InventoryAdd(item.getInventoryType(), item, clientSlot);
    }
    public override async Task PutItem(short position, Item item, bool fromLogin)
    {
        if (position <= 0)
        {
            //  throw new ArgumentException($"{nameof(InsertItem)}({position}, {item})");
            return;
        }

        SetItemPosition(item, position);

        await OnItemEnter(position, item, fromLogin);
    }

    public override void RemoveFromMove(short slot)
    {
        inventory[MapServerSlot(slot)] = null;
    }

    public override void SwapFromMove(short sSlot, short dSlot)
    {
        var dItem = getItem(dSlot);
        var sItem = getItem(sSlot);
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
    public override Item? getItem(short slot)
    {
        return inventory[MapServerSlot(slot)];
    }


    protected override async Task OnItemEnter(short position, Item item, bool fromLogin)
    {
        if (!fromLogin)
        {
            // 登录时会遍历背包处理
            if (item.SourceTemplate is CouponItemTemplate)
            {
                Owner.CalculateCoupon(Owner.Client.CurrentServer.Node.getCurrentTime());
            }
        }
        await base.OnItemEnter(position, item, fromLogin);
    }

    protected override async Task OnItemLeave(Item item)
    {
        if (item.SourceTemplate is CouponItemTemplate)
        {
            Owner.CalculateCoupon(Owner.Client.CurrentServer.Node.getCurrentTime());
        }
        else if (item is Pet pet)
        {
            pet.MapPet?.Recall();
        }
        await base.OnItemLeave(item);
    }

    public override async Task<IInventoryOperationCommand?> removeSlot(short slot)
    {
        var item = getItem(slot);
        if (item != null)
        {
            inventory[MapServerSlot(slot)] = null;

            await OnItemLeave(item);
            return new InventoryRemove(item!.getInventoryType(), slot);
        }
        return null;
    }

    public override void Dispose()
    {
        Array.Fill(inventory, null);
    }

    /// <summary>
    /// 从 <paramref name="slot"/> 处取 <paramref name="quantity"/> 生成 <paramref name="item"/>
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="quantity"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<(IInventoryOperationCommand?, Item?)> Take(short slot, short quantity)
    {
        var srcItem = getItem(slot);
        if (srcItem == null)
        {
            return (null, null);
        }

        if (ItemConstants.isRechargeable(srcItem.getItemId()))
        {
            quantity = srcItem.getQuantity();
        }

        var (op, _) = await removeItem(slot, quantity);

        var item = srcItem.copy();
        item.setQuantity(quantity);
        return (op, item);
    }

    public override IEnumerator<Item> GetEnumerator()
    {
        foreach (var item in inventory)
            if (item != null)
                yield return item;
    }

    #region Utils
    private static bool CheckItemRestricted(IEnumerable<TypedItemQuantity> items)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        // thanks Shavit for noticing set creation that would be only effective in rare situations
        foreach (var p in items)
        {
            if (ii.isPickupRestricted(p.Item.ItemId) && p.Item.Quantity > 1)
            {
                // 固有道具可以叠放？
                Log.Logger.Debug("ItemId={ItemId} 固有道具、Quantity={Quantity}", p.Item.ItemId, p.Item.Quantity);
                return false;
            }
        }

        return true;
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
                // 固有道具可以叠放？
                Log.Logger.Debug("ItemId={ItemId} 固有道具、Quantity={Quantity}", itemid, p.Item.getQuantity());
                return false;
            }
        }

        return true;
    }

    public static bool checkSpot(Player chr, Item item)
    {
        // thanks Vcoc for noticing pshops not checking item stacks when taking item back
        return checkSpot(chr, [item]);
    }

    public static bool checkSpot(Player chr, List<Item> items)
    {
        return checkSpotsAndOwnership(chr, items.Select(x => new ItemInventoryType(x, x.getInventoryType())).ToList());
    }


    public static bool checkSpots(Player chr, IEnumerable<TypedItemQuantity> items)
    {
        List<int> zeroedList = Enumerable.Repeat(0, EnumCache<InventoryType>.Values.Length).ToList();

        return checkSpots(chr, items, zeroedList, true);
    }

    public static bool checkSpots(Player chr, IEnumerable<ItemQuantity> items)
    {
        List<int> zeroedList = Enumerable.Repeat(0, EnumCache<InventoryType>.Values.Length).ToList();

        return checkSpots(chr, items.Select(x => new TypedItemQuantity((sbyte)ItemConstants.getInventoryType(x.ItemId), x)), zeroedList, false);
    }

    static bool checkSpots(Player chr, IEnumerable<TypedItemQuantity> items, List<int> typesSlotsUsed, bool useProofInv)
    {
        // assumption: no "UNDEFINED" or "EQUIPPED" items shall be tested here, all counts are >= 0.

        // 固有道具检测
        if (!CheckItemRestricted(items))
        {
            return false;
        }

        Dictionary<int, List<int>> rcvItems = new();
        Dictionary<int, sbyte> rcvTypes = new();

        foreach (var item in items)
        {

            if (rcvItems.TryGetValue(item.Item.ItemId, out var qty))
            {
                if (!ItemConstants.isEquipment(item.Item.ItemId) && !ItemConstants.isRechargeable(item.Item.ItemId))
                {
                    qty[0] += item.Item.Quantity;
                }
                else
                {
                    qty.Add(item.Item.Quantity);
                }
            }
            else
            {
                rcvItems[item.Item.ItemId] = new List<int>() { item.Item.Quantity };
                rcvTypes[item.Item.ItemId] = item.Type;
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
                typesSlotsUsed[itemType] = (result >> 1);
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
            rv ^= (byte)k[i];
            rv *= FNV_32_PRIME;
        }

        return rv;
    }

    private static long hashKey(int itemId, string owner)
    {
        return ((long)itemId << 32) + fnvHash32(owner);
    }


    public static bool checkSpotsAndOwnership(Player chr, List<ItemInventoryType> items, bool useProofInv = false)
    {
        List<int> zeroedList = Enumerable.Repeat(0, 5).ToList();

        return checkSpotsAndOwnership(chr, items, zeroedList, useProofInv);
    }

    static bool checkSpotsAndOwnership(Player chr, List<ItemInventoryType> items, List<int> typesSlotsUsed, bool useProofInv)
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
                typesSlotsUsed[itemType] = (result >> 1);
            }
        }

        return true;
    }
    #endregion
}
