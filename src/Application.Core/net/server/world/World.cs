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


using Application.Core.Managers;
using Application.Core.scripting.Event;
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
using static client.BuddyList;

/**
 * @author kevintjuh93
 * @author Ronan - thread-oriented (world schedules + guild queue + marriages + party chars)
 */
public class World
{
    private ILogger log;

    private int id;
    private int flag;
    private int exprate;
    private int droprate;
    private int bossdroprate;
    private int mesorate;
    private int questrate;
    private int travelrate;
    private int fishingrate;
    private string eventmsg;
    private List<Channel> channels = new();
    private Dictionary<int, byte> pnpcStep = new();
    private Dictionary<int, short> pnpcPodium = new();
    private Dictionary<int, Messenger> messengers = new();
    private AtomicInteger runningMessengerId = new AtomicInteger();
    private Dictionary<int, Family> families = new();
    private Dictionary<int, int> relationships = new();
    private Dictionary<int, KeyValuePair<int, int>?> relationshipCouples = new();
    private Dictionary<int, GuildSummary> gsStore = new();
    private PlayerStorage players = new PlayerStorage();
    private ServicesManager<WorldServices> services = new ServicesManager<WorldServices>(WorldServices.SAVE_CHARACTER);
    private MatchCheckerCoordinator matchChecker = new MatchCheckerCoordinator();
    private PartySearchCoordinator partySearch = new PartySearchCoordinator();

    private ReaderWriterLockSlim chnLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    private Dictionary<int, SortedDictionary<int, Character>> accountChars = new();
    private Dictionary<int, Storage> accountStorages = new();
    private object accountCharsLock = new object();

    private HashSet<int> queuedGuilds = new();
    private Dictionary<int, KeyValuePair<KeyValuePair<bool, bool>, KeyValuePair<int, int>>> queuedMarriages = new();
    private ConcurrentDictionary<int, HashSet<int>> marriageGuests = new();

    private Dictionary<int, int> partyChars = new();
    private Dictionary<int, Party> parties = new();
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
    private Dictionary<int, PlayerShop> activePlayerShops = new();

    private object activeMerchantsLock = new object();
    private Dictionary<int, KeyValuePair<HiredMerchant, int>> activeMerchants = new();
    private ScheduledFuture? merchantSchedule;
    private long merchantUpdate;

    private Dictionary<Action, long> registeredTimedMapObjects = new();
    private ScheduledFuture? timedMapObjectsSchedule;
    private object timedMapObjectLock = new object();

    private Dictionary<Character, int> fishingAttempters = new Dictionary<Character, int>();
    private Dictionary<Character, int> playerHpDec = new Dictionary<Character, int>();

    private ScheduledFuture? charactersSchedule;
    private ScheduledFuture? marriagesSchedule;
    private ScheduledFuture? mapOwnershipSchedule;
    private ScheduledFuture? fishingSchedule;
    private ScheduledFuture? partySearchSchedule;
    private ScheduledFuture? timeoutSchedule;
    private ScheduledFuture? hpDecSchedule;

    public World(int world, int flag, string eventmsg, int exprate, int droprate, int bossdroprate, int mesorate, int questrate, int travelrate, int fishingrate)
    {
        this.id = world;
        this.flag = flag;
        this.eventmsg = eventmsg;
        this.exprate = exprate;
        this.droprate = droprate;
        this.bossdroprate = bossdroprate;
        this.mesorate = mesorate;
        this.questrate = questrate;
        this.travelrate = travelrate;
        this.fishingrate = fishingrate;

        log = LogFactory.GetLogger("World_" + id);

        runningPartyId.set(1000000001); // partyid must not clash with charid to solve update item looting issues, found thanks to Vcoc
        runningMessengerId.set(1);

        petUpdate = Server.getInstance().getCurrentTime();
        mountUpdate = petUpdate;

        for (int i = 0; i < 9; i++)
        {
            cashItemBought.Add(new());
        }

        TimerManager tman = TimerManager.getInstance();
        petsSchedule = tman.register(new PetFullnessTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        srvMessagesSchedule = tman.register(new ServerMessageTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        mountsSchedule = tman.register(new MountTirednessTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        merchantSchedule = tman.register(new HiredMerchantTask(this), 10 * TimeSpan.FromMinutes(1), 10 * TimeSpan.FromMinutes(1));
        timedMapObjectsSchedule = tman.register(new TimedMapObjectTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        charactersSchedule = tman.register(new CharacterAutosaverTask(this), TimeSpan.FromHours(1), TimeSpan.FromHours(1));
        marriagesSchedule = tman.register(new WeddingReservationTask(this), TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL), TimeSpan.FromMinutes(YamlConfig.config.server.WEDDING_RESERVATION_INTERVAL));
        mapOwnershipSchedule = tman.register(new MapOwnershipTask(this), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(20));
        fishingSchedule = tman.register(new FishingTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        partySearchSchedule = tman.register(new PartySearchTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        timeoutSchedule = tman.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        hpDecSchedule = tman.register(new CharacterHpDecreaseTask(this), YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL, YamlConfig.config.server.MAP_DAMAGE_OVERTIME_INTERVAL);

        if (YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            var timeLeft = Server.getTimeLeftForNextDay();
            FamilyManager.resetEntitlementUsage(this);
            tman.register(new FamilyDailyResetTask(this), TimeSpan.FromDays(1), timeLeft);
        }
    }

    public int getChannelsSize()
    {
        chnLock.EnterReadLock();
        try
        {
            return channels.Count;
        }
        finally
        {
            chnLock.ExitReadLock();
        }
    }

    public List<Channel> getChannels()
    {
        chnLock.EnterReadLock();
        try
        {
            return new(channels);
        }
        finally
        {
            chnLock.ExitReadLock();
        }
    }

    public Channel getChannel(int channel)
    {
        chnLock.EnterReadLock();
        try
        {
            try
            {
                return channels.get(channel - 1);
            }
            catch (IndexOutOfRangeException e)
            {
                return null;
            }
        }
        finally
        {
            chnLock.ExitReadLock();
        }
    }

    public bool addChannel(Channel channel)
    {
        chnLock.EnterWriteLock();
        try
        {
            if (channel.getId() == channels.Count + 1)
            {
                channels.Add(channel);
                return true;
            }
            else
            {
                return false;
            }
        }
        finally
        {
            chnLock.ExitWriteLock();
        }
    }

    public int removeChannel()
    {
        Channel ch;
        int chIdx;

        chnLock.EnterReadLock();
        try
        {
            chIdx = channels.Count - 1;
            if (chIdx < 0)
            {
                return -1;
            }

            ch = channels.get(chIdx);
        }
        finally
        {
            chnLock.ExitReadLock();
        }

        if (ch == null || !ch.canUninstall())
        {
            return -1;
        }

        chnLock.EnterWriteLock();
        try
        {
            if (chIdx == channels.Count - 1)
            {
                channels.RemoveAt(chIdx);
            }
            else
            {
                return -1;
            }
        }
        finally
        {
            chnLock.ExitWriteLock();
        }

        ch.shutdown();
        return ch.getId();
    }

    public bool canUninstall()
    {
        if (players.getSize() > 0) return false;

        foreach (Channel ch in this.getChannels())
        {
            if (!ch.canUninstall())
            {
                return false;
            }
        }

        return true;
    }

    public void setFlag(byte b)
    {
        this.flag = b;
    }

    public int getFlag()
    {
        return flag;
    }

    public string getEventMessage()
    {
        return eventmsg;
    }

    public int getExpRate()
    {
        return exprate;
    }

    public void setExpRate(int exp)
    {
        var list = getPlayerStorage().getAllCharacters();

        foreach (Character chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.revertWorldRates();
        }
        this.exprate = exp;
        foreach (Character chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.setWorldRates();
        }
    }

    public int getDropRate()
    {
        return droprate;
    }

    public void setDropRate(int drop)
    {
        var list = getPlayerStorage().getAllCharacters();

        foreach (Character chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.revertWorldRates();
        }
        this.droprate = drop;
        foreach (Character chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.setWorldRates();
        }
    }

    public int getBossDropRate()
    {  // boss rate concept thanks to Lapeiro
        return bossdroprate;
    }

    public void setBossDropRate(int bossdrop)
    {
        bossdroprate = bossdrop;
    }

    public int getMesoRate()
    {
        return mesorate;
    }

    public void setMesoRate(int meso)
    {
        var list = getPlayerStorage().getAllCharacters();

        foreach (Character chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.revertWorldRates();
        }
        this.mesorate = meso;
        foreach (Character chr in list)
        {
            if (!chr.isLoggedin())
            {
                continue;
            }
            chr.setWorldRates();
        }
    }

    public int getQuestRate()
    {
        return questrate;
    }

    public void setQuestRate(int quest)
    {
        this.questrate = quest;
    }

    public int getTravelRate()
    {
        return travelrate;
    }

    public void setTravelRate(int travel)
    {
        this.travelrate = travel;
    }

    public int getTransportationTime(int travelTime)
    {
        return (int)Math.Ceiling((double)travelTime / travelrate);
    }

    public int getFishingRate()
    {
        return fishingrate;
    }

    public void setFishingRate(int quest)
    {
        this.fishingrate = quest;
    }

    public void loadAccountCharactersView(int accountId, List<Character> chars)
    {
        SortedDictionary<int, Character> charsMap = new();
        foreach (Character chr in chars)
        {
            charsMap.AddOrUpdate(chr.getId(), chr);
        }

        Monitor.Enter(accountCharsLock);    // accountCharsLock should be used after server's lgnWLock for compliance
        try
        {
            accountChars.AddOrUpdate(accountId, charsMap);
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }
    }

    public void registerAccountCharacterView(int accountId, Character chr)
    {
        Monitor.Enter(accountCharsLock);
        try
        {
            if (accountChars.ContainsKey(accountId))
                accountChars[accountId].AddOrUpdate(chr.getId(), chr);
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }
    }

    public void unregisterAccountCharacterView(int accountId, int chrId)
    {
        Monitor.Enter(accountCharsLock);
        try
        {
            if (accountChars.ContainsKey(accountId))
                accountChars[accountId].Remove(chrId);
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }
    }

    public void clearAccountCharacterView(int accountId)
    {
        Monitor.Enter(accountCharsLock);
        try
        {
            accountChars.Remove(accountId, out var accChars);
            if (accChars != null)
            {
                accChars.Clear();
            }
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }
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
        Storage storage = Storage.loadOrCreateFromDB(accountId, this.id);
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

    public Storage? getAccountStorage(int accountId)
    {
        return accountStorages.GetValueOrDefault(accountId);
    }

    private static List<KeyValuePair<int, SortedDictionary<int, Character>>> getSortedAccountCharacterView(Dictionary<int, SortedDictionary<int, Character>> map)
    {
        return map.OrderBy(x => x.Key).ToList(); ;
    }

    public List<Character> loadAndGetAllCharactersView()
    {
        Server.getInstance().loadAllAccountsCharactersView();
        return getAllCharactersView();
    }

    public List<Character> getAllCharactersView()
    {    // sorting by accountid, charid
        List<Character> chrList = new();
        Dictionary<int, SortedDictionary<int, Character>> accChars;

        Monitor.Enter(accountCharsLock);
        try
        {
            accChars = new(accountChars);
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }

        foreach (var e in getSortedAccountCharacterView(accChars))
        {
            chrList.AddRange(e.Value.Values);
        }

        return chrList;
    }

    public List<Character>? getAccountCharactersView(int accountId)
    {
        List<Character> chrList;

        Monitor.Enter(accountCharsLock);
        try
        {
            SortedDictionary<int, Character>? accChars = accountChars.GetValueOrDefault(accountId);

            if (accChars != null)
            {
                chrList = new(accChars.Values);
            }
            else
            {
                accountChars.AddOrUpdate(accountId, new());
                chrList = null;
            }
        }
        finally
        {
            Monitor.Exit(accountCharsLock);
        }

        return chrList;
    }

    public PlayerStorage getPlayerStorage()
    {
        return players;
    }

    public MatchCheckerCoordinator getMatchCheckerCoordinator()
    {
        return matchChecker;
    }

    public PartySearchCoordinator getPartySearchCoordinator()
    {
        return partySearch;
    }

    public void addPlayer(Character chr)
    {
        players.addPlayer(chr);
    }

    public void removePlayer(Character chr)
    {
        Channel cserv = chr.getClient().getChannelServer();

        if (cserv != null)
        {
            if (!cserv.removePlayer(chr))
            {
                // oy the player is not where they should be, find this mf

                foreach (Channel ch in getChannels())
                {
                    if (ch.removePlayer(chr))
                    {
                        break;
                    }
                }
            }
        }

        players.removePlayer(chr.getId());
    }

    public int getId()
    {
        return id;
    }

    public void addFamily(int id, Family f)
    {
        lock (families)
        {
            if (!families.ContainsKey(id))
            {
                families.Add(id, f);
            }
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

    public Guild? getGuild(GuildCharacter? mgc)
    {
        if (mgc == null)
        {
            return null;
        }

        int gid = mgc.getGuildId();
        var g = Server.getInstance().getGuild(gid, mgc.getWorld(), mgc.getCharacter());
        if (gsStore.GetValueOrDefault(gid) == null)
        {
            gsStore.AddOrUpdate(gid, new GuildSummary(g));
        }
        return g;
    }

    public bool isWorldCapacityFull()
    {
        return getWorldCapacityStatus() == 2;
    }

    public int getWorldCapacityStatus()
    {
        int worldCap = getChannelsSize() * YamlConfig.config.server.CHANNEL_LOAD;
        int num = players.getSize();

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

    public GuildSummary? getGuildSummary(int gid, int wid)
    {
        if (gsStore.ContainsKey(gid))
        {
            return gsStore[gid];
        }
        else
        {
            var g = Server.getInstance().getGuild(gid, wid, null);
            if (g != null)
            {
                gsStore.AddOrUpdate(gid, new GuildSummary(g));
            }
            return gsStore.GetValueOrDefault(gid);
        }
    }

    public void updateGuildSummary(int gid, GuildSummary mgs)
    {
        gsStore.AddOrUpdate(gid, mgs);
    }

    public void reloadGuildSummary()
    {
        Guild? g;
        Server server = Server.getInstance();
        foreach (int i in gsStore.Keys)
        {
            g = server.getGuild(i, getId(), null);
            if (g != null)
            {
                gsStore.AddOrUpdate(i, new GuildSummary(g));
            }
            else
            {
                gsStore.Remove(i);
            }
        }
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
        if (mc == null)
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
            bDifferentGuild = guildid != mc.getGuildId();
            mc.getMGC().setGuildId(guildid);
            mc.getMGC().setGuildRank(rank);

            if (bDifferentGuild)
            {
                mc.getMGC().setAllianceRank(5);
            }

            mc.saveGuildStatus();
        }
        if (bDifferentGuild)
        {
            if (mc.isLoggedinWorld())
            {
                var guild = Server.getInstance().getGuild(guildid);
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

    public void changeEmblem(int gid, List<int> affectedPlayers, GuildSummary mgs)
    {
        updateGuildSummary(gid, mgs);
        sendPacket(affectedPlayers, GuildPackets.guildEmblemChange(gid, mgs.getLogoBG(), mgs.getLogoBGColor(), mgs.getLogo(), mgs.getLogoColor()), -1);
        setGuildAndRank(affectedPlayers, -1, -1, -1);    //respawn player
    }

    public void sendPacket(List<int> targetIds, Packet packet, int exception)
    {
        Character? chr;
        foreach (int i in targetIds)
        {
            if (i == exception)
            {
                continue;
            }
            chr = getPlayerStorage().getCharacterById(i);
            if (chr != null)
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

    public KeyValuePair<int, int>? getMarriageQueuedCouple(int marriageId)
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

    public KeyValuePair<int, int>? getWeddingCoupleForGuest(int guestId, bool cathedral)
    {
        foreach (Channel ch in getChannels())
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
                foreach (Channel ch in getChannels())
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

    private void registerCharacterParty(int chrid, int partyid)
    {
        Monitor.Enter(partyLock);
        try
        {
            partyChars.AddOrUpdate(chrid, partyid);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    private void unregisterCharacterPartyInternal(int chrid)
    {
        partyChars.Remove(chrid);
    }

    private void unregisterCharacterParty(int chrid)
    {
        Monitor.Enter(partyLock);
        try
        {
            unregisterCharacterPartyInternal(chrid);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    public int getCharacterPartyid(int chrid)
    {
        Monitor.Enter(partyLock);
        try
        {
            return partyChars.GetValueOrDefault(chrid);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    public Party createParty(PartyCharacter chrfor)
    {
        int partyid = runningPartyId.getAndIncrement();
        Party party = new Party(partyid, chrfor);

        Monitor.Enter(partyLock);
        try
        {
            parties.AddOrUpdate(party.getId(), party);
            registerCharacterParty(chrfor.getId(), partyid);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }

        party.addMember(chrfor);
        return party;
    }

    public Party? getParty(int partyid)
    {
        Monitor.Enter(partyLock);
        try
        {
            return parties.GetValueOrDefault(partyid);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    private Party disbandParty(int partyid)
    {
        Monitor.Enter(partyLock);
        try
        {
            parties.Remove(partyid, out var d);
            return d;
        }
        finally
        {
            Monitor.Exit(partyLock);
        }
    }

    private void updateCharacterParty(Party party, PartyOperation operation, PartyCharacter target, ICollection<PartyCharacter> partyMembers)
    {
        switch (operation)
        {
            case PartyOperation.JOIN:
                registerCharacterParty(target.getId(), party.getId());
                break;

            case PartyOperation.LEAVE:
            case PartyOperation.EXPEL:
                unregisterCharacterParty(target.getId());
                break;

            case PartyOperation.DISBAND:
                Monitor.Enter(partyLock);
                try
                {
                    foreach (PartyCharacter partychar in partyMembers)
                    {
                        unregisterCharacterPartyInternal(partychar.getId());
                    }
                }
                finally
                {
                    Monitor.Exit(partyLock);
                }
                break;

            default:
                break;
        }
    }

    private void updateParty(Party party, PartyOperation operation, PartyCharacter target)
    {
        var partyMembers = party.getMembers();
        updateCharacterParty(party, operation, target, partyMembers);

        foreach (PartyCharacter partychar in partyMembers)
        {
            var chr = getPlayerStorage().getCharacterById(partychar.getId());
            if (chr != null)
            {
                if (operation == PartyOperation.DISBAND)
                {
                    chr.setParty(null);
                    chr.setMPC(null);
                }
                else
                {
                    chr.setParty(party);
                    chr.setMPC(partychar);
                }
                chr.sendPacket(PacketCreator.updateParty(chr.getClient().getChannel(), party, operation, target));
            }
        }
        switch (operation)
        {
            case PartyOperation.LEAVE:
            case PartyOperation.EXPEL:
                var chr = getPlayerStorage().getCharacterById(target.getId());
                if (chr != null)
                {
                    chr.sendPacket(PacketCreator.updateParty(chr.getClient().getChannel(), party, operation, target));
                    chr.setParty(null);
                    chr.setMPC(null);
                }
                break;
            default:
                break;
        }
    }

    public void updateParty(int partyid, PartyOperation operation, PartyCharacter target)
    {
        Party? party = getParty(partyid);
        if (party == null)
        {
            throw new ArgumentException("no party with the specified partyid exists");
        }
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
                Character mc = party.getLeader().getPlayer();
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

        foreach (PartyCharacter mpc in party.getMembers())
        {
            Character mc = mpc.getPlayer();
            if (mc != null)
            {
                MapleMap map = mc.getMap();
                if (map != null)
                {
                    map.removeParty(partyid);
                }
            }
        }
    }

    public int find(string name)
    {
        int channel = -1;
        Character? chr = getPlayerStorage().getCharacterByName(name);
        if (chr != null)
        {
            channel = chr.getClient().getChannel();
        }
        return channel;
    }

    public int find(int id)
    {
        int channel = -1;
        Character? chr = getPlayerStorage().getCharacterById(id);
        if (chr != null)
        {
            channel = chr.getClient().getChannel();
        }
        return channel;
    }

    public void partyChat(Party party, string chattext, string namefrom)
    {
        foreach (PartyCharacter partychar in party.getMembers())
        {
            if (!(partychar.getName().Equals(namefrom)))
            {
                Character? chr = getPlayerStorage().getCharacterByName(partychar.getName());
                if (chr != null)
                {
                    chr.sendPacket(PacketCreator.multiChat(namefrom, chattext, 1));
                }
            }
        }
    }

    public void buddyChat(int[] recipientCharacterIds, int cidFrom, string nameFrom, string chattext)
    {
        PlayerStorage playerStorage = getPlayerStorage();
        foreach (int characterId in recipientCharacterIds)
        {
            Character? chr = playerStorage.getCharacterById(characterId);
            if (chr != null)
            {
                if (chr.getBuddylist().containsVisible(cidFrom))
                {
                    chr.sendPacket(PacketCreator.multiChat(nameFrom, chattext, 0));
                }
            }
        }
    }

    public CharacterIdChannelPair[] multiBuddyFind(int charIdFrom, int[] characterIds)
    {
        List<CharacterIdChannelPair> foundsChars = new(characterIds.Length);
        foreach (Channel ch in getChannels())
        {
            foreach (int charid in ch.multiBuddyFind(charIdFrom, characterIds))
            {
                foundsChars.Add(new CharacterIdChannelPair(charid, ch.getId()));
            }
        }
        return foundsChars.ToArray();
    }

    public Messenger? getMessenger(int messengerid)
    {
        return messengers.GetValueOrDefault(messengerid);
    }

    public void leaveMessenger(int messengerid, MessengerCharacter target)
    {
        var messenger = getMessenger(messengerid);
        if (messenger == null)
        {
            throw new ArgumentException("No messenger with the specified messengerid exists");
        }
        int position = messenger.getPositionByName(target.getName());
        messenger.removeMember(target);
        removeMessengerPlayer(messenger, position);
    }

    public void messengerInvite(string sender, int messengerid, string target, int fromchannel)
    {
        if (isConnected(target))
        {
            var targetChr = getPlayerStorage().getCharacterByName(target);
            if (targetChr != null)
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
        foreach (MessengerCharacter messengerchar in messenger.getMembers())
        {
            var chr = getPlayerStorage().getCharacterByName(messengerchar.getName());
            if (chr == null)
            {
                continue;
            }
            if (!messengerchar.getName().Equals(namefrom))
            {
                var from = getChannel(fromchannel).getPlayerStorage().getCharacterByName(namefrom);
                chr.sendPacket(PacketCreator.addMessengerPlayer(namefrom, from, position, (byte)(fromchannel - 1)));
                from.sendPacket(PacketCreator.addMessengerPlayer(chr.getName(), chr, messengerchar.getPosition(), (byte)(messengerchar.getChannel() - 1)));
            }
            else
            {
                chr.sendPacket(PacketCreator.joinMessenger(messengerchar.getPosition()));
            }
        }
    }

    public void removeMessengerPlayer(Messenger messenger, int position)
    {
        foreach (MessengerCharacter messengerchar in messenger.getMembers())
        {
            var chr = getPlayerStorage().getCharacterByName(messengerchar.getName());
            if (chr != null)
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
        foreach (MessengerCharacter messengerchar in messenger.getMembers())
        {
            if (!(messengerchar.getName().Equals(namefrom)))
            {
                var chr = getPlayerStorage().getCharacterByName(messengerchar.getName());
                if (chr != null)
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

    public void declineChat(string sender, Character player)
    {
        if (isConnected(sender))
        {
            var senderChr = getPlayerStorage().getCharacterByName(sender);
            if (senderChr != null && senderChr.getMessenger() != null)
            {
                if (InviteCoordinator.answerInvite(InviteType.MESSENGER, player.getId(), senderChr.getMessenger().getId(), false).result == InviteResultType.DENIED)
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
        foreach (MessengerCharacter messengerchar in messenger.getMembers())
        {
            Channel ch = getChannel(fromchannel);
            if (!(messengerchar.getName().Equals(namefrom)))
            {
                var chr = ch.getPlayerStorage().getCharacterByName(messengerchar.getName());
                if (chr != null)
                {
                    chr.sendPacket(PacketCreator.updateMessengerPlayer(namefrom, getChannel(fromchannel).getPlayerStorage().getCharacterByName(namefrom), position, (byte)(fromchannel - 1)));
                }
            }
        }
    }

    public void silentLeaveMessenger(int messengerid, MessengerCharacter target)
    {
        var messenger = getMessenger(messengerid);
        if (messenger == null)
        {
            throw new ArgumentException("No messenger with the specified messengerid exists");
        }
        messenger.addMember(target, target.getPosition());
    }

    public void joinMessenger(int messengerid, MessengerCharacter target, string from, int fromchannel)
    {
        var messenger = getMessenger(messengerid);
        if (messenger == null)
        {
            throw new ArgumentException("No messenger with the specified messengerid exists");
        }
        messenger.addMember(target, target.getPosition());
        addMessengerPlayer(messenger, from, fromchannel, target.getPosition());
    }

    public void silentJoinMessenger(int messengerid, MessengerCharacter target, int position)
    {
        var messenger = getMessenger(messengerid);
        if (messenger == null)
        {
            throw new ArgumentException("No messenger with the specified messengerid exists");
        }
        messenger.addMember(target, position);
    }

    public Messenger createMessenger(MessengerCharacter chrfor)
    {
        int messengerid = runningMessengerId.getAndIncrement();
        Messenger messenger = new Messenger(messengerid, chrfor);
        messengers.AddOrUpdate(messenger.getId(), messenger);
        return messenger;
    }

    public bool isConnected(string charName)
    {
        return getPlayerStorage().getCharacterByName(charName) != null;
    }

    public BuddyAddResult requestBuddyAdd(string addName, int channelFrom, int cidFrom, string nameFrom)
    {
        var addChar = getPlayerStorage().getCharacterByName(addName);
        if (addChar != null)
        {
            BuddyList buddylist = addChar.getBuddylist();
            if (buddylist.isFull())
            {
                return BuddyAddResult.BUDDYLIST_FULL;
            }
            if (!buddylist.contains(cidFrom))
            {
                buddylist.addBuddyRequest(addChar.getClient(), cidFrom, nameFrom, channelFrom);
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
        if (addChar != null)
        {
            BuddyList buddylist = addChar.getBuddylist();
            switch (operation)
            {
                case BuddyOperation.ADDED:
                    if (buddylist.contains(cidFrom))
                    {
                        buddylist.put(new BuddylistEntry(name, "Default Group", cidFrom, channel, true));
                        addChar.sendPacket(PacketCreator.updateBuddyChannel(cidFrom, (channel - 1)));
                    }
                    break;
                case BuddyOperation.DELETED:
                    if (buddylist.contains(cidFrom))
                    {
                        buddylist.put(new BuddylistEntry(name, "Default Group", cidFrom, -1, buddylist.get(cidFrom).isVisible()));
                        addChar.sendPacket(PacketCreator.updateBuddyChannel(cidFrom, -1));
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
        PlayerStorage playerStorage = getPlayerStorage();
        foreach (int buddy in buddies)
        {
            var chr = playerStorage.getCharacterById(buddy);
            if (chr != null)
            {
                var ble = chr.getBuddylist().get(characterId);
                if (ble != null && ble.isVisible())
                {
                    int mcChannel;
                    if (offline)
                    {
                        ble.setChannel(-1);
                        mcChannel = -1;
                    }
                    else
                    {
                        ble.setChannel(channel);
                        mcChannel = (byte)(channel - 1);
                    }
                    chr.getBuddylist().put(ble);
                    chr.sendPacket(PacketCreator.updateBuddyChannel(ble.getCharacterId(), mcChannel));
                }
            }
        }
    }

    private static int getPetKey(Character chr, sbyte petSlot)
    {    // assuming max 3 pets
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

            var cur = tabItemBought.get(snid) ?? 0;
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

    public void registerPetHunger(Character chr, sbyte petSlot)
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

    public void unregisterPetHunger(Character chr, sbyte petSlot)
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

    public void registerMountHunger(Character chr)
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

    public void unregisterMountHunger(Character chr)
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
                {   // 1440 minutes == 24hrs
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

    public void addPlayerHpDecrease(Character chr)
    {
        playerHpDec.TryAdd(chr, 0);
    }

    public void removePlayerHpDecrease(Character chr)
    {
        playerHpDec.Remove(chr);
    }

    public void runPlayerHpDecreaseSchedule()
    {
        Dictionary<Character, int> m = new();
        m.putAll(playerHpDec);

        foreach (var e in m)
        {
            Character chr = e.Key;

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
                {   // ~35sec duration, 10sec update
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
                var chr = players.getCharacterById(chrid);

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
            dbContext.PlayernpcsFields.Where(x => x.World == worldid && x.Map == mapid).ExecuteUpdate(x => isPodium ? x.SetProperty(y => y.Podium, value) : x.SetProperty(y => y.Step, value));
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
                    executePlayerNpcMapDataUpdate(dbContext, false, pnpcStep, (short)step, id, mapid);
                }

                if (podium != -1)
                {
                    executePlayerNpcMapDataUpdate(dbContext, true, pnpcPodium, (short)podium, id, mapid);
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

    public void setServerMessage(string msg)
    {
        foreach (Channel ch in getChannels())
        {
            ch.setServerMessage(msg);
        }
    }

    public void broadcastPacket(Packet packet)
    {
        foreach (Character chr in players.getAllCharacters())
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

    private void pushRelationshipCouple(KeyValuePair<int, KeyValuePair<int, int>> couple)
    {
        int mid = couple.Key, hid = couple.Value.Key, wid = couple.Value.Value;
        relationshipCouples.AddOrUpdate(mid, couple.Value);
        relationships.AddOrUpdate(hid, mid);
        relationships.AddOrUpdate(wid, mid);
    }

    public KeyValuePair<int, int>? getRelationshipCouple(int relationshipId)
    {
        var rc = relationshipCouples.GetValueOrDefault(relationshipId);

        if (rc == null)
        {
            var couple = getRelationshipCoupleFromDb(relationshipId, true);
            if (couple == null)
            {
                return null;
            }

            pushRelationshipCouple(couple.Value);
            rc = couple.Value.Value;
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

            pushRelationshipCouple(couple.Value);
            return couple.Value.Key;
        }

        return relationships[playerId];
    }

    private KeyValuePair<int, KeyValuePair<int, int>>? getRelationshipCoupleFromDb(int id, bool usingMarriageId)
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

            return new KeyValuePair<int, KeyValuePair<int, int>>(model.Marriageid, new KeyValuePair<int, int>(model.Husbandid, model.Wifeid));
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

        pushRelationshipCouple(new(ret, new(groomId, brideId)));
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
        foreach (Character player in getPlayerStorage().getAllCharacters())
        {
            player.dropMessage(type, message);
        }
    }

    public bool registerFisherPlayer(Character chr, int baitLevel)
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

    public int unregisterFisherPlayer(Character chr)
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
            List<Character> fishingAttemptersList;

            lock (fishingAttempters)
            {
                fishingAttemptersList = new(fishingAttempters.Keys);
            }

            foreach (Character chr in fishingAttemptersList)
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
        List<Party> pList;
        Monitor.Enter(partyLock);
        try
        {
            pList = new(parties.Values);
        }
        finally
        {
            Monitor.Exit(partyLock);
        }

        closeWorldServices();
    }

    public void shutdown()
    {
        foreach (Channel ch in getChannels())
        {
            ch.shutdown();
        }

        if (petsSchedule != null)
        {
            petsSchedule.cancel(false);
            petsSchedule = null;
        }

        if (srvMessagesSchedule != null)
        {
            srvMessagesSchedule.cancel(false);
            srvMessagesSchedule = null;
        }

        if (mountsSchedule != null)
        {
            mountsSchedule.cancel(false);
            mountsSchedule = null;
        }

        if (merchantSchedule != null)
        {
            merchantSchedule.cancel(false);
            merchantSchedule = null;
        }

        if (timedMapObjectsSchedule != null)
        {
            timedMapObjectsSchedule.cancel(false);
            timedMapObjectsSchedule = null;
        }

        if (charactersSchedule != null)
        {
            charactersSchedule.cancel(false);
            charactersSchedule = null;
        }

        if (marriagesSchedule != null)
        {
            marriagesSchedule.cancel(false);
            marriagesSchedule = null;
        }

        if (mapOwnershipSchedule != null)
        {
            mapOwnershipSchedule.cancel(false);
            mapOwnershipSchedule = null;
        }

        if (fishingSchedule != null)
        {
            fishingSchedule.cancel(false);
            fishingSchedule = null;
        }

        if (partySearchSchedule != null)
        {
            partySearchSchedule.cancel(false);
            partySearchSchedule = null;
        }

        if (timeoutSchedule != null)
        {
            timeoutSchedule.cancel(false);
            timeoutSchedule = null;
        }

        if (hpDecSchedule != null)
        {
            hpDecSchedule.cancel(false);
            hpDecSchedule = null;
        }

        players.disconnectAll();
        players = null;

        clearWorldData();
        log.Information("Finished shutting down world {WorldId}", id);
    }
}
