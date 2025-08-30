using Application.Core.Channel;
using Application.Core.Game.Maps;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Trades;

public class HiredMerchant : AbstractMapObject, IPlayerShop
{
    public IPlayer? Owner { get; private set; }
    public int Mesos { get; private set; }
    public long StartTime { get; }
    public long ExpirationTime { get; }
    public TimeSpan LeftTime { get; }


    private List<KeyValuePair<string, byte>> messages = new();
    private AtomicBoolean open = new AtomicBoolean();

    private Visitor?[] visitors = new Visitor[3];
    FixedSizeQueue<PastVisitor> visitorHistory = new(Limits.VISITOR_HISTORY_LIMIT);
    private object visitorLock = new object();
    public AtomicEnum<PlayerShopStatus> Status { get; set; }
    public HashSet<string> BlackList { get; }
    public int SourceItemId { get; }
    public int Channel { get; }
    public WorldChannel ChannelServer { get; }
    public bool SoldNotify { get; }
    public string Title { get; private set; }
    public int OwnerId { get; }
    public string OwnerName { get; }
    public List<PlayerShopItem> Commodity { get; }

    public List<SoldItem> SoldHistory { get; }

    public PlayerShopType Type { get; }

    public HiredMerchant(IPlayer owner, string desc, Item item)
    {
        setPosition(owner.getPosition());
        StartTime = owner.Client.CurrentServerContainer.getCurrentTime();
        ExpirationTime = item.getExpiration();
        Owner = owner;
        OwnerName = owner.Name;
        OwnerId = owner.getId();
        Channel = owner.getClient().getChannel();
        ChannelServer = owner.getChannelServer();
        SourceItemId = item.getItemId();
        Title = desc;
        setMap(owner.MapModel);
        Channel = owner.Channel;
        Commodity = [];
        SoldHistory = [];
        BlackList = [];

        Type = PlayerShopType.HiredMerchant;
        SoldNotify = ItemId.MerchantWithNotifyBegin <= item.getItemId() && ItemId.MerchantWithNotifyEnd >= item.getItemId();
        Status = new AtomicEnum<PlayerShopStatus>(PlayerShopStatus.Maintenance);
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
        foreach (var visitor in visitors)
        {
            if (visitor != null)
            {
                visitor.chr.sendPacket(packet);
            }
        }
    }

    public void BroadcastShopItemUpdate()
    {
        foreach (var visitor in visitors)
        {
            if (visitor != null)
            {
                visitor.chr.sendPacket(PacketCreator.updateHiredMerchant(this, visitor.chr));
            }
        }
    }

    public byte[] getShopRoomInfo()
    {
        Monitor.Enter(visitorLock);
        try
        {
            byte count = 0;
            if (Status.Is(PlayerShopStatus.Opening))
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

    public bool AddVisitor(IPlayer visitor)
    {
        Monitor.Enter(visitorLock);
        try
        {
            int i = getFreeSlot();
            if (i > -1)
            {
                visitors[i] = new Visitor(visitor, ChannelServer.Container.GetCurrentTimeDateTimeOffSet());
                broadcastToVisitors(PacketCreator.hiredMerchantVisitorAdd(visitor, i + 1));
                MapModel.broadcastMessage(PacketCreator.updateHiredMerchantBox(this));

                return true;
            }

            return false;
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void OwnerLeave(IPlayer chr)
    {
        if (Owner == null)
            return;

        if (chr.Id != OwnerId)
            return;

        SetOpen();
    }

    public void RemoveVisitor(IPlayer chr)
    {
        Monitor.Enter(visitorLock);
        try
        {
            int slot = getVisitorSlot(chr);
            if (slot < 0)
            {
                //Not found
                return;
            }

            var visitor = visitors[slot];
            if (visitor != null && visitor.chr.getId() == chr.getId())
            {
                visitors[slot] = null;
                addVisitorToHistory(visitor);
                broadcastToVisitors(PacketCreator.hiredMerchantVisitorLeave(slot + 1));
                getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));
            }
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    private void addVisitorToHistory(Visitor visitor)
    {
        // 只需要时间差，不需要使用系统时间
        TimeSpan visitDuration = ChannelServer.Container.GetCurrentTimeDateTimeOffSet() - visitor.enteredAt;
        visitorHistory.Enqueue(new PastVisitor(visitor.chr.getName(), visitDuration));
    }

    public int getVisitorSlotThreadsafe(IPlayer visitor)
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

    private int getVisitorSlot(IPlayer visitor)
    {
        return Array.FindIndex(getVisitorCharacters(), x => x?.Id == visitor.Id);
    }

    private void removeAllVisitors()
    {
        Monitor.Enter(visitorLock);
        try
        {
            for (int i = 0; i < 3; i++)
            {
                var visitor = visitors[i];
                if (visitor != null)
                {
                    IPlayer visitorChr = visitor.chr;
                    visitorChr.VisitingShop = null;
                    visitorChr.sendPacket(PacketCreator.leaveHiredMerchant(i + 1, 0x11));
                    visitorChr.sendPacket(PacketCreator.hiredMerchantMaintenanceMessage());
                    visitors[i] = null;
                    addVisitorToHistory(visitor);
                }
            }

            getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public void ProcessVisitingOwner()
    {
        if (Owner != null)
        {
            Owner.sendPacket(PacketCreator.hiredMerchantOwnerLeave());
            Owner.sendPacket(PacketCreator.leaveHiredMerchant(0x00, 0x03));
            Owner.VisitingShop = null;
            Owner = null;
        }
    }

    Lock mesoLock = new Lock();
    public void withdrawMesos(IPlayer chr)
    {
        if (IsOwner(chr))
        {
            lock (mesoLock)
            {
                Mesos = chr.GainMeso(Mesos, false);
            }
        }
    }

    public void takeItemBack(int slot, IPlayer chr)
    {
        lock (Commodity)
        {
            var shopItem = Commodity[slot];
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
                chr.sendPacket(PacketCreator.updateHiredMerchant(this, chr));
            }

            if (YamlConfig.config.server.USE_ENFORCE_MERCHANT_SAVE)
            {
                chr.saveCharToDB();
            }
        }
    }

    private static bool canBuy(IChannelClient c, Item newItem)
    {
        // thanks xiaokelvin (Conrad) for noticing a leaked test code here
        return InventoryManipulator.checkSpace(c, newItem.getItemId(), newItem.getQuantity(), newItem.getOwner()) && InventoryManipulator.addFromDrop(c, newItem, false);
    }

    public void GainMeso(int meso)
    {
        lock (mesoLock)
        {
            Mesos += meso;
        }
    }

    public string? MesoCheck(int meso)
    {
        if ((long)Mesos + meso > int.MaxValue)
        {
            return "Transaction failed since the shop owner can't hold any more mesos.";
        }
        return null;
    }

    public void InsertSoldHistory(int idx, SoldItem soldItem)
    {
        SoldHistory.Add(soldItem);
    }

    public void OnCommoditySellout()
    {

    }

    private int getQuantityLeft(int itemid)
    {
        lock (Commodity)
        {
            int count = 0;

            foreach (var mpsi in Commodity)
            {
                if (mpsi.getItem().getItemId() == itemid)
                {
                    count += mpsi.getBundles() * mpsi.getItem().getQuantity();
                }
            }

            return count;
        }
    }

    public void Close()
    {
        MapModel.removeMapObject(this);
        MapModel.broadcastMessage(PacketCreator.removeHiredMerchantBox(OwnerId));

        ProcessVisitingOwner();

        removeAllVisitors();
    }


    public void SetOpen()
    {
        if (Owner == null)
            return;

        Status.Set(PlayerShopStatus.Opening);
        Owner.VisitingShop = null;
        Owner = null;
    }

    public void SetMaintenance(IPlayer chr)
    {
        if (chr.Id == OwnerId)
        {
            Status.Set(PlayerShopStatus.Maintenance);
            Owner = chr;

            removeAllVisitors();
        }
    }

    public bool Retrieve(IPlayer owner)
    {
        if (owner.Id != this.OwnerId)
            return false;

        lock (Commodity)
        {
            if (check(owner, Commodity))
            {
                foreach (PlayerShopItem mpsi in Commodity)
                {
                    if (mpsi.isExist())
                    {
                        if (mpsi.getItem().getInventoryType().Equals(InventoryType.EQUIP))
                        {
                            InventoryManipulator.addFromDrop(owner.Client, mpsi.getItem(), false);
                        }
                        else
                        {
                            InventoryManipulator.addById(owner.Client,
                                mpsi.getItem().getItemId(),
                                (short)(mpsi.getBundles() * mpsi.getItem().getQuantity()),
                                mpsi.getItem().getOwner(),
                                mpsi.getItem().getFlag(),
                                mpsi.getItem().getExpiration());
                        }
                    }
                }

                Commodity.Clear();
                return true;
            }
            return false;
        }

    }

    public bool VisitShop(IPlayer chr)
    {
        Monitor.Enter(visitorLock);
        try
        {
            if (chr.VisitingShop != null)
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(17));
                return false;
            }
            if (IsOwner(chr))
            {
                SetMaintenance(chr);
            }
            else if (!Status.Is(PlayerShopStatus.Opening))
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(18));
                return false;
            }
            else if (BlackList.Contains(chr.getName()))
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(17));
                return false;
            }
            else if (!AddVisitor(chr))
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(2));
                return false;
            }

            chr.sendPacket(PacketCreator.getHiredMerchant(chr, this, false));
            chr.VisitingShop = this;
            return true;
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }


    public IPlayer?[] getVisitorCharacters()
    {
        Monitor.Enter(visitorLock);
        try
        {
            return visitors.Select(x => x?.chr).ToArray();
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
    }

    public List<PlayerShopItem> getItems()
    {
        lock (Commodity)
        {
            return Commodity;
        }
    }

    public bool hasItem(int itemid)
    {
        return getItems().Any(x => x.getItem().getItemId() == itemid && x.isExist() && x.getBundles() > 0);
    }

    public bool AddCommodity(PlayerShopItem item)
    {
        lock (Commodity)
        {
            if (Commodity.Count >= Limits.MaxPlayerShopItemCount)
            {
                return false;
            }

            Commodity.Add(item);
            return true;
        }
    }
    /// <summary>
    /// 整理道具
    /// </summary>
    /// <param name="chr"></param>
    public void Restore(IPlayer chr)
    {
        if (IsOwner(chr))
        {
            lock (Commodity)
            {
                Commodity.RemoveAll(x => !x.isExist());
            }

            lock (mesoLock)
            {
                Mesos = chr.GainMeso(Mesos, false);
            }
        }
    }

    public void clearInexistentItems()
    {
        lock (Commodity)
        {
            Commodity.RemoveAll(x => !x.isExist());
        }
    }

    private void removeFromSlot(int slot)
    {
        Commodity.RemoveAt(slot);
    }

    private int getFreeSlot()
    {
        return Array.IndexOf(visitors, null);
    }


    public bool IsOwner(IPlayer chr)
    {
        return chr.Id == OwnerId;
    }

    public void sendMessage(IPlayer chr, string msg)
    {
        string message = chr.Name + " : " + msg;
        byte slot = (byte)(getVisitorSlot(chr) + 1);

        lock (messages)
        {
            messages.Add(new(message, slot));
        }
        broadcastToVisitorsThreadsafe(PacketCreator.hiredMerchantChat(message, slot));
        if (Owner != null)
        {
            Owner.sendPacket(PacketCreator.hiredMerchantChat(message, slot));
        }
    }

    public List<PlayerShopItem> QueryAvailableBundles(int itemid)
    {
        if (!open)
        {
            return [];
        }

        lock (Commodity)
        {
            return Commodity.Where(x => x.getItem().getItemId() == itemid && x.getBundles() > 0 && x.isExist()).ToList();
        }
    }

    //public void saveItems(bool shutdown)
    //{
    //    List<ItemInventoryType> itemsWithType = new();
    //    List<short> bundles = new();

    //    foreach (PlayerShopItem pItems in getItems())
    //    {
    //        Item newItem = pItems.getItem();
    //        short newBundle = pItems.getBundles();

    //        newItem.setQuantity(pItems.getItem().getQuantity());
    //        if (newBundle > 0)
    //        {
    //            itemsWithType.Add(new(newItem, newItem.getInventoryType()));
    //            bundles.Add(newBundle);
    //        }
    //    }

    //    using var dbContext = new DBContext();
    //    using var dbTrans = dbContext.Database.BeginTransaction();
    //    ItemFactory.MERCHANT.saveItems(itemsWithType, bundles, ownerId, dbContext);

    //    FredrickProcessor.insertFredrickLog(dbContext, ownerId);
    //    dbTrans.Commit();
    //}

    private static bool check(IPlayer chr, List<PlayerShopItem> items)
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


    //public int getTimeOpen()
    //{
    //    double openTime = (DateTimeOffset.UtcNow - StartTime).TotalMinutes;
    //    openTime /= 1440;   // heuristics since engineered method to count time here is unknown
    //    openTime *= 1318;  // 这里为什么要 * 1318

    //    return (int)Math.Ceiling(openTime);
    //}

    public int GetTimeLeft()
    {
        var timeLeft = TimeSpan.FromMilliseconds(ExpirationTime - StartTime).TotalDays;
        timeLeft *= 1318;

        return (int)Math.Ceiling(timeLeft);
    }

    public void clearMessages()
    {
        lock (messages)
        {
            messages.Clear();
        }
    }

    public List<KeyValuePair<string, byte>> getMessages()
    {
        lock (messages)
        {
            return new List<KeyValuePair<string, byte>>(messages);
        }
    }

    public List<PastVisitor> getVisitorHistory()
    {
        return visitorHistory.ToList();
    }

    // 只有店主能够操作 不会并发 不需要加锁
    public void addToBlacklist(string chrName)
    {
        // 是否需要像PlayerShop.ban一样 踢掉访问者
        if (BlackList.Count >= Limits.BLACKLIST_LIMIT)
        {
            return;
        }
        BlackList.Add(chrName);
    }

    public void removeFromBlacklist(string chrName)
    {
        BlackList.Remove(chrName);
    }

    public int getMapId()
    {
        return MapModel.getId();
    }

    public override IMap getMap()
    {
        return MapModel;
    }


    public override MapObjectType getType()
    {
        return MapObjectType.HIRED_MERCHANT;
    }

    public override void sendDestroyData(IChannelClient client) { }

    public override void sendSpawnData(IChannelClient client)
    {
        client.sendPacket(PacketCreator.spawnHiredMerchantBox(this));
    }

    public override string GetName()
    {
        return Title;
    }

    public override int GetSourceId()
    {
        return SourceItemId;
    }

    Lock tradeLock = new();
    public bool TradeLock()
    {
        return tradeLock.TryEnter();
    }

    public void TradeUnlock()
    {
        tradeLock.Exit();
    }

    public void ExpiredInvoke()
    {
        Close();
        ChannelServer.PlayerShopManager.UnregisterShop(this);
    }
}

