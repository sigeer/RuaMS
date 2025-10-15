using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Resources.Messages;
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Game.Trades;

public class PlayerShop : AbstractMapObject, IPlayerShop
{

    public IPlayer Owner { get; private set; }
    private int itemid;

    private IPlayer?[] visitors = new IPlayer[3];


    private int boughtnumber = 0;
    private List<KeyValuePair<IPlayer, string>> chatLog = new();
    private Dictionary<int, byte> chatSlot = new();
    private object visitorLock = new object();

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

    public string OwnerName { get; }

    public List<SoldItem> SoldHistory { get; }

    public PlayerShopType Type { get; }

    public PlayerShop(IPlayer owner, string description, Item item)
    {
        setPosition(owner.getPosition());
        Owner = owner;
        ChannelServer = owner.getChannelServer();
        StartTime = ChannelServer.Container.getCurrentTime();
        ExpirationTime = item.getExpiration();
        this.Title = description;
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

    public bool IsOwner(IPlayer chr)
    {
        return Owner!.Equals(chr);
    }

    private void addVisitor(IPlayer visitor)
    {
        var freeSlot = GetFreeSlot();
        if (freeSlot != -1)
        {
            visitors[freeSlot] = visitor;

            broadcast(PacketCreator.getPlayerShopNewVisitor(visitor, freeSlot + 1));
            MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
        }
    }

    public void forceRemoveVisitor(IPlayer visitor)
    {
        Monitor.Enter(visitorLock);
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
            Monitor.Exit(visitorLock);
        }
    }

    public void RemoveVisitor(IPlayer visitor)
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
            Monitor.Exit(visitorLock);
        }

        MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
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

    public void takeItemBack(int slot, IPlayer chr)
    {
        lock (Commodity)
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
                        return;
                    }

                    InventoryManipulator.addFromDrop(chr.Client, iitem, true);
                }

                removeFromSlot(slot);
                chr.sendPacket(PacketCreator.getPlayerShopItemUpdate(this));
            }
        }
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

    private byte getVisitorSlot(IPlayer chr)
    {
        return (byte)(Array.FindIndex(getVisitors(), x => x?.Id == chr.Id) + 1);
    }

    public void sendMessage(IPlayer fromChar, string chat)
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

    public void SetOpen()
    {
        Status.Set(PlayerShopStatus.Opening);
    }

    public void SetMaintenance(IPlayer chr)
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
        Monitor.Enter(visitorLock);
        try
        {
            c.sendPacket(PacketCreator.getPlayerShop(this, IsOwner(c.OnlinedCharacter)));
        }
        finally
        {
            Monitor.Exit(visitorLock);
        }
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
            RemoveVisitor(target);
        }
    }

    public bool isBanned(string name)
    {
        return BlackList.Contains(name);
    }

    public bool VisitShop(IPlayer chr)
    {
        Monitor.Enter(visitorLock);
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
            Monitor.Exit(visitorLock);
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


    public bool Retrieve(IPlayer owner)
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
