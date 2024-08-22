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
using client.processor.npc;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using tools;

namespace server.maps;



/**
 * @author XoticStory
 * @author Ronan - concurrency protection
 */
public class HiredMerchant : AbstractMapObject
{
    private static int VISITOR_HISTORY_LIMIT = 10;
    private static int BLACKLIST_LIMIT = 20;

    private int ownerId;
    private int itemId;
    private int mesos = 0;
    private int channel;
    private int world;
    private long start;
    private string ownerName = "";
    private string description = "";
    private List<PlayerShopItem> items = new();
    private List<KeyValuePair<string, byte>> messages = new();
    private List<SoldItem> sold = new();
    private AtomicBoolean open = new AtomicBoolean();
    private bool published = false;
    private MapleMap? map;
    private Visitor?[] visitors = new Visitor[3];
    private List<PastVisitor> visitorHistory = new();
    private List<string> blacklist = new(); // case-sensitive character names
    private object visitorLock = new object();

    private record Visitor(Character chr, DateTimeOffset enteredAt) { }

    public record PastVisitor(string chrName, TimeSpan visitDuration) { }

    public HiredMerchant(Character owner, string desc, int itemId)
    {
        this.setPosition(owner.getPosition());
        this.start = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        this.ownerId = owner.getId();
        this.channel = owner.getClient().getChannel();
        this.world = owner.getWorld();
        this.itemId = itemId;
        this.ownerName = owner.getName();
        this.description = desc;
        this.map = owner.getMap();
    }

    public void broadcastToVisitorsThreadsafe(Packet packet)
    {
        Monitor.Enter(visitorLock);
        try
        {
            broadcastToVisitors(packet);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    private void broadcastToVisitors(Packet packet)
    {
        foreach (Visitor visitor in visitors)
        {
            if (visitor != null)
            {
                visitor.chr.sendPacket(packet);
            }
        }
    }

    public byte[] getShopRoomInfo()
    {
        Monitor.Enter(visitorLock);
        try
        {
            byte count = 0;
            if (this.isOpen())
            {
                foreach (var visitor in visitors)
                {
                    if (visitor != null)
                    {
                        count++;
                    }
                }
            }
            else
            {
                count = (byte)(visitors.Length + 1);
            }

            return new byte[] { count, (byte)(visitors.Length + 1) };
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public bool addVisitor(Character visitor)
    {
        Monitor.Enter(visitorLock);
        try
        {
            int i = this.getFreeSlot();
            if (i > -1)
            {
                visitors[i] = new Visitor(visitor, DateTimeOffset.Now);
                broadcastToVisitors(PacketCreator.hiredMerchantVisitorAdd(visitor, i + 1));
                this.getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));

                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void removeVisitor(Character chr)
    {
        Monitor.Enter(visitorLock);
        try
        {
            int slot = getVisitorSlot(chr);
            if (slot < 0)
            { //Not found
                return;
            }

            var visitor = visitors[slot];
            if (visitor != null && visitor.chr.getId() == chr.getId())
            {
                visitors[slot] = null;
                addVisitorToHistory(visitor);
                broadcastToVisitors(PacketCreator.hiredMerchantVisitorLeave(slot + 1));
                this.getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));
            }
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    private void addVisitorToHistory(Visitor visitor)
    {
        TimeSpan visitDuration = visitor.enteredAt - DateTimeOffset.Now;
        visitorHistory.Insert(0, new PastVisitor(visitor.chr.getName(), visitDuration));
        while (visitorHistory.Count > VISITOR_HISTORY_LIMIT)
        {
            visitorHistory.RemoveAt(visitorHistory.Count - 1);
        }
    }

    public int getVisitorSlotThreadsafe(Character visitor)
    {
        Monitor.Enter(visitorLock);
        try
        {
            return getVisitorSlot(visitor);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    private int getVisitorSlot(Character visitor)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null && visitors[i].chr.getId() == visitor.getId())
            {
                return i;
            }
        }
        return -1; //Actually 0 because of the +1's.
    }

    private void removeAllVisitors()
    {
        Monitor.Enter(visitorLock);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                Visitor visitor = visitors[i];

                if (visitor != null)
                {
                    Character visitorChr = visitor.chr;
                    visitorChr.setHiredMerchant(null);
                    visitorChr.sendPacket(PacketCreator.leaveHiredMerchant(i + 1, 0x11));
                    visitorChr.sendPacket(PacketCreator.hiredMerchantMaintenanceMessage());
                    visitors[i] = null;
                    addVisitorToHistory(visitor);
                }
            }

            this.getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    private void removeOwner(Character owner)
    {
        if (owner.getHiredMerchant() == this)
        {
            owner.sendPacket(PacketCreator.hiredMerchantOwnerLeave());
            owner.sendPacket(PacketCreator.leaveHiredMerchant(0x00, 0x03));
            owner.setHiredMerchant(null);
        }
    }

    public void withdrawMesos(Character chr)
    {
        if (isOwner(chr))
        {
            lock (items)
            {
                chr.withdrawMerchantMesos();
            }
        }
    }

    public void takeItemBack(int slot, Character chr)
    {
        lock (items)
        {
            PlayerShopItem shopItem = items.get(slot);
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
                chr.sendPacket(PacketCreator.updateHiredMerchant(this, chr));
            }

            if (YamlConfig.config.server.USE_ENFORCE_MERCHANT_SAVE)
            {
                chr.saveCharToDB(false);
            }
        }
    }

    private static bool canBuy(Client c, Item newItem)
    {    // thanks xiaokelvin (Conrad) for noticing a leaked test code here
        return InventoryManipulator.checkSpace(c, newItem.getItemId(), newItem.getQuantity(), newItem.getOwner()) && InventoryManipulator.addFromDrop(c, newItem, false);
    }

    private int getQuantityLeft(int itemid)
    {
        lock (items)
        {
            int count = 0;

            foreach (PlayerShopItem mpsi in items)
            {
                if (mpsi.getItem().getItemId() == itemid)
                {
                    count += (mpsi.getBundles() * mpsi.getItem().getQuantity());
                }
            }

            return count;
        }
    }

    public void buy(Client c, int item, short quantity)
    {
        lock (items)
        {
            PlayerShopItem pItem = items.get(item);
            Item newItem = pItem.getItem().copy();

            newItem.setQuantity((short)((pItem.getItem().getQuantity() * quantity)));
            if (quantity < 1 || !pItem.isExist() || pItem.getBundles() < quantity)
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }
            else if (newItem.getInventoryType().Equals(InventoryType.EQUIP) && newItem.getQuantity() > 1)
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            KarmaManipulator.toggleKarmaFlagToUntradeable(newItem);

            int price = (int)Math.Min((float)pItem.getPrice() * quantity, int.MaxValue);
            if (c.getPlayer().getMeso() >= price)
            {
                if (canBuy(c, newItem))
                {
                    c.getPlayer().gainMeso(-price, false);
                    price -= Trade.getFee(price);  // thanks BHB for pointing out trade fees not applying here

                    lock (sold)
                    {
                        sold.Add(new SoldItem(c.getPlayer().getName(), pItem.getItem().getItemId(), newItem.getQuantity(), price));
                    }

                    pItem.setBundles((short)(pItem.getBundles() - quantity));
                    if (pItem.getBundles() < 1)
                    {
                        pItem.setDoesExist(false);
                    }

                    if (YamlConfig.config.server.USE_ANNOUNCE_SHOPITEMSOLD)
                    {   // idea thanks to Vcoc
                        announceItemSold(newItem, price, getQuantityLeft(pItem.getItem().getItemId()));
                    }

                    var owner = Server.getInstance().getWorld(world).getPlayerStorage().getCharacterByName(ownerName);
                    if (owner != null)
                    {
                        owner.addMerchantMesos(price);
                    }
                    else
                    {
                        using var dbContext = new DBContext();
                        var merchantMesos = dbContext.Characters.Where(x => x.Id == ownerId).Select(x => new { x.MerchantMesos }).FirstOrDefault()?.MerchantMesos ?? 0;
                        merchantMesos += price;

                        dbContext.Characters.Where(x => x.Id == ownerId).ExecuteUpdate(x => x.SetProperty(y => y.MerchantMesos, Math.Min(merchantMesos, int.MaxValue)));
                    }
                }
                else
                {
                    c.getPlayer().dropMessage(1, "Your inventory is full. Please clear a slot before buying this item.");
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }
            }
            else
            {
                c.getPlayer().dropMessage(1, "You don't have enough mesos to purchase this item.");
                c.sendPacket(PacketCreator.enableActions());
                return;
            }
            try
            {
                this.saveItems(false);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }
        }
    }

    private void announceItemSold(Item item, int mesos, int inStore)
    {
        string qtyStr = (item.getQuantity() > 1) ? " x " + item.getQuantity() : "";

        var player = Server.getInstance().getWorld(world).getPlayerStorage().getCharacterById(ownerId);
        if (player != null && player.isLoggedinWorld())
        {
            player.dropMessage(6, "[Hired Merchant] Item '" + ItemInformationProvider.getInstance().getName(item.getItemId()) + "'" + qtyStr + " has been sold for " + mesos + " mesos. (" + inStore + " left)");
        }
    }

    public void forceClose()
    {
        //Server.getInstance().getChannel(world, channel).removeHiredMerchant(ownerId);
        map.broadcastMessage(PacketCreator.removeHiredMerchantBox(getOwnerId()));
        map.removeMapObject(this);

        var owner = Server.getInstance().getWorld(world).getPlayerStorage().getCharacterById(ownerId);

        Monitor.Enter(visitorLock);
        try
        {
            setOpen(false);
            removeAllVisitors();

            if (owner != null && owner.isLoggedinWorld() && this == owner.getHiredMerchant())
            {
                closeOwnerMerchant(owner);
            }
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }

        Server.getInstance().getWorld(world).unregisterHiredMerchant(this);

        try
        {
            saveItems(true);
            lock (items)
            {
                items.Clear();
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
        }

        var player = Server.getInstance().getWorld(world).getPlayerStorage().getCharacterById(ownerId);
        if (player != null)
        {
            player.setHasMerchant(false);
        }
        else
        {
            try
            {
                using var dbContext = new DBContext();
                dbContext.Characters.Where(x => x.Id == ownerId)
                    .ExecuteUpdate(x => x.SetProperty(y => y.HasMerchant, false));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }

        map = null;
    }

    public void closeOwnerMerchant(Character chr)
    {
        if (this.isOwner(chr))
        {
            this.closeShop(chr.getClient(), false);
            chr.setHasMerchant(false);
        }
    }

    private void closeShop(Client c, bool timeout)
    {
        map.removeMapObject(this);
        map.broadcastMessage(PacketCreator.removeHiredMerchantBox(ownerId));
        c.getChannelServer().removeHiredMerchant(ownerId);

        this.removeAllVisitors();
        this.removeOwner(c.getPlayer());

        try
        {
            List<PlayerShopItem> copyItems = getItems();
            if (check(c.getPlayer(), copyItems) && !timeout)
            {
                foreach (PlayerShopItem mpsi in copyItems)
                {
                    if (mpsi.isExist())
                    {
                        if (mpsi.getItem().getInventoryType().Equals(InventoryType.EQUIP))
                        {
                            InventoryManipulator.addFromDrop(c, mpsi.getItem(), false);
                        }
                        else
                        {
                            InventoryManipulator.addById(c, mpsi.getItem().getItemId(), (short)(mpsi.getBundles() * mpsi.getItem().getQuantity()), mpsi.getItem().getOwner(), -1, mpsi.getItem().getFlag(), mpsi.getItem().getExpiration());
                        }
                    }
                }

                lock (items)
                {
                    items.Clear();
                }
            }

            try
            {
                this.saveItems(timeout);
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.ToString());
            }

            // thanks Rohenn for noticing a possible dupe scenario on closing shop
            var player = c.getWorldServer().getPlayerStorage().getCharacterById(ownerId);
            if (player != null)
            {
                player.setHasMerchant(false);
            }
            else
            {
                using var dbContext = new DBContext();
                dbContext.Characters.Where(x => x.Id == ownerId)
                    .ExecuteUpdate(x => x.SetProperty(y => y.HasMerchant, false));


                if (YamlConfig.config.server.USE_ENFORCE_MERCHANT_SAVE)
                {
                    c.getPlayer().saveCharToDB(false);
                }

                lock (items)
                {
                    items.Clear();
                }
            }
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }

        Server.getInstance().getWorld(world).unregisterHiredMerchant(this);
    }

    public void visitShop(Character chr)
    {
        Monitor.Enter(visitorLock);
        try
        {
            if (this.isOwner(chr))
            {
                this.setOpen(false);
                this.removeAllVisitors();

                chr.sendPacket(PacketCreator.getHiredMerchant(chr, this, false));
            }
            else if (!this.isOpen())
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(18));
                return;
            }
            else if (isBlacklisted(chr.getName()))
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(17));
                return;
            }
            else if (!this.addVisitor(chr))
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(2));
                return;
            }
            else
            {
                chr.sendPacket(PacketCreator.getHiredMerchant(chr, this, false));
            }
            chr.setHiredMerchant(this);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public string getOwner()
    {
        return ownerName;
    }

    public void clearItems()
    {
        lock (items)
        {
            items.Clear();
        }
    }

    public int getOwnerId()
    {
        return ownerId;
    }

    public string getDescription()
    {
        return description;
    }

    public Character[] getVisitorCharacters()
    {
        Monitor.Enter(visitorLock);
        try
        {
            Character[] copy = new Character[3];
            for (int i = 0; i < visitors.Length; i++)
            {
                Visitor visitor = visitors[i];
                if (visitor != null)
                {
                    copy[i] = visitor.chr;
                }
            }

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
            return items;
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

    public void clearInexistentItems()
    {
        lock (items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (!items.get(i).isExist())
                {
                    items.RemoveAt(i);
                }
            }

            try
            {
                this.saveItems(false);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.ToString());
            }
        }
    }

    private void removeFromSlot(int slot)
    {
        items.RemoveAt(slot);

        try
        {
            this.saveItems(false);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex.ToString());
        }
    }

    private int getFreeSlot()
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    public void setDescription(string description)
    {
        this.description = description;
    }

    public bool isPublished()
    {
        return published;
    }

    public bool isOpen()
    {
        return open.Get();
    }

    public void setOpen(bool set)
    {
        open.GetAndSet(set);
        published = true;
    }

    public int getItemId()
    {
        return itemId;
    }

    public bool isOwner(Character chr)
    {
        return chr.getId() == ownerId;
    }

    public void sendMessage(Character chr, string msg)
    {
        string message = chr.getName() + " : " + msg;
        byte slot = (byte)(getVisitorSlot(chr) + 1);

        lock (messages)
        {
            messages.Add(new(message, slot));
        }
        broadcastToVisitorsThreadsafe(PacketCreator.hiredMerchantChat(message, slot));
    }

    public List<PlayerShopItem> sendAvailableBundles(int itemid)
    {
        List<PlayerShopItem> list = new();
        List<PlayerShopItem> all = new();

        if (!open)
        {
            return list;
        }

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

    public void saveItems(bool shutdown)
    {
        List<ItemInventoryType> itemsWithType = new();
        List<short> bundles = new();

        foreach (PlayerShopItem pItems in getItems())
        {
            Item newItem = pItems.getItem();
            short newBundle = pItems.getBundles();

            if (shutdown)
            { //is "shutdown" really necessary?
                newItem.setQuantity(pItems.getItem().getQuantity());
            }
            else
            {
                newItem.setQuantity(pItems.getItem().getQuantity());
            }
            if (newBundle > 0)
            {
                itemsWithType.Add(new(newItem, newItem.getInventoryType()));
                bundles.Add(newBundle);
            }
        }

        using var dbContext = new DBContext();
        ItemFactory.MERCHANT.saveItems(itemsWithType, bundles, this.ownerId, dbContext);

        FredrickProcessor.insertFredrickLog(this.ownerId);
    }

    private static bool check(Character chr, List<PlayerShopItem> items)
    {
        List<ItemInventoryType> li = new();
        foreach (PlayerShopItem item in items)
        {
            Item it = item.getItem().copy();
            it.setQuantity((short)(it.getQuantity() * item.getBundles()));

            li.Add(new(it, it.getInventoryType()));
        }

        return Inventory.checkSpotsAndOwnership(chr, li);
    }

    public int getChannel()
    {
        return channel;
    }

    public int getTimeOpen()
    {
        double openTime = (DateTimeOffset.Now.ToUnixTimeMilliseconds() - start) / 60000;
        openTime /= 1440;   // heuristics since engineered method to count time here is unknown
        openTime *= 1318;

        return (int)Math.Ceiling(openTime);
    }

    public void clearMessages()
    {
        lock (messages)
        {
            messages.Clear();
        }
    }

    public List<KeyValuePair<string, Byte>> getMessages()
    {
        lock (messages)
        {
            List<KeyValuePair<string, Byte>> msgList = new();
            msgList.AddRange(messages);

            return msgList;
        }
    }

    public List<PastVisitor> getVisitorHistory()
    {
        return visitorHistory;
    }

    public void addToBlacklist(string chrName)
    {
        Monitor.Enter(visitorLock);
        try
        {
            if (blacklist.Count >= BLACKLIST_LIMIT)
            {
                return;
            }
            blacklist.Add(chrName);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void removeFromBlacklist(string chrName)
    {
        Monitor.Enter(visitorLock);
        try
        {
            blacklist.Remove(chrName);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public HashSet<string> getBlacklist()
    {
        return blacklist.ToHashSet();
    }

    private bool isBlacklisted(string chrName)
    {
        Monitor.Enter(visitorLock);
        try
        {
            return blacklist.Contains(chrName);
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public int getMapId()
    {
        return map.getId();
    }

    public MapleMap? getMap()
    {
        return map;
    }

    public List<SoldItem> getSold()
    {
        lock (sold)
        {
            return sold;
        }
    }

    public int getMesos()
    {
        return mesos;
    }

    public override MapObjectType getType()
    {
        return MapObjectType.HIRED_MERCHANT;
    }

    public override void sendDestroyData(Client client) { }

    public override void sendSpawnData(Client client)
    {
        client.sendPacket(PacketCreator.spawnHiredMerchantBox(this));
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
