using Application.Core.Channel;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Maps;
using Application.Resources.Messages;
using Application.Templates.Item.Cash;
using Application.Utility.Performance;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Trades;

public class HiredMerchant : AbstractMapObject, IPlayerShop
{
    public Player? Owner { get; private set; }
    public int Mesos { get; private set; }
    public long StartTime { get; }
    public long ExpirationTime { get; }
    public TimeSpan LeftTime { get; }


    private List<KeyValuePair<string, byte>> messages = new();
    private AtomicBoolean open = new AtomicBoolean();

    private Visitor?[] visitors = new Visitor[3];
    FixedSizeQueue<PastVisitor> visitorHistory = new(Limits.VISITOR_HISTORY_LIMIT);

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

    public HiredMerchant(Player owner, string desc, Item item) : base(owner.MapModel, owner.getPosition())
    {
        StartTime = owner.Client.CurrentServer.Node.getCurrentTime();
        ExpirationTime = item.getExpiration();
        Owner = owner;
        OwnerName = owner.Name;
        OwnerId = owner.getId();
        Channel = owner.getClient().getChannel();
        ChannelServer = owner.getChannelServer();
        SourceItemId = item.getItemId();
        Title = desc;
        Channel = owner.Channel;
        Commodity = [];
        SoldHistory = [];
        BlackList = [];

        Type = PlayerShopType.HiredMerchant;
        SoldNotify = (ItemInformationProvider.getInstance().GetTemplate(item.getItemId()) as HiredMerchantItemTemplate)!.NotifyWhenSold;
        Status = new AtomicEnum<PlayerShopStatus>(PlayerShopStatus.Maintenance);
    }

    public async Task broadcastToVisitorsThreadsafe(Packet packet)
    {
        await broadcastToVisitors(packet);
    }

    private async Task broadcastToVisitors(Packet packet)
    {
        foreach (var visitor in visitors)
        {
            if (visitor != null)
            {
                await visitor.chr.SendPacket(packet);
            }
        }
    }

    public async Task BroadcastShopItemUpdate()
    {
        foreach (var visitor in visitors)
        {
            if (visitor != null)
            {
                await visitor.chr.SendPacket(PacketCreator.updateHiredMerchant(this, visitor.chr));
            }
        }
    }

    public byte[] getShopRoomInfo()
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

    public async Task<bool> AddVisitor(Player visitor)
    {
        int i = getFreeSlot();
        if (i > -1)
        {
            visitors[i] = new Visitor(visitor, ChannelServer.Node.GetCurrentTimeDateTimeOffset());
            await broadcastToVisitors(PacketCreator.hiredMerchantVisitorAdd(visitor, i + 1));
            await MapModel.broadcastMessage(PacketCreator.updateHiredMerchantBox(this));

            return true;
        }

        return false;
    }

    public void OwnerLeave(Player chr)
    {
        if (Owner == null)
            return;

        if (chr.Id != OwnerId)
            return;

        SetOpen();
    }

    public async Task RemoveVisitor(Player chr)
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
            await broadcastToVisitors(PacketCreator.hiredMerchantVisitorLeave(slot + 1));
            await getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));
        }
    }

    private void addVisitorToHistory(Visitor visitor)
    {
        // 只需要时间差，不需要使用系统时间
        TimeSpan visitDuration = ChannelServer.Node.GetCurrentTimeDateTimeOffset() - visitor.enteredAt;
        visitorHistory.Enqueue(new PastVisitor(visitor.chr.getName(), visitDuration));
    }

    public int getVisitorSlotThreadsafe(Player visitor)
    {
        return getVisitorSlot(visitor);
    }

    private int getVisitorSlot(Player visitor)
    {
        return Array.FindIndex(getVisitorCharacters(), x => x?.Id == visitor.Id);
    }

    private async Task removeAllVisitors()
    {
        for (int i = 0; i < 3; i++)
        {
            var visitor = visitors[i];
            if (visitor != null)
            {
                Player visitorChr = visitor.chr;
                visitorChr.VisitingShop = null;
                await visitorChr.SendPacket(PacketCreator.leaveHiredMerchant(i + 1, 0x11));
                await visitorChr.SendPacket(PacketCreator.hiredMerchantMaintenanceMessage());
                visitors[i] = null;
                addVisitorToHistory(visitor);
            }
        }

        await getMap().broadcastMessage(PacketCreator.updateHiredMerchantBox(this));
    }

    public async Task ProcessVisitingOwner()
    {
        if (Owner != null)
        {
            await Owner.SendPacket(PacketCreator.hiredMerchantOwnerLeave());
            await Owner.SendPacket(PacketCreator.leaveHiredMerchant(0x00, 0x03));
            Owner.VisitingShop = null;
            Owner = null;
        }
    }

    public async Task withdrawMesos(Player chr)
    {
        if (IsOwner(chr))
        {
            Mesos = await chr.GainMeso(Mesos);
        }
    }

    public async Task takeItemBack(int slot, Player chr)
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
                    await chr.Popup(nameof(ClientMessage.PlayerShop_TakeItemBackFail_InventoryFull));
                    await chr.SendPacket(PacketCreator.enableActions());
                    return;
                }

                await InventoryManipulator.addFromDrop(chr.Client, iitem, true);
            }

            removeFromSlot(slot);
            await chr.SendPacket(PacketCreator.updateHiredMerchant(this, chr));
        }

        if (YamlConfig.config.server.USE_ENFORCE_MERCHANT_SAVE)
        {
            await chr.SyncCharAsync();
        }
    }

    private static async Task<bool> canBuy(IChannelClient c, Item newItem)
    {
        // thanks xiaokelvin (Conrad) for noticing a leaked test code here
        return InventoryManipulator.checkSpace(c, newItem.getItemId(), newItem.getQuantity(), newItem.getOwner()) && await InventoryManipulator.addFromDrop(c, newItem, false);
    }

    public Task GainMeso(int meso)
    {
        using var activity = GameMetrics.ActivitySource.StartActivity("HiredMerchantGainMeso");
        activity?.SetTag("PlayerId", OwnerId);
        activity?.SetTag("Player", OwnerName);
        activity?.SetTag("Meso", meso);

        Mesos += meso;
        return Task.CompletedTask;
    }

    public string? MesoCheck(int meso)
    {
        if ((long)Mesos + meso > int.MaxValue)
        {
            return "Transaction failed since the shop owner can't hold any more mesos.";
        }
        return null;
    }

    public Task InsertSoldHistory(int idx, SoldItem soldItem)
    {
        SoldHistory.Add(soldItem);
        return Task.CompletedTask;
    }

    public Task OnCommoditySellout()
    {
        return Task.CompletedTask;
    }

    private int getQuantityLeft(int itemid)
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

    public async Task Close()
    {
        await MapModel.RemoveMapObject(this, chr => sendDestroyData(chr.Client));

        await ProcessVisitingOwner();

        await removeAllVisitors();
    }


    public void SetOpen()
    {
        if (Owner == null)
            return;

        Status.Set(PlayerShopStatus.Opening);
        Owner.VisitingShop = null;
        Owner = null;
    }

    public async Task SetMaintenance(Player chr)
    {
        if (chr.Id == OwnerId)
        {
            Status.Set(PlayerShopStatus.Maintenance);
            Owner = chr;

            await removeAllVisitors();
        }
    }

    public async Task<bool> Retrieve(Player owner)
    {
        if (owner.Id != this.OwnerId)
            return false;

        if (check(owner, Commodity))
        {
            foreach (PlayerShopItem mpsi in Commodity)
            {
                if (mpsi.isExist())
                {
                    Item iItem = mpsi.getItem().copy();
                    iItem.setQuantity((short)(mpsi.getBundles() * iItem.getQuantity()));

                    await InventoryManipulator.addFromDrop(owner.getClient(), mpsi.getItem(), true);
                }
            }

            Commodity.Clear();
            return true;
        }
        return false;

    }

    public async Task<bool> VisitShop(Player chr)
    {
        if (chr.VisitingShop != null)
        {
            await chr.SendPacket(PacketCreator.getMiniRoomError(17));
            return false;
        }
        if (IsOwner(chr))
        {
            await SetMaintenance(chr);
        }
        else if (!Status.Is(PlayerShopStatus.Opening))
        {
            await chr.SendPacket(PacketCreator.getMiniRoomError(18));
            return false;
        }
        else if (BlackList.Contains(chr.getName()))
        {
            await chr.SendPacket(PacketCreator.getMiniRoomError(17));
            return false;
        }
        else if (!await AddVisitor(chr))
        {
            await chr.SendPacket(PacketCreator.getMiniRoomError(2));
            return false;
        }

        await chr.SendPacket(PacketCreator.getHiredMerchant(chr, this, false));
        chr.VisitingShop = this;
        return true;
    }


    public Player?[] getVisitorCharacters()
    {
        return visitors.Select(x => x?.chr).ToArray();
    }

    public List<PlayerShopItem> getItems()
    {
        return Commodity;
    }

    public bool hasItem(int itemid)
    {
        return getItems().Any(x => x.getItem().getItemId() == itemid && x.isExist() && x.getBundles() > 0);
    }

    public bool AddCommodity(PlayerShopItem item)
    {
        if (Commodity.Count >= Limits.MaxPlayerShopItemCount)
        {
            return false;
        }

        Commodity.Add(item);
        return true;
    }
    /// <summary>
    /// 整理道具
    /// </summary>
    /// <param name="chr"></param>
    public async Task Restore(Player chr)
    {
        if (IsOwner(chr))
        {
            Commodity.RemoveAll(x => !x.isExist());

            Mesos = await chr.GainMeso(Mesos);
        }
    }

    public void clearInexistentItems()
    {
        Commodity.RemoveAll(x => !x.isExist());
    }

    private void removeFromSlot(int slot)
    {
        Commodity.RemoveAt(slot);
    }

    private int getFreeSlot()
    {
        return Array.IndexOf(visitors, null);
    }


    public bool IsOwner(Player chr)
    {
        return chr.Id == OwnerId;
    }

    public async Task sendMessage(Player chr, string msg)
    {
        string message = chr.Name + " : " + msg;
        byte slot = (byte)(getVisitorSlot(chr) + 1);

        messages.Add(new(message, slot));
        await broadcastToVisitorsThreadsafe(PacketCreator.hiredMerchantChat(message, slot));
        if (Owner != null)
        {
            await Owner.SendPacket(PacketCreator.hiredMerchantChat(message, slot));
        }
    }

    public List<PlayerShopItem> QueryAvailableBundles(int itemid)
    {
        if (!open)
        {
            return [];
        }

        return Commodity.Where(x => x.getItem().getItemId() == itemid && x.getBundles() > 0 && x.isExist()).ToList();
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

    private static bool check(Player chr, List<PlayerShopItem> items)
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
        messages.Clear();
    }

    public List<KeyValuePair<string, byte>> getMessages()
    {
        return new List<KeyValuePair<string, byte>>(messages);
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

    public override async Task sendDestroyData(IChannelClient client)
    {
        await client.SendPacket(PacketCreator.removeHiredMerchantBox(getObjectId()));
    }

    public override async Task sendSpawnData(IChannelClient client)
    {
        await client.SendPacket(PacketCreator.spawnHiredMerchantBox(this));
    }

    public Packet MakeSpawnPacket() => PacketCreator.spawnHiredMerchantBox(this);

    public override string GetName()
    {
        return Title;
    }

    public override int GetSourceId()
    {
        return SourceItemId;
    }

    public async Task ExpiredInvoke()
    {
        await Close();
        ChannelServer.PlayerShopManager.UnregisterShop(this);
    }
}

