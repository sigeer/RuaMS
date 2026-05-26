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
using Application.Core.Game.Maps.AnimatedObjects;
using Application.Core.Models;
using Application.Resources.Messages;
using Application.Templates.Item.Pet;
using tools;

namespace client.inventory.manipulator;



/**
 * @author Matze
 * @author Ronan - improved check space feature and removed redundant object calls
 */
public class InventoryManipulator
{

    /// <summary>
    /// 如果物品数量大于1时，可能返回false但是会获取到一部分物品
    /// </summary>
    /// <param name="c"></param>
    /// <param name="item"></param>
    /// <param name="show"></param>
    /// <returns></returns>
    public static bool addFromDrop(IChannelClient c, Item item, bool show = true)
    {
        var chr = c.OnlinedCharacter;
        InventoryType type = item.getInventoryType();

        var inv = chr.GetInventory(type);

        return addFromDropInternal(c, type, inv, item, show);
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
        short slotMax = ii.getSlotMax(c, itemid);

        List<IInventoryOperationCommand> ops = [];
        // 可叠加（飞镖子弹虽然有数量，但是不可叠加）
        if (slotMax > 1 && !ItemConstants.isRechargeable(itemid))
        {
            List<Item> existing = inv.listById(itemid);
            if (existing.Count > 0)
            {
                // first update all existing slots to slotMax
                foreach (var eItem in existing)
                {
                    if (quantity <= 0)
                        break;

                    short oldQ = eItem.getQuantity();
                    // 相同属性才能叠加
                    if (oldQ < slotMax
                        && item.getFlag() == eItem.getFlag()
                        && item.getOwner().Equals(eItem.getOwner())
                        && item.getExpiration() == eItem.getExpiration())
                    {
                        short newQ = (short)Math.Min(oldQ + quantity, slotMax);
                        quantity -= (short)(newQ - oldQ);
                        eItem.setQuantity(newQ);
                        ops.Add(new InventoryUpdateQuantity(eItem.getInventoryType(), eItem.getPosition(), newQ));
                    }
                }
            }
            while (quantity > 0)
            {
                short newQ = Math.Min(quantity, slotMax);
                quantity -= newQ;
                // 装备、宠物都不可叠加，这里必定是Item
                Item nItem = Item.CreateVirtualItem(itemid, newQ);
                nItem.setExpiration(item.getExpiration());
                nItem.setOwner(item.getOwner());
                nItem.setFlag(item.getFlag());

                var addR = inv.AddItem(nItem);
                if (addR == null)
                {
                    c.sendPacket(PacketCreator.getInventoryFull());
                    c.sendPacket(PacketCreator.getShowInventoryFull());
                    // 剩余空间不足、回滚
                    item.setQuantity((short)(quantity + newQ));
                    return false;
                }

                ops.Add(addR);
                if (isSandboxItem(nItem))
                {
                    chr.setHasSandboxItem();
                }
            }
        }
        else
        {
            if (item is Equip && quantity != 1)
            {
                Log.Logger.Warning("Tried to pickup Id={ItemId} containing more than 1 quantity --> {ItemQuantity}", itemid, quantity);
                c.sendPacket(PacketCreator.getInventoryFull());
                c.sendPacket(PacketCreator.showItemUnavailable());
                return false;
            }

            item.setExpiration(item.getExpiration());
            item.setFlag(item.getFlag());
            item.setOwner(item.getOwner());
            var addR = inv.AddItem(item);
            if (addR == null)
            {
                c.sendPacket(PacketCreator.getInventoryFull());
                c.sendPacket(PacketCreator.getShowInventoryFull());
                return false;
            }
            ops.Add(addR);

            if (isSandboxItem(item))
            {
                chr.setHasSandboxItem();
            }
        }

        if (ops.Count > 0)
        {
            chr.SyncClientInventory(ops, true);
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
        Player chr = c.OnlinedCharacter;
        var inv = chr.GetInventory(type);

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
        Player chr = c.OnlinedCharacter;
        var inv = chr.GetInventory(type);

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
    /// <param name="consume">true：数量0时不会移除</param>
    public static void removeFromSlot(IChannelClient c, InventoryType type, short slot, short quantity, bool fromDrop, bool consume = false)
    {
        Player chr = c.OnlinedCharacter;
        var inv = chr.getInventory(type);
        var item = inv.getItem(slot)!;

        bool allowZero = consume && ItemConstants.isRechargeable(item.getItemId());
        var removeRes = inv.removeItem(slot, out _, quantity, allowZero);
        if (removeRes != null && type != InventoryType.CANHOLD)
        {
            chr.SyncClientInventory(removeRes, fromDrop);
        }
    }

    public static void removeById(IChannelClient c, InventoryType type, int itemId, int quantity, bool fromDrop, bool consume)
    {
        c.OnlinedCharacter.Bag.RemoveFromInventory(type, quantity, i => i.getItemId() == itemId || i.getCashId() == itemId, fromDrop, consume);
    }

    private static bool isSameOwner(Item source, Item target)
    {
        return source.getOwner().Equals(target.getOwner());
    }

    public static void equip(IChannelClient c, short src, short dst)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();

        Player chr = c.OnlinedCharacter;
        var eqpInv = chr.GetInventory(InventoryType.EQUIP);
        var eqpdInv = chr.GetEquipped();

        var source = eqpInv.getItem(src) as Equip;
        if (source == null || !ii.canWearEquipment(chr, source, dst))
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        else if ((ItemId.isExplorerMount(source.getItemId()) && chr.isCygnus()) ||
                ((ItemId.isCygnusMount(source.getItemId())) && !chr.isCygnus()))
        {
            // Adventurer taming equipment
            return;
        }

        List<IInventoryOperationCommand> ops = [];
        List<ModifyInventory> mods = new();
        if (source.SourceTemplate.EquipTradeBlock)
        {
            short flag = source.getFlag();      // thanks BHB for noticing flags missing after equipping these
            flag |= ItemConstants.UNTRADEABLE;
            source.setFlag(flag);

            ops.AddRange([new InventoryRemove(InventoryType.EQUIP, src), new InventoryAdd(InventoryType.EQUIP, source, src)]);
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
            case EquipSlot.Shield: // check if weapon is two-handed
                var weapon = eqpdInv.getItem(EquipSlot.Weapon);
                if (weapon != null && ii.isTwoHanded(weapon.getItemId()))
                {
                    if (eqpInv.isFull())
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        return;
                    }
                    unequip(c, EquipSlot.Weapon, eqpInv.getNextFreeSlot());
                }
                break;
            case EquipSlot.Weapon:
                var shield = eqpdInv.getItem(EquipSlot.Shield);
                if (shield != null && ii.isTwoHanded(source.getItemId()))
                {
                    if (eqpInv.isFull())
                    {
                        c.sendPacket(PacketCreator.getInventoryFull());
                        c.sendPacket(PacketCreator.getShowInventoryFull());
                        return;
                    }
                    unequip(c, EquipSlot.Shield, eqpInv.getNextFreeSlot());
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
        // source = (Equip?)eqpInv.getItem(src); // 这里为什么又获取一遍？


        var originalEqp = eqpdInv.Equip(dst, source);

        eqpInv.removeSlot(src);
        if (originalEqp != null)
            eqpInv.PutItem(src, originalEqp);

        if (chr.getBuffedValue(BuffStat.BOOSTER) != null && ItemConstants.isWeapon(source.getItemId()))
        {
            chr.cancelBuffStats(BuffStat.BOOSTER);
        }

        ops.Add(new InventoryMove(InventoryType.EQUIP, src, dst));
        chr.SyncClientInventory(ops, true);
    }

    public static void unequip(IChannelClient c, short src, short dst)
    {
        Player chr = c.OnlinedCharacter;
        var eqpInv = chr.GetInventory(InventoryType.EQUIP);
        var eqpdInv = chr.GetEquipped();

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

        eqpdInv.removeSlot(src);
        eqpInv.PutItem(dst, source);

        c.OnlinedCharacter.SyncClientInventory(new InventoryMove(InventoryType.EQUIP, src, dst));
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
    public static void drop(IChannelClient c, InventoryType type, short sourceSlot, short quantity)
    {
        if (sourceSlot < 0)
        {
            type = InventoryType.EQUIPPED;
        }

        Player chr = c.OnlinedCharacter;
        var inv = chr.GetInventory(type);
        var source = inv.getItem(sourceSlot);

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

        var op = inv.Take(sourceSlot, quantity, out var newItem);
        if (op != null)
        {
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

            Point dropPos = chr.getPosition();
            if (isDisappearingItemDrop(source))
            {
                map.DropItemDestroy(source.getItemId(), dropPos);
            }
            else
            {
                map.spawnItemDrop(chr, chr, newItem!, dropPos, true, true);
            }

            chr.SyncClientInventory(op);
        }

        int quantityNow = chr.getItemQuantity(itemId);
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
