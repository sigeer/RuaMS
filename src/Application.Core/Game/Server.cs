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
using client;
using client.inventory.manipulator;
using client.newyear;
using client.processor.npc;
using constants.inventory;
using constants.net;
using database.note;
using Microsoft.EntityFrameworkCore;
using net.packet;
using net.server.task;
using server;
using server.expeditions;
using server.quest;
using service;
using System.Diagnostics;
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

    public Dictionary<int, IWorld> RunningWorlds { get; set; } = new();

    /// <summary>
    /// AccountId - cid
    /// </summary>
    private Dictionary<int, HashSet<AccountInfo>> AccountCharacterCache = new();


    private PlayerBuffStorage buffStorage = new PlayerBuffStorage();

    private Dictionary<int, NewYearCardRecord> newyears = new();
    private Queue<IChannelClient> processDiseaseAnnouncePlayers = new();
    private Queue<IChannelClient> registeredDiseaseAnnouncePlayers = new();

    /// <summary>
    /// World - Data
    /// </summary>
    private Dictionary<int, List<RankedCharacterInfo>> playerRanking = new();

    private object disLock = new object();

    private AtomicLong currentTime = new AtomicLong(0);
    private long serverCurrentTime = 0;

    private volatile bool availableDeveloperRoom = false;
    public bool IsOnline { get; set; }
    public static DateTimeOffset uptime = DateTimeOffset.Now;
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

    private void loadPlayerNpcMapStepFromDb(DBContext dbContext)
    {
        var list = dbContext.PlayernpcsFields.AsNoTracking().ToList();
        list.ForEach(rs =>
        {
            RunningWorlds[rs.World]?.setPlayerNpcMapData(rs.Map, rs.Step, rs.Podium);
        });
    }

    public IWorld getWorld(int id)
    {
        return RunningWorlds.GetValueOrDefault(id) ?? throw new BusinessException($"World {id} not exsited");
    }

    public List<IWorld> getWorlds()
    {
        return RunningWorlds.Values.ToList();
    }

    public int getWorldsSize()
    {
        return RunningWorlds.Count;
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
        return getWorld(world).getChannels().Select(x => x.getId()).ToHashSet();
    }

    public bool AddWorld(WorldConfigEntity worldConfig)
    {
        return InitWorld(worldConfig);
    }

    public bool InitWorld(WorldConfigEntity worldConfig)
    {
        if (RunningWorlds.ContainsKey(worldConfig.Id))
            return false;

        log.Information("初始化世界 {WorldId}", worldConfig.Id);

        var world = new World(worldConfig);

        RunningWorlds[worldConfig.Id] = world;
        log.Information("世界 {WorldId} 加载完成", world.Id);
        return true;
    }


    private void resetServerWorlds()
    {
        RunningWorlds.Clear();
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
        couponRates = dbContext.Nxcoupons.AsNoTracking().Select(x => new { x.CouponId, x.Rate }).ToList().ToDictionary(x => x.CouponId, x => x.Rate);
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
            weekDay = weekDay == 0 ? 7 : weekDay;
            int hourDay = d.Hour;

            int weekdayMask = 1 << weekDay;
            activeCoupons = dbContext.Nxcoupons.Where(x => x.Starthour <= hourDay && x.Endhour > hourDay && (x.Activeday & weekdayMask) == weekdayMask)
                    .Select(x => x.CouponId).ToList();

        }
    }
    #endregion
    public void runAnnouncePlayerDiseasesSchedule()
    {
        Queue<IChannelClient> processDiseaseAnnounceClients;
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

        while (processDiseaseAnnounceClients.TryDequeue(out var c))
        {
            var player = c.Character;
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
            while (registeredDiseaseAnnouncePlayers.TryDequeue(out var c))
            {
                processDiseaseAnnouncePlayers.Enqueue(c);
            }
        }
        finally
        {
            Monitor.Exit(disLock);
        }
    }

    public void registerAnnouncePlayerDiseases(IChannelClient c)
    {
        Monitor.Enter(disLock);
        try
        {
            registeredDiseaseAnnouncePlayers.Enqueue(c);
        }
        finally
        {
            Monitor.Exit(disLock);
        }
    }

    public List<RankedCharacterInfo> getWorldPlayerRanking(int worldId)
    {
        var filteredWorldId = YamlConfig.config.server.USE_WHOLE_SERVER_RANKING ? -1 : worldId;
        if (playerRanking.TryGetValue(filteredWorldId, out var value))
            return value;

        return [];
    }

    public Dictionary<int, List<RankedCharacterInfo>> LoadPlayerRanking(DBContext dbContext)
    {
        return playerRanking = RankManager.LoadPlayerRankingFromDB(dbContext);
    }

    private async Task InitialDataBase()
    {
        log.Information("初始化数据库...");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        using var dbContext = new DBContext();

        try
        {
            log.Information("数据库迁移...");
            await dbContext.Database.MigrateAsync();
            log.Information("数据库迁移成功");

            sw.Stop();
            log.Information("初始化数据库成功，耗时{StarupCost}秒", sw.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            log.Error(ex, "初始化数据库失败");
            throw;
        }
    }

    bool basedCached = false;
    private async Task Initialize(bool ignoreCache = false)
    {
        if (!ignoreCache && basedCached)
            return;

        if (!Directory.Exists(ScriptResFactory.ScriptDirName) || !Directory.Exists(WZFiles.DIRECTORY))
            throw new DirectoryNotFoundException();

        await InitialDataBase();

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SkillFactory.LoadAllSkills();
            sw.Stop();
            log.Debug("WZ - 技能加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            CashItemFactory.loadAllCashItems();
            sw.Stop();
            log.Debug("WZ - 现金道具加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Quest.loadAllQuests();
            sw.Stop();
            log.Debug("WZ - 任务加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        _ = Task.Run(() =>
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            SkillbookInformationProvider.loadAllSkillbookInformation();
            sw.Stop();
            log.Debug("WZ - 能手册加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
        });

        OpcodeConstants.generateOpcodeNames();
        CommandExecutor.getInstance();

        basedCached = true;
    }


    public bool IsStarting { get; set; }
    public async Task Start(bool ignoreCache = false)
    {
        if (IsStarting || IsOnline)
            return;

        IsStarting = true;
        log.Information("服务器配置加载中...");

        Stopwatch totalSw = new Stopwatch();
        totalSw.Start();

        try
        {
            await Initialize(ignoreCache);

            AppDomain.CurrentDomain.ProcessExit += (obj, evt) => shutdown(false);

            var startTimelyTask = InitializeTimelyTasks(TaskEngine.Quartz);    // aggregated method for timely tasks thanks to lxconan

            var worlds = ServerManager.LoadAllWorld().ToList();
            foreach (var worldConfig in worlds)
            {
                InitWorld(worldConfig);
            }

            using var dbContext = new DBContext();
            LoadAccountCharacterCache(dbContext);

            setAllLoggedOut(dbContext);
            setAllMerchantsInactive(dbContext);
            cleanNxcodeCoupons(dbContext);
            loadCouponRates(dbContext);
            updateActiveCoupons(dbContext);
            NewYearCardRecord.startPendingNewYearCardRequests(dbContext);
            CashIdGenerator.loadExistentCashIdsFromDb(dbContext);
            applyAllNameChanges(dbContext); // -- name changes can be missed by INSTANT_NAME_CHANGE --
            PlayerNPC.loadRunningRankData(dbContext);

            LoadPlayerRanking(dbContext);

            loadPlayerNpcMapStepFromDb(dbContext);

            if (YamlConfig.config.server.USE_FAMILY_SYSTEM)
            {
                Family.loadAllFamilies(dbContext);
            }

            IsOnline = true;

            totalSw.Stop();
            log.Information("服务器配置加载完成，耗时 {StartupCost}s.", totalSw.Elapsed.TotalSeconds);

            await startTimelyTask;
        }
        catch (Exception ex)
        {
            log.Error(ex, "服务器配置加载失败");
            throw;
        }
        finally
        {
            IsStarting = false;
        }
    }

    private ChannelDependencies registerChannelDependencies()
    {
        NoteService noteService = new NoteService(new NoteDao());
        FredrickProcessor fredrickProcessor = new FredrickProcessor(noteService);
        ChannelDependencies channelDependencies = new ChannelDependencies(noteService, fredrickProcessor);

        return channelDependencies;
    }

    private static void setAllLoggedOut(DBContext dbContext)
    {
        dbContext.Accounts.ExecuteUpdate(x => x.SetProperty(y => y.Loggedin, 0));
    }

    private static void setAllMerchantsInactive(DBContext dbContext)
    {
        dbContext.Characters.ExecuteUpdate(x => x.SetProperty(y => y.HasMerchant, false));
    }

    public async Task InitializeTimelyTasks(TaskEngine engine)
    {
        var channelDependencies = registerChannelDependencies();

        await TimerManager.InitializeAsync(engine);
        var tMan = TimerManager.getInstance();
        await tMan.Start();

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
            return accChars.Any(x => x.Id == chrid);
        }
        finally
        {
            lgnLock.ExitReadLock();
        }
    }

    public int getAccountCharacterCount(int accountid)
    {
        return AccountCharacterCache.GetValueOrDefault(accountid, []).Count;
    }

    public int getAccountWorldCharacterCount(int accountid, int worldid)
    {
        return AccountCharacterCache.GetValueOrDefault(accountid, []).Count(x => x.World == worldid);
    }

    private HashSet<IPlayer> getAccountCharacterEntries(int accountid, int loadLevel = 0)
    {
        lgnLock.EnterReadLock();
        try
        {
            if (AccountCharacterCache.TryGetValue(accountid, out var d))
                return AllPlayerStorage.GetPlayersByIds(d.Select(x => x.Id).ToArray(), loadLevel).ToHashSet();
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

            AccountCharacterCache[accountid].Add(new AccountInfo(chr.Id, chr.World, chr.AccountId));
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
                accChars = accChars.Where(x => x.Id != chrid).ToHashSet();
        }
        finally
        {
            lgnLock.ExitWriteLock();
        }
    }


    /// <summary>
    /// world - players
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="visibleWorlds"></param>
    /// <returns></returns>
    public SortedDictionary<int, List<IPlayer>> LoadAccountCharList(int accountId, int visibleWorlds)
    {
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

    private Dictionary<int, HashSet<AccountInfo>> LoadAccountCharacterCache(DBContext dbContext)
    {
        AccountCharacterCache = (from a in dbContext.Accounts
                                 let chars = dbContext.Characters.Where(x => x.AccountId == a.Id).Select(x => new { x.AccountId, x.Id, x.World }).ToList()
                                 select new { AccountId = a.Id, CharIdList = chars }).ToList()
                       .ToDictionary(x => x.AccountId, x => x.CharIdList.Select(x => new AccountInfo(x.Id, x.World, x.AccountId)).ToHashSet());
        return AccountCharacterCache;
    }

    public List<IPlayer> loadAllAccountsCharactersView()
    {
        var idList = AccountCharacterCache.Keys.ToList();

        var list = new List<IPlayer>();
        idList.ForEach(accountId =>
        {
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
    public Action shutdown(bool restart)
    {
        //no player should be online when trying to shutdown!
        return async () => await Stop(restart);
    }

    public void Reset()
    {
        instance = new Lazy<Server>(new Server());
    }

    //synchronized
    public async Task Stop(bool restart, bool force = false)
    {
        if (!IsOnline)
        {
            log.Warning("不能停止未启动的服务");
            return;
        }

        log.Information("{0} the server!", restart ? "Restarting" : "Shutting down");
        var runningWorlds = getWorlds();
        if (runningWorlds.Count == 0)
        {
            return;//already shutdown
        }
        foreach (World w in runningWorlds)
        {
            await w.Shutdown();
        }

        resetServerWorlds();

        ThreadManager.getInstance().stop();
        await TimerManager.getInstance().Stop();

        IsOnline = false;
        if (force)
            basedCached = false;

        log.Information("Worlds and channels are offline.");
        if (restart)
        {
            log.Information("Restarting the server...");
            Reset();
            await getInstance().Start();//DID I DO EVERYTHING?! D:
        }
    }
}
