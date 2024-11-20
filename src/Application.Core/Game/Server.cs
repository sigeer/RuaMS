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


using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Commands;
using Application.Core.Game.Life;
using Application.Core.Game.Skills;
using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using Application.Core.model;
using Application.Utility;
using client;
using client.inventory.manipulator;
using client.newyear;
using client.processor.npc;
using constants.inventory;
using constants.net;
using database.note;
using Microsoft.EntityFrameworkCore;
using net.netty;
using net.packet;
using net.server.channel;
using net.server.coordinator.session;
using net.server.task;
using server;
using server.expeditions;
using server.quest;
using service;
using System.Diagnostics;
using System.Text.RegularExpressions;
using static server.CashShop;

namespace net.server;

public class Server
{
    private static ILogger log = LogFactory.GetLogger(LogType.Server);
    private static Lazy<Server> instance = new Lazy<Server>(new Server());

    public static Server getInstance() => instance.Value;

    private static HashSet<int> activeFly = new();

    private static Dictionary<int, int> couponRates = new(30);
    private static List<int> activeCoupons = new();

    private static ChannelDependencies channelDependencies;

    private LoginServer loginServer = null!;
    public Dictionary<int, IWorld> RunningWorlds { get; set; } = new();

    /// <summary>
    /// AccountId - cid
    /// </summary>
    private Dictionary<int, HashSet<int>> AccountCharacterCache = new();

    private Dictionary<string, int> transitioningChars = new();

    private Dictionary<IClient, DateTimeOffset> inLoginState = new(100);

    private PlayerBuffStorage buffStorage = new PlayerBuffStorage();

    private Dictionary<int, NewYearCardRecord> newyears = new();
    private List<IClient> processDiseaseAnnouncePlayers = new();
    private List<IClient> registeredDiseaseAnnouncePlayers = new();

    /// <summary>
    /// World - Data
    /// </summary>
    private Dictionary<int, List<RankedCharacterInfo>> playerRanking = new();

    private object srvLock = new object();
    private object disLock = new object();

    private AtomicLong currentTime = new AtomicLong(0);
    private long serverCurrentTime = 0;

    private volatile bool availableDeveloperRoom = false;
    private bool online = false;
    public static DateTimeOffset uptime = DateTimeOffset.Now;
    ReaderWriterLockSlim wldLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    ReaderWriterLockSlim lgnLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Server()
    {
    }

    public int getCurrentTimestamp()
    {
        return (int)(getCurrentTime() - Server.uptime.ToUnixTimeMilliseconds());
    }

    public long getCurrentTime()
    {  // returns a slightly delayed time value, under frequency of UPDATE_INTERVAL
        return serverCurrentTime;
    }

    public void updateCurrentTime()
    {
        serverCurrentTime = currentTime.addAndGet(YamlConfig.config.server.UPDATE_INTERVAL);
    }

    public long forceUpdateCurrentTime()
    {
        long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        serverCurrentTime = timeNow;
        currentTime.set(timeNow);

        return timeNow;
    }

    public bool isOnline()
    {
        return online;
    }

    public void setNewYearCard(NewYearCardRecord nyc)
    {
        newyears.AddOrUpdate(nyc.getId(), nyc);
    }

    public NewYearCardRecord? getNewYearCard(int cardid)
    {
        return newyears.GetValueOrDefault(cardid);
    }

    public NewYearCardRecord? removeNewYearCard(int cardid)
    {
        if (newyears.Remove(cardid, out var d))
            return d;
        return null;
    }

    public void setAvailableDeveloperRoom()
    {
        availableDeveloperRoom = true;
    }

    public bool canEnterDeveloperRoom()
    {
        return availableDeveloperRoom;
    }

    private void loadPlayerNpcMapStepFromDb()
    {
        var wlist = this.getWorlds();

        using var dbContext = new DBContext();
        var list = dbContext.PlayernpcsFields.AsNoTracking().ToList();
        list.ForEach(rs =>
        {
            var w = wlist.FirstOrDefault(x => x.getId() == rs.World);
            if (w != null)
                w.setPlayerNpcMapData(rs.Map, rs.Step, rs.Podium);
        });
    }

    public IWorld getWorld(int id)
    {
        wldLock.EnterReadLock();
        try
        {
            return RunningWorlds.GetValueOrDefault(id) ?? throw new BusinessException($"World {id} not exsited");
        }
        finally
        {
            wldLock.ExitReadLock();
        }
    }

    public List<IWorld> getWorlds()
    {
        wldLock.EnterReadLock();
        try
        {
            return RunningWorlds.Values.ToList();
        }
        finally
        {
            wldLock.ExitReadLock();
        }
    }

    public int getWorldsSize()
    {
        wldLock.EnterReadLock();
        try
        {
            return RunningWorlds.Count;
        }
        finally
        {
            wldLock.ExitReadLock();
        }
    }

    public IWorldChannel getChannel(int world, int channel)
    {
        return this.getWorld(world).getChannel(channel);
    }

    public List<IWorldChannel> getChannelsFromWorld(int world)
    {
        return this.getWorld(world).getChannels();
    }

    public List<IWorldChannel> getAllChannels()
    {
        try
        {
            return RunningWorlds.Values.SelectMany(x => x.Channels).ToList();
        }
        catch (NullReferenceException)
        {
            return new(0);
        }
    }

    public HashSet<int> getOpenChannels(int world)
    {
        wldLock.EnterReadLock();
        try
        {
            return getWorld(world).getChannels().Select(x => x.getId()).ToHashSet();
        }
        finally
        {
            wldLock.ExitReadLock();
        }
    }

    private string? getIP(int world, int channel)
    {
        wldLock.EnterReadLock();
        try
        {
            return getWorld(world).getChannel(channel).getIP();
        }
        finally
        {
            wldLock.ExitReadLock();
        }
    }

    public string[] getInetSocket(IClient client, int world, int channel)
    {
        string remoteIp = client.getRemoteAddress();

        string[] hostAddress = getIP(world, channel).Split(":");
        if (IpAddresses.isLocalAddress(remoteIp))
        {
            hostAddress[0] = YamlConfig.config.server.LOCALHOST;
        }
        else if (IpAddresses.isLanAddress(remoteIp))
        {
            hostAddress[0] = YamlConfig.config.server.LANHOST;
        }

        return hostAddress;
    }

    public int addChannel(int worldid)
    {
        IWorld? world;
        int channelid;

        wldLock.EnterReadLock();
        try
        {
            world = this.getWorld(worldid);

            if (worldid >= RunningWorlds.Count)
            {
                return -3;
            }


            channelid = world.Channels.Count;
            if (channelid >= YamlConfig.config.server.CHANNEL_SIZE)
            {
                return -2;
            }

            channelid++;
            world = this.getWorld(worldid);
        }
        finally
        {
            wldLock.ExitReadLock();
        }

        IWorldChannel channel = new WorldChannel(world, channelid, getCurrentTime());
        channel.setServerMessage(world.ServerMessage);

        world.addChannel(channel);
        return channelid;
    }

    public bool AddWorld(WorldConfigEntity worldConfig)
    {
        if (InitWorld(worldConfig))
        {
            HashSet<int> accounts;
            lgnLock.EnterReadLock();
            try
            {
                accounts = new(AccountCharacterCache.Keys);
            }
            finally
            {
                lgnLock.ExitReadLock();
            }

            foreach (int accId in accounts)
            {
                LoadAccountCharactersView(accId);
            }
            return true;
        }

        return false;
    }

    public bool InitWorld(WorldConfigEntity worldConfig)
    {
        if (!worldConfig.CanDeploy)
        {
            log.Information("初始化 {WorldId} 失败: 未启用 或者 未设置端口", worldConfig.Id);
            return false;
        }

        if (RunningWorlds.ContainsKey(worldConfig.Id))
            return false;

        log.Information("Starting world {WorldId}", worldConfig.Id);

        var world = new World(worldConfig);

        long bootTime = getCurrentTime();
        for (int j = 1; j <= worldConfig.ChannelCount; j++)
        {
            int channelid = j;
            var channel = new WorldChannel(world, channelid, bootTime);

            world.addChannel(channel);
        }

        wldLock.EnterWriteLock();    // thanks Ashen for noticing a deadlock issue when trying to deploy a channel
        try
        {
            RunningWorlds[worldConfig.Id] = world;
        }
        finally
        {
            wldLock.ExitWriteLock();
        }
        log.Information("Finished loading world {WorldId}", world.Id);
        return true;
    }

    public bool removeChannel(int worldid)
    {
        //lol don't!
        IWorld? world;

        wldLock.EnterReadLock();
        try
        {
            if (worldid >= RunningWorlds.Count)
            {
                return false;
            }
            world = RunningWorlds.GetValueOrDefault(worldid);
        }
        finally
        {
            wldLock.ExitReadLock();
        }

        if (world != null)
        {
            int channel = world.removeChannel();
            return channel > -1;
        }

        return false;
    }

    public bool RemoveWorld(int worldId)
    {
        //lol don't!
        IWorld? w;
        wldLock.EnterReadLock();
        try
        {
            w = RunningWorlds.GetValueOrDefault(worldId);
        }
        finally
        {
            wldLock.ExitReadLock();
        }

        if (w == null || !w.canUninstall())
        {
            return false;
        }

        LoadPlayerRanking();
        w.shutdown();

        wldLock.EnterWriteLock();
        try
        {
            RunningWorlds.Remove(worldId);
        }
        finally
        {
            wldLock.ExitWriteLock();
        }

        return true;
    }

    private void resetServerWorlds()
    {
        // thanks maple006 for noticing proprietary lists assigned to null
        wldLock.EnterWriteLock();
        try
        {
            RunningWorlds.Clear();
        }
        finally
        {
            wldLock.ExitWriteLock();
        }
    }

    #region coupon
    public Dictionary<int, int> getCouponRates()
    {
        return couponRates;
    }

    public static void cleanNxcodeCoupons(DBContext dbContext)
    {
        if (!YamlConfig.config.server.USE_CLEAR_OUTDATED_COUPONS)
        {
            return;
        }

        long timeClear = DateTimeOffset.Now.AddDays(-14).ToUnixTimeMilliseconds();

        using var dbTrans = dbContext.Database.BeginTransaction();
        var codeList = dbContext.Nxcodes.Where(x => x.Expiration <= timeClear).ToList();
        var codeIdList = codeList.Select(x => x.Id).ToList();
        dbContext.NxcodeItems.Where(x => codeIdList.Contains(x.Codeid)).ExecuteDelete();
        dbContext.Nxcodes.RemoveRange(codeList);
        dbContext.SaveChanges();
        dbTrans.Commit();
    }

    private void loadCouponRates(DBContext dbContext)
    {
        var list = dbContext.Nxcoupons.AsNoTracking().ToList();
        list.ForEach(rs =>
        {
            couponRates.AddOrUpdate(rs.CouponId, rs.Rate);
        });
    }

    public List<int> getActiveCoupons()
    {
        lock (activeCoupons)
        {
            return activeCoupons;
        }
    }

    public void commitActiveCoupons()
    {
        foreach (var world in getWorlds())
        {
            foreach (var chr in world.getPlayerStorage().GetAllOnlinedPlayers())
            {
                if (!chr.isLoggedin())
                {
                    continue;
                }

                chr.updateCouponRates();
            }
        }
    }

    public void toggleCoupon(int couponId)
    {
        if (ItemConstants.isRateCoupon(couponId))
        {
            lock (activeCoupons)
            {
                if (activeCoupons.Contains(couponId))
                {
                    activeCoupons.Remove(couponId);
                }
                else
                {
                    activeCoupons.Add(couponId);
                }

                commitActiveCoupons();
            }
        }
    }

    public void updateActiveCoupons(DBContext dbContext)
    {
        lock (activeCoupons)
        {
            activeCoupons.Clear();
            var d = DateTimeOffset.Now;

            int weekDay = (int)d.DayOfWeek;
            int hourDay = d.Hour;

            int weekdayMask = (1 << weekDay);
            activeCoupons = dbContext.Nxcoupons.Where(x => x.Starthour <= hourDay && x.Endhour > hourDay && (x.Activeday & weekdayMask) == weekdayMask)
                    .Select(x => x.CouponId).ToList();

        }
    }
    #endregion
    public void runAnnouncePlayerDiseasesSchedule()
    {
        List<IClient> processDiseaseAnnounceClients;
        Monitor.Enter(disLock);
        try
        {
            processDiseaseAnnounceClients = new(processDiseaseAnnouncePlayers);
            processDiseaseAnnouncePlayers.Clear();
        }
        finally
        {
            Monitor.Exit(disLock);
        }

        while (processDiseaseAnnounceClients.Count > 0)
        {
            var c = processDiseaseAnnounceClients.remove(0);
            var player = c.getPlayer();
            if (player != null && player.isLoggedinWorld())
            {
                player.announceDiseases();
                player.collectDiseases();
            }
        }

        Monitor.Enter(disLock);
        try
        {
            // this is to force the system to wait for at least one complete tick before releasing disease info for the registered clients
            while (registeredDiseaseAnnouncePlayers.Count > 0)
            {
                var c = registeredDiseaseAnnouncePlayers.remove(0);
                processDiseaseAnnouncePlayers.Add(c);
            }
        }
        finally
        {
            Monitor.Exit(disLock);
        }
    }

    public void registerAnnouncePlayerDiseases(IClient c)
    {
        Monitor.Enter(disLock);
        try
        {
            registeredDiseaseAnnouncePlayers.Add(c);
        }
        finally
        {
            Monitor.Exit(disLock);
        }
    }

    public List<RankedCharacterInfo> getWorldPlayerRanking(int worldId)
    {
        wldLock.EnterReadLock();
        try
        {
            var filteredWorldId = YamlConfig.config.server.USE_WHOLE_SERVER_RANKING ? -1 : worldId;
            if (playerRanking.TryGetValue(filteredWorldId, out var value))
                return value;

            return [];
        }
        finally
        {
            wldLock.ExitReadLock();
        }
    }

    public Dictionary<int, List<RankedCharacterInfo>> LoadPlayerRanking()
    {
        return playerRanking = RankManager.LoadPlayerRankingFromDB();
    }


    private async Task InitialDataBase()
    {
        log.Debug("初始化数据库");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        using var dbContext = new DBContext();

        await dbContext.Database.MigrateAsync();

        if (!dbContext.Shops.Any())
        {
            var sqls = Directory.GetFiles("sql").OrderBy(x => Regex.Match(x, "v([0-9]+)\\S*\\.sql$").Groups[0].Value);
            foreach (var file in sqls)
            {
                var sqlStr = File.ReadAllText(file);
                await dbContext.Database.ExecuteSqlRawAsync(sqlStr);
            }
        }
        sw.Stop();
        log.Debug("初始化数据库====完成，耗时{StarupCost}秒", sw.Elapsed.TotalSeconds);
    }

    public async Task Start()
    {
        log.Information("Cosmic v{Version} starting up.", ServerConstants.VERSION);

        await InitialDataBase();

        Stopwatch totalSw = new Stopwatch();
        totalSw.Start();

        if (YamlConfig.config.server.SHUTDOWNHOOK)
        {
            AppDomain.CurrentDomain.ProcessExit += (obj, evt) => shutdown(false);
        }

        channelDependencies = registerChannelDependencies();

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SkillFactory.LoadAllSkills();
            sw.Stop();
            log.Debug("Skills loaded in {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            CashItemFactory.loadAllCashItems();
            sw.Stop();
            log.Debug("CashItems loaded in {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Quest.loadAllQuests();
            sw.Stop();
            log.Debug("Quest loaded in {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SkillbookInformationProvider.loadAllSkillbookInformation();
            sw.Stop();
            log.Debug("Skillbook loaded in {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        try
        {
            using var dbContext = new DBContext();
            setAllLoggedOut(dbContext);
            setAllMerchantsInactive(dbContext);
            cleanNxcodeCoupons(dbContext);
            loadCouponRates(dbContext);
            updateActiveCoupons(dbContext);
            NewYearCardRecord.startPendingNewYearCardRequests(dbContext);
            CashIdGenerator.loadExistentCashIdsFromDb(dbContext);
            applyAllNameChanges(dbContext); // -- name changes can be missed by INSTANT_NAME_CHANGE --
            applyAllWorldTransfers(dbContext);
            PlayerNPC.loadRunningRankData(dbContext);
        }
        catch (Exception sqle)
        {
            log.Error(sqle, "Failed to run all startup-bound database tasks");
            throw;
        }

        await initializeTimelyTasks(channelDependencies);    // aggregated method for timely tasks thanks to lxconan

        try
        {
            var worlds = ServerManager.LoadAllWorld().Where(x => x.CanDeploy).ToList();
            foreach (var worldConfig in worlds)
            {
                InitWorld(worldConfig);
            }
            LoadPlayerRanking();

            loadPlayerNpcMapStepFromDb();

            if (YamlConfig.config.server.USE_FAMILY_SYSTEM)
            {
                using var dbContext = new DBContext();
                Family.loadAllFamilies(dbContext);
            }
        }
        catch (Exception e)
        {
            log.Error(e, "[SEVERE] Syntax error in 'world.ini'."); //For those who get errors
            Environment.Exit(0);
        }

        // Wait on all async tasks to complete

        loginServer = await initLoginServer(8484);
        online = true;

        log.Information("Listening on port 8484");

        totalSw.Stop();
        log.Information("Cosmic is now online after {StartupCost}s.", totalSw.Elapsed.TotalSeconds);

        foreach (var ch in this.getAllChannels())
        {
            ch.reloadEventScriptManager();
        }

        OpcodeConstants.generateOpcodeNames();
        CommandExecutor.getInstance();
        ItemInformationProvider.getInstance();
    }

    private ChannelDependencies registerChannelDependencies()
    {
        NoteService noteService = new NoteService(new NoteDao());
        FredrickProcessor fredrickProcessor = new FredrickProcessor(noteService);
        ChannelDependencies channelDependencies = new ChannelDependencies(noteService, fredrickProcessor);

        PacketProcessor.registerGameHandlerDependencies(channelDependencies);

        return channelDependencies;
    }

    private async Task<LoginServer> initLoginServer(int port)
    {
        LoginServer loginServer = new LoginServer(port);
        await loginServer.Start();
        return loginServer;
    }

    private static void setAllLoggedOut(DBContext dbContext)
    {
        dbContext.Accounts.ExecuteUpdate(x => x.SetProperty(y => y.Loggedin, 0));
    }

    private static void setAllMerchantsInactive(DBContext dbContext)
    {
        dbContext.Characters.ExecuteUpdate(x => x.SetProperty(y => y.HasMerchant, false));
    }

    private async Task initializeTimelyTasks(ChannelDependencies channelDependencies)
    {
        TimerManager tMan = TimerManager.getInstance();
        await tMan.start();
        tMan.register(tMan.purge, YamlConfig.config.server.PURGING_INTERVAL);//Purging ftw...
        disconnectIdlesOnLoginTask();

        var timeLeft = TimeUtils.GetTimeLeftForNextHour();
        tMan.register(new CharacterDiseaseTask(), YamlConfig.config.server.UPDATE_INTERVAL, YamlConfig.config.server.UPDATE_INTERVAL);
        tMan.register(new CouponTask(), YamlConfig.config.server.COUPON_INTERVAL, (long)timeLeft.TotalMilliseconds);
        tMan.register(new RankingCommandTask(), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
        tMan.register(new RankingLoginTask(), YamlConfig.config.server.RANKING_INTERVAL, (long)timeLeft.TotalMilliseconds);
        tMan.register(new LoginCoordinatorTask(), TimeSpan.FromHours(1), timeLeft);
        tMan.register(new EventRecallCoordinatorTask(), TimeSpan.FromHours(1), timeLeft);
        tMan.register(new LoginStorageTask(), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
        tMan.register(new DueyFredrickTask(channelDependencies.fredrickProcessor), TimeSpan.FromHours(1), timeLeft);
        tMan.register(new InvitationTask(), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        tMan.register(new RespawnTask(), YamlConfig.config.server.RESPAWN_INTERVAL, YamlConfig.config.server.RESPAWN_INTERVAL);

        timeLeft = TimeUtils.GetTimeLeftForNextDay();
        ExpeditionBossLog.resetBossLogTable();
        tMan.register(new BossLogTask(), TimeSpan.FromDays(1), timeLeft);
    }

    public PlayerBuffStorage getPlayerBuffStorage()
    {
        return buffStorage;
    }

    public void broadcastMessage(int world, Packet packet)
    {
        foreach (var ch in getChannelsFromWorld(world))
        {
            ch.broadcastPacket(packet);
        }
    }

    public void broadcastGMMessage(int world, Packet packet)
    {
        foreach (var ch in getChannelsFromWorld(world))
        {
            ch.broadcastGMPacket(packet);
        }
    }

    public bool isGmOnline(int world)
    {
        return getWorld(world).Players.GetAllOnlinedPlayers().Any(x => x.isGM());
    }

    public void changeFly(int accountid, bool canFly)
    {
        if (canFly)
        {
            activeFly.Add(accountid);
        }
        else
        {
            activeFly.Remove(accountid);
        }
    }

    public bool canFly(int accountid)
    {
        return activeFly.Contains(accountid);
    }

    public int getCharacterWorld(int chrid)
    {
        lgnLock.EnterReadLock();
        try
        {
            return AllPlayerStorage.GetOrAddCharacterById(chrid)?.World ?? -1;
        }
        finally
        {
            lgnLock.ExitReadLock();
        }
    }

    public bool haveCharacterEntry(int accountid, int chrid)
    {
        lgnLock.EnterReadLock();
        try
        {
            var accChars = AccountCharacterCache.GetValueOrDefault(accountid) ?? [];
            return accChars.Contains(chrid);
        }
        finally
        {
            lgnLock.ExitReadLock();
        }
    }

    public short getAccountCharacterCount(int accountid)
    {
        return (short)getAccountCharacterEntries(accountid).Count;
    }

    public short getAccountWorldCharacterCount(int accountid, int worldid)
    {
        return (short)getAccountCharacterEntries(accountid).Count(x => x.World == worldid);
    }

    private HashSet<IPlayer> getAccountCharacterEntries(int accountid, int loadLevel = 0)
    {
        lgnLock.EnterReadLock();
        try
        {
            if (AccountCharacterCache.ContainsKey(accountid))
                return AllPlayerStorage.GetPlayersByIds(AccountCharacterCache[accountid], loadLevel).ToHashSet();
            return [];
        }
        finally
        {
            lgnLock.ExitReadLock();
        }
    }

    public void createCharacterEntry(IPlayer chr)
    {
        int accountid = chr.getAccountID();

        lgnLock.EnterWriteLock();
        try
        {
            if (!AccountCharacterCache.ContainsKey(accountid))
                AccountCharacterCache.Add(accountid, new());

            AccountCharacterCache[accountid].Add(chr.Id);
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }

    public void deleteCharacterEntry(int accountid, int chrid)
    {
        lgnLock.EnterWriteLock();
        try
        {
            var accChars = AccountCharacterCache.GetValueOrDefault(accountid);
            if (accChars != null)
                accChars.Remove(chrid);
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }

    //public void transferWorldCharacterEntry(IPlayer chr, int toWorld)
    //{
    //    // used before setting the new worldid on the character object
    //    lgnLock.EnterWriteLock();
    //    try
    //    {
    //        int chrid = chr.getId(), accountid = chr.getAccountID();

    //        var wservTmp = this.getWorld(chr.World);
    //        if (wservTmp != null)
    //        {
    //            wservTmp.unregisterAccountCharacterView(accountid, chrid);
    //        }

    //        var wserv = this.getWorld(toWorld);
    //        if (wserv != null)
    //        {
    //            wserv.registerAccountCharacterView(chr.AccountId, chr);
    //        }
    //    }
    //    finally
    //    {
    //        lgnLock.ExitWriteLock();
    //    }
    //}

    /*
    public void deleteAccountEntry(int accountid) { is this even a thing?
        lgnLock.EnterWriteLock();
        try {
            accountCharacterCount.Remove(accountid);
            accountChars.Remove(accountid);
        } finally {
            lgnLock.ExitWriteLock();
        }

        foreach(World wserv in this.getWorlds()) {
            wserv.clearAccountCharacterView(accountid);
            wserv.unregisterAccountStorage(accountid);
        }
    }
    */

    /// <summary>
    /// world - players
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="visibleWorlds"></param>
    /// <returns></returns>
    public SortedDictionary<int, List<IPlayer>> LoadAccountCharList(int accountId, int visibleWorlds)
    {
        LoadAccountCharactersView(accountId);

        var accountData = getAccountCharacterEntries(accountId, 1);
        SortedDictionary<int, List<IPlayer>> worldChrs = new();

        lgnLock.EnterReadLock();
        try
        {
            for (int worldId = 0; worldId < visibleWorlds; worldId++)
            {
                var chrs = accountData.Where(x => x.World == worldId).ToList();
                worldChrs.Add(worldId, chrs);
            }
        }
        finally
        {
            lgnLock.ExitReadLock();
        }

        return worldChrs;
    }


    public List<IPlayer> loadAllAccountsCharactersView()
    {
        using var dbContext = new DBContext();
        var idList = dbContext.Accounts.Select(x => x.Id).ToList();

        var list = new List<IPlayer>();
        idList.ForEach(accountId =>
        {
            LoadAccountCharactersView(accountId, force: false);
            list.AddRange(getAccountCharacterEntries(accountId));
        });

        return list;
    }


    private static void applyAllNameChanges(DBContext dbContext)
    {
        try
        {
            List<NameChangePair> changedNames = new();
            using var dbTrans = dbContext.Database.BeginTransaction();
            var allChanges = dbContext.Namechanges.Where(x => x.CompletionTime == null).ToList();
            allChanges.ForEach(x =>
            {
                bool success = CharacterManager.doNameChange(dbContext, x.Characterid, x.Old, x.New, x.Id);
                if (!success)
                    dbTrans.Rollback();
                else
                {
                    dbTrans.Commit();
                    changedNames.Add(new(x.Old, x.New));
                }
            });

            //log
            foreach (var namePair in changedNames)
            {
                log.Information("Name change applied - from: \"{CharacterName}\" to \"{CharacterName}\"", namePair.OldName, namePair.NewName);
            }
        }
        catch (Exception e)
        {
            log.Warning(e, "Failed to retrieve list of pending name changes");
            throw;
        }
    }

    private static void applyAllWorldTransfers(DBContext dbContext)
    {
        try
        {
            var ds = dbContext.Worldtransfers.Where(x => x.CompletionTime == null).ToList();
            List<int> removedTransfers = new();

            ds.ForEach(x =>
            {
                string? reason = CharacterManager.checkWorldTransferEligibility(dbContext, x.Characterid, x.From, x.To);
                if (!string.IsNullOrEmpty(reason))
                {
                    removedTransfers.Add(x.Id);
                    dbContext.Worldtransfers.Remove(x);
                    log.Information("World transfer canceled: chrId {CharacterId}, reason {WorldTransferReason}", x.Characterid, reason);
                }
            });

            using var dbTrans = dbContext.Database.BeginTransaction();
            List<CharacterWorldTransferPair> worldTransfers = new(); //logging only <charid, <oldWorld, newWorld>>

            ds.ForEach(x =>
            {
                var success = CharacterManager.doWorldTransfer(dbContext, x.Characterid, x.From, x.To, x.Id);
                if (!success)
                    dbTrans.Rollback();
                else
                {
                    dbTrans.Commit();
                    worldTransfers.Add(new(x.Characterid, x.From, x.To));
                }
            });

            //log
            foreach (var worldTransferPair in worldTransfers)
            {
                int charId = worldTransferPair.CharacterId;
                int oldWorld = worldTransferPair.OldId;
                int newWorld = worldTransferPair.NewId;
                log.Information("World transfer applied - character id {CharacterId} from world {WorldId} to world {WorldId}", charId, oldWorld, newWorld);
            }
        }
        catch (Exception e)
        {
            log.Warning(e, "Failed to retrieve list of pending world transfers");
            throw;
        }
    }

    public void loadAccountCharacters(IClient c)
    {
        int accId = c.getAccID();
        LoadAccountCharactersView(accId);

        var accData = getAccountCharacterEntries(accId);
        int gmLevel = accData.Count == 0 ? 0 : accData.Max(x => x.gmLevel());
        c.setGMLevel(gmLevel);
    }

    private void LoadAccountCharactersView(int accId, int worldId = -1, bool force = true)
    {
        var filterWorlds = new List<int>();
        if (worldId == -1)
        {
            filterWorlds = RunningWorlds.Keys.ToList();
        }
        else
        {
            filterWorlds.Add(worldId);
        }

        lgnLock.EnterWriteLock();
        try
        {
            if (!AccountCharacterCache.ContainsKey(accId))
            {
                AccountCharacterCache[accId] = new();
            }
            else
            {
                if (!force)
                    return;
            }

            var playerIdList = AccountManager.LoadAccountWorldPlayers(accId, filterWorlds);
            foreach (var cid in playerIdList)
            {
                AccountCharacterCache[accId].Add(cid);
            }
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }

    public void loadAccountStorages(IClient c)
    {
        int accountId = c.getAccID();
        var accWorlds = getAccountCharacterEntries(accountId).Select(x => x.World).ToHashSet();

        var worldList = this.getWorlds();
        foreach (int worldid in accWorlds)
        {
            if (worldid < worldList.Count)
            {
                var wserv = worldList.get(worldid);
                wserv.loadAccountStorage(accountId);
            }
        }
    }

    private static string getRemoteHost(IClient client)
    {
        return SessionCoordinator.getSessionRemoteHost(client);
    }

    public void setCharacteridInTransition(IClient client, int charId)
    {
        string remoteIp = getRemoteHost(client);

        lgnLock.EnterWriteLock();
        try
        {
            transitioningChars.AddOrUpdate(remoteIp, charId);
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }

    public bool validateCharacteridInTransition(IClient client, int charId)
    {
        if (!YamlConfig.config.server.USE_IP_VALIDATION)
        {
            return true;
        }

        string remoteIp = getRemoteHost(client);

        lgnLock.EnterWriteLock();
        try
        {
            return transitioningChars.Remove(remoteIp, out var cid) && cid == charId;
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }

    public int? freeCharacteridInTransition(IClient client)
    {
        if (!YamlConfig.config.server.USE_IP_VALIDATION)
        {
            return null;
        }

        string remoteIp = getRemoteHost(client);

        lgnLock.EnterWriteLock();
        try
        {
            if (transitioningChars.Remove(remoteIp, out var d))
                return d;
            return null;
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }

    public bool hasCharacteridInTransition(IClient client)
    {
        if (!YamlConfig.config.server.USE_IP_VALIDATION)
        {
            return true;
        }

        string remoteIp = getRemoteHost(client);

        lgnLock.EnterReadLock();
        try
        {
            return transitioningChars.ContainsKey(remoteIp);
        }
        finally
        {
            lgnLock.ExitReadLock();
        }
    }

    public void registerLoginState(IClient c)
    {
        Monitor.Enter(srvLock);
        try
        {
            inLoginState.AddOrUpdate(c, DateTimeOffset.Now.AddMinutes(10));
        }
        finally
        {
            Monitor.Exit(srvLock);
        }
    }

    public void unregisterLoginState(IClient c)
    {
        Monitor.Enter(srvLock);
        try
        {
            inLoginState.Remove(c);
        }
        finally
        {
            Monitor.Exit(srvLock);
        }
    }

    private void disconnectIdlesOnLoginState()
    {
        List<IClient> toDisconnect = new();

        Monitor.Enter(srvLock);
        try
        {
            var timeNow = DateTimeOffset.Now;

            foreach (var mc in inLoginState)
            {
                if (timeNow > mc.Value)
                {
                    toDisconnect.Add(mc.Key);
                }
            }

            foreach (IClient c in toDisconnect)
            {
                inLoginState.Remove(c);
            }
        }
        finally
        {
            Monitor.Exit(srvLock);
        }

        foreach (IClient c in toDisconnect)
        {
            // thanks Lei for pointing a deadlock issue with srvLock
            if (c.isLoggedIn())
            {
                c.disconnect(false, false);
            }
            else
            {
                SessionCoordinator.getInstance().closeSession(c, true);
            }
        }
    }

    private void disconnectIdlesOnLoginTask()
    {
        TimerManager.getInstance().register(() => disconnectIdlesOnLoginState(), TimeSpan.FromMinutes(5));
    }

    public Action shutdown(bool restart)
    {
        //no player should be online when trying to shutdown!
        return async () => await Stop(restart);
    }

    //synchronized
    public async Task Stop(bool restart)
    {
        log.Information("{0} the server!", restart ? "Restarting" : "Shutting down");
        if (getWorlds() == null)
        {
            return;//already shutdown
        }
        foreach (World w in getWorlds())
        {
            w.shutdown();
        }

        /*foreach(World w in getWorlds()) {
            while (w.getPlayerStorage().getAllCharacters().Count > 0) {
                try {
                    Thread.sleep(1000);
                } catch (ThreadInterruptedException ie) {
                    System.err.println("FUCK MY LIFE");
                }
            }
        }
        foreach(Channel ch in getAllChannels()) {
            while (ch.getConnectedClients() > 0) {
                try {
                    Thread.sleep(1000);
                } catch (ThreadInterruptedException ie) {
                    System.err.println("FUCK MY LIFE");
                }
            }
        }*/

        var allChannels = getAllChannels();

        foreach (var ch in allChannels)
        {
            while (!ch.finishedShutdown())
            {
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException ie)
                {
                    log.Error(ie, "Error during shutdown sleep");
                }
            }
        }

        resetServerWorlds();

        ThreadManager.getInstance().stop();
        TimerManager.getInstance().purge();
        TimerManager.getInstance().stop();

        log.Information("Worlds and channels are offline.");
        await loginServer.Stop();
        if (!restart)
        {  // shutdown hook deadlocks if System.exit() method is used within its body chores, thanks MIKE for pointing that out
            // We disabled log4j's shutdown hook in the config file, so we have to manually shut it down here,
            // after our last log statement.
            await Log.CloseAndFlushAsync();
            new Thread(() => Environment.Exit(0)).Start();
        }
        else
        {
            log.Information("Restarting the server...");
            instance = new Lazy<Server>(new Server());
            await getInstance().Start();//DID I DO EVERYTHING?! D:
        }
    }
}
