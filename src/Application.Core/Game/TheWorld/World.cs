using Application.Core.Channel;
using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Invites;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.WorldEvents;
using Application.Core.Managers;
using client;
using Microsoft.EntityFrameworkCore;
using net.server;
using net.server.channel;
using net.server.coordinator.matchchecker;
using net.server.task;
using server;
using tools;
using static Application.Core.Game.Relation.BuddyList;

namespace Application.Core.Game.TheWorld;

public class World
{
    private ILogger log;
    public int Id { get; set; }
    public string Name { get; set; }

    public List<WorldChannel> Channels { get; }
    WorldPlayerStorage? _players;
    public WorldPlayerStorage Players => _players ?? (_players = new WorldPlayerStorage(Id));
    public Dictionary<int, Team> TeamStorage { get; }

    private Dictionary<int, byte> pnpcStep = new();
    private Dictionary<int, short> pnpcPodium = new();

    private Dictionary<int, Family> families = new();
    public FishingWorldInstance FishingInstance { get; }

    private MatchCheckerCoordinator matchChecker = new MatchCheckerCoordinator();

    private ScheduledFuture? marriagesSchedule;
    private ScheduledFuture? fishingSchedule;
    private ScheduledFuture? partySearchSchedule;
    private ScheduledFuture? timeoutSchedule;

    public World(WorldConfigEntity config)
    {
        log = LogFactory.GetLogger("World_" + Id);
        Channels = new List<WorldChannel>();

        this.Id = config.Id;
        Name = config.Name;

        var tman = TimerManager.getInstance();

        marriagesSchedule = tman.register(new WeddingReservationTask(this),
            TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL),
            TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL));
        fishingSchedule = tman.register(new FishingTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
#if !DEBUG
        timeoutSchedule = tman.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
#endif

        FishingInstance = new FishingWorldInstance(this);

        if (YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            var timeLeft = TimeUtils.GetTimeLeftForNextDay();
            FamilyManager.resetEntitlementUsage(this);
            tman.register(new FamilyDailyResetTask(this), TimeSpan.FromDays(1), timeLeft);
        }
    }

    public List<WorldChannel> getChannels()
    {
        return new(Channels);
    }

    public WorldChannel getChannel(int channel)
    {
        return Channels.ElementAtOrDefault(channel - 1) ?? throw new BusinessFatalException($"Channel {channel} not existed");
    }

    public int addChannel(WorldChannel channel)
    {
        Channels.Add(channel);
        Players.RelateChannel(channel.getId(), channel.Players);
        return Channels.Count;
    }

    public async Task<int> removeChannel()
    {
        var chIdx = Channels.Count - 1;
        if (chIdx < 0)
        {
            return -1;
        }

        var ch = Channels.ElementAtOrDefault(chIdx);
        if (ch == null || !ch.canUninstall())
        {
            return -1;
        }

        await ch.Shutdown();
        Channels.RemoveAt(chIdx);

        return ch.getId();
    }


    public bool canUninstall()
    {
        if (Players.Count() > 0) return false;

        return this.getChannels().All(x => x.canUninstall());
    }



    public List<IPlayer> loadAndGetAllCharactersView()
    {
        return Server.getInstance().loadAllAccountsCharactersView().Where(x => x.World == Id).ToList();
    }



    public WorldPlayerStorage getPlayerStorage()
    {
        return Players;
    }

    public MatchCheckerCoordinator getMatchCheckerCoordinator()
    {
        return matchChecker;
    }


    public void removePlayer(IPlayer chr)
    {
        var cserv = chr.getChannelServer();

        if (cserv != null)
        {
            if (!cserv.removePlayer(chr))
            {
                // oy the player is not where they should be, find this mf

                foreach (var ch in Channels)
                {
                    if (ch.removePlayer(chr))
                    {
                        break;
                    }
                }
            }
        }
    }

    public int getId()
    {
        return Id;
    }

    public void addFamily(int id, Family f)
    {
        lock (families)
        {
            families.TryAdd(id, f);
        }
    }

    public void removeFamily(int id)
    {
        lock (families)
        {
            families.Remove(id);
        }
    }

    public Family? getFamily(int id)
    {
        lock (families)
        {
            return families.GetValueOrDefault(id);
        }
    }

    public ICollection<Family> getFamilies()
    {
        lock (families)
        {
            return families.Values.ToList();
        }
    }


    public int find(string name)
    {
        var chr = getPlayerStorage().getCharacterByName(name);
        return chr?.Channel ?? -1;
    }

    public int find(int id)
    {
        var chr = getPlayerStorage().getCharacterById(id);
        return chr?.Channel ?? -1;
    }

    public void buddyChat(int[] recipientCharacterIds, int cidFrom, string nameFrom, string chattext)
    {
        var playerStorage = getPlayerStorage();
        foreach (int characterId in recipientCharacterIds)
        {
            var chr = playerStorage.getCharacterById(characterId);
            if (chr != null && chr.IsOnlined)
            {
                if (chr.BuddyList.containsVisible(cidFrom))
                {
                    chr.sendPacket(PacketCreator.multiChat(nameFrom, chattext, 0));
                }
            }
        }
    }

    public CharacterIdChannelPair[] multiBuddyFind(int charIdFrom, int[] characterIds)
    {
        List<CharacterIdChannelPair> foundsChars = new(characterIds.Length);
        foreach (var ch in getChannels())
        {
            foreach (int charid in ch.multiBuddyFind(charIdFrom, characterIds))
            {
                foundsChars.Add(new CharacterIdChannelPair(charid, ch.getId()));
            }
        }
        return foundsChars.ToArray();
    }

    #region Messenger


    public void messengerInvite(string sender, int messengerid, string target, int fromchannel)
    {
        if (isConnected(target))
        {
            var targetChr = getPlayerStorage().getCharacterByName(target);
            if (targetChr != null && targetChr.IsOnlined)
            {
                if (targetChr.ChatRoomId == 0)
                {
                    var from = getChannel(fromchannel).getPlayerStorage().getCharacterByName(sender);
                    if (from != null)
                    {
                        if (InviteType.MESSENGER.CreateInvite(new ChatInviteRequest(from, targetChr, messengerid)))
                        {
                            targetChr.sendPacket(PacketCreator.messengerInvite(sender, messengerid));
                            from.sendPacket(PacketCreator.messengerNote(target, 4, 1));
                        }
                        else
                        {
                            from.sendPacket(PacketCreator.messengerChat(sender + " : " + target + " is already managing a Maple Messenger invitation"));
                        }
                    }
                }
                else
                {
                    var from = getChannel(fromchannel).getPlayerStorage().getCharacterByName(sender);
                    from?.sendPacket(PacketCreator.messengerChat(sender + " : " + target + " is already using Maple Messenger"));
                }
            }
        }
    }

    public void declineChat(string sender, IPlayer player)
    {
        if (isConnected(sender))
        {
            var senderChr = getPlayerStorage().getCharacterByName(sender);
            if (senderChr != null && senderChr.IsOnlined && senderChr.ChatRoomId > 0)
            {
                if (InviteType.MESSENGER.AnswerInvite(player.getId(), senderChr.ChatRoomId, false).Result == InviteResultType.DENIED)
                {
                    senderChr.sendPacket(PacketCreator.messengerNote(player.getName(), 5, 0));
                }
            }
        }
    }
    #endregion

    public bool isConnected(string charName)
    {
        return getPlayerStorage().getCharacterByName(charName)?.IsOnlined == true;
    }

    public BuddyAddResult requestBuddyAdd(string addName, int channelFrom, int cidFrom, string nameFrom)
    {
        var addChar = getPlayerStorage().getCharacterByName(addName);
        if (addChar != null && addChar.IsOnlined)
        {
            BuddyList buddylist = addChar.getBuddylist();
            if (buddylist.isFull())
            {
                return BuddyAddResult.BUDDYLIST_FULL;
            }
            if (!buddylist.contains(cidFrom))
            {
                buddylist.addBuddyRequest(addChar.Client, cidFrom, nameFrom, channelFrom);
            }
            else if (buddylist.containsVisible(cidFrom))
            {
                return BuddyAddResult.ALREADY_ON_LIST;
            }
        }
        return BuddyAddResult.OK;
    }

    public void buddyChanged(int cid, int cidFrom, string name, int channel, BuddyOperation operation)
    {
        var addChar = getPlayerStorage().getCharacterById(cid);
        if (addChar != null && addChar.IsOnlined)
        {
            var buddylist = addChar.BuddyList;
            switch (operation)
            {
                case BuddyOperation.ADDED:
                    if (!buddylist.contains(cidFrom))
                    {
                        buddylist.put("Default Group", cidFrom);
                        addChar.sendPacket(PacketCreator.updateBuddyChannel(cidFrom, (channel - 1)));
                    }
                    break;
                case BuddyOperation.DELETED:
                    if (buddylist.contains(cidFrom))
                    {
                        addChar.deleteBuddy(cidFrom);
                    }
                    break;
            }
        }
    }

    public void loggedOff(string name, int characterId, int channel, int[] buddies)
    {
        updateBuddies(characterId, channel, buddies, true);
    }

    public void loggedOn(string name, int characterId, int channel, int[] buddies)
    {
        updateBuddies(characterId, channel, buddies, false);
    }

    private void updateBuddies(int characterId, int channel, int[] buddies, bool offline)
    {
        var playerStorage = getPlayerStorage();
        foreach (int buddy in buddies)
        {
            var chr = playerStorage.getCharacterById(buddy);
            if (chr != null && chr.IsOnlined)
            {
                var ble = chr.getBuddylist().get(characterId);
                if (ble != null && ble.Visible)
                {
                    int mcChannel;
                    if (offline)
                    {
                        mcChannel = -1;
                    }
                    else
                    {
                        mcChannel = (byte)(channel - 1);
                    }
                    chr.getBuddylist().put(ble);
                    chr.sendPacket(PacketCreator.updateBuddyChannel(ble.getCharacterId(), mcChannel));
                }
            }
        }
    }

    public void setPlayerNpcMapStep(int mapid, int step)
    {
        setPlayerNpcMapData(mapid, step, -1, false);
    }

    public void setPlayerNpcMapPodiumData(int mapid, int podium)
    {
        setPlayerNpcMapData(mapid, -1, podium, false);
    }

    public void setPlayerNpcMapData(int mapid, int step, int podium)
    {
        setPlayerNpcMapData(mapid, step, podium, true);
    }

    private static void executePlayerNpcMapDataUpdate<T>(DBContext dbContext, bool isPodium, Dictionary<int, T> pnpcData, short value, int worldid, int mapid)
    {
        if (pnpcData.ContainsKey(mapid))
        {
            dbContext.PlayernpcsFields.Where(x => x.World == worldid && x.Map == mapid)
                .ExecuteUpdate(x => isPodium ? x.SetProperty(y => y.Podium, value) : x.SetProperty(y => y.Step, value));
        }
        else
        {
            var model = new PlayernpcsField
            {
                Map = mapid,
                World = worldid,
            };
            if (isPodium)
                model.Podium = value;
            else
                model.Step = (sbyte)value;
            dbContext.PlayernpcsFields.Add(model);
            dbContext.SaveChanges();
        }
    }

    private void setPlayerNpcMapData(int mapid, int step, int podium, bool silent)
    {
        if (!silent)
        {
            try
            {
                using var dbContext = new DBContext();

                if (step != -1)
                {
                    executePlayerNpcMapDataUpdate(dbContext, false, pnpcStep, (short)step, Id, mapid);
                }

                if (podium != -1)
                {
                    executePlayerNpcMapDataUpdate(dbContext, true, pnpcPodium, (short)podium, Id, mapid);
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }
        }

        if (step != -1)
            pnpcStep.AddOrUpdate(mapid, (byte)step);
        if (podium != -1)
            pnpcPodium.AddOrUpdate(mapid, (short)podium);
    }

    public int getPlayerNpcMapStep(int mapid)
    {
        return pnpcStep.GetValueOrDefault(mapid);
    }

    public int getPlayerNpcMapPodiumData(int mapid)
    {
        return pnpcPodium.GetValueOrDefault(mapid, (short)1);
    }

    public void resetPlayerNpcMapData()
    {
        pnpcStep.Clear();
        pnpcPodium.Clear();
    }

    public void broadcastPacket(Packet packet)
    {
        foreach (IPlayer chr in Players.GetAllOnlinedPlayers())
        {
            chr.sendPacket(packet);
        }
    }

    public List<KeyValuePair<PlayerShopItem, AbstractMapObject>> getAvailableItemBundles(int itemid)
    {
        List<KeyValuePair<PlayerShopItem, AbstractMapObject>> hmsAvailable = new();

        foreach (var ch in getChannels())
        {
            foreach (var hm in ch.HiredMerchantManager.getActiveMerchants())
            {
                List<PlayerShopItem> itemBundles = hm.sendAvailableBundles(itemid);

                foreach (PlayerShopItem mpsi in itemBundles)
                {
                    hmsAvailable.Add(new(mpsi, hm));
                }
            }

            foreach (PlayerShop ps in ch.PlayerShopManager.getActivePlayerShops())
            {
                List<PlayerShopItem> itemBundles = ps.sendAvailableBundles(itemid);

                foreach (PlayerShopItem mpsi in itemBundles)
                {
                    hmsAvailable.Add(new(mpsi, ps));
                }
            }
        }


        hmsAvailable = hmsAvailable.OrderBy(x => x.Key.getPrice()).Take(200).ToList();
        return hmsAvailable;
    }


    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().GetAllOnlinedPlayers())
        {
            player.dropMessage(type, message);
        }
    }

    public async Task Shutdown()
    {
        foreach (var ch in getChannels())
        {
            await ch.Shutdown();
        }

        if (marriagesSchedule != null)
        {
            await marriagesSchedule.CancelAsync(false);
            marriagesSchedule = null;
        }

        if (fishingSchedule != null)
        {
            await fishingSchedule.CancelAsync(false);
            fishingSchedule = null;
        }

        if (partySearchSchedule != null)
        {
            await partySearchSchedule.CancelAsync(false);
            partySearchSchedule = null;
        }

        if (timeoutSchedule != null)
        {
            await timeoutSchedule.CancelAsync(false);
            timeoutSchedule = null;
        }
        Players.disconnectAll();
        log.Information("Finished shutting down world {WorldId}", Id);
    }
}
