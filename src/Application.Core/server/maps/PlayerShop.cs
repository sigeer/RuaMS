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


using client;
using client.inventory;
using client.inventory.manipulator;
using net.packet;
using tools;

namespace server.maps;

/**
 * @author Matze
 * @author Ronan - concurrency protection
 */
public class PlayerShop : AbstractMapObject
{
    private AtomicBoolean open = new AtomicBoolean(false);
    private Character owner;
    private int itemid;

    private Character?[] visitors = new Character[3];
    private List<PlayerShopItem> items = new();
    private List<SoldItem> sold = new();
    private string description;
    private int boughtnumber = 0;
    private List<string> bannedList = new();
    private List<KeyValuePair<Character, string>> chatLog = new();
    private Dictionary<int, byte> chatSlot = new();
    private object visitorLock = new object();

    public PlayerShop(Character owner, string description, int itemid)
    {
        this.setPosition(owner.getPosition());
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

    public bool hasFreeSlot()
    {
        Monitor.Enter(visitorLock);
        try
        {
            return visitors[0] == null || visitors[1] == null || visitors[2] == null;
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
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

    public bool isOwner(Character chr)
    {
        return owner.Equals(chr);
    }

    private void addVisitor(Character visitor)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] == null)
            {
                visitors[i] = visitor;
                visitor.setSlot(i);

                this.broadcast(PacketCreator.getPlayerShopNewVisitor(visitor, i + 1));
                owner.getMap().broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                break;
            }
        }
    }

    public void forceRemoveVisitor(Character visitor)
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
                    visitor.setSlot(-1);

                    this.broadcast(PacketCreator.getPlayerShopRemoveVisitor(i + 1));
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

    public void removeVisitor(Character visitor)
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
                    if (visitors[i] != null && visitors[i].getId() == visitor.getId())
                    {
                        visitor.setSlot(-1);    //absolutely cant remove player slot for late players without dc'ing them... heh

                        for (int j = i; j < 2; j++)
                        {
                            if (visitors[j] != null)
                            {
                                owner.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(j + 1));
                            }
                            visitors[j] = visitors[j + 1];
                            if (visitors[j] != null)
                            {
                                visitors[j]!.setSlot(j);
                            }
                        }
                        visitors[2] = null;
                        for (int j = i; j < 2; j++)
                        {
                            if (visitors[j] != null)
                            {
                                owner.sendPacket(PacketCreator.getPlayerShopNewVisitor(visitors[j], j + 1));
                            }
                        }

                        this.broadcastRestoreToVisitors();
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

    public bool isVisitor(Character visitor)
    {
        Monitor.Enter(visitorLock);
        try
        {
            return visitors[0] == visitor || visitors[1] == visitor || visitors[2] == visitor;
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

    private static bool canBuy(Client c, Item newItem)
    {
        return InventoryManipulator.checkSpace(c, newItem.getItemId(), newItem.getQuantity(), newItem.getOwner()) && InventoryManipulator.addFromDrop(c, newItem, false);
    }

    public void takeItemBack(int slot, Character chr)
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

                    InventoryManipulator.addFromDrop(chr.getClient(), iitem, true);
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
    public bool buy(Client c, int item, short quantity)
    {
        lock (items)
        {
            if (isVisitor(c.getPlayer()))
            {
                PlayerShopItem pItem = items.get(item);
                Item newItem = pItem.getItem().copy();

                newItem.setQuantity((short)((pItem.getItem().getQuantity() * quantity)));
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

                    if (c.getPlayer().getMeso() >= price)
                    {
                        if (!owner.canHoldMeso(price))
                        {    // thanks Rohenn for noticing owner hold check misplaced
                            c.getPlayer().dropMessage(1, "Transaction failed since the shop owner can't hold any more mesos.");
                            c.sendPacket(PacketCreator.enableActions());
                            return false;
                        }

                        if (canBuy(c, newItem))
                        {
                            c.getPlayer().gainMeso(-price, false);
                            price -= Trade.getFee(price);  // thanks BHB for pointing out trade fees not applying here
                            owner.gainMeso(price, true);

                            SoldItem soldItem = new SoldItem(c.getPlayer().getName(), pItem.getItem().getItemId(), quantity, price);
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
                                    this.setOpen(false);
                                    this.closeShop();
                                    owner.dropMessage(1, "Your items are sold out, and therefore your shop is closed.");
                                }
                            }
                        }
                        else
                        {
                            c.getPlayer().dropMessage(1, "Your inventory is full. Please clear a slot before buying this item.");
                            c.sendPacket(PacketCreator.enableActions());
                            return false;
                        }
                    }
                    else
                    {
                        c.getPlayer().dropMessage(1, "You don't have enough mesos to purchase this item.");
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
            for (int i = 0; i < 3; i++)
            {
                if (visitors[i] != null)
                {
                    visitors[i]!.sendPacket(packet);
                }
            }
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
            for (int i = 0; i < 3; i++)
            {
                if (visitors[i] != null)
                {
                    visitors[i]!.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(i + 1));
                }
            }

            for (int i = 0; i < 3; i++)
            {
                if (visitors[i] != null)
                {
                    visitors[i]!.sendPacket(PacketCreator.getPlayerShop(this, false));
                }
            }

            recoverChatLog();
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void removeVisitors()
    {
        List<Character> visitorList = new(3);

        Monitor.Enter(visitorLock);
        try
        {
            try
            {
                for (int i = 0; i < 3; i++)
                {
                    if (visitors[i] != null)
                    {
                        visitors[i]!.sendPacket(PacketCreator.shopErrorMessage(10, 1));
                        visitorList.Add(visitors[i]!);
                    }
                }
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

        foreach (Character mc in visitorList)
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
        Client client = owner.getClient();
        if (client != null)
        {
            client.sendPacket(packet);
        }
        broadcastToVisitors(packet);
    }

    private byte getVisitorSlot(Character chr)
    {
        byte s = 0;
        foreach (Character mc in getVisitors())
        {
            s++;
            if (mc != null)
            {
                if (mc.getName().Equals(chr.getName(), StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            else if (s == 3)
            {
                s = 0;
            }
        }

        return s;
    }

    public void chat(Client c, string chat)
    {
        byte s = getVisitorSlot(c.getPlayer());

        lock (chatLog)
        {
            chatLog.Add(new(c.getPlayer(), chat));
            if (chatLog.Count > 25)
            {
                chatLog.RemoveAt(0);
            }
            chatSlot.AddOrUpdate(c.getPlayer().getId(), s);
        }

        broadcast(PacketCreator.getPlayerShopChat(c.getPlayer(), chat, s));
    }

    private void recoverChatLog()
    {
        lock (chatLog)
        {
            foreach (var it in chatLog)
            {
                Character chr = it.Key;
                var pos = chatSlot.get(chr.getId());

                broadcastToVisitors(PacketCreator.getPlayerShopChat(chr, it.Value, pos.Value));
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

    public void sendShop(Client c)
    {
        Monitor.Enter(visitorLock);
        try
        {
            c.sendPacket(PacketCreator.getPlayerShop(this, isOwner(c.getPlayer())));
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public Character getOwner()
    {
        return owner;
    }

    public Character?[] getVisitors()
    {
        Monitor.Enter(visitorLock);
        try
        {
            Character?[] copy = new Character[3];
            Array.ConstrainedCopy(visitors, 0, copy, 0, 3);
            return copy;
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
        foreach (PlayerShopItem mpsi in getItems())
        {
            if (mpsi.getItem().getItemId() == itemid && mpsi.isExist() && mpsi.getBundles() > 0)
            {
                return true;
            }
        }

        return false;
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

        Character? target = null;
        Monitor.Enter(visitorLock);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                if (visitors[i] != null && visitors[i]!.getName().Equals(name))
                {
                    target = visitors[i];
                    break;
                }
            }
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
    public bool visitShop(Character chr)
    {
        if (this.isBanned(chr.getName()))
        {
            chr.dropMessage(1, "You have been banned from this store.");
            return false;
        }

        Monitor.Enter(visitorLock);
        try
        {
            if (!open.Get())
            {
                chr.dropMessage(1, "This store is not yet open.");
                return false;
            }

            if (this.hasFreeSlot() && !this.isVisitor(chr))
            {
                this.addVisitor(chr);
                chr.setPlayerShop(this);
                this.sendShop(chr.getClient());

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
        List<PlayerShopItem> list = new();
        List<PlayerShopItem> all = new();

        lock (items)
        {
            all.AddRange(items);
        }

        foreach (PlayerShopItem mpsi in all)
        {
            if (mpsi.getItem().getItemId() == itemid && mpsi.getBundles() > 0 && mpsi.isExist())
            {
                list.Add(mpsi);
            }
        }
        return list;
    }

    public List<SoldItem> getSold()
    {
        lock (sold)
        {
            return new List<SoldItem>(sold);
        }
    }

    public override void sendDestroyData(Client client)
    {
        client.sendPacket(PacketCreator.removePlayerShopBox(this));
    }

    public override void sendSpawnData(Client client)
    {
        client.sendPacket(PacketCreator.updatePlayerShopBox(this));
    }

    public override MapObjectType getType()
    {
        return MapObjectType.SHOP;
    }

    public class SoldItem
    {

        int itemid, mesos;
        short quantity;
        string buyer;

        public SoldItem(string buyer, int itemid, short quantity, int mesos)
        {
            this.buyer = buyer;
            this.itemid = itemid;
            this.quantity = quantity;
            this.mesos = mesos;
        }

        public string getBuyer()
        {
            return buyer;
        }

        public int getItemId()
        {
            return itemid;
        }

        public short getQuantity()
        {
            return quantity;
        }

        public int getMesos()
        {
            return mesos;
        }
    }
}
