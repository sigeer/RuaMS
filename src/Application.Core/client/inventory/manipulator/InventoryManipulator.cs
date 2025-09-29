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
using Application.Core.Game.Items;
using Application.Resources.Messages;
using tools;

namespace client.inventory.manipulator;



/**
 * @author Matze
 * @author Ronan - improved check space feature and removed redundant object calls
 */
public class InventoryManipulator
{
    /// <summary>
    /// 尽量使用addByDrop
    /// </summary>
    /// <param name="c"></param>
    /// <param name="itemId"></param>
    /// <param name="quantity"></param>
    /// <param name="owner"></param>
    /// <param name="flag"></param>
    /// <param name="expiration"></param>
    /// <returns></returns>
    public static bool addById(IChannelClient c, int itemId, short quantity, string? owner = null, short flag = 0, long expiration = -1)
    {
        IPlayer chr = c.OnlinedCharacter;
        InventoryType type = ItemConstants.getInventoryType(itemId);

        Inventory inv = chr.getInventory(type);
        inv.lockInventory();
        try
        {
            return addByIdInternal(c, chr, type, inv, itemId, quantity, owner, flag, expiration);
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    private static bool addByIdInternal(IChannelClient c, IPlayer chr, InventoryType type, Inventory inv, int itemId, short quantity, string? owner, short flag, long expiration)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        if (!type.Equals(InventoryType.EQUIP))
        {
            var isPet = ItemConstants.isPet(itemId);
            short slotMax = ii.getSlotMax(c, itemId);
            List<Item> existing = inv.listById(itemId);
            if (!ItemConstants.isRechargeable(itemId) && !isPet)
            {
                if (existing.Count > 0)
                { // first update all existing slots to slotMax
                    foreach (var eItem in existing)
                    {
                        if (quantity <= 0)
                            break;

                        short oldQ = eItem.getQuantity();
                        if (oldQ < slotMax && ((eItem.getOwner().Equals(owner) || owner == null) && eItem.getFlag() == flag))
                        {
                            short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                            quantity -= (short)(newQ - oldQ);
                            eItem.setQuantity(newQ);
                            eItem.setExpiration(expiration);
                            c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(1, eItem))));
                        }
                    }
                }
                while (quantity > 0)
                {
                    short newQ = Math.Min(quantity, slotMax);
                    if (newQ != 0)
                    {
                        quantity -= newQ;
                        Item nItem = Item.CreateVirtualItem(itemId, newQ);
                        nItem.setFlag(flag);
                        nItem.setExpiration(expiration);
                        short newSlot = inv.addItem(nItem);
                        if (newSlot == -1)
                        {
                            c.sendPacket(PacketCreator.getInventoryFull());
                            c.sendPacket(PacketCreator.getShowInventoryFull());
                            return false;
                        }
                        if (owner != null)
                        {
                            nItem.setOwner(owner);
                        }
                        c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(0, nItem))));
                        if (isSandboxItem(flag))
                        {
                            chr.setHasSandboxItem();
                        }
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return false;
                    }
                }
            }
            else
            {
                Item nItem = isPet ? new Pet(itemId, 0, Yitter.IdGenerator.YitIdHelper.NextId()) : Item.CreateVirtualItem(itemId, quantity);
                nItem.setFlag(flag);
                nItem.setExpiration(expiration);
                short newSlot = inv.addItem(nItem);
                if (newSlot == -1)
                {
                    c.sendPacket(PacketCreator.getInventoryFull());
                    c.sendPacket(PacketCreator.getShowInventoryFull());
                    return false;
                }
                c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(0, nItem))));
                if (isSandboxItem(nItem))
                {
                    chr.setHasSandboxItem();
                }
            }
        }
        else if (quantity == 1)
        {
            Item nEquip = ii.getEquipById(itemId);
            nEquip.setFlag(flag);
            nEquip.setExpiration(expiration);
            if (owner != null)
            {
                nEquip.setOwner(owner);
            }
            short newSlot = inv.addItem(nEquip);
            if (newSlot == -1)
            {
                c.sendPacket(PacketCreator.getInventoryFull());
                c.sendPacket(PacketCreator.getShowInventoryFull());
                return false;
            }
            c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(0, nEquip))));
            if (isSandboxItem(nEquip))
            {
                chr.setHasSandboxItem();
            }
        }
        else
        {
            throw new Exception("Trying to create equip with non-one quantity");
        }
        return true;
    }

    /// <summary>
    /// 有背包空间验证，但是如果物品数量大于1时，可能返回false但是会获取到一部分物品
    /// </summary>
    /// <param name="c"></param>
    /// <param name="item"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public static bool addFromDrop(IChannelClient c, Item item, bool show = true)
    {
        var chr = c.OnlinedCharacter;
        InventoryType type = item.getInventoryType();

        Inventory inv = chr.getInventory(type);
        inv.lockInventory();
        try
        {
            return addFromDropInternal(c, type, inv, item, show);
        }
        finally
        {
            inv.unlockInventory();
        }
    }

    private static bool addFromDropInternal(IChannelClient c, InventoryType type, Inventory inv, Item item, bool show)
    {
        var chr = c.OnlinedCharacter;
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        int itemid = item.getItemId();
        if (!chr.CanHoldUniquesOnly(itemid))
        {
            c.sendPacket(PacketCreator.getInventoryFull());
            c.sendPacket(PacketCreator.showItemUnavailable());
            return false;
        }
        short quantity = item.getQuantity();

        if (!type.Equals(InventoryType.EQUIP))
        {
            short slotMax = ii.getSlotMax(c, itemid);
            List<Item> existing = inv.listById(itemid);
            if (!ItemConstants.isRechargeable(itemid) && item is not Pet)
            {
                if (existing.Count > 0)
                {
                    // first update all existing slots to slotMax
                    foreach (var eItem in existing)
                    {
                        if (quantity <= 0)
                            break;

                        short oldQ = eItem.getQuantity();
                        if (oldQ < slotMax && item.getFlag() == eItem.getFlag() && item.getOwner().Equals(eItem.getOwner()))
                        {
                            short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                            quantity -= (short)(newQ - oldQ);
                            eItem.setQuantity(newQ);
                            item.setPosition(eItem.getPosition());
                            c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(1, eItem))));
                        }
                    }
                }
                while (quantity > 0)
                {
                    short newQ = Math.Min(quantity, slotMax);
                    quantity -= newQ;
                    Item nItem = Item.CreateVirtualItem(itemid, newQ);
                    nItem.setExpiration(item.getExpiration());
                    nItem.setOwner(item.getOwner());
                    nItem.setFlag(item.getFlag());
                    short newSlot = inv.addItem(nItem);
                    if (newSlot == -1)
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        // Q.没有修改过item的数量，这里操作失败后为什么要修改？--一部分数量已经被获取，这里的修改成剩余的数量
                        item.setQuantity((short)(quantity + newQ));
                        return false;
                    }
                    // 是否有意义？
                    item.setPosition(newSlot);
                    c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(0, nItem))));
                    if (isSandboxItem(nItem))
                    {
                        chr.setHasSandboxItem();
                    }
                }
            }
            else
            {
                // 飞镖 或 宠物 没有宠物会通过这种方式获取

                // 这里为什么要new Item？--不需要，可以和装备一样直接加入到背包
                //Item nItem = item is Pet ? new Pet(itemid, 0, Yitter.IdGenerator.YitIdHelper.NextId()) : Item.CreateVirtualItem(itemid, quantity);
                //nItem.setExpiration(item.getExpiration());
                //nItem.setFlag(item.getFlag());
                //nItem.setOwner(item.getOwner());

                short newSlot = inv.addItem(item);
                if (newSlot == -1)
                {
                    c.sendPacket(PacketCreator.getInventoryFull());
                    c.sendPacket(PacketCreator.getShowInventoryFull());
                    return false;
                }
                item.setPosition(newSlot);
                c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(0, item))));
                if (isSandboxItem(item))
                {
                    chr.setHasSandboxItem();
                }
                // 为什么这里会有 enableActions，其他的不用调用？，addById也没调用，似乎可以移除
                // c.sendPacket(PacketCreator.enableActions());
            }
        }
        else if (quantity == 1)
        {
            short newSlot = inv.addItem(item);
            if (newSlot == -1)
            {
                c.sendPacket(PacketCreator.getInventoryFull());
                c.sendPacket(PacketCreator.getShowInventoryFull());
                return false;
            }
            item.setPosition(newSlot);
            c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(0, item))));
            if (isSandboxItem(item))
            {
                chr.setHasSandboxItem();
            }
        }
        else
        {
            Log.Logger.Warning("Tried to pickup Equip id {ItemId} containing more than 1 quantity --> {ItemQuantity}", itemid, quantity);
            c.sendPacket(PacketCreator.getInventoryFull());
            c.sendPacket(PacketCreator.showItemUnavailable());
            return false;
        }
        if (show)
        {
            c.sendPacket(PacketCreator.getShowItemGain(itemid, item.getQuantity()));
        }
        return true;
    }

    public static bool checkSpace(IChannelClient c, int itemid, int quantity, string owner)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        InventoryType type = ItemConstants.getInventoryType(itemid);
        IPlayer chr = c.OnlinedCharacter;
        Inventory inv = chr.getInventory(type);

        if (!chr.CanHoldUniquesOnly(itemid))
        {
            return false;
        }

        if (!type.Equals(InventoryType.EQUIP))
        {
            short slotMax = ii.getSlotMax(c, itemid);
            List<Item> existing = inv.listById(itemid);

            int numSlotsNeeded;
            if (ItemConstants.isRechargeable(itemid))
            {
                numSlotsNeeded = 1;
            }
            else
            {
                if (existing.Count > 0) // first update all existing slots to slotMax
                {
                    foreach (Item eItem in existing)
                    {
                        short oldQ = eItem.getQuantity();
                        if (oldQ < slotMax && owner.Equals(eItem.getOwner()))
                        {
                            short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                            quantity -= (newQ - oldQ);
                        }
                        if (quantity <= 0)
                        {
                            break;
                        }
                    }
                }

                if (slotMax > 0)
                {
                    numSlotsNeeded = (int)(Math.Ceiling(((double)quantity) / slotMax));
                }
                else
                {
                    numSlotsNeeded = 1;
                }
            }

            return !inv.isFull(numSlotsNeeded - 1);
        }
        else
        {
            return !inv.isFull();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="itemid"></param>
    /// <param name="quantity"></param>
    /// <param name="owner"></param>
    /// <param name="usedSlots"></param>
    /// <param name="useProofInv"></param>
    /// <returns>低位（bit0）表示：是否有空间。高位（其他位）表示：使用了多少额外的格子。</returns>
    public static int checkSpaceProgressively(IChannelClient c, int itemid, int quantity, string owner, int usedSlots, bool useProofInv)
    {
        // return value --> bit0: if has space for this one;
        //                  value after: new slots filled;
        // assumption: equipments always have slotMax == 1.

        int returnValue;

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        InventoryType type = !useProofInv ? ItemConstants.getInventoryType(itemid) : InventoryType.CANHOLD;
        IPlayer chr = c.OnlinedCharacter;
        Inventory inv = chr.getInventory(type);

        if (!chr.CanHoldUniquesOnly(itemid))
        {
            return 0;
        }

        if (!type.Equals(InventoryType.EQUIP))
        {
            short slotMax = ii.getSlotMax(c, itemid);
            int numSlotsNeeded;

            if (ItemConstants.isRechargeable(itemid))
            {
                numSlotsNeeded = 1;
            }
            else
            {
                List<Item> existing = inv.listById(itemid);

                if (existing.Count > 0) // first update all existing slots to slotMax
                {
                    foreach (Item eItem in existing)
                    {
                        short oldQ = eItem.getQuantity();
                        if (oldQ < slotMax && owner.Equals(eItem.getOwner()))
                        {
                            short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                            quantity -= (newQ - oldQ);
                        }
                        if (quantity <= 0)
                        {
                            break;
                        }
                    }
                }

                if (slotMax > 0)
                {
                    numSlotsNeeded = (int)(Math.Ceiling(((double)quantity) / slotMax));
                }
                else
                {
                    numSlotsNeeded = 1;
                }
            }

            returnValue = ((numSlotsNeeded + usedSlots) << 1);
            returnValue += (numSlotsNeeded == 0 || !inv.isFullAfterSomeItems(numSlotsNeeded - 1, usedSlots)) ? 1 : 0;
            //System.out.print(" needed " + numSlotsNeeded + " used " + usedSlots + " rval " + returnValue);
        }
        else
        {
            returnValue = ((quantity + usedSlots) << 1);
            returnValue += (!inv.isFullAfterSomeItems(0, usedSlots)) ? 1 : 0;
            //System.out.print(" eqpneeded " + 1 + " used " + usedSlots + " rval " + returnValue);
        }

        return returnValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    /// <param name="type"></param>
    /// <param name="slot"></param>
    /// <param name="quantity"></param>
    /// <param name="fromDrop">不明</param>
    /// <param name="consume">对于子弹、飞镖等物品，consume为true时不会移除</param>
    public static void removeFromSlot(IChannelClient c, InventoryType type, short slot, short quantity, bool fromDrop, bool consume = false)
    {
        IPlayer chr = c.OnlinedCharacter;
        Inventory inv = chr.getInventory(type);
        var item = inv.getItem(slot)!;

        bool allowZero = consume && ItemConstants.isRechargeable(item.getItemId());

        if (type == InventoryType.EQUIPPED)
        {
            inv.lockInventory();
            try
            {
                chr.unequippedItem((Equip)item);
                inv.removeItem(slot, quantity, allowZero);
            }
            finally
            {
                inv.unlockInventory();
            }

            AnnounceModifyInventory(c, item, fromDrop, allowZero);
        }
        else
        {
            if (item is Pet petObj)
            {
                // thanks Vcoc for finding a d/c issue with equipped pets and pets remaining on DB here
                int petIdx = chr.getPetIndex(petObj.PetId);
                if (petIdx > -1)
                {
                    chr.unequipPet(petObj, true);
                }

                inv.removeItem(slot, quantity, allowZero);
                if (type != InventoryType.CANHOLD)
                {
                    AnnounceModifyInventory(c, item, fromDrop, allowZero);
                }

                // thanks Robin Schulz for noticing pet issues when moving pets out of inventory
            }
            else
            {
                inv.removeItem(slot, quantity, allowZero);
                if (type != InventoryType.CANHOLD)
                {
                    AnnounceModifyInventory(c, item, fromDrop, allowZero);
                }
            }
        }
    }

    public static void AnnounceModifyInventory(IChannelClient c, Item item, bool fromDrop, bool allowZero)
    {
        if (item.getQuantity() == 0 && !allowZero)
        {
            c.sendPacket(PacketCreator.modifyInventory(fromDrop, Collections.singletonList(new ModifyInventory(3, item))));
        }
        else
        {
            c.sendPacket(PacketCreator.modifyInventory(fromDrop, Collections.singletonList(new ModifyInventory(1, item))));
        }
    }

    public static void removeById(IChannelClient c, InventoryType type, int itemId, int quantity, bool fromDrop, bool consume)
    {
        int removeQuantity = quantity;
        Inventory inv = c.OnlinedCharacter.Bag[type];
        int slotLimit = type == InventoryType.EQUIPPED ? 128 : inv.getSlotLimit();

        for (short i = 0; i <= slotLimit; i++)
        {
            var item = inv.getItem((short)(type == InventoryType.EQUIPPED ? -i : i));
            if (item != null)
            {
                if (item.getItemId() == itemId || item.getCashId() == itemId)
                {
                    if (removeQuantity <= item.getQuantity())
                    {
                        removeFromSlot(c, type, item.getPosition(), (short)removeQuantity, fromDrop, consume);
                        removeQuantity = 0;
                        break;
                    }
                    else
                    {
                        removeQuantity -= item.getQuantity();
                        removeFromSlot(c, type, item.getPosition(), item.getQuantity(), fromDrop, consume);
                    }
                }
            }
        }
        if (removeQuantity > 0 && type != InventoryType.CANHOLD)
        {
            throw new BusinessException("[Hack] Not enough items available of Item:" + itemId + ", Quantity (After Quantity/Over Current Quantity): " + (quantity - removeQuantity) + "/" + quantity);
        }
    }

    private static bool isSameOwner(Item source, Item target)
    {
        return source.getOwner().Equals(target.getOwner());
    }
    public static void move(IChannelClient c, InventoryType type, short src, short dst)
    {
        Inventory inv = c.OnlinedCharacter.getInventory(type);

        if (src < 0 || dst < 0)
        {
            return;
        }
        if (dst > inv.getSlotLimit())
        {
            return;
        }
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var source = inv.getItem(src);

        if (source == null)
        {
            return;
        }
        short olddstQ = -1;
        var initialTarget = inv.getItem(dst);
        if (initialTarget != null)
        {
            olddstQ = initialTarget.getQuantity();
        }
        short oldsrcQ = source.getQuantity();
        short slotMax = ii.getSlotMax(c, source.getItemId());
        inv.move(src, dst, slotMax);
        List<ModifyInventory> mods = new();
        if (!(type.Equals(InventoryType.EQUIP) || type.Equals(InventoryType.CASH)) && initialTarget != null && initialTarget.getItemId() == source.getItemId() && !ItemConstants.isRechargeable(source.getItemId()) && isSameOwner(source, initialTarget))
        {
            if ((olddstQ + oldsrcQ) > slotMax)
            {
                mods.Add(new ModifyInventory(1, source));
                mods.Add(new ModifyInventory(1, initialTarget));
            }
            else
            {
                mods.Add(new ModifyInventory(3, source));
                mods.Add(new ModifyInventory(1, initialTarget));
            }
        }
        else
        {
            mods.Add(new ModifyInventory(2, source, src));
        }
        c.sendPacket(PacketCreator.modifyInventory(true, mods));
    }

    public static void equip(IChannelClient c, short src, short dst)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        IPlayer chr = c.OnlinedCharacter;
        Inventory eqpInv = chr.getInventory(InventoryType.EQUIP);
        Inventory eqpdInv = chr.getInventory(InventoryType.EQUIPPED);

        var source = (Equip?)eqpInv.getItem(src);
        if (source == null || !ii.canWearEquipment(chr, source, dst))
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        else if ((ItemId.isExplorerMount(source.getItemId()) && chr.isCygnus()) ||
                ((ItemId.isCygnusMount(source.getItemId())) && !chr.isCygnus()))
        {// Adventurer taming equipment
            return;
        }
        bool itemChanged = false;
        if (ii.isUntradeableOnEquip(source.getItemId()))
        {
            short flag = source.getFlag();      // thanks BHB for noticing flags missing after equipping these
            flag |= ItemConstants.UNTRADEABLE;
            source.setFlag(flag);

            itemChanged = true;
        }
        switch (dst)
        {
            case -6: // unequip the overall
                var top = eqpdInv.getItem(-5);
                if (top != null && ItemConstants.isOverall(top.getItemId()))
                {
                    if (eqpInv.isFull())
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        return;
                    }
                    unequip(c, -5, eqpInv.getNextFreeSlot());
                }
                break;
            case -5:
                var bottom = eqpdInv.getItem(-6);
                if (bottom != null && ItemConstants.isOverall(source.getItemId()))
                {
                    if (eqpInv.isFull())
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        return;
                    }
                    unequip(c, -6, eqpInv.getNextFreeSlot());
                }
                break;
            case -10: // check if weapon is two-handed
                var weapon = eqpdInv.getItem(-11);
                if (weapon != null && ii.isTwoHanded(weapon.getItemId()))
                {
                    if (eqpInv.isFull())
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        return;
                    }
                    unequip(c, -11, eqpInv.getNextFreeSlot());
                }
                break;
            case -11:
                var shield = eqpdInv.getItem(-10);
                if (shield != null && ii.isTwoHanded(source.getItemId()))
                {
                    if (eqpInv.isFull())
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        return;
                    }
                    unequip(c, -10, eqpInv.getNextFreeSlot());
                }
                break;
            case -18:
                if (chr.getMount() != null)
                {
                    chr.getMount()!.setItemId(source.getItemId());
                }
                break;
        }

        //1112413, 1112414, 1112405 (Lilin's Ring)
        source = (Equip?)eqpInv.getItem(src);
        eqpInv.removeSlot(src);

        Equip? target;
        eqpdInv.lockInventory();
        try
        {
            target = (Equip?)eqpdInv.getItem(dst);
            if (target != null)
            {
                chr.unequippedItem(target);
                eqpdInv.removeSlot(dst);
            }
        }
        finally
        {
            eqpdInv.unlockInventory();
        }

        List<ModifyInventory> mods = new();
        if (itemChanged)
        {
            mods.Add(new ModifyInventory(3, source));
            mods.Add(new ModifyInventory(0, source.copy()));//to prevent crashes
        }

        source.setPosition(dst);

        eqpdInv.lockInventory();
        try
        {
            if (source.getRingId() > -1)
            {
                chr.getRingById(source.getRingId()).equip();
            }
            chr.equippedItem(source);
            eqpdInv.addItemFromDB(source);
        }
        finally
        {
            eqpdInv.unlockInventory();
        }

        if (target != null)
        {
            target.setPosition(src);
            eqpInv.addItemFromDB(target);
        }
        if (chr.getBuffedValue(BuffStat.BOOSTER) != null && ItemConstants.isWeapon(source.getItemId()))
        {
            chr.cancelBuffStats(BuffStat.BOOSTER);
        }

        mods.Add(new ModifyInventory(2, source, src));
        c.sendPacket(PacketCreator.modifyInventory(true, mods));
        chr.equipChanged();
    }

    public static void unequip(IChannelClient c, short src, short dst)
    {
        IPlayer chr = c.OnlinedCharacter;
        Inventory eqpInv = chr.getInventory(InventoryType.EQUIP);
        Inventory eqpdInv = chr.getInventory(InventoryType.EQUIPPED);

        if (dst < 0)
        {
            return;
        }
        var source = (Equip?)eqpdInv.getItem(src);
        if (source == null)
        {
            return;
        }
        var target = (Equip?)eqpInv.getItem(dst);
        if (target != null && src <= 0)
        {
            c.sendPacket(PacketCreator.getInventoryFull());
            return;
        }

        eqpdInv.lockInventory();
        try
        {
            if (source.getRingId() > -1)
            {
                chr.getRingById(source.getRingId()).unequip();
            }
            chr.unequippedItem(source);
            eqpdInv.removeSlot(src);
        }
        finally
        {
            eqpdInv.unlockInventory();
        }

        if (target != null)
        {
            eqpInv.removeSlot(dst);
        }
        source.setPosition(dst);
        eqpInv.addItemFromDB(source);
        if (target != null)
        {
            target.setPosition(src);
            eqpdInv.addItemFromDB(target);
        }
        c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(2, source, src))));
        chr.equipChanged();
    }

    private static bool isDisappearingItemDrop(Item it)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        if (ii.isDropRestricted(it.getItemId()))
        {
            return true;
        }
        else if (ii.isCash(it.getItemId()))
        {
            if (YamlConfig.config.server.USE_ENFORCE_UNMERCHABLE_CASH)
            {
                // thanks Ari for noticing cash drops not available server-side
                return true;
            }
            else
            {
                return ItemConstants.isPet(it.getItemId()) && YamlConfig.config.server.USE_ENFORCE_UNMERCHABLE_PET;
            }
        }
        else if (YamlConfig.config.server.USE_ERASE_UNTRADEABLE_DROP && it.isUntradeable())
        {
            return true;
        }
        else
        {
            return ItemId.isWeddingRing(it.getItemId());
        }
    }
    public static void drop(IChannelClient c, InventoryType type, short srcItemId, short quantity)
    {
        if (srcItemId < 0)
        {
            type = InventoryType.EQUIPPED;
        }

        IPlayer chr = c.OnlinedCharacter;
        Inventory inv = chr.getInventory(type);
        var source = inv.getItem(srcItemId);

        if (chr.getTrade() != null || chr.getMiniGame() != null || source == null)
        {
            //Only check needed would prob be merchants (to see if the player is in one)
            return;
        }

        if (chr.isGM() && chr.gmLevel() < YamlConfig.config.server.MINIMUM_GM_LEVEL_TO_DROP)
        {
            chr.MessageI18N(nameof(ClientMessage.DropItem_NoAccess));
            LogFactory.GM.Information("GM {CharacterName} tried to drop item id {ItemId}", chr.getName(), source.getItemId());
            return;
        }


        int itemId = source.getItemId();

        var map = chr.getMap();
        if ((!ItemConstants.isRechargeable(itemId) && source.getQuantity() < quantity) || quantity < 0)
        {
            return;
        }

        if (source is Pet petObj)
        {
            int petIdx = chr.getPetIndex(petObj.PetId);
            if (petIdx > -1)
            {
                var pet = chr.getPet(petIdx);
                if (pet != null)
                    chr.unequipPet(pet, true);
            }
        }

        Point dropPos = chr.getPosition();
        if (quantity < source.getQuantity() && !ItemConstants.isRechargeable(itemId))
        {
            Item target = source.copy();
            target.setQuantity(quantity);
            source.setQuantity((short)(source.getQuantity() - quantity));
            c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(1, source))));

            if (ItemConstants.isNewYearCardEtc(itemId))
            {
                if (itemId == ItemId.NEW_YEARS_CARD_SEND)
                {
                    chr.DiscardNewYearRecord(true);
                    c.getAbstractPlayerInteraction().removeAll(ItemId.NEW_YEARS_CARD_SEND);
                }
                else
                {
                    chr.DiscardNewYearRecord(false);
                    c.getAbstractPlayerInteraction().removeAll(ItemId.NEW_YEARS_CARD_RECEIVED);
                }
            }

            if (isDisappearingItemDrop(target))
            {
                map.disappearingItemDrop(chr, chr, target, dropPos);
            }
            else
            {
                map.spawnItemDrop(chr, chr, target, dropPos, true, true);
            }
        }
        else
        {
            if (type == InventoryType.EQUIPPED)
            {
                inv.lockInventory();
                try
                {
                    chr.unequippedItem((Equip)source);
                    inv.removeSlot(srcItemId);
                }
                finally
                {
                    inv.unlockInventory();
                }
            }
            else
            {
                inv.removeSlot(srcItemId);
            }

            c.sendPacket(PacketCreator.modifyInventory(true, Collections.singletonList(new ModifyInventory(3, source))));
            if (srcItemId < 0)
            {
                chr.equipChanged();
            }
            else if (ItemConstants.isNewYearCardEtc(itemId))
            {
                if (itemId == ItemId.NEW_YEARS_CARD_SEND)
                {
                    chr.DiscardNewYearRecord(true);
                    c.getAbstractPlayerInteraction().removeAll(ItemId.NEW_YEARS_CARD_SEND);
                }
                else
                {
                    chr.DiscardNewYearRecord(false);
                    c.getAbstractPlayerInteraction().removeAll(ItemId.NEW_YEARS_CARD_RECEIVED);
                }
            }

            if (isDisappearingItemDrop(source))
            {
                map.disappearingItemDrop(chr, chr, source, dropPos);
            }
            else
            {
                map.spawnItemDrop(chr, chr, source, dropPos, true, true);
            }
        }

        int quantityNow = chr.getItemQuantity(itemId, false);
        if (itemId == chr.getItemEffect())
        {
            if (quantityNow <= 0)
            {
                chr.setItemEffect(0);
                map.broadcastMessage(PacketCreator.itemEffect(chr.getId(), 0));
            }
        }
        else if (itemId == ItemId.CHALKBOARD_1 || itemId == ItemId.CHALKBOARD_2)
        {
            if (source.getQuantity() <= 0)
            {
                chr.setChalkboard(null);
            }
        }
        else if (itemId == ItemId.ARPQ_SPIRIT_JEWEL)
        {
            chr.updateAriantScore(quantityNow);
        }
    }


    public static bool isSandboxItem(Item it) => isSandboxItem(it.getFlag());

    static bool isSandboxItem(short itFlag)
    {
        return (itFlag & ItemConstants.SANDBOX) == ItemConstants.SANDBOX;
    }
}
