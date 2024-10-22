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


using Application.Core.Game.Maps;
using client.inventory;
using client.inventory.manipulator;
using net.packet;
using server;
using server.maps;
using System.Numerics;
using tools;

namespace Application.Core.Game.Trades;

/**
 * @author Matze
 * @author Ronan - concurrency protection
 */
public class PlayerShop : AbstractMapObject
{
    private AtomicBoolean open = new AtomicBoolean(false);
    private IPlayer owner;
    private int itemid;

    private IPlayer?[] visitors = new IPlayer[3];
    private List<PlayerShopItem> items = new();
    private List<SoldItem> sold = new();
    private string description;
    private int boughtnumber = 0;
    private List<string> bannedList = new();
    private List<KeyValuePair<IPlayer, string>> chatLog = new();
    private Dictionary<int, byte> chatSlot = new();
    private object visitorLock = new object();

    public PlayerShop(IPlayer owner, string description, int itemid)
    {
        setPosition(owner.getPosition());
        this.owner = owner;
        this.description = description;
        this.itemid = itemid;
    }

    public int getChannel()
    {
        return owner.getClient().getChannel();
    }

    public int getMapId()
    {
        return owner.getMapId();
    }

    public int getItemId()
    {
        return itemid;
    }

    public bool isOpen()
    {
        return open.Get();
    }

    public void setOpen(bool openShop)
    {
        open.Set(openShop);
    }

    public int GetFreeSlot()
    {
        Monitor.Enter(visitorLock);
        try
        {
            return Array.IndexOf(visitors, null);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public bool hasFreeSlot()
    {
        return GetFreeSlot() != -1;
    }

    public byte[] getShopRoomInfo()
    {
        Monitor.Enter(visitorLock);
        try
        {
            var count = (byte)visitors.Count(x => x != null);
            return new byte[] { count, (byte)visitors.Length };
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public bool isOwner(IPlayer chr)
    {
        return owner.Equals(chr);
    }

    private void addVisitor(IPlayer visitor)
    {
        var freeSlot = GetFreeSlot();
        if (freeSlot != -1)
        {
            visitors[freeSlot] = visitor;

            broadcast(PacketCreator.getPlayerShopNewVisitor(visitor, freeSlot + 1));
            owner.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(this));
        }
    }

    public void forceRemoveVisitor(IPlayer visitor)
    {
        if (visitor == owner)
        {
            owner.getMap().removeMapObject(this);
            owner.setPlayerShop(null);
        }

        Monitor.Enter(visitorLock);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                if (visitors[i] != null && visitors[i]!.getId() == visitor.getId())
                {
                    visitors[i]!.setPlayerShop(null);
                    visitors[i] = null;

                    broadcast(PacketCreator.getPlayerShopRemoveVisitor(i + 1));
                    owner.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                    return;
                }
            }
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void removeVisitor(IPlayer visitor)
    {
        if (visitor == owner)
        {
            owner.getMap().removeMapObject(this);
            owner.setPlayerShop(null);
        }
        else
        {
            Monitor.Enter(visitorLock);
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    if (visitors[i] != null && visitors[i]!.getId() == visitor.getId())
                    {
                        for (int j = i; j < 2; j++)
                        {
                            if (visitors[j] != null)
                            {
                                owner.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(j + 1));
                            }
                            visitors[j] = visitors[j + 1];
                        }
                        visitors[2] = null;
                        for (int j = i; j < 2; j++)
                        {
                            if (visitors[j] != null)
                            {
                                owner.sendPacket(PacketCreator.getPlayerShopNewVisitor(visitors[j]!, j + 1));
                            }
                        }

                        broadcastRestoreToVisitors();
                        owner.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                        return;
                    }
                }
            }
            finally
            {
                Monitor.Exit(visitorLock);
            }

            owner.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(this));
        }
    }

    public bool isVisitor(IPlayer visitor)
    {
        Monitor.Enter(visitorLock);
        try
        {
            return visitors.Contains(visitor);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public bool addItem(PlayerShopItem item)
    {
        lock (items)
        {
            if (items.Count >= 16)
            {
                return false;
            }

            items.Add(item);
            return true;
        }
    }

    private void removeFromSlot(int slot)
    {
        items.RemoveAt(slot);
    }

    private static bool canBuy(IClient c, Item newItem)
    {
        return InventoryManipulator.checkSpace(c, newItem.getItemId(), newItem.getQuantity(), newItem.getOwner()) && InventoryManipulator.addFromDrop(c, newItem, false);
    }

    public void takeItemBack(int slot, IPlayer chr)
    {
        lock (items)
        {
            var shopItem = items.ElementAt(slot);
            if (shopItem.isExist())
            {
                if (shopItem.getBundles() > 0)
                {
                    Item iitem = shopItem.getItem().copy();
                    iitem.setQuantity((short)(shopItem.getItem().getQuantity() * shopItem.getBundles()));

                    if (!Inventory.checkSpot(chr, iitem))
                    {
                        chr.sendPacket(PacketCreator.serverNotice(1, "Have a slot available on your inventory to claim back the item."));
                        chr.sendPacket(PacketCreator.enableActions());
                        return;
                    }

                    InventoryManipulator.addFromDrop(chr.Client, iitem, true);
                }

                removeFromSlot(slot);
                chr.sendPacket(PacketCreator.getPlayerShopItemUpdate(this));
            }
        }
    }

    /**
     * no warnings for now o.o
     *
     * @param c
     * @param item
     * @param quantity
     */
    public bool buy(IClient c, int item, short quantity)
    {
        lock (items)
        {
            if (isVisitor(c.OnlinedCharacter))
            {
                PlayerShopItem pItem = items.get(item);
                Item newItem = pItem.getItem().copy();

                newItem.setQuantity((short)(pItem.getItem().getQuantity() * quantity));
                if (quantity < 1 || !pItem.isExist() || pItem.getBundles() < quantity)
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return false;
                }
                else if (newItem.getInventoryType().Equals(InventoryType.EQUIP) && newItem.getQuantity() > 1)
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return false;
                }

                KarmaManipulator.toggleKarmaFlagToUntradeable(newItem);

                Monitor.Enter(visitorLock);
                try
                {
                    int price = (int)Math.Min((float)pItem.getPrice() * quantity, int.MaxValue);

                    if (c.OnlinedCharacter.getMeso() >= price)
                    {
                        if (!owner.canHoldMeso(price))
                        {    // thanks Rohenn for noticing owner hold check misplaced
                            c.OnlinedCharacter.dropMessage(1, "Transaction failed since the shop owner can't hold any more mesos.");
                            c.sendPacket(PacketCreator.enableActions());
                            return false;
                        }

                        if (canBuy(c, newItem))
                        {
                            c.OnlinedCharacter.gainMeso(-price, false);
                            price -= Trade.getFee(price);  // thanks BHB for pointing out trade fees not applying here
                            owner.gainMeso(price, true);

                            SoldItem soldItem = new SoldItem(c.OnlinedCharacter.getName(), pItem.getItem().getItemId(), quantity, price);
                            owner.sendPacket(PacketCreator.getPlayerShopOwnerUpdate(soldItem, item));

                            lock (sold)
                            {
                                sold.Add(soldItem);
                            }

                            pItem.setBundles((short)(pItem.getBundles() - quantity));
                            if (pItem.getBundles() < 1)
                            {
                                pItem.setDoesExist(false);
                                if (++boughtnumber == items.Count)
                                {
                                    owner.setPlayerShop(null);
                                    setOpen(false);
                                    closeShop();
                                    owner.dropMessage(1, "Your items are sold out, and therefore your shop is closed.");
                                }
                            }
                        }
                        else
                        {
                            c.OnlinedCharacter.dropMessage(1, "Your inventory is full. Please clear a slot before buying this item.");
                            c.sendPacket(PacketCreator.enableActions());
                            return false;
                        }
                    }
                    else
                    {
                        c.OnlinedCharacter.dropMessage(1, "You don't have enough mesos to purchase this item.");
                        c.sendPacket(PacketCreator.enableActions());
                        return false;
                    }

                    return true;
                }
                finally
                {
                    Monitor.Exit(visitorLock);
                }
            }
            else
            {
                return false;
            }
        }
    }

    public void broadcastToVisitors(Packet packet)
    {
        Monitor.Enter(visitorLock);
        try
        {
            InvokeAllVisitor(x => x.sendPacket(packet));
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void broadcastRestoreToVisitors()
    {
        Monitor.Enter(visitorLock);
        try
        {
            InvokeAllVisitor((visitor, i) => visitor.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(i + 1)));
            InvokeAllVisitor(visitor => visitor.sendPacket(PacketCreator.getPlayerShop(this, false)));

            recoverChatLog();
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    private void InvokeAllVisitor(Action<IPlayer> action)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null)
            {
                action(visitors[i]!);
            }
        }
    }

    private void InvokeAllVisitor(Action<IPlayer, int> action)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null)
            {
                action(visitors[i]!, i);
            }
        }
    }

    public void removeVisitors()
    {
        List<IPlayer> visitorList = new(3);

        Monitor.Enter(visitorLock);
        try
        {
            try
            {
                InvokeAllVisitor(visitor =>
                {
                    visitor.sendPacket(PacketCreator.shopErrorMessage(10, 1));
                    visitorList.Add(visitor);
                });
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }

        foreach (IPlayer mc in visitorList)
        {
            forceRemoveVisitor(mc);
        }
        if (owner != null)
        {
            forceRemoveVisitor(owner);
        }
    }

    public void broadcast(Packet packet)
    {
        var client = owner.getClient();
        if (client != null)
        {
            client.sendPacket(packet);
        }
        broadcastToVisitors(packet);
    }

    private byte getVisitorSlot(IPlayer chr)
    {
        return (byte)(Array.FindIndex(getVisitors(), x => x?.Id == chr.Id) + 1);
    }

    public void chat(IClient c, string chat)
    {
        byte s = getVisitorSlot(c.OnlinedCharacter);

        lock (chatLog)
        {
            chatLog.Add(new(c.OnlinedCharacter, chat));
            if (chatLog.Count > 25)
            {
                chatLog.RemoveAt(0);
            }
            chatSlot.AddOrUpdate(c.OnlinedCharacter.getId(), s);
        }

        broadcast(PacketCreator.getPlayerShopChat(c.OnlinedCharacter, chat, s));
    }

    private void recoverChatLog()
    {
        lock (chatLog)
        {
            foreach (var it in chatLog)
            {
                IPlayer chr = it.Key;
                var pos = chatSlot.GetValueOrDefault(chr.getId());

                broadcastToVisitors(PacketCreator.getPlayerShopChat(chr, it.Value, pos));
            }
        }
    }

    private void clearChatLog()
    {
        lock (chatLog)
        {
            chatLog.Clear();
        }
    }

    public void closeShop()
    {
        clearChatLog();
        removeVisitors();
        owner.getMap().broadcastMessage(PacketCreator.removePlayerShopBox(this));
    }

    public void sendShop(IClient c)
    {
        Monitor.Enter(visitorLock);
        try
        {
            c.sendPacket(PacketCreator.getPlayerShop(this, isOwner(c.OnlinedCharacter)));
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public IPlayer getOwner()
    {
        return owner;
    }

    public IPlayer?[] getVisitors()
    {
        Monitor.Enter(visitorLock);
        try
        {
            return visitors.ToArray();
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public List<PlayerShopItem> getItems()
    {
        lock (items)
        {
            return new(items);
        }
    }

    public bool hasItem(int itemid)
    {
        return getItems().Any(x => x.getItem().getItemId() == itemid && x.isExist() && x.getBundles() > 0);
    }

    public string getDescription()
    {
        return description;
    }

    public void setDescription(string description)
    {
        this.description = description;
    }

    public void banPlayer(string name)
    {
        if (!bannedList.Contains(name))
        {
            bannedList.Add(name);
        }

        IPlayer? target = null;
        Monitor.Enter(visitorLock);
        try
        {
            target = visitors.FirstOrDefault(x => x?.Name == name);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }

        if (target != null)
        {
            target.sendPacket(PacketCreator.shopErrorMessage(5, 1));
            removeVisitor(target);
        }
    }

    public bool isBanned(string name)
    {
        return bannedList.Contains(name);
    }

    // synchronized
    public bool visitShop(IPlayer chr)
    {
        Monitor.Enter(visitorLock);
        try
        {
            if (isBanned(chr.getName()))
            {
                chr.dropMessage(1, "You have been banned from this store.");
                return false;
            }

            if (!isOpen())
            {
                chr.dropMessage(1, "This store is not yet open.");
                return false;
            }

            if (hasFreeSlot() && !isVisitor(chr))
            {
                addVisitor(chr);
                chr.setPlayerShop(this);
                sendShop(chr.getClient());

                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }

    }

    public List<PlayerShopItem> sendAvailableBundles(int itemid)
    {
        lock (items)
        {
            return items.Where(x => x.getItem().getItemId() == itemid && x.getBundles() > 0 && x.isExist()).ToList();
        }
    }

    public List<SoldItem> getSold()
    {
        lock (sold)
        {
            return new List<SoldItem>(sold);
        }
    }

    public override void sendDestroyData(IClient client)
    {
        client.sendPacket(PacketCreator.removePlayerShopBox(this));
    }

    public override void sendSpawnData(IClient client)
    {
        client.sendPacket(PacketCreator.updatePlayerShopBox(this));
    }

    public override MapObjectType getType()
    {
        return MapObjectType.SHOP;
    }
}
