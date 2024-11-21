using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Core.Managers;
using Application.Core.model;
using Application.Core.scripting.Event;
using Application.Utility;
using client;
using constants.game;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using net.server.channel;
using net.server.coordinator.matchchecker;
using net.server.coordinator.partysearch;
using net.server.coordinator.world;
using net.server.guild;
using net.server.services;
using net.server.services.type;
using net.server.task;
using net.server.world;
using server;
using server.maps;
using System.Collections.Concurrent;
using tools;
using tools.packets;
using static Application.Core.Game.Relation.BuddyList;

namespace Application.Core.Game.TheWorld;

public class World : IWorld
{
    private ILogger log;
    public int Id { get; set; }
    public string Name { get; set; }
    public string WhyAmIRecommended { get; set; }
    public int Flag { get; set; }
    public float ExpRate { get; set; }
    public float DropRate { get; set; }
    public float BossDropRate { get; set; }
    public float MesoRate { get; set; }
    public float QuestRate { get; set; }
    public float TravelRate { get; set; }
    public float FishingRate { get; set; }
    private float _mobRate;
    public float MobRate
    {
        get => _mobRate;
        set
        {
            if (value <= 0)
                _mobRate = 1;
            else
                _mobRate = Math.Min(value, 5);
        }
    }

    string _serverMessage;
    public string ServerMessage
    {
        get
        {
            return _serverMessage;
        }
        set
        {
            if (_serverMessage != value)
            {
                _serverMessage = value;

                foreach (var ch in getChannels())
                {
                    ch.setServerMessage(_serverMessage);
                }
            }
        }
    }

    public string EventMessage { get; set; }

    public List<IWorldChannel> Channels { get; }
    WorldPlayerStorage? _players;
    public WorldPlayerStorage Players => _players ?? (_players = new WorldPlayerStorage(Id));
    public WorldGuildStorage GuildStorage { get; }
    public Dictionary<int, ITeam> TeamStorage { get; }

    private Dictionary<int, byte> pnpcStep = new();
    private Dictionary<int, short> pnpcPodium = new();

    private Dictionary<int, Messenger> messengers = new();
    private AtomicInteger runningMessengerId = new AtomicInteger();
    private Dictionary<int, Family> families = new();

    private Dictionary<int, int> relationships = new();
    private Dictionary<int, CoupleIdPair> relationshipCouples = new();

    private ServicesManager<WorldServices> services = new ServicesManager<WorldServices>(WorldServices.SAVE_CHARACTER);
    private MatchCheckerCoordinator matchChecker = new MatchCheckerCoordinator();
    private PartySearchCoordinator partySearch = new PartySearchCoordinator();

    private Dictionary<int, Storage> accountStorages = new();
    private object accountCharsLock = new object();

    private HashSet<int> queuedGuilds = new();
    private Dictionary<int, KeyValuePair<KeyValuePair<bool, bool>, CoupleIdPair>> queuedMarriages = new();
    private ConcurrentDictionary<int, HashSet<int>> marriageGuests = new();

    private AtomicInteger runningPartyId = new AtomicInteger();
    private object partyLock = new object();

    private Dictionary<int, int> owlSearched = new();
    private List<Dictionary<int, int>> cashItemBought = new(9);
    private ReaderWriterLockSlim suggestLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    private Dictionary<int, int> disabledServerMessages = new();    // reuse owl lock
    private object srvMessagesLock = new object();
    private ScheduledFuture? srvMessagesSchedule;

    private object activePetsLock = new object();
    private Dictionary<int, int> activePets = new();
    private ScheduledFuture? petsSchedule;
    private long petUpdate;

    private object activeMountsLock = new object();
    private Dictionary<int, int> activeMounts = new();
    private ScheduledFuture? mountsSchedule;
    private long mountUpdate;

    private object activePlayerShopsLock = new object();
    /// <summary>
    /// PlayerId - PlayerShop
    /// </summary>
    private Dictionary<int, PlayerShop> activePlayerShops = new();

    private object activeMerchantsLock = new object();
    private Dictionary<int, KeyValuePair<HiredMerchant, int>> activeMerchants = new();
    private ScheduledFuture? merchantSchedule;
    private long merchantUpdate;

    private Dictionary<Action, long> registeredTimedMapObjects = new();
    private ScheduledFuture? timedMapObjectsSchedule;
    private object timedMapObjectLock = new object();

    private Dictionary<IPlayer, int> fishingAttempters = new Dictionary<IPlayer, int>();
    private Dictionary<IPlayer, int> playerHpDec = new Dictionary<IPlayer, int>();

    private ScheduledFuture? charactersSchedule;
    private ScheduledFuture? marriagesSchedule;
    private ScheduledFuture? mapOwnershipSchedule;
    private ScheduledFuture? fishingSchedule;
    private ScheduledFuture? partySearchSchedule;
    private ScheduledFuture? timeoutSchedule;
    private ScheduledFuture? hpDecSchedule;

    public WorldConfigEntity Configs { get; set; }

    public World(WorldConfigEntity config)
    {
        log = LogFactory.GetLogger("World_" + Id);
        Channels = new List<IWorldChannel>();
        TeamStorage = new Dictionary<int, ITeam>();
        runningPartyId.set(1000000001); // partyid must not clash with charid to solve update item looting issues, found thanks to Vcoc
        runningMessengerId.set(1);
        petUpdate = Server.getInstance().getCurrentTime();
        mountUpdate = petUpdate;
        GuildStorage = new WorldGuildStorage();

        for (int i = 0; i < 9; i++)
        {
            cashItemBought.Add(new());
        }

        Configs = config;
        this.Id = config.Id;
        this.Flag = config.Flag;
        Name = config.Name;
        WhyAmIRecommended = config.RecommendMessage;
        ServerMessage = config.ServerMessage;
        this.EventMessage = config.EventMessage;
        this.ExpRate = config.ExpRate;
        this.DropRate = config.DropRate;
        this.BossDropRate = config.BossDropRate;
        this.MesoRate = config.MesoRate;
        this.QuestRate = config.QuestRate;
        this.TravelRate = config.TravelRate;
        this.FishingRate = config.FishingRate;
        MobRate = config.MobRate;

        TimerManager tman = TimerManager.getInstance();
        petsSchedule = tman.register(new PetFullnessTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        srvMessagesSchedule = tman.register(new ServerMessageTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        mountsSchedule = tman.register(new MountTirednessTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        merchantSchedule = tman.register(new HiredMerchantTask(this), 10 * TimeSpan.FromMinutes(1), 10 * TimeSpan.FromMinutes(1));
        timedMapObjectsSchedule = tman.register(new TimedMapObjectTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        charactersSchedule = tman.register(new CharacterAutosaverTask(this), TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        marriagesSchedule = tman.register(new WeddingReservationTask(this), 
            TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL), 
            TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL));
        mapOwnershipSchedule = tman.register(new MapOwnershipTask(this), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        fishingSchedule = tman.register(new FishingTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        partySearchSchedule = tman.register(new PartySearchTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        timeoutSchedule = tman.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        hpDecSchedule = tman.register(new CharacterHpDecreaseTask(this), 
            YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL, 
            YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL);

        if (YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            var timeLeft = TimeUtils.GetTimeLeftForNextDay();
            FamilyManager.resetEntitlementUsage(this);
            tman.register(new FamilyDailyResetTask(this), TimeSpan.FromDays(1), timeLeft);
        }
    }

    public int getChannelsSize()
    {
        return Channels.Count;
    }

    public List<IWorldChannel> getChannels()
    {
        return new(Channels);
    }

    public IWorldChannel getChannel(int channel)
    {
        return Channels.ElementAtOrDefault(channel - 1) ?? throw new BusinessFatalException($"Channel {channel} not existed");
    }

    public bool addChannel(IWorldChannel channel)
    {
        if (channel.getId() == Channels.Count + 1)
        {
            Channels.Add(channel);
            Players.RelateChannel(channel.getId(), channel.Players);
            return true;
        }
        else
        {
            return false;
        }
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

    public async Task ResizeChannel(int channelSize)
    {
        if (Channels.Count == channelSize)
            return;

        var srv = Server.getInstance();
        while (Channels.Count != channelSize)
        {
            if (Channels.Count > channelSize)
                await srv.RemoveWorldChannel(Id);
            else
                srv.AddWorldChannel(Id);
        }
    }

    public bool canUninstall()
    {
        if (Players.Count() > 0) return false;

        return this.getChannels().All(x => x.canUninstall());
    }


    public void setExpRate(int exp)
    {
        var list = getPlayerStorage().GetAllOnlinedPlayers();

        foreach (IPlayer chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.revertWorldRates();
        }
        this.ExpRate = exp;
        foreach (IPlayer chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.setWorldRates();
        }
    }


    public void setDropRate(int drop)
    {
        var list = getPlayerStorage().GetAllOnlinedPlayers();

        foreach (IPlayer chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.revertWorldRates();
        }
        this.DropRate = drop;
        foreach (IPlayer chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.setWorldRates();
        }
    }
    public void setMesoRate(int meso)
    {
        var list = getPlayerStorage().GetAllOnlinedPlayers();

        foreach (IPlayer chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.revertWorldRates();
        }
        this.MesoRate = meso;
        foreach (IPlayer chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.setWorldRates();
        }
    }

    public int getTransportationTime(double travelTime)
    {
        return (int)Math.Ceiling(travelTime / TravelRate);
    }



    public void loadAccountStorage(int accountId)
    {
        if (getAccountStorage(accountId) == null)
        {
            registerAccountStorage(accountId);
        }
    }

    private void registerAccountStorage(int accountId)
    {
        Storage storage = Storage.loadOrCreateFromDB(accountId, this.Id);
        Monitor.Enter(accountCharsLock);
        try
        {
            accountStorages.AddOrUpdate(accountId, storage);
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }
    }

    public void unregisterAccountStorage(int accountId)
    {
        Monitor.Enter(accountCharsLock);
        try
        {
            accountStorages.Remove(accountId);
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }
    }

    public Storage getAccountStorage(int accountId)
    {
        var m = accountStorages.GetValueOrDefault(accountId);
        if (m == null)
        {
            registerAccountStorage(accountId);
            return accountStorages.GetValueOrDefault(accountId) ?? throw new BusinessException($"Register Storage for AccountId {accountId} failed");
        }
        return m;
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

    public PartySearchCoordinator getPartySearchCoordinator()
    {
        return partySearch;
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

    public IGuild? getGuild(IPlayer? mgc)
    {
        if (mgc == null)
        {
            return null;
        }

        return mgc.GuildModel;
    }

    public bool isWorldCapacityFull()
    {
        return getWorldCapacityStatus() == 2;
    }

    public int getWorldCapacityStatus()
    {
        int worldCap = getChannelsSize() * YamlConfig.config.server.CHANNEL_LOAD;
        int num = Players.Count();

        int status;
        if (num >= worldCap)
        {
            status = 2;
        }
        else if (num >= worldCap * .8)
        { // More than 80 percent o___o
            status = 1;
        }
        else
        {
            status = 0;
        }

        return status;
    }


    public void setGuildAndRank(List<int> cids, int guildid, int rank, int exception)
    {
        foreach (int cid in cids)
        {
            if (cid != exception)
            {
                setGuildAndRank(cid, guildid, rank);
            }
        }
    }

    public void setOfflineGuildStatus(int guildid, int guildrank, int cid)
    {
        using var dbContext = new DBContext();
        dbContext.Characters.Where(x => x.Id == cid).ExecuteUpdate(x => x.SetProperty(y => y.GuildId, guildid).SetProperty(y => y.GuildRank, guildrank));
    }

    public void setGuildAndRank(int cid, int guildid, int rank)
    {
        var mc = getPlayerStorage().getCharacterById(cid);
        if (mc == null || !mc.IsOnlined)
        {
            return;
        }
        bool bDifferentGuild;
        if (guildid == -1 && rank == -1)
        {
            bDifferentGuild = true;
        }
        else
        {
            bDifferentGuild = guildid != mc.GuildId;
            mc.GuildId = guildid;
            mc.GuildRank = rank;

            if (bDifferentGuild)
            {
                mc.AllianceRank = 5;
            }

            mc.saveGuildStatus();
        }
        if (bDifferentGuild)
        {
            if (mc.isLoggedinWorld())
            {
                var guild = AllGuildStorage.GetGuildById(guildid);
                if (guild != null)
                {
                    mc.getMap().broadcastPacket(mc, GuildPackets.guildNameChanged(cid, guild.getName()));
                    mc.getMap().broadcastPacket(mc, GuildPackets.guildMarkChanged(cid, guild));
                }
                else
                {
                    mc.getMap().broadcastPacket(mc, GuildPackets.guildNameChanged(cid, ""));
                }
            }
        }
    }

    public void changeEmblem(int gid, List<int> affectedPlayers, IGuild mgs)
    {
        sendPacket(affectedPlayers, GuildPackets.guildEmblemChange(gid, (short)mgs.LogoBg, (byte)mgs.LogoBgColor, (short)mgs.Logo, (byte)mgs.LogoColor), -1);
        setGuildAndRank(affectedPlayers, -1, -1, -1);    //respawn player
    }

    public void sendPacket(List<int> targetIds, Packet packet, int exception)
    {
        IPlayer? chr;
        foreach (int i in targetIds)
        {
            if (i == exception)
            {
                continue;
            }
            chr = getPlayerStorage().getCharacterById(i);
            if (chr != null && chr.isLoggedinWorld())
            {
                chr.sendPacket(packet);
            }
        }
    }

    public bool isGuildQueued(int guildId)
    {
        return queuedGuilds.Contains(guildId);
    }

    public void putGuildQueued(int guildId)
    {
        queuedGuilds.Add(guildId);
    }

    public void removeGuildQueued(int guildId)
    {
        queuedGuilds.Remove(guildId);
    }

    public bool isMarriageQueued(int marriageId)
    {
        return queuedMarriages.ContainsKey(marriageId);
    }

    public KeyValuePair<bool, bool>? getMarriageQueuedLocation(int marriageId)
    {
        var qm = queuedMarriages.get(marriageId);
        return (qm != null) ? qm.Value.Key : null;
    }

    public CoupleIdPair? getMarriageQueuedCouple(int marriageId)
    {
        var qm = queuedMarriages.get(marriageId);
        return (qm != null) ? qm.Value.Value : null;
    }

    public void putMarriageQueued(int marriageId, bool cathedral, bool premium, int groomId, int brideId)
    {
        queuedMarriages.AddOrUpdate(marriageId, new(new(cathedral, premium), new(groomId, brideId)));
        marriageGuests.AddOrUpdate(marriageId, new());
    }

    public KeyValuePair<bool, HashSet<int>> removeMarriageQueued(int marriageId)
    {
        queuedMarriages.Remove(marriageId, out var d);
        marriageGuests.Remove(marriageId, out var guests);

        return new(d.Key.Value, guests);
    }

    public bool addMarriageGuest(int marriageId, int playerId)
    {
        HashSet<int>? guests = marriageGuests.GetValueOrDefault(marriageId);
        if (guests != null)
        {
            if (guests.Contains(playerId)) return false;

            guests.Add(playerId);
            return true;
        }

        return false;
    }

    public CoupleIdPair? getWeddingCoupleForGuest(int guestId, bool cathedral)
    {
        foreach (var ch in getChannels())
        {
            var p = ch.getWeddingCoupleForGuest(guestId, cathedral);
            if (p != null)
            {
                return p;
            }
        }

        List<int> possibleWeddings = new();
        foreach (var mg in marriageGuests)
        {
            if (mg.Value.Contains(guestId))
            {
                var loc = getMarriageQueuedLocation(mg.Key);
                if (loc != null && cathedral.Equals(loc.Value.Key))
                {
                    possibleWeddings.Add(mg.Key);
                }
            }
        }

        int pwSize = possibleWeddings.Count;
        if (pwSize == 0)
        {
            return null;
        }
        else if (pwSize > 1)
        {
            int selectedPw = -1;
            int selectedPos = int.MaxValue;

            foreach (int pw in possibleWeddings)
            {
                foreach (var ch in getChannels())
                {
                    int pos = ch.getWeddingReservationStatus(pw, cathedral);
                    if (pos != -1)
                    {
                        if (pos < selectedPos)
                        {
                            selectedPos = pos;
                            selectedPw = pw;
                            break;
                        }
                    }
                }
            }

            if (selectedPw == -1)
            {
                return null;
            }

            possibleWeddings.Clear();
            possibleWeddings.Add(selectedPw);
        }

        return getMarriageQueuedCouple(possibleWeddings[0]);
    }

    public void debugMarriageStatus()
    {
        log.Debug("Queued marriages: " + queuedMarriages);
        log.Debug("Guest list: " + marriageGuests);
    }

    public ITeam createParty(IPlayer chrfor)
    {
        int partyid = runningPartyId.getAndIncrement();
        var party = new Team(partyid, chrfor);

        Monitor.Enter(partyLock);
        try
        {
            TeamStorage.AddOrUpdate(party.getId(), party);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }

        party.addMember(chrfor);
        return party;
    }

    public ITeam? getParty(int partyid)
    {
        Monitor.Enter(partyLock);
        try
        {
            return TeamStorage.GetValueOrDefault(partyid);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    private ITeam disbandParty(int partyid)
    {
        Monitor.Enter(partyLock);
        try
        {
            TeamStorage.Remove(partyid, out var d);
            return d;
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    private void updateParty(ITeam party, PartyOperation operation, IPlayer target)
    {
        var partyMembers = party.getMembers();

        foreach (var partychar in partyMembers)
        {
            partychar.setParty(operation == PartyOperation.DISBAND ? null : party);
            if (partychar.IsOnlined)
            {
                partychar.sendPacket(PacketCreator.updateParty(partychar.getClient().getChannel(), party, operation, target));
            }
        }
        switch (operation)
        {
            case PartyOperation.LEAVE:
            case PartyOperation.EXPEL:
                target.setParty(null);
                if (target.IsOnlined)
                {
                    target.sendPacket(PacketCreator.updateParty(target.Client.getChannel(), party, operation, target));
                }
                break;
            default:
                break;
        }
    }

    public void updateParty(int partyid, PartyOperation operation, IPlayer target)
    {
        var party = getParty(partyid) ?? throw new ArgumentException("no party with the specified partyid exists");
        switch (operation)
        {
            case PartyOperation.JOIN:
                party.addMember(target);
                break;
            case PartyOperation.EXPEL:
            case PartyOperation.LEAVE:
                party.removeMember(target);
                break;
            case PartyOperation.DISBAND:
                disbandParty(partyid);
                break;
            case PartyOperation.SILENT_UPDATE:
            case PartyOperation.LOG_ONOFF:
                party.updateMember(target);
                break;
            case PartyOperation.CHANGE_LEADER:
                var mc = party.getLeader();
                if (mc != null)
                {
                    var eim = mc.getEventInstance();

                    if (eim != null && eim.isEventLeader(mc))
                    {
                        eim.changedLeader(target);
                    }
                    else
                    {
                        int oldLeaderMapid = mc.getMapId();

                        if (MiniDungeonInfo.isDungeonMap(oldLeaderMapid))
                        {
                            if (oldLeaderMapid != target.getMapId())
                            {
                                var mmd = mc.getClient().getChannelServer().getMiniDungeon(oldLeaderMapid);
                                if (mmd != null)
                                {
                                    mmd.close();
                                }
                            }
                        }
                    }
                    party.setLeader(target);
                }
                break;
            default:
                log.Warning("Unhandled updateParty operation: {PartyOperation}", operation.ToString());
                break;
        }
        updateParty(party, operation, target);
    }

    public void removeMapPartyMembers(int partyid)
    {
        var party = getParty(partyid);
        if (party == null)
        {
            return;
        }

        foreach (var mc in party.getMembers())
        {
            if (mc != null)
            {
                var map = mc.getMap();
                if (map != null)
                {
                    map.removeParty(partyid);
                }
            }
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

    public void partyChat(ITeam party, string chattext, string namefrom)
    {
        foreach (IPlayer partychar in party.getMembers())
        {
            if (!partychar.getName().Equals(namefrom))
            {
                if (partychar.IsOnlined)
                {
                    partychar.sendPacket(PacketCreator.multiChat(namefrom, chattext, 1));
                }
            }
        }
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

    public Messenger createMessenger(MessengerCharacter chrfor)
    {
        int messengerid = runningMessengerId.getAndIncrement();
        Messenger messenger = new Messenger(messengerid, chrfor);
        messengers.AddOrUpdate(messenger.getId(), messenger);
        return messenger;
    }

    public Messenger? getMessenger(int messengerid)
    {
        return messengers.GetValueOrDefault(messengerid);
    }

    public void leaveMessenger(int messengerid, MessengerCharacter target)
    {
        var messenger = getMessenger(messengerid) ?? throw new ArgumentException("No messenger with the specified messengerid exists");
        int position = messenger.getPositionByName(target.getName());
        messenger.removeMember(target);
        removeMessengerPlayer(messenger, position);
    }

    public void messengerInvite(string sender, int messengerid, string target, int fromchannel)
    {
        if (isConnected(target))
        {
            var targetChr = getPlayerStorage().getCharacterByName(target);
            if (targetChr != null && targetChr.IsOnlined)
            {
                var messenger = targetChr.getMessenger();
                if (messenger == null)
                {
                    var from = getChannel(fromchannel).getPlayerStorage().getCharacterByName(sender);
                    if (from != null)
                    {
                        if (InviteCoordinator.createInvite(InviteType.MESSENGER, from, messengerid, targetChr.getId()))
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

    public void addMessengerPlayer(Messenger messenger, string namefrom, int fromchannel, int position)
    {
        foreach (var messengerchar in messenger.getMembers())
        {
            var chr = getPlayerStorage().getCharacterByName(messengerchar.getName());
            if (chr == null || !chr.IsOnlined)
            {
                continue;
            }
            if (!messengerchar.getName().Equals(namefrom))
            {
                var from = getChannel(fromchannel).getPlayerStorage().getCharacterByName(namefrom);
                if (from != null)
                {
                    chr.sendPacket(PacketCreator.addMessengerPlayer(namefrom, from, position, (byte)(fromchannel - 1)));
                    from.sendPacket(PacketCreator.addMessengerPlayer(chr.Name, chr, messengerchar.getPosition(), (byte)(messengerchar.getChannel() - 1)));
                }
            }
            else
            {
                chr.sendPacket(PacketCreator.joinMessenger(messengerchar.getPosition()));
            }
        }
    }

    public void removeMessengerPlayer(Messenger messenger, int position)
    {
        foreach (var messengerchar in messenger.getMembers())
        {
            var chr = getPlayerStorage().getCharacterByName(messengerchar.getName());
            if (chr != null && chr.IsOnlined)
            {
                chr.sendPacket(PacketCreator.removeMessengerPlayer(position));
            }
        }
    }

    public void messengerChat(Messenger messenger, string chattext, string namefrom)
    {
        string from = "";
        string to1 = "";
        string to2 = "";
        foreach (var messengerchar in messenger.getMembers())
        {
            if (!(messengerchar.getName().Equals(namefrom)))
            {
                var chr = getPlayerStorage().getCharacterByName(messengerchar.getName());
                if (chr != null && chr.IsOnlined)
                {
                    chr.sendPacket(PacketCreator.messengerChat(chattext));
                    if (to1.Equals(""))
                    {
                        to1 = messengerchar.getName();
                    }
                    else if (to2.Equals(""))
                    {
                        to2 = messengerchar.getName();
                    }
                }
            }
            else
            {
                from = messengerchar.getName();
            }
        }
    }

    public void declineChat(string sender, IPlayer player)
    {
        if (isConnected(sender))
        {
            var senderChr = getPlayerStorage().getCharacterByName(sender);
            if (senderChr != null && senderChr.IsOnlined && senderChr.Messenger != null)
            {
                if (InviteCoordinator.answerInvite(InviteType.MESSENGER, player.getId(), senderChr.Messenger.getId(), false).result == InviteResultType.DENIED)
                {
                    senderChr.sendPacket(PacketCreator.messengerNote(player.getName(), 5, 0));
                }
            }
        }
    }

    public void updateMessenger(int messengerid, string namefrom, int fromchannel)
    {
        var messenger = getMessenger(messengerid);
        int position = messenger.getPositionByName(namefrom);
        updateMessenger(messenger, namefrom, position, fromchannel);
    }

    public void updateMessenger(Messenger messenger, string namefrom, int position, int fromchannel)
    {
        foreach (var messengerchar in messenger.getMembers())
        {
            var ch = getChannel(fromchannel);
            if (!(messengerchar.getName().Equals(namefrom)))
            {
                var chr = ch.getPlayerStorage().getCharacterByName(messengerchar.getName());
                if (chr != null)
                {
                    var fromPlayer = getChannel(fromchannel).getPlayerStorage().getCharacterByName(namefrom);
                    if (fromPlayer != null)
                        chr.sendPacket(PacketCreator.updateMessengerPlayer(namefrom, fromPlayer, position, (byte)(fromchannel - 1)));
                }
            }
        }
    }

    public void silentLeaveMessenger(int messengerid, MessengerCharacter target)
    {
        var messenger = getMessenger(messengerid) ?? throw new ArgumentException("No messenger with the specified messengerid exists");
        messenger.addMember(target, target.getPosition());
    }

    public void joinMessenger(int messengerid, MessengerCharacter target, string from, int fromchannel)
    {
        var messenger = getMessenger(messengerid) ?? throw new ArgumentException("No messenger with the specified messengerid exists");
        messenger.addMember(target, target.getPosition());
        addMessengerPlayer(messenger, from, fromchannel, target.getPosition());
    }

    public void silentJoinMessenger(int messengerid, MessengerCharacter target, int position)
    {
        var messenger = getMessenger(messengerid) ?? throw new ArgumentException("No messenger with the specified messengerid exists");
        messenger.addMember(target, position);
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

    private static int getPetKey(IPlayer chr, sbyte petSlot)
    {
        // assuming max 3 pets
        return (chr.getId() << 2) + petSlot;
    }

    public void addOwlItemSearch(int itemid)
    {
        suggestLock.EnterWriteLock();
        try
        {
            var cur = owlSearched.get(itemid) ?? 0;
            owlSearched.AddOrUpdate(itemid, cur + 1);
        }
        finally
        {
            suggestLock.ExitWriteLock();
        }
    }

    public List<KeyValuePair<int, int>> getOwlSearchedItems()
    {
        if (YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
        {
            return new(0);
        }

        suggestLock.EnterReadLock();
        try
        {
            List<KeyValuePair<int, int>> searchCounts = new(owlSearched.Count);

            foreach (var e in owlSearched)
            {
                searchCounts.Add(new(e.Key, e.Value));
            }

            return searchCounts;
        }
        finally
        {
            suggestLock.ExitReadLock();
        }
    }

    public void addCashItemBought(int snid)
    {
        suggestLock.EnterWriteLock();
        try
        {
            Dictionary<int, int> tabItemBought = cashItemBought[snid / 10000000];

            var cur = tabItemBought.GetValueOrDefault(snid);
            tabItemBought.AddOrUpdate(snid, cur + 1);
        }
        finally
        {
            suggestLock.ExitWriteLock();
        }
    }

    private List<List<KeyValuePair<int, int>>> getBoughtCashItems()
    {
        if (YamlConfig.config.server.USE_ENFORCE_ITEM_SUGGESTION)
        {
            List<List<KeyValuePair<int, int>>> boughtCounts = new(9);

            // thanks GabrielSin for pointing out an issue here
            for (int i = 0; i < 9; i++)
            {
                List<KeyValuePair<int, int>> tabCounts = new(0);
                boughtCounts.Add(tabCounts);
            }

            return boughtCounts;
        }

        suggestLock.EnterReadLock();
        try
        {
            List<List<KeyValuePair<int, int>>> boughtCounts = new(cashItemBought.Count);

            foreach (Dictionary<int, int> tab in cashItemBought)
            {
                List<KeyValuePair<int, int>> tabItems = new();
                boughtCounts.Add(tabItems);

                foreach (var e in tab)
                {
                    tabItems.Add(new(e.Key, e.Value));
                }
            }

            return boughtCounts;
        }
        finally
        {
            suggestLock.ExitReadLock();
        }
    }

    private List<int> getMostSellerOnTab(List<KeyValuePair<int, int>> tabSellers)
    {
        return tabSellers.OrderByDescending(x => x.Value).Select(x => x.Key).Take(5).ToList();
    }

    public List<List<int>> getMostSellerCashItems()
    {
        List<List<KeyValuePair<int, int>>> mostSellers = this.getBoughtCashItems();
        List<List<int>> cashLeaderboards = new(9);
        List<int> tabLeaderboards;
        List<int> allLeaderboards = new List<int>();

        foreach (var tabSellers in mostSellers)
        {
            if (tabSellers.Count < 5)
            {
                if (allLeaderboards == null)
                {
                    List<KeyValuePair<int, int>> allSellers = new();
                    foreach (var tabItems in mostSellers)
                    {
                        allSellers.AddRange(tabItems);
                    }

                    allLeaderboards = getMostSellerOnTab(allSellers);
                }

                tabLeaderboards = new();
                if (allLeaderboards.Count < 5)
                {
                    foreach (int i in GameConstants.CASH_DATA)
                    {
                        tabLeaderboards.Add(i);
                    }
                }
                else
                {
                    tabLeaderboards.AddRange(allLeaderboards);
                }
            }
            else
            {
                tabLeaderboards = getMostSellerOnTab(tabSellers);
            }

            cashLeaderboards.Add(tabLeaderboards);
        }

        return cashLeaderboards;
    }

    public void registerPetHunger(IPlayer chr, sbyte petSlot)
    {
        if (chr.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
        {
            return;
        }

        int key = getPetKey(chr, petSlot);

        Monitor.Enter(activePetsLock);
        try
        {
            int initProc;
            if (Server.getInstance().getCurrentTime() - petUpdate > 55000)
            {
                initProc = YamlConfig.config.server.PET_EXHAUST_COUNT - 2;
            }
            else
            {
                initProc = YamlConfig.config.server.PET_EXHAUST_COUNT - 1;
            }

            activePets.AddOrUpdate(key, initProc);
        }
        finally
        {
            Monitor.Exit(activePetsLock);
        }
    }

    public void unregisterPetHunger(IPlayer chr, sbyte petSlot)
    {
        int key = getPetKey(chr, petSlot);

        Monitor.Enter(activePetsLock);
        try
        {
            activePets.Remove(key);
        }
        finally
        {
            Monitor.Exit(activePetsLock);
        }
    }

    public void runPetSchedule()
    {
        Dictionary<int, int> deployedPets;

        Monitor.Enter(activePetsLock);
        try
        {
            petUpdate = Server.getInstance().getCurrentTime();
            deployedPets = new(activePets);   // exception here found thanks to MedicOP
        }
        finally
        {
            Monitor.Exit(activePetsLock);
        }

        foreach (var dp in deployedPets)
        {
            var chr = this.getPlayerStorage().getCharacterById(dp.Key / 4);
            if (chr == null || !chr.isLoggedinWorld())
            {
                continue;
            }

            int dpVal = dp.Value + 1;
            if (dpVal == YamlConfig.config.server.PET_EXHAUST_COUNT)
            {
                chr.runFullnessSchedule(dp.Key % 4);
                dpVal = 0;
            }

            Monitor.Enter(activePetsLock);
            try
            {
                activePets.AddOrUpdate(dp.Key, dpVal);
            }
            finally
            {
                Monitor.Exit(activePetsLock);
            }
        }
    }

    public void registerMountHunger(IPlayer chr)
    {
        if (chr.isGM() && YamlConfig.config.server.GM_PETS_NEVER_HUNGRY || YamlConfig.config.server.PETS_NEVER_HUNGRY)
        {
            return;
        }

        int key = chr.getId();
        Monitor.Enter(activeMountsLock);
        try
        {
            int initProc;
            if (Server.getInstance().getCurrentTime() - mountUpdate > 45000)
            {
                initProc = YamlConfig.config.server.MOUNT_EXHAUST_COUNT - 2;
            }
            else
            {
                initProc = YamlConfig.config.server.MOUNT_EXHAUST_COUNT - 1;
            }

            activeMounts.AddOrUpdate(key, initProc);
        }
        finally
        {
            Monitor.Exit(activeMountsLock);
        }
    }

    public void unregisterMountHunger(IPlayer chr)
    {
        int key = chr.getId();

        Monitor.Enter(activeMountsLock);
        try
        {
            activeMounts.Remove(key);
        }
        finally
        {
            Monitor.Exit(activeMountsLock);
        }
    }

    public void runMountSchedule()
    {
        Dictionary<int, int> deployedMounts;
        Monitor.Enter(activeMountsLock);
        try
        {
            mountUpdate = Server.getInstance().getCurrentTime();
            deployedMounts = new(activeMounts);
        }
        finally
        {
            Monitor.Exit(activeMountsLock);
        }

        foreach (var dp in deployedMounts)
        {
            var chr = this.getPlayerStorage().getCharacterById(dp.Key);
            if (chr == null || !chr.isLoggedinWorld())
            {
                continue;
            }

            int dpVal = dp.Value + 1;
            if (dpVal == YamlConfig.config.server.MOUNT_EXHAUST_COUNT)
            {
                if (!chr.runTirednessSchedule())
                {
                    continue;
                }
                dpVal = 0;
            }

            Monitor.Enter(activeMountsLock);
            try
            {
                activeMounts.AddOrUpdate(dp.Key, dpVal);
            }
            finally
            {
                Monitor.Exit(activeMountsLock);
            }
        }
    }

    public void registerPlayerShop(PlayerShop ps)
    {
        Monitor.Enter(activePlayerShopsLock);
        try
        {
            activePlayerShops.AddOrUpdate(ps.getOwner().getId(), ps);
        }
        finally
        {
            Monitor.Exit(activePlayerShopsLock);
        }
    }

    public void unregisterPlayerShop(PlayerShop ps)
    {
        Monitor.Enter(activePlayerShopsLock);
        try
        {
            activePlayerShops.Remove(ps.getOwner().getId());
        }
        finally
        {
            Monitor.Exit(activePlayerShopsLock);
        }
    }

    public List<PlayerShop> getActivePlayerShops()
    {
        Monitor.Enter(activePlayerShopsLock);
        try
        {
            return activePlayerShops.Values.ToList();
        }
        finally
        {
            Monitor.Exit(activePlayerShopsLock);
        }
    }

    public PlayerShop? getPlayerShop(int ownerid)
    {
        Monitor.Enter(activePlayerShopsLock);
        try
        {
            return activePlayerShops.GetValueOrDefault(ownerid);
        }
        finally
        {
            Monitor.Exit(activePlayerShopsLock);
        }
    }

    public void registerHiredMerchant(HiredMerchant hm)
    {
        Monitor.Enter(activeMerchantsLock);
        try
        {
            int initProc;
            if (Server.getInstance().getCurrentTime() - merchantUpdate > TimeSpan.FromMinutes(5).TotalMilliseconds)
            {
                initProc = 1;
            }
            else
            {
                initProc = 0;
            }

            activeMerchants.AddOrUpdate(hm.getOwnerId(), new(hm, initProc));
        }
        finally
        {
            Monitor.Exit(activeMerchantsLock);
        }
    }

    public void unregisterHiredMerchant(HiredMerchant hm)
    {
        Monitor.Enter(activeMerchantsLock);
        try
        {
            activeMerchants.Remove(hm.getOwnerId());
        }
        finally
        {
            Monitor.Exit(activeMerchantsLock);
        }
    }

    public void runHiredMerchantSchedule()
    {
        Dictionary<int, KeyValuePair<HiredMerchant, int>> deployedMerchants;
        Monitor.Enter(activeMerchantsLock);
        try
        {
            merchantUpdate = Server.getInstance().getCurrentTime();
            deployedMerchants = new(activeMerchants);

            foreach (var dm in deployedMerchants)
            {
                int timeOn = dm.Value.Value;
                HiredMerchant hm = dm.Value.Key;

                if (timeOn <= 144)
                {
                    // 1440 minutes == 24hrs
                    activeMerchants.AddOrUpdate(hm.getOwnerId(), new(dm.Value.Key, timeOn + 1));
                }
                else
                {
                    hm.forceClose();
                    this.getChannel(hm.getChannel()).removeHiredMerchant(hm.getOwnerId());

                    activeMerchants.Remove(dm.Key);
                }
            }
        }
        finally
        {
            Monitor.Exit(activeMerchantsLock);
        }
    }

    public List<HiredMerchant> getActiveMerchants()
    {
        List<HiredMerchant> hmList = new();
        Monitor.Enter(activeMerchantsLock);
        try
        {
            foreach (KeyValuePair<HiredMerchant, int> hmp in activeMerchants.Values)
            {
                HiredMerchant hm = hmp.Key;
                if (hm.isOpen())
                {
                    hmList.Add(hm);
                }
            }

            return hmList;
        }
        finally
        {
            Monitor.Exit(activeMerchantsLock);
        }
    }

    public HiredMerchant? getHiredMerchant(int ownerid)
    {
        Monitor.Enter(activeMerchantsLock);
        try
        {
            if (activeMerchants.ContainsKey(ownerid))
            {
                return activeMerchants[ownerid].Key;
            }

            return null;
        }
        finally
        {
            Monitor.Exit(activeMerchantsLock);
        }
    }

    public void registerTimedMapObject(Action r, long duration)
    {
        Monitor.Enter(timedMapObjectLock);
        try
        {
            long expirationTime = Server.getInstance().getCurrentTime() + duration;
            registeredTimedMapObjects.AddOrUpdate(r, expirationTime);
        }
        finally
        {
            Monitor.Exit(timedMapObjectLock);
        }
    }

    public void runTimedMapObjectSchedule()
    {
        List<Action> toRemove = new();

        Monitor.Enter(timedMapObjectLock);
        try
        {
            long timeNow = Server.getInstance().getCurrentTime();

            foreach (var rtmo in registeredTimedMapObjects)
            {
                if (rtmo.Value <= timeNow)
                {
                    toRemove.Add(rtmo.Key);
                }
            }

            foreach (Action r in toRemove)
            {
                registeredTimedMapObjects.Remove(r);
            }
        }
        finally
        {
            Monitor.Exit(timedMapObjectLock);
        }

        foreach (Action r in toRemove)
        {
            r.Invoke();
        }
    }

    public void addPlayerHpDecrease(IPlayer chr)
    {
        playerHpDec.TryAdd(chr, 0);
    }

    public void removePlayerHpDecrease(IPlayer chr)
    {
        playerHpDec.Remove(chr);
    }

    public void runPlayerHpDecreaseSchedule()
    {
        Dictionary<IPlayer, int> m = new();
        m.putAll(playerHpDec);

        foreach (var e in m)
        {
            IPlayer chr = e.Key;

            if (!chr.isAwayFromWorld())
            {
                int c = e.Value;
                c = (c + 1) % YamlConfig.config.server.MAP_DAMAGE_OVERTIME_COUNT;
                playerHpDec.AddOrUpdate(chr, c);

                if (c == 0)
                {
                    chr.doHurtHp();
                }
            }
        }
    }

    public void resetDisabledServerMessages()
    {
        Monitor.Enter(srvMessagesLock);
        try
        {
            disabledServerMessages.Clear();
        }
        finally
        {
            Monitor.Exit(srvMessagesLock);
        }
    }

    public bool registerDisabledServerMessage(int chrid)
    {
        Monitor.Enter(srvMessagesLock);
        try
        {
            bool alreadyDisabled = disabledServerMessages.ContainsKey(chrid);
            disabledServerMessages.AddOrUpdate(chrid, 0);

            return alreadyDisabled;
        }
        finally
        {
            Monitor.Exit(srvMessagesLock);
        }
    }

    public bool unregisterDisabledServerMessage(int chrid)
    {
        Monitor.Enter(srvMessagesLock);
        try
        {
            return disabledServerMessages.Remove(chrid, out var d);
        }
        finally
        {
            Monitor.Exit(srvMessagesLock);
        }
    }

    public void runDisabledServerMessagesSchedule()
    {
        List<int> toRemove = new();

        Monitor.Enter(srvMessagesLock);
        try
        {
            foreach (var dsm in disabledServerMessages)
            {
                int b = dsm.Value;
                if (b >= 4)
                {
                    // ~35sec duration, 10sec update
                    toRemove.Add(dsm.Key);
                }
                else
                {
                    disabledServerMessages.AddOrUpdate(dsm.Key, ++b);
                }
            }

            foreach (int chrid in toRemove)
            {
                disabledServerMessages.Remove(chrid);
            }
        }
        finally
        {
            Monitor.Exit(srvMessagesLock);
        }

        if (toRemove.Count > 0)
        {
            foreach (int chrid in toRemove)
            {
                var chr = Players.getCharacterById(chrid);

                if (chr != null && chr.isLoggedinWorld())
                {
                    chr.sendPacket(PacketCreator.serverMessage(chr.getClient().getChannelServer().getServerMessage()));
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

        foreach (HiredMerchant hm in getActiveMerchants())
        {
            List<PlayerShopItem> itemBundles = hm.sendAvailableBundles(itemid);

            foreach (PlayerShopItem mpsi in itemBundles)
            {
                hmsAvailable.Add(new(mpsi, hm));
            }
        }

        foreach (PlayerShop ps in getActivePlayerShops())
        {
            List<PlayerShopItem> itemBundles = ps.sendAvailableBundles(itemid);

            foreach (PlayerShopItem mpsi in itemBundles)
            {
                hmsAvailable.Add(new(mpsi, ps));
            }
        }
        hmsAvailable = hmsAvailable.OrderBy(x => x.Key.getPrice()).Take(200).ToList();
        return hmsAvailable;
    }

    private void pushRelationshipCouple(CoupleTotal couple)
    {
        int mid = couple.MarriageId, hid = couple.HusbandId, wid = couple.WifeId;
        relationshipCouples.AddOrUpdate(mid, new(hid, wid));
        relationships.AddOrUpdate(hid, mid);
        relationships.AddOrUpdate(wid, mid);
    }

    public CoupleIdPair? getRelationshipCouple(int relationshipId)
    {
        var rc = relationshipCouples.GetValueOrDefault(relationshipId);

        if (rc == null)
        {
            var couple = getRelationshipCoupleFromDb(relationshipId, true);
            if (couple == null)
            {
                return null;
            }

            pushRelationshipCouple(couple);
            rc = new(couple.HusbandId, couple.WifeId);
        }

        return rc;
    }

    public int getRelationshipId(int playerId)
    {
        if (!relationships.ContainsKey(playerId))
        {
            var couple = getRelationshipCoupleFromDb(playerId, false);
            if (couple == null)
            {
                return -1;
            }

            pushRelationshipCouple(couple);
            return couple.MarriageId;
        }

        return relationships[playerId];
    }

    private CoupleTotal? getRelationshipCoupleFromDb(int id, bool usingMarriageId)
    {
        try
        {
            using var dbContext = new DBContext();
            DB_Marriage? model = null;
            if (usingMarriageId)
            {
                model = dbContext.Marriages.FirstOrDefault(x => x.Marriageid == id);
            }
            else
            {
                model = dbContext.Marriages.FirstOrDefault(x => x.Husbandid == id || x.Wifeid == id);
            }
            if (model == null)
                return null;

            return new CoupleTotal(model.Marriageid, model.Husbandid, model.Wifeid);
        }
        catch (Exception se)
        {
            log.Error(se.ToString());
            return null;
        }
    }

    public int createRelationship(int groomId, int brideId)
    {
        int ret = addRelationshipToDb(groomId, brideId);

        pushRelationshipCouple(new(ret, groomId, brideId));
        return ret;
    }

    private int addRelationshipToDb(int groomId, int brideId)
    {
        try
        {
            using var dbContext = new DBContext();
            var dbModel = new DB_Marriage
            {
                Husbandid = groomId,
                Wifeid = brideId
            };
            dbContext.Marriages.Add(dbModel);
            dbContext.SaveChanges();
            return dbModel.Marriageid;
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            return -1;
        }
    }

    public void deleteRelationship(int playerId, int partnerId)
    {
        int relationshipId = relationships.GetValueOrDefault(playerId);
        deleteRelationshipFromDb(relationshipId);

        relationshipCouples.Remove(relationshipId);
        relationships.Remove(playerId);
        relationships.Remove(partnerId);
    }

    private void deleteRelationshipFromDb(int playerId)
    {
        try
        {
            using var dbContext = new DBContext();
            dbContext.Marriages.Where(x => x.Marriageid == playerId).ExecuteDelete();
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().GetAllOnlinedPlayers())
        {
            player.dropMessage(type, message);
        }
    }

    public bool registerFisherPlayer(IPlayer chr, int baitLevel)
    {
        lock (fishingAttempters)
        {
            if (fishingAttempters.ContainsKey(chr))
            {
                return false;
            }

            fishingAttempters.Add(chr, baitLevel);
            return true;
        }
    }

    public int unregisterFisherPlayer(IPlayer chr)
    {
        if (fishingAttempters.Remove(chr, out var baitLevel))
        {
            return baitLevel;
        }
        else
        {
            return 0;
        }
    }

    public void runCheckFishingSchedule()
    {
        double[] fishingLikelihoods = Fishing.fetchFishingLikelihood();
        double yearLikelihood = fishingLikelihoods[0], timeLikelihood = fishingLikelihoods[1];

        if (fishingAttempters.Count > 0)
        {
            List<IPlayer> fishingAttemptersList;

            lock (fishingAttempters)
            {
                fishingAttemptersList = new(fishingAttempters.Keys);
            }

            foreach (IPlayer chr in fishingAttemptersList)
            {
                int baitLevel = unregisterFisherPlayer(chr);
                Fishing.doFishing(chr, baitLevel, yearLikelihood, timeLikelihood);
            }
        }
    }

    public void runPartySearchUpdateSchedule()
    {
        partySearch.updatePartySearchStorage();
        partySearch.runPartySearch();
    }

    public BaseService getServiceAccess(WorldServices sv)
    {
        return services.getAccess(sv).getService();
    }

    private void closeWorldServices()
    {
        services.shutdown();
    }

    private void clearWorldData()
    {
        TeamStorage.Clear();

        closeWorldServices();
    }

    public async Task Shutdown()
    {
        foreach (var ch in getChannels())
        {
            await ch.Shutdown();
        }

        if (petsSchedule != null)
        {
            await petsSchedule.CancelAsync(false);
            petsSchedule = null;
        }

        if (srvMessagesSchedule != null)
        {
            await srvMessagesSchedule.CancelAsync(false);
            srvMessagesSchedule = null;
        }

        if (mountsSchedule != null)
        {
            await mountsSchedule.CancelAsync(false);
            mountsSchedule = null;
        }

        if (merchantSchedule != null)
        {
            await merchantSchedule.CancelAsync(false);
            merchantSchedule = null;
        }

        if (timedMapObjectsSchedule != null)
        {
            await timedMapObjectsSchedule.CancelAsync(false);
            timedMapObjectsSchedule = null;
        }

        if (charactersSchedule != null)
        {
            await charactersSchedule.CancelAsync(false);
            charactersSchedule = null;
        }

        if (marriagesSchedule != null)
        {
            await marriagesSchedule.CancelAsync(false);
            marriagesSchedule = null;
        }

        if (mapOwnershipSchedule != null)
        {
            await mapOwnershipSchedule.CancelAsync(false);
            mapOwnershipSchedule = null;
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

        if (hpDecSchedule != null)
        {
            await hpDecSchedule.CancelAsync(false);
            hpDecSchedule = null;
        }
        await SchedulerManage.Scheduler.Clear();

        Players.disconnectAll();

        clearWorldData();
        log.Information("Finished shutting down world {WorldId}", Id);
    }
}
