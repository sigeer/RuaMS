using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Invites;
using Application.Core.Game.Maps;
using Application.Core.Game.Relation;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.WorldEvents;
using Application.Core.Managers;
using client;
using constants.game;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server;
using net.server.channel;
using net.server.coordinator.matchchecker;
using net.server.coordinator.partysearch;
using net.server.guild;
using net.server.services;
using net.server.services.type;
using net.server.task;
using net.server.world;
using server;
using server.maps;
using tools;
using static Application.Core.Game.Relation.BuddyList;

namespace Application.Core.Game.TheWorld;

public class World : IWorld
{
    private ILogger log;
    public int Id { get; set; }
    public string Name { get; set; }
    public string WhyAmIRecommended { get; set; }
    public int Flag { get; set; }

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
    public FishingWorldInstance FishingInstance { get; }

    private ServicesManager<WorldServices> services = new ServicesManager<WorldServices>(WorldServices.SAVE_CHARACTER);
    private MatchCheckerCoordinator matchChecker = new MatchCheckerCoordinator();
    private PartySearchCoordinator partySearch = new PartySearchCoordinator();

    private Dictionary<int, Storage> accountStorages = new();
    private object accountCharsLock = new object();

    private AtomicInteger runningPartyId = new AtomicInteger();
    private object partyLock = new object();

    private Dictionary<int, int> owlSearched = new();
    private List<Dictionary<int, int>> cashItemBought = new(9);
    private ReaderWriterLockSlim suggestLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    private object activePlayerShopsLock = new object();
    /// <summary>
    /// PlayerId - PlayerShop
    /// </summary>
    private Dictionary<int, PlayerShop> activePlayerShops = new();



    private ScheduledFuture? charactersSchedule;
    private ScheduledFuture? marriagesSchedule;
    private ScheduledFuture? mapOwnershipSchedule;
    private ScheduledFuture? fishingSchedule;
    private ScheduledFuture? partySearchSchedule;
    private ScheduledFuture? timeoutSchedule;

    public WorldConfigEntity Configs { get; set; }

    public World(WorldConfigEntity config)
    {
        log = LogFactory.GetLogger("World_" + Id);
        Channels = new List<IWorldChannel>();
        TeamStorage = new Dictionary<int, ITeam>();
        runningPartyId.set(1000000001); // partyid must not clash with charid to solve update item looting issues, found thanks to Vcoc
        runningMessengerId.set(1);
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
        this.EventMessage = config.EventMessage;

        var tman = TimerManager.getInstance();

        // charactersSchedule = tman.register(new CharacterAutosaverTask(this), TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        marriagesSchedule = tman.register(new WeddingReservationTask(this),
            TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL),
            TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL));
        mapOwnershipSchedule = tman.register(new MapOwnershipTask(this), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        fishingSchedule = tman.register(new FishingTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        partySearchSchedule = tman.register(new PartySearchTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        timeoutSchedule = tman.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

        FishingInstance = new FishingWorldInstance(this);

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

    public int addChannel(IWorldChannel channel)
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
                    target.sendPacket(PacketCreator.updateParty(target.Client.CurrentServer.getId(), party, operation, target));
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
                var messenger = targetChr.Messenger;
                if (messenger == null)
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
                if (InviteType.MESSENGER.AnswerInvite(player.getId(), senderChr.Messenger.getId(), false).Result == InviteResultType.DENIED)
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
            var cur = owlSearched.GetValueOrDefault(itemid);
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

    #region
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
    #endregion

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
            foreach (var hm in ch.HiredMerchantController.getActiveMerchants())
            {
                List<PlayerShopItem> itemBundles = hm.sendAvailableBundles(itemid);

                foreach (PlayerShopItem mpsi in itemBundles)
                {
                    hmsAvailable.Add(new(mpsi, hm));
                }
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


    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().GetAllOnlinedPlayers())
        {
            player.dropMessage(type, message);
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
        Players.disconnectAll();

        clearWorldData();
        log.Information("Finished shutting down world {WorldId}", Id);
    }
}
