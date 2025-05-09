using Application.Core.Channel.ServerTransports;
using Application.Core.Game;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Core.Game.Relation;
using Application.Core.Game.Tasks;
using Application.Core.Game.TheWorld;
using Application.Core.Game.Trades;
using Application.Core.Gameplay.ChannelEvents;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Utility.Configs;
using Application.Utility.Loggers;
using net.netty;
using net.packet;
using net.server.services;
using net.server.services.type;
using scripting.Event;
using Serilog;
using server.events.gm;
using server.expeditions;
using server.maps;
using System.Net;
using tools;

namespace Application.Core.Channel;

public partial class WorldChannel : IWorldChannel
{
    public string InstanceId { get; }
    private ILogger log;
    public bool IsRunning { get; private set; }

    public int Port { get; set; }
    private IPEndPoint ipEndPoint;
    private int world;
    private int channel;

    public ChannelPlayerStorage Players { get; }
    public AbstractServer NettyServer { get; }

    private MapManager mapManager;
    private EventScriptManager eventSM;
    private ServicesManager<ChannelServices> services;
    private Dictionary<int, HiredMerchant> hiredMerchants = new();
    private Dictionary<int, int> storedVars = new();
    private HashSet<int> playersAway = new();
    private Dictionary<ExpeditionType, Expedition> expeditions = new();
    private Dictionary<int, MiniDungeon> dungeons = new();
    private HashSet<IMap> ownedMaps = new();
    private Event? @event;
    private HashSet<int> usedMC = new();

    public DateTimeOffset StartupTime { get; }

    public DojoInstance DojoInstance { get; }

    public WeddingChannelInstance WeddingInstance { get; }


    private object lockObj = new object();
    private ReaderWriterLockSlim merchLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

    // public IWorld WorldModel { get; set; }
    public event Action? OnWorldMobRateChanged;
    float _worldMobRate;
    public float WorldMobRate
    {
        get => _worldMobRate;
        private set
        {
            _worldMobRate = value;
            OnWorldMobRateChanged?.Invoke();
        }
    }

    public event Action? OnWorldMesoRateChanged;
    private float worldMesoRate;

    public float WorldMesoRate { get => worldMesoRate; private set { worldMesoRate = value; OnWorldMesoRateChanged?.Invoke(); } }
    private float worldExpRate;
    public event Action? OnWorldExpRateChanged;
    public float WorldExpRate { get => worldExpRate; private set { worldExpRate = value; OnWorldExpRateChanged?.Invoke(); } }
    public event Action? OnWorldDropRateChanged;
    private float worldDropRate;

    public float WorldDropRate { get => worldDropRate; private set { worldDropRate = value; OnWorldDropRateChanged?.Invoke(); } }
    public event Action? OnWorldBossDropRateChanged;
    private float worldBossDropRate;

    public float WorldBossDropRate { get => worldBossDropRate; private set { worldBossDropRate = value; OnWorldBossDropRateChanged?.Invoke(); } }
    public event Action? OnWorldQuestRateChanged;
    private float worldQuestRate;
    public float WorldQuestRate { get => worldQuestRate; private set { worldQuestRate = value; OnWorldQuestRateChanged?.Invoke(); } }
    public float WorldTravelRate { get; private set; }
    public float WorldFishingRate { get; private set; }
    public string WorldServerMessage { get; private set; }
    public IChannelServerTransport Transport { get; }
    public ChannelServerConfig ServerConfig { get; }
    public ServerMessageController ServerMessageController { get; }
    public CharacterHpDecreaseController CharacterHpDecreaseController { get; }
    public MapObjectController MapObjectController { get; }
    public MountTirednessController MountTirednessController { get; }
    public HiredMerchantController HiredMerchantController { get; }
    private WorldChannel(ChannelServerConfig config)
    {
        InstanceId = Guid.NewGuid().ToString();
        ServerConfig = config;
        WorldServerMessage = "";

        this.world = 0;

        Players = new ChannelPlayerStorage();

        this.mapManager = new MapManager(null, this);
        this.Port = config.Port;
        this.ipEndPoint = new IPEndPoint(IPAddress.Parse(config.Host), Port);

        NettyServer = new ChannelServer(this);
        log = LogFactory.GetLogger($"Channel_{InstanceId}");

        ServerMessageController = new ServerMessageController(this);
        CharacterHpDecreaseController = new CharacterHpDecreaseController(this);
        MapObjectController = new MapObjectController(this);
        MountTirednessController = new MountTirednessController(this);
        HiredMerchantController = new HiredMerchantController(this);

        DojoInstance = new DojoInstance(this);
        WeddingInstance = new WeddingChannelInstance(this);

        services = new ServicesManager<ChannelServices>(ChannelServices.OVERALL);
        eventSM = new EventScriptManager(this, ScriptResFactory.GetEvents());

        StartupTime = DateTimeOffset.Now;
    }
    public WorldChannel(ChannelServerConfig config, IChannelServerTransport transport) : this(config)
    {
        Transport = transport;
    }

    public WorldChannel(ChannelServerConfig config, IMasterServer server, IWorld world) : this(config)
    {
        Transport = new LocalChannelServerTransport(server, world, this);
    }

    public int getTransportationTime(double travelTime)
    {
        return (int)Math.Ceiling(travelTime / WorldTravelRate);
    }

    public void UpdateWorldConfig(WorldConfigPatch updatePatch)
    {
        if (updatePatch.MobRate.HasValue)
        {
            WorldMobRate = updatePatch.MobRate.Value;
        }
        if (updatePatch.MesoRate.HasValue)
        {
            WorldMesoRate = updatePatch.MesoRate.Value;
        }
        if (updatePatch.ExpRate.HasValue)
        {
            WorldExpRate = updatePatch.ExpRate.Value;
        }
        if (updatePatch.DropRate.HasValue)
        {
            WorldDropRate = updatePatch.DropRate.Value;
        }
        if (updatePatch.BossDropRate.HasValue)
        {
            WorldBossDropRate = updatePatch.BossDropRate.Value;
        }
        if (updatePatch.TravelRate.HasValue)
        {
            WorldTravelRate = updatePatch.TravelRate.Value;
        }
        if (updatePatch.FishingRate.HasValue)
        {
            WorldFishingRate = updatePatch.FishingRate.Value;
        }
        if (updatePatch.ServerMessage != null)
        {
            setServerMessage(updatePatch.ServerMessage);
        }
    }


    object loadEvetScriptLock = new object();
    public void reloadEventScriptManager()
    {
        lock (loadEvetScriptLock)
        {
            if (!IsRunning)
            {
                return;
            }

            eventSM.cancel();
            eventSM.dispose();
            eventSM = new EventScriptManager(this, ScriptResFactory.GetEvents());
        }
    }

    public async Task StartServer()
    {
        log.Information("频道服务器{InstanceId}启动中...", InstanceId);
        await NettyServer.Start();
        log.Information("频道服务器{InstanceId}启动成功：监听端口{Port}", InstanceId, Port);

        channel = await Transport.RegisterServer();
        log.Information("频道服务器{InstanceId}注册成功：频道号{Channel}", InstanceId, channel);

        ServerMessageController.Register();
        CharacterHpDecreaseController.Register();
        MapObjectController.Register();
        MountTirednessController.Register();
        HiredMerchantController.Register();

        IsRunning = true;
    }

    bool isShuttingDown = false;


    public async Task Shutdown()
    {
        try
        {
            if (isShuttingDown || !IsRunning)
            {
                return;
            }

            isShuttingDown = true;
            log.Information("正在停止频道{Channel}", channel);

            log.Information("频道{Channel}停止定时任务...", channel);
            await ServerMessageController.StopAsync();
            await CharacterHpDecreaseController.StopAsync();
            await MapObjectController.StopAsync();
            await MountTirednessController.StopAsync();
            await HiredMerchantController.StopAsync();
            log.Information("频道{Channel}停止定时任务...完成", channel);

            closeAllMerchants();
            disconnectAwayPlayers();
            Players.disconnectAll();

            eventSM.dispose();

            mapManager.Dispose();

            await closeChannelSchedules();

            await NettyServer.Stop();
            log.Information("频道{Channel}停止监听", channel);


            IsRunning = false;
            log.Information("Successfully shut down channel {ChannelId} in world {WorldId}", channel, world);
        }
        catch (Exception e)
        {
            log.Error(e, "Error while shutting down channel {ChannelId} in world {WorldId}", channel, world);
        }
        finally
        {
            isShuttingDown = false;
        }
    }

    private void closeChannelServices()
    {
        services.shutdown();
    }

    private async Task closeChannelSchedules()
    {
        await DojoInstance.DisposeAsync();

        closeChannelServices();
    }

    private void closeAllMerchants()
    {
        try
        {
            List<HiredMerchant> merchs;

            merchLock.EnterWriteLock();
            try
            {
                merchs = new(hiredMerchants.Values);
                hiredMerchants.Clear();
            }
            finally
            {
                merchLock.ExitWriteLock();
            }

            foreach (HiredMerchant merch in merchs)
            {
                merch.forceClose();
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }

    public MapManager getMapFactory()
    {
        return mapManager;
    }

    public BaseService getServiceAccess(ChannelServices sv)
    {
        return services.getAccess(sv).getService();
    }

    public int getWorld()
    {
        return world;
    }

    public void addPlayer(IPlayer chr)
    {
        Players.AddPlayer(chr);
        chr.sendPacket(PacketCreator.serverMessage(WorldServerMessage));
    }

    public string getServerMessage()
    {
        return WorldServerMessage;
    }

    public ChannelPlayerStorage getPlayerStorage()
    {
        return Players;
    }

    public bool removePlayer(IPlayer chr)
    {
        return Players.RemovePlayer(chr.Id) != null;
    }

    public int getChannelCapacity()
    {
        return (int)(Math.Ceiling(((float)Players.getAllCharacters().Count / YamlConfig.config.server.CHANNEL_LOAD) * 800));
    }

    public void broadcastPacket(Packet packet)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.sendPacket(packet);
        }
    }

    public int getId()
    {
        return channel;
    }

    public IPEndPoint getIP()
    {
        return ipEndPoint;
    }

    public Event? getEvent()
    {
        return @event;
    }

    public void setEvent(Event? evt)
    {
        this.@event = evt;
    }

    public EventScriptManager getEventSM()
    {
        return eventSM;
    }

    public void broadcastGMPacket(Packet packet)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            if (chr.isGM())
            {
                chr.sendPacket(packet);
            }
        }
    }
    public void insertPlayerAway(int chrId)
    {
        // either they in CS or MTS
        playersAway.Add(chrId);
    }

    public void removePlayerAway(int chrId)
    {
        playersAway.Remove(chrId);
    }

    public bool canUninstall()
    {
        return Players.Count() == 0 && playersAway.Count == 0;
    }

    private void disconnectAwayPlayers()
    {
        Transport.DisconnectPlayers(playersAway);
    }


    public int[] multiBuddyFind(int charIdFrom, int[] characterIds)
    {
        List<int> ret = new(characterIds.Length);
        var playerStorage = Players;
        foreach (int characterId in characterIds)
        {
            var chr = playerStorage.getCharacterById(characterId);
            if (chr != null)
            {
                if (chr.BuddyList.containsVisible(charIdFrom))
                {
                    ret.Add(characterId);
                }
            }
        }
        return ret.ToArray();
    }

    public bool addExpedition(Expedition exped)
    {
        lock (expeditions)
        {
            if (expeditions.TryAdd(exped.getType(), exped))
            {
                exped.beginRegistration();
                return true;
            }
            return false;
        }
    }

    public void removeExpedition(Expedition exped)
    {
        lock (expeditions)
        {
            expeditions.Remove(exped.getType());
        }
    }

    public Expedition? getExpedition(ExpeditionType type)
    {
        return expeditions.GetValueOrDefault(type);
    }

    public List<Expedition> getExpeditions()
    {
        lock (expeditions)
        {
            return new(expeditions.Values);
        }
    }

    public bool isConnected(string name)
    {
        return Players.getCharacterByName(name) != null;
    }

    public bool isActive()
    {
        EventScriptManager esm = this.getEventSM();
        return esm != null && esm.isActive();
    }

    public void setServerMessage(string message)
    {
        this.WorldServerMessage = message;
        broadcastPacket(PacketCreator.serverMessage(message));
        ServerMessageController.resetDisabledServerMessages();
    }

    public int getStoredVar(int key)
    {
        return storedVars.GetValueOrDefault(key);
    }

    public void setStoredVar(int key, int val)
    {
        this.storedVars[key] = val;
    }

    #region dojo
    public int lookupPartyDojo(ITeam? party)
    {
        return DojoInstance.LookupPartyDojo(party);
    }

    public int ingressDojo(bool isPartyDojo, int fromStage)
    {
        return ingressDojo(isPartyDojo, null, fromStage);
    }

    public int ingressDojo(bool isPartyDojo, ITeam? party, int fromStage)
    {
        return DojoInstance.IngressDojo(isPartyDojo, party, fromStage);
    }

    public void resetDojoMap(int fromMapId)
    {
        DojoInstance.ResetDojoMap(fromMapId);
    }

    public void resetDojo(int dojoMapId)
    {
        DojoInstance.ResetDojo(dojoMapId, -1);
    }

    public void freeDojoSectionIfEmpty(int dojoMapId)
    {
        DojoInstance.FreeDojoSectionIfEmpty(dojoMapId);
    }

    public void dismissDojoSchedule(int dojoMapId, ITeam party)
    {
        DojoInstance.DismissDojoSchedule(dojoMapId, party);
    }

    public bool setDojoProgress(int dojoMapId)
    {
        return DojoInstance.SetDojoProgress(dojoMapId);
    }

    public long getDojoFinishTime(int dojoMapId)
    {
        return DojoInstance.GetDojoFinishTime(dojoMapId);
    }
    #endregion

    public bool addMiniDungeon(int dungeonid)
    {
        Monitor.Enter(lockObj);
        try
        {
            if (dungeons.ContainsKey(dungeonid))
            {
                return false;
            }

            MiniDungeonInfo mmdi = MiniDungeonInfo.getDungeon(dungeonid);
            MiniDungeon mmd = new MiniDungeon(mmdi.getBase(), this.getMapFactory().getMap(mmdi.getDungeonId()).getTimeLimit());   // thanks Conrad for noticing hardcoded time limit for minidungeons

            dungeons.Add(dungeonid, mmd);
            return true;
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public MiniDungeon? getMiniDungeon(int dungeonid)
    {
        Monitor.Enter(lockObj);
        try
        {
            return dungeons.GetValueOrDefault(dungeonid);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    public void removeMiniDungeon(int dungeonid)
    {
        Monitor.Enter(lockObj);
        try
        {
            dungeons.Remove(dungeonid);
        }
        finally
        {
            Monitor.Exit(lockObj);
        }
    }

    #region wedding
    public bool isWeddingReserved(int weddingId)
    {
        return WeddingInstance.IsWeddingReserved(weddingId);
    }

    public int getWeddingReservationStatus(int? weddingId, bool cathedral)
    {
        return WeddingInstance.GetWeddingReservationStatus(weddingId, cathedral);
    }

    public int pushWeddingReservation(int? weddingId, bool cathedral, bool premium, int groomId, int brideId)
    {
        return WeddingInstance.PushWeddingReservation(weddingId, cathedral, premium, groomId, brideId);
    }

    public bool isOngoingWeddingGuest(bool cathedral, int playerId)
    {
        return WeddingInstance.IsOngoingWeddingGuest(cathedral, playerId);
    }

    public int getOngoingWedding(bool cathedral)
    {
        return WeddingInstance.GetOngoingWedding(cathedral);
    }

    public void closeOngoingWedding(bool cathedral)
    {
        WeddingInstance.CloseOngoingWedding(cathedral);
    }

    public bool acceptOngoingWedding(bool cathedral)
    {
        return WeddingInstance.AcceptOngoingWedding(cathedral);
    }

    public string? getWeddingReservationTimeLeft(int? weddingId)
    {
        return WeddingInstance.GetWeddingReservationTimeLeft(weddingId);
    }

    #endregion
    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().getAllCharacters())
        {
            player.dropMessage(type, message);
        }
    }

    public void registerOwnedMap(IMap map)
    {
        ownedMaps.Add(map);
    }

    public void unregisterOwnedMap(IMap map)
    {
        ownedMaps.Remove(map);
    }

    public void runCheckOwnedMapsSchedule()
    {
        if (ownedMaps.Count > 0)
        {
            List<IMap> ownedMapsList;

            lock (ownedMaps)
            {
                ownedMapsList = new(ownedMaps);
            }

            foreach (var map in ownedMapsList)
            {
                map.checkMapOwnerActivity();
            }
        }
    }

    private static int getMonsterCarnivalRoom(bool cpq1, int field)
    {
        return (cpq1 ? 0 : 100) + field;
    }

    public void initMonsterCarnival(bool cpq1, int field)
    {
        usedMC.Add(getMonsterCarnivalRoom(cpq1, field));
    }

    public void finishMonsterCarnival(bool cpq1, int field)
    {
        usedMC.Remove(getMonsterCarnivalRoom(cpq1, field));
    }

    public bool canInitMonsterCarnival(bool cpq1, int field)
    {
        return !usedMC.Contains(getMonsterCarnivalRoom(cpq1, field));
    }

}
