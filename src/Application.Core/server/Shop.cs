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
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace server;


/// <summary>
/// NPC商店？
/// @author Matze
/// </summary>
public class Shop
{
    private ILogger log;
    private static HashSet<int> rechargeableItems = new();

    private int id;
    private int npcId;
    private List<ShopItem> items;
    private int tokenvalue = 1000000000;
    private int token = ItemId.GOLDEN_MAPLE_LEAF;

    static Shop()
    {
        foreach (int throwingStarId in ItemId.allThrowingStarIds())
        {
            rechargeableItems.Add(throwingStarId);
        }
        rechargeableItems.Add(ItemId.BLAZE_CAPSULE);
        rechargeableItems.Add(ItemId.GLAZE_CAPSULE);
        rechargeableItems.Add(ItemId.BALANCED_FURY);
        rechargeableItems.Remove(ItemId.DEVIL_RAIN_THROWING_STAR); // doesn't exist
        foreach (int bulletId in ItemId.allBulletIds())
        {
            rechargeableItems.Add(bulletId);
        }
    }

    public Shop(int id, int npcId, List<ShopItem> items)
    {
        this.id = id;
        this.npcId = npcId;
        this.items = items;

        log = LogFactory.GetLogger($"Shop/Shop_{id}_Npc_{npcId}");
    }

    public void sendShop(IChannelClient c)
    {
        c.OnlinedCharacter.setShop(this);
        c.sendPacket(PacketCreator.getNPCShop(c, getNpcId(), items));
    }

    public void buy(IChannelClient c, short slot, int itemId, short quantity)
    {
        ShopItem item = findBySlot(slot);
        if (item != null)
        {
            if (item.getItemId() != itemId)
            {
                log.Warning("Wrong slot number in shop {ShopId}", id);
                return;
            }
        }
        else
        {
            return;
        }
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        if (item.getPrice() > 0)
        {
            int amount = Math.Min(item.getPrice() * quantity, int.MaxValue);
            if (c.OnlinedCharacter.getMeso() >= amount)
            {
                if (InventoryManipulator.checkSpace(c, itemId, quantity, ""))
                {
                    if (!ItemConstants.isRechargeable(itemId))
                    { //Pets can't be bought from shops
                        InventoryManipulator.addById(c, itemId, quantity, "");
                        c.OnlinedCharacter.gainMeso(-amount, false);
                    }
                    else
                    {
                        short slotMax = ii.getSlotMax(c, item.getItemId());
                        quantity = slotMax;
                        InventoryManipulator.addById(c, itemId, quantity, "");
                        c.OnlinedCharacter.gainMeso(-item.getPrice(), false);
                    }
                    c.sendPacket(PacketCreator.shopTransaction(0));
                }
                else
                {
                    c.sendPacket(PacketCreator.shopTransaction(3));
                }

            }
            else
            {
                c.sendPacket(PacketCreator.shopTransaction(2));
            }

        }
        else if (item.getPitch() > 0)
        {
            int amount = Math.Min(item.getPitch() * quantity, int.MaxValue);

            if (c.OnlinedCharacter.getInventory(InventoryType.ETC).countById(ItemId.PERFECT_PITCH) >= amount)
            {
                if (InventoryManipulator.checkSpace(c, itemId, quantity, ""))
                {
                    if (!ItemConstants.isRechargeable(itemId))
                    {
                        InventoryManipulator.addById(c, itemId, quantity, "");
                        InventoryManipulator.removeById(c, InventoryType.ETC, ItemId.PERFECT_PITCH, amount, false, false);
                    }
                    else
                    {
                        short slotMax = ii.getSlotMax(c, item.getItemId());
                        quantity = slotMax;
                        InventoryManipulator.addById(c, itemId, quantity, "");
                        InventoryManipulator.removeById(c, InventoryType.ETC, ItemId.PERFECT_PITCH, amount, false, false);
                    }
                    c.sendPacket(PacketCreator.shopTransaction(0));
                }
                else
                {
                    c.sendPacket(PacketCreator.shopTransaction(3));
                }
            }

        }
        else if (c.OnlinedCharacter.getInventory(InventoryType.CASH).countById(token) != 0)
        {
            int amount = c.OnlinedCharacter.getInventory(InventoryType.CASH).countById(token);
            int value = amount * tokenvalue;
            int cost = item.getPrice() * quantity;
            if (c.OnlinedCharacter.getMeso() + value >= cost)
            {
                int cardreduce = value - cost;
                int diff = cardreduce + c.OnlinedCharacter.getMeso();
                if (InventoryManipulator.checkSpace(c, itemId, quantity, ""))
                {
                    InventoryManipulator.addById(c, itemId, quantity, "", expiration: -1);
                    c.OnlinedCharacter.gainMeso(diff, false);
                }
                else
                {
                    c.sendPacket(PacketCreator.shopTransaction(3));
                }
                c.sendPacket(PacketCreator.shopTransaction(0));
            }
            else
            {
                c.sendPacket(PacketCreator.shopTransaction(2));
            }
        }
    }

    private static bool canSell(Item item, short quantity)
    {
        if (item == null)
        {
            //Basic check
            return false;
        }

        short iQuant = item.getQuantity();
        if (iQuant < 0)
        {
            return false;
        }

        if (!ItemConstants.isRechargeable(item.getItemId()))
        {
            return iQuant != 0 && quantity <= iQuant;
        }

        return true;
    }

    private static short getSellingQuantity(Item item, short quantity)
    {
        if (ItemConstants.isRechargeable(item.getItemId()))
        {
            quantity = item.getQuantity();
        }

        return quantity;
    }

    public void sell(IChannelClient c, InventoryType type, short slot, short quantity)
    {
        if (quantity <= 0)
        {
            return;
        }

        var item = c.OnlinedCharacter.getInventory(type).getItem(slot);
        if (item == null)
            return;

        if (canSell(item, quantity))
        {
            quantity = getSellingQuantity(item, quantity);
            InventoryManipulator.removeFromSlot(c, type, (byte)slot, quantity, false);

            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            int recvMesos = ii.getPrice(item.getItemId(), quantity);
            if (recvMesos > 0)
            {
                c.OnlinedCharacter.gainMeso(recvMesos, false);
            }
            c.sendPacket(PacketCreator.shopTransaction(0x8));
        }
        else
        {
            c.sendPacket(PacketCreator.shopTransaction(0x5));
        }
    }

    public void recharge(IChannelClient c, short slot)
    {
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var item = c.OnlinedCharacter.getInventory(InventoryType.USE).getItem(slot);
        if (item == null || !ItemConstants.isRechargeable(item.getItemId()))
        {
            return;
        }
        short slotMax = ii.getSlotMax(c, item.getItemId());
        if (item.getQuantity() < 0)
        {
            return;
        }
        if (item.getQuantity() < slotMax)
        {
            int price = (int)Math.Ceiling(ii.getUnitPrice(item.getItemId()) * (slotMax - item.getQuantity()));
            if (c.OnlinedCharacter.getMeso() >= price)
            {
                item.setQuantity(slotMax);
                c.OnlinedCharacter.forceUpdateItem(item);
                c.OnlinedCharacter.gainMeso(-price, false, true, false);
                c.sendPacket(PacketCreator.shopTransaction(0x8));
            }
            else
            {
                c.sendPacket(PacketCreator.shopTransaction(0x2));
            }
        }
    }

    private ShopItem findBySlot(short slot)
    {
        return items.get(slot);
    }


    public int getNpcId()
    {
        return npcId;
    }

    public int getId()
    {
        return id;
    }
}
