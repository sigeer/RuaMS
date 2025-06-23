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


using Application.Core.Channel;
using Application.Core.EF.Entities.SystemBase;
using Application.Core.Game.Life;
using Application.Core.Game.Skills;
using Application.Core.Managers;
using Application.Core.model;
using client;
using client.newyear;
using Microsoft.EntityFrameworkCore;
using net.server.task;
using server;
using server.expeditions;
using server.quest;
using System.Diagnostics;
using static server.CashShop;

namespace net.server;

public class Server
{
    private static ILogger log = LogFactory.GetLogger(LogType.Server);
    private static Lazy<Server> instance = new Lazy<Server>(new Server());

    public static Server getInstance() => instance.Value;

    private static HashSet<int> activeFly = new();


    public Dictionary<int, World> RunningWorlds { get; set; } = new();

    /// <summary>
    /// AccountId - cid
    /// </summary>
    private Dictionary<int, HashSet<AccountInfo>> AccountCharacterCache = new();

    private Dictionary<int, NewYearCardRecord> newyears = new();


    private volatile bool availableDeveloperRoom = false;
    public bool IsOnline { get; set; }
    public static DateTimeOffset uptime = DateTimeOffset.UtcNow;
    ReaderWriterLockSlim lgnLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private Server()
    {
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

    public World getWorld(int id)
    {
        return RunningWorlds.GetValueOrDefault(id) ?? throw new BusinessException($"World {id} not exsited");
    }

    public List<World> getWorlds()
    {
        return RunningWorlds.Values.ToList();
    }

    public int getWorldsSize()
    {
        return RunningWorlds.Count;
    }

    public List<WorldChannel> getChannelsFromWorld(int world)
    {
        return this.getWorld(world).getChannels();
    }

    public List<WorldChannel> getAllChannels()
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


    bool basedCached = false;
    private void Initialize(bool ignoreCache = false)
    {
        if (!ignoreCache && basedCached)
            return;

        if (!Directory.Exists(ScriptResFactory.ScriptDirName) || !Directory.Exists(WZFiles.DIRECTORY))
            throw new DirectoryNotFoundException();

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

        OpcodeConstants.generateOpcodeNames();

        basedCached = true;
    }

    public void LoadWorld()
    {
        InitWorld(new WorldConfigEntity(0, "Scania")
        {
            Enable = true,
            ServerMessage = "Welcome to Scania!",
            EventMessage = "Scania!",
            RecommendMessage = "Welcome to Scania!",
            ExpRate = 10,
            DropRate = 10,
            MobRate = 1,
            TravelRate = 10,
            QuestRate = 5,
            FishingRate = 10,
            BossDropRate = 10,
            MesoRate = 10,
            StartPort = 7575
        });
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
            Initialize(ignoreCache);

            var startTimelyTask = InitializeTimelyTasks(TaskEngine.Quartz);    // aggregated method for timely tasks thanks to lxconan

            LoadWorld();

            using var dbContext = new DBContext();
            LoadAccountCharacterCache(dbContext);

            NewYearCardRecord.startPendingNewYearCardRequests(dbContext);
            // CashIdGenerator.loadExistentCashIdsFromDb(dbContext);
            applyAllNameChanges(dbContext); // -- name changes can be missed by INSTANT_NAME_CHANGE --
            PlayerNPC.loadRunningRankData(dbContext);

            loadPlayerNpcMapStepFromDb(dbContext);

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

    public ITimerManager GlobalTimerManager { get; private set; }
    public async Task InitializeTimelyTasks(TaskEngine engine)
    {

        GlobalTimerManager = await TimerManager.InitializeAsync(engine, "Temp");

        var timeLeft = TimeUtils.GetTimeLeftForNextHour();
        GlobalTimerManager.register(new EventRecallCoordinatorTask(), TimeSpan.FromHours(1), timeLeft);

        timeLeft = TimeUtils.GetTimeLeftForNextDay();
        ExpeditionBossLog.resetBossLogTable();
        GlobalTimerManager.register(new BossLogTask(), TimeSpan.FromDays(1), timeLeft);
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
        await GlobalTimerManager.Stop();

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
