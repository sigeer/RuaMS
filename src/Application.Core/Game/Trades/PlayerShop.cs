using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Resources.Messages;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Trades;

public class PlayerShop : AbstractMapObject, IPlayerShop
{

    public Player Owner { get; private set; }

    private Player?[] visitors = new Player[3];


    private int boughtnumber = 0;
    private List<KeyValuePair<Player, string>> chatLog = new();
    private Dictionary<int, byte> chatSlot = new();
    private Lock visitorLock = new ();

    public WorldChannel ChannelServer { get; }
    public long StartTime { get; }
    public long ExpirationTime { get; }
    public AtomicEnum<PlayerShopStatus> Status { get; set; }
    public HashSet<string> BlackList { get; }
    public string Title { get; private set; }
    public int Channel { get; }
    public string TypeName { get; }
    public int OwnerId { get; }
    public int Mesos { get; }
    public List<PlayerShopItem> Commodity { get; }

    public int SourceItemId { get; }
    public Item SourceItem { get; }

    public string OwnerName { get; }

    public List<SoldItem> SoldHistory { get; }

    public PlayerShopType Type { get; }

    public PlayerShop(Player owner, string description, Item item)
    {
        setPosition(owner.getPosition());
        Owner = owner;
        ChannelServer = owner.getChannelServer();
        StartTime = ChannelServer.Container.getCurrentTime();
        ExpirationTime = item.getExpiration();
        this.Title = description;
        SourceItem = item;
        SourceItemId = item.getItemId();
        OwnerId = owner.Id;
        OwnerName = owner.Name;
        Commodity = [];
        SoldHistory = [];
        BlackList = [];

        setMap(owner.getMap());
        Channel = owner.Channel;

        TypeName = "shop";
        Type = PlayerShopType.PlayerShop;
        Status = new AtomicEnum<PlayerShopStatus>(PlayerShopStatus.Maintenance);
    }

    public int GetFreeSlot()
    {
        visitorLock.Enter();
        try
        {
            return Array.IndexOf(visitors, null);
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public bool hasFreeSlot()
    {
        return GetFreeSlot() != -1;
    }

    public byte[] getShopRoomInfo()
    {
        visitorLock.Enter();
        try
        {
            var count = (byte)visitors.Count(x => x != null);
            return new byte[] { count, (byte)visitors.Length };
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public bool IsOwner(Player chr)
    {
        return Owner!.Equals(chr);
    }

    private void addVisitor(Player visitor)
    {
        var freeSlot = GetFreeSlot();
        if (freeSlot != -1)
        {
            visitors[freeSlot] = visitor;

            broadcast(PacketCreator.getPlayerShopNewVisitor(visitor, freeSlot + 1));
            MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
        }
    }

    public void forceRemoveVisitor(Player visitor)
    {
        visitorLock.Enter();
        try
        {
            for (int i = 0; i < 3; i++)
            {
                if (visitors[i] != null && visitors[i]!.getId() == visitor.getId())
                {
                    visitors[i]!.VisitingShop = null;
                    visitors[i] = null;

                    broadcast(PacketCreator.getPlayerShopRemoveVisitor(i + 1));
                    MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                    return;
                }
            }
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public void RemoveVisitor(Player visitor)
    {
        visitorLock.Enter();
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
                            Owner.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(j + 1));
                        }
                        visitors[j] = visitors[j + 1];
                    }
                    visitors[2] = null;
                    for (int j = i; j < 2; j++)
                    {
                        if (visitors[j] != null)
                        {
                            Owner.sendPacket(PacketCreator.getPlayerShopNewVisitor(visitors[j]!, j + 1));
                        }
                    }

                    broadcastRestoreToVisitors();
                    MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                    return;
                }
            }
        }
        finally
        {
            visitorLock.Exit();
        }

        MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
    }

    public bool isVisitor(Player visitor)
    {
        visitorLock.Enter();
        try
        {
            return visitors.Contains(visitor);
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public bool AddCommodity(PlayerShopItem item)
    {
        lock (Commodity)
        {
            if (Commodity.Count >= 16)
            {
                return false;
            }

            Commodity.Add(item);
            return true;
        }
    }

    private void removeFromSlot(int slot)
    {
        Commodity.RemoveAt(slot);
    }

    public Task takeItemBack(int slot, Player chr)
    {
        var shopItem = Commodity.ElementAt(slot);
        if (shopItem.isExist())
        {
            if (shopItem.getBundles() > 0)
            {
                Item iitem = shopItem.getItem().copy();
                iitem.setQuantity((short)(shopItem.getItem().getQuantity() * shopItem.getBundles()));

                if (!Inventory.checkSpot(chr, iitem))
                {
                    chr.Popup(nameof(ClientMessage.PlayerShop_TakeItemBackFail_InventoryFull));
                    chr.sendPacket(PacketCreator.enableActions());
                    return Task.CompletedTask;
                }

                InventoryManipulator.addFromDrop(chr.Client, iitem, true);
            }

            removeFromSlot(slot);
            chr.sendPacket(PacketCreator.getPlayerShopItemUpdate(this));
        }
        return Task.CompletedTask;
    }

    public void GainMeso(int meso)
    {
        Owner.GainMeso(meso, true);
    }

    public string? MesoCheck(int meso)
    {
        if (!Owner.canHoldMeso(meso))
        {
            return "Transaction failed since the shop owner can't hold any more mesos.";
        }
        return null;
    }

    public void InsertSoldHistory(int idx, SoldItem soldItem)
    {
        Owner.sendPacket(PacketCreator.getPlayerShopOwnerUpdate(soldItem, idx));

        SoldHistory.Add(soldItem);
    }

    public void OnCommoditySellout()
    {
        if (++boughtnumber == Commodity.Count)
        {
            Close();
            Owner.dropMessage(1, "Your items are sold out, and therefore your shop is closed.");
        }
    }

    public void broadcastToVisitors(Packet packet)
    {
        visitorLock.Enter();
        try
        {
            InvokeAllVisitor(x => x.sendPacket(packet));
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public void broadcastRestoreToVisitors()
    {
        visitorLock.Enter();
        try
        {
            InvokeAllVisitor((visitor, i) => visitor.sendPacket(PacketCreator.getPlayerShopRemoveVisitor(i + 1)));
            InvokeAllVisitor(visitor => visitor.sendPacket(PacketCreator.getPlayerShop(this, false)));

            recoverChatLog();
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    private void InvokeAllVisitor(Action<Player> action)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null)
            {
                action(visitors[i]!);
            }
        }
    }

    private void InvokeAllVisitor(Action<Player, int> action)
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
        List<Player> visitorList = new(3);

        visitorLock.Enter();
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
            visitorLock.Exit();
        }

        foreach (Player mc in visitorList)
        {
            forceRemoveVisitor(mc);
        }
    }

    public void BroadcastShopItemUpdate()
    {
        var client = Owner.getClient();
        if (client != null)
        {
            client.sendPacket(PacketCreator.getPlayerShopItemUpdate(this));
        }
        broadcastToVisitors(PacketCreator.getPlayerShopItemUpdate(this));
    }

    public void broadcast(Packet packet)
    {
        var client = Owner.getClient();
        if (client != null)
        {
            client.sendPacket(packet);
        }
        broadcastToVisitors(packet);
    }

    private byte getVisitorSlot(Player chr)
    {
        return (byte)(Array.FindIndex(getVisitors(), x => x?.Id == chr.Id) + 1);
    }

    public void sendMessage(Player fromChar, string chat)
    {
        byte s = getVisitorSlot(fromChar);

        lock (chatLog)
        {
            chatLog.Add(new(fromChar, chat));
            if (chatLog.Count > 25)
            {
                chatLog.RemoveAt(0);
            }
            chatSlot.AddOrUpdate(fromChar.getId(), s);
        }

        broadcast(PacketCreator.getPlayerShopChat(fromChar, chat, s));
    }

    private void recoverChatLog()
    {
        lock (chatLog)
        {
            foreach (var it in chatLog)
            {
                Player chr = it.Key;
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

    public void SetOpen()
    {
        Status.Set(PlayerShopStatus.Opening);
    }

    public void SetMaintenance(Player chr)
    {
        Status.Set(PlayerShopStatus.Maintenance);
    }
    public void Close()
    {
        clearChatLog();

        MapModel.removeMapObject(this);
        MapModel.broadcastMessage(PacketCreator.removePlayerShopBox(OwnerId));

        removeVisitors();

        Retrieve(Owner);
        Owner.VisitingShop = null;
    }


    public void closeShop()
    {
        clearChatLog();
        removeVisitors();
        MapModel.broadcastMessage(PacketCreator.removePlayerShopBox(OwnerId));
    }

    public void sendShop(IChannelClient c)
    {
        visitorLock.Enter();
        try
        {
            c.sendPacket(PacketCreator.getPlayerShop(this, IsOwner(c.OnlinedCharacter)));
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public Player?[] getVisitors()
    {
        visitorLock.Enter();
        try
        {
            return visitors.ToArray();
        }
        finally
        {
            visitorLock.Exit();
        }
    }

    public List<PlayerShopItem> getItems()
    {
        lock (Commodity)
        {
            return new(Commodity);
        }
    }

    public bool hasItem(int itemid)
    {
        return getItems().Any(x => x.getItem().getItemId() == itemid && x.isExist() && x.getBundles() > 0);
    }

    public string getDescription()
    {
        return Title;
    }

    public void setDescription(string description)
    {
        this.Title = description;
    }

    public void banPlayer(string name)
    {
        if (!BlackList.Contains(name))
        {
            BlackList.Add(name);
        }

        Player? target = null;
        visitorLock.Enter();
        try
        {
            target = visitors.FirstOrDefault(x => x?.Name == name);
        }
        finally
        {
            visitorLock.Exit();
        }

        if (target != null)
        {
            target.sendPacket(PacketCreator.shopErrorMessage(5, 1));
            RemoveVisitor(target);
        }
    }

    public bool isBanned(string name)
    {
        return BlackList.Contains(name);
    }

    public bool VisitShop(Player chr)
    {
        visitorLock.Enter();
        try
        {
            if (BlackList.Contains(chr.getName()))
            {
                chr.dropMessage(1, "You have been banned from this store.");
                return false;
            }

            if (Status.Is(PlayerShopStatus.Opening))
            {
                chr.dropMessage(1, "This store is not yet open.");
                return false;
            }

            if (hasFreeSlot() && !isVisitor(chr))
            {
                addVisitor(chr);
                chr.VisitingShop = this;
                sendShop(chr.getClient());

                return true;
            }

            return false;
        }
        finally
        {
            visitorLock.Exit();
        }

    }

    public List<PlayerShopItem> QueryAvailableBundles(int itemid)
    {
        lock (Commodity)
        {
            return Commodity.Where(x => x.getItem().getItemId() == itemid && x.getBundles() > 0 && x.isExist()).ToList();
        }
    }

    public override void sendDestroyData(IChannelClient client)
    {
        client.sendPacket(PacketCreator.removePlayerShopBox(OwnerId));
    }

    public override void sendSpawnData(IChannelClient client)
    {
        client.sendPacket(PacketCreator.updatePlayerShopBox(this));
    }

    public override MapObjectType getType()
    {
        return MapObjectType.PLAYER_SHOP;
    }

    public override string GetName()
    {
        return OwnerName;
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


    public bool Retrieve(Player owner)
    {
        if (owner.Id != this.OwnerId)
            return false;

        lock (Commodity)
        {
            foreach (var mpsi in Commodity)
            {
                if (mpsi.getBundles() >= 2)
                {
                    var iItem = mpsi.getItem().copy();
                    iItem.setQuantity((short)(mpsi.getBundles() * iItem.getQuantity()));
                    InventoryManipulator.addFromDrop(Owner.Client, iItem, false);
                }
                else if (mpsi.isExist())
                {
                    InventoryManipulator.addFromDrop(Owner.Client, mpsi.getItem(), true);
                }
            }
        }


        return true;
    }
}
