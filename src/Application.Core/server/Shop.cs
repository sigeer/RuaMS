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

    private int id;
    private int npcId;
    private List<ShopItem> items;
    private int tokenvalue = 1000000000;
    private int token = ItemId.GOLDEN_MAPLE_LEAF;

    public Shop(int id, int npcId, List<ShopItem> items)
    {
        this.id = id;
        this.npcId = npcId;
        this.items = items;

        log = LogFactory.GetLogger($"Shop/Shop_{id}_Npc_{npcId}");
    }

    public async Task sendShop(IChannelClient c)
    {
        c.OnlinedCharacter.setShop(this);
        await c.SendPacket(PacketCreator.getNPCShop(c, getNpcId(), items));
    }

    public async Task buy(IChannelClient c, short slot, int itemId, short quantity)
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
                    {
                        //Pets can't be bought from shops
                        await c.OnlinedCharacter.GainItem(itemId, quantity);
                        await c.OnlinedCharacter.GainMeso(-amount);
                    }
                    else
                    {
                        short slotMax = ii.getSlotMax(c, item.getItemId());
                        quantity = slotMax;

                        await c.OnlinedCharacter.GainItem(itemId, quantity);
                        await c.OnlinedCharacter.GainMeso(-item.getPrice());
                    }
                    await c.SendPacket(PacketCreator.shopTransaction(0));
                }
                else
                {
                    await c.SendPacket(PacketCreator.shopTransaction(3));
                }

            }
            else
            {
                await c.SendPacket(PacketCreator.shopTransaction(2));
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
                        await c.OnlinedCharacter.GainItem(itemId, quantity);
                        await InventoryManipulator.removeById(c, InventoryType.ETC, ItemId.PERFECT_PITCH, amount, false, false);
                    }
                    else
                    {
                        short slotMax = ii.getSlotMax(c, item.getItemId());
                        quantity = slotMax;
                        await c.OnlinedCharacter.GainItem(itemId, quantity);
                        await InventoryManipulator.removeById(c, InventoryType.ETC, ItemId.PERFECT_PITCH, amount, false, false);
                    }
                    await c.SendPacket(PacketCreator.shopTransaction(0));
                }
                else
                {
                    await c.SendPacket(PacketCreator.shopTransaction(3));
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
                    await c.OnlinedCharacter.GainItem(itemId, quantity);
                    await c.OnlinedCharacter.GainMeso(diff);
                }
                else
                {
                    await c.SendPacket(PacketCreator.shopTransaction(3));
                }
                await c.SendPacket(PacketCreator.shopTransaction(0));
            }
            else
            {
                await c.SendPacket(PacketCreator.shopTransaction(2));
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

    public async Task sell(IChannelClient c, InventoryType type, short slot, short quantity)
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
            await InventoryManipulator.removeFromSlot(c, type, (byte)slot, quantity, false);

            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            int recvMesos = ii.getPrice(item.getItemId(), quantity);
            if (recvMesos > 0)
            {
                await c.OnlinedCharacter.GainMeso(recvMesos);
            }
            await c.SendPacket(PacketCreator.shopTransaction(0x8));
        }
        else
        {
            await c.SendPacket(PacketCreator.shopTransaction(0x5));
        }
    }

    public async Task recharge(IChannelClient c, short slot)
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
                await c.OnlinedCharacter.SyncClientInventory(new InventoryUpdateQuantity(InventoryType.USE, slot, slotMax));

                await c.OnlinedCharacter.GainMeso(-price, enableActions: true);
                await c.SendPacket(PacketCreator.shopTransaction(0x8));
            }
            else
            {
                await c.SendPacket(PacketCreator.shopTransaction(0x2));
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
