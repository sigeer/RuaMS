using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Resources.Messages;
using Application.Utility.Performance;
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

    public PlayerShop(Player owner, string description, Item item) : base(owner.MapModel, owner.getPosition())
    {
        Owner = owner;
        ChannelServer = owner.getChannelServer();
        StartTime = ChannelServer.Node.getCurrentTime();
        ExpirationTime = item.getExpiration();
        this.Title = description;
        SourceItem = item;
        SourceItemId = item.getItemId();
        OwnerId = owner.Id;
        OwnerName = owner.Name;
        Commodity = [];
        SoldHistory = [];
        BlackList = [];
        Channel = owner.Channel;

        TypeName = "shop";
        Type = PlayerShopType.PlayerShop;
        Status = new AtomicEnum<PlayerShopStatus>(PlayerShopStatus.Maintenance);
    }

    public int GetFreeSlot()
    {
        return Array.IndexOf(visitors, null);
    }

    public bool hasFreeSlot()
    {
        return GetFreeSlot() != -1;
    }

    public byte[] getShopRoomInfo()
    {
        var count = (byte)visitors.Count(x => x != null);
        return new byte[] { count, (byte)visitors.Length };
    }

    public bool IsOwner(Player chr)
    {
        return Owner!.Equals(chr);
    }

    private async Task addVisitor(Player visitor)
    {
        var freeSlot = GetFreeSlot();
        if (freeSlot != -1)
        {
            visitors[freeSlot] = visitor;

            await broadcast(PacketCreator.getPlayerShopNewVisitor(visitor, freeSlot + 1));
            await MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
        }
    }

    public async Task forceRemoveVisitor(Player visitor)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null && visitors[i]!.getId() == visitor.getId())
            {
                visitors[i]!.VisitingShop = null;
                visitors[i] = null;

                await broadcast(PacketCreator.getPlayerShopRemoveVisitor(i + 1));
                await MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                return;
            }
        }
    }

    public async Task RemoveVisitor(Player visitor)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null && visitors[i]!.getId() == visitor.getId())
            {
                for (int j = i; j < 2; j++)
                {
                    if (visitors[j] != null)
                    {
                        await Owner.SendPacket(PacketCreator.getPlayerShopRemoveVisitor(j + 1));
                    }
                    visitors[j] = visitors[j + 1];
                }
                visitors[2] = null;
                for (int j = i; j < 2; j++)
                {
                    if (visitors[j] != null)
                    {
                        await Owner.SendPacket(PacketCreator.getPlayerShopNewVisitor(visitors[j]!, j + 1));
                    }
                }

                await broadcastRestoreToVisitors();
                await MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
                return;
            }
        }
        await MapModel.broadcastMessage(PacketCreator.updatePlayerShopBox(this));
    }

    public bool isVisitor(Player visitor)
    {
        return visitors.Contains(visitor);
    }

    public bool AddCommodity(PlayerShopItem item)
    {
        if (Commodity.Count >= 16)
        {
            return false;
        }

        Commodity.Add(item);
        return true;
    }

    private void removeFromSlot(int slot)
    {
        Commodity.RemoveAt(slot);
    }

    public async Task takeItemBack(int slot, Player chr)
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
                    await chr.Popup(nameof(ClientMessage.PlayerShop_TakeItemBackFail_InventoryFull));
                    await chr.SendPacket(PacketCreator.enableActions());
                    return;
                }

                await InventoryManipulator.addFromDrop(chr.Client, iitem, true);
            }

            removeFromSlot(slot);
            await chr.SendPacket(PacketCreator.getPlayerShopItemUpdate(this));
        }
    }

    public async Task GainMeso(int meso)
    {
        using var activity = GameMetrics.ActivitySource.StartActivity("PlayerShopGainMeso");
        activity?.SetTag("PlayerId", Owner.Id);
        activity?.SetTag("Player", Owner.Name);
        activity?.SetTag("Meso", meso);

        await Owner.GainMeso(meso, GainItemShow.ShowInMessage);
    }

    public string? MesoCheck(int meso)
    {
        if (!Owner.canHoldMeso(meso))
        {
            return "Transaction failed since the shop owner can't hold any more mesos.";
        }
        return null;
    }

    public async Task InsertSoldHistory(int idx, SoldItem soldItem)
    {
        await Owner.SendPacket(PacketCreator.getPlayerShopOwnerUpdate(soldItem, idx));

        SoldHistory.Add(soldItem);
    }

    public async Task OnCommoditySellout()
    {
        if (++boughtnumber == Commodity.Count)
        {
            await Close();
            await Owner.dropMessage(1, "Your items are sold out, and therefore your shop is closed.");
        }
    }

    public async Task broadcastToVisitors(Packet packet)
    {
        await InvokeAllVisitor(x => x.SendPacket(packet));
    }

    public async Task broadcastRestoreToVisitors()
    {
        await InvokeAllVisitor((visitor, i) => visitor.SendPacket(PacketCreator.getPlayerShopRemoveVisitor(i + 1)));
        await InvokeAllVisitor(visitor => visitor.SendPacket(PacketCreator.getPlayerShop(this, false)));

        await recoverChatLog();
    }

    private async Task InvokeAllVisitor(Func<Player, Task> action)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null)
            {
                await action(visitors[i]!);
            }
        }
    }

    private async Task InvokeAllVisitor(Func<Player, int, Task> action)
    {
        for (int i = 0; i < 3; i++)
        {
            if (visitors[i] != null)
            {
                await action(visitors[i]!, i);
            }
        }
    }

    public async Task removeVisitors()
    {
        List<Player> visitorList = new(3);

        try
        {
            await InvokeAllVisitor(async visitor =>
            {
                await visitor.SendPacket(PacketCreator.shopErrorMessage(10, 1));
                visitorList.Add(visitor);
            });
        }
        catch (Exception e)
        {
            Log.Logger.Error(e.ToString());
        }

        foreach (Player mc in visitorList)
        {
            await forceRemoveVisitor(mc);
        }
    }

    public async Task BroadcastShopItemUpdate()
    {
        var client = Owner.getClient();
        if (client != null)
        {
            await client.SendPacket(PacketCreator.getPlayerShopItemUpdate(this));
        }
        await broadcastToVisitors(PacketCreator.getPlayerShopItemUpdate(this));
    }

    public async Task broadcast(Packet packet)
    {
        var client = Owner.getClient();
        if (client != null)
        {
            await client.SendPacket(packet);
        }
        await broadcastToVisitors(packet);
    }

    private byte getVisitorSlot(Player chr)
    {
        return (byte)(Array.FindIndex(getVisitors(), x => x?.Id == chr.Id) + 1);
    }

    public async Task sendMessage(Player fromChar, string chat)
    {
        byte s = getVisitorSlot(fromChar);

        chatLog.Add(new(fromChar, chat));
        if (chatLog.Count > 25)
        {
            chatLog.RemoveAt(0);
        }
        chatSlot.AddOrUpdate(fromChar.getId(), s);

        await broadcast(PacketCreator.getPlayerShopChat(fromChar, chat, s));
    }

    private async Task recoverChatLog()
    {
        foreach (var it in chatLog)
        {
            Player chr = it.Key;
            var pos = chatSlot.GetValueOrDefault(chr.getId());

            await broadcastToVisitors(PacketCreator.getPlayerShopChat(chr, it.Value, pos));
        }
    }

    private void clearChatLog()
    {

        chatLog.Clear();
    }

    public void SetOpen()
    {
        Status.Set(PlayerShopStatus.Opening);
    }

    public async Task SetMaintenance(Player chr)
    {
        Status.Set(PlayerShopStatus.Maintenance);

        await removeVisitors();
    }
    public async Task Close()
    {
        clearChatLog();

        await MapModel.RemoveMapObject(this, chr => sendDestroyData(chr.Client));

        await removeVisitors();

        await Retrieve(Owner);
        Owner.VisitingShop = null;
    }


    public async Task sendShop(IChannelClient c)
    {
        await c.SendPacket(PacketCreator.getPlayerShop(this, IsOwner(c.OnlinedCharacter)));
    }

    public Player?[] getVisitors()
    {
        return visitors.ToArray();
    }

    public List<PlayerShopItem> getItems()
    {
        return new(Commodity);
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

    public async Task banPlayer(string name)
    {
        if (!BlackList.Contains(name))
        {
            BlackList.Add(name);
        }

        Player? target = null;

        target = visitors.FirstOrDefault(x => x?.Name == name);

        if (target != null)
        {
            await target.SendPacket(PacketCreator.shopErrorMessage(5, 1));
            await RemoveVisitor(target);
        }
    }

    public bool isBanned(string name)
    {
        return BlackList.Contains(name);
    }

    public async Task<bool> VisitShop(Player chr)
    {
        if (BlackList.Contains(chr.getName()))
        {
            await chr.dropMessage(1, "You have been banned from this store.");
            return false;
        }

        if (Status.Is(PlayerShopStatus.Opening))
        {
            await chr.dropMessage(1, "This store is not yet open.");
            return false;
        }

        if (hasFreeSlot() && !isVisitor(chr))
        {
            await addVisitor(chr);
            chr.VisitingShop = this;
            await sendShop(chr.getClient());

            return true;
        }

        return false;
    }

    public List<PlayerShopItem> QueryAvailableBundles(int itemid)
    {
        return Commodity.Where(x => x.getItem().getItemId() == itemid && x.getBundles() > 0 && x.isExist()).ToList();
    }

    public override async Task sendDestroyData(IChannelClient client)
    {
        await client.SendPacket(PacketCreator.removePlayerShopBox(OwnerId));
    }

    public override async Task sendSpawnData(IChannelClient client)
    {
        await client.SendPacket(PacketCreator.updatePlayerShopBox(this));
    }

    public Packet MakeSpawnPacket() => PacketCreator.updatePlayerShopBox(this);

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


    public async Task ExpiredInvoke()
    {
        await Close();
        ChannelServer.PlayerShopManager.UnregisterShop(this);
    }


    public async Task<bool> Retrieve(Player owner)
    {
        if (owner.Id != this.OwnerId)
            return false;

        foreach (var mpsi in Commodity)
        {
            if (mpsi.isExist())
            {
                Item iItem = mpsi.getItem().copy();
                iItem.setQuantity((short)(mpsi.getBundles() * iItem.getQuantity()));

                await InventoryManipulator.addFromDrop(owner.getClient(), mpsi.getItem(), true);
            }
        }
        return true;
    }
}
