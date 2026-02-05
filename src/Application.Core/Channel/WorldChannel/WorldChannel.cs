using Application.Core.Channel.Commands;
using Application.Core.Channel.Invitation;
using Application.Core.Channel.Net;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Commands.Gm6;
using Application.Core.Game.Relation;
using Application.Core.Gameplay.ChannelEvents;
using Application.Core.ServerTransports;
using Application.Shared.Events;
using Application.Shared.Servers;
using Application.Utility.Performance;
using Application.Utility.Pipeline;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using net.server.services.task.channel;
using scripting.Event;
using scripting.map;
using scripting.npc;
using scripting.portal;
using scripting.quest;
using scripting.reactor;
using server.events.gm;
using server.expeditions;
using server.maps;
using System.Net;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Channel;

public partial class WorldChannel : ISocketServer, IClientMessenger, IActor<ChannelCommandContext>, INamedInstance
{
    public int Id => channel;
    public string InstanceName { get; }
    private ILogger log;
    public bool IsRunning { get; private set; }

    public int Port { get; set; }
    private IPEndPoint ipEndPoint;
    private int world;
    private int channel;

    public ChannelPlayerStorage Players { get; }
    public AbstractNettyServer NettyServer { get; }

    private MapManager mapManager;
    private EventScriptManager eventSM;
    public MapScriptManager MapScriptManager { get; }
    public ReactorScriptManager ReactorScriptManager { get; }
    public NPCScriptManager NPCScriptManager { get; }
    public PortalScriptManager PortalScriptManager { get; }
    public QuestScriptManager QuestScriptManager { get; }
    public DevtestScriptManager DevtestScriptManager { get; }


    private Dictionary<int, int> storedVars = new();
    /// <summary>
    /// 处于现金商城
    /// </summary>
    public Dictionary<int, Player> PlayersAway { get; set; } = new();
    private Dictionary<ExpeditionType, Expedition> expeditions = new();
    private Dictionary<int, MiniDungeon> dungeons = new();

    private Event? @event;
    private HashSet<int> usedMC = new();

    public DojoInstance DojoInstance { get; }


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


    public ChannelConfig ChannelConfig { get; }
    public PlayerShopManager PlayerShopManager { get; }
    public IServiceScope LifeScope { get; }


    #region
    public MobClearSkillService MobClearSkillService { get; }
    public MobMistService MobMistService { get; }
    public MobStatusService MobStatusService { get; }
    public OverallService OverallService { get; }
    #endregion
    public WorldChannelCommandLoop CommandLoop { get; }
    public EventRecallManager? EventRecallManager { get; private set; }

    RespawnTask? _respawnTask;

    public ChannelClientStorage ClientStorage { get; }
    public ChannelService Service { get; }
    public IServerBase<IChannelServerTransport> Node { get; }
    public IActor<ChannelNodeCommandContext> NodeActor { get; }
    public IServiceCenter NodeService { get; }
    public IMapper Mapper { get; }

    public CharacterDiseaseManager CharacterDiseaseManager { get; }
    public PetHungerManager PetHungerManager { get; }

    public CharacterHpDecreaseManager CharacterHpDecreaseManager { get; }
    public MapObjectManager MapObjectManager { get; }
    public MountTirednessManager MountTirednessManager { get; }
    public MapOwnershipManager MapOwnershipManager { get; }
    public ServerMessageManager ServerMessageManager { get; }
    public InviteChannelHandlerRegistry InviteChannelHandlerRegistry { get; }
    public Dictionary<int, Door?> PlayerDoors { get; }
    public WorldChannel(int channelId, WorldChannelServer serverContainer, IServiceScope scope, string serverHost, ChannelConfig config, NettyChannelServer nettyServer)
    {
        channel = channelId;
        NodeActor = serverContainer;
        Node = serverContainer;
        NodeService = serverContainer;
        LifeScope = scope;

        InstanceName = $"{serverContainer.InstanceName}:频道{channel}（{config.Port}）";
        ChannelConfig = config;
        WorldServerMessage = "";

        ClientStorage = new ChannelClientStorage(this);
        Service = ActivatorUtilities.CreateInstance<ChannelService>(LifeScope.ServiceProvider, this);

        this.world = 0;

        Players = new ChannelPlayerStorage();
        PlayerDoors = new();

        this.mapManager = new MapManager(null, this);
        this.Port = config.Port;
        this.ipEndPoint = new IPEndPoint(IPAddress.Parse(serverHost), Port);

        NettyServer = nettyServer;
        log = LogFactory.GetLogger(InstanceName);

        PlayerShopManager = ActivatorUtilities.CreateInstance<PlayerShopManager>(LifeScope.ServiceProvider, this);


        DojoInstance = new DojoInstance(this);

        MobClearSkillService = new MobClearSkillService(this);
        MobMistService = new MobMistService(this);
        MobStatusService = new MobStatusService(this);
        OverallService = new OverallService(this);

        eventSM = ActivatorUtilities.CreateInstance<EventScriptManager>(LifeScope.ServiceProvider, this);
        MapScriptManager = ActivatorUtilities.CreateInstance<MapScriptManager>(LifeScope.ServiceProvider, this);
        ReactorScriptManager = ActivatorUtilities.CreateInstance<ReactorScriptManager>(LifeScope.ServiceProvider, this);
        NPCScriptManager = ActivatorUtilities.CreateInstance<NPCScriptManager>(LifeScope.ServiceProvider, this);
        PortalScriptManager = ActivatorUtilities.CreateInstance<PortalScriptManager>(LifeScope.ServiceProvider, this);
        QuestScriptManager = ActivatorUtilities.CreateInstance<QuestScriptManager>(LifeScope.ServiceProvider, this);
        DevtestScriptManager = ActivatorUtilities.CreateInstance<DevtestScriptManager>(LifeScope.ServiceProvider, this);
        Mapper = LifeScope.ServiceProvider.GetRequiredService<IMapper>();

        CharacterDiseaseManager = new CharacterDiseaseManager(this);
        PetHungerManager = new PetHungerManager(this);

        CharacterHpDecreaseManager = new CharacterHpDecreaseManager(this);
        MapObjectManager = new MapObjectManager(this);
        MountTirednessManager = new MountTirednessManager(this);
        MapOwnershipManager = new MapOwnershipManager(this);
        ServerMessageManager = new ServerMessageManager(this);

        CommandLoop = new WorldChannelCommandLoop(this);
        InviteChannelHandlerRegistry = new();
    }

    public int getTransportationTime(double travelTime)
    {
        return (int)Math.Ceiling(travelTime / WorldTravelRate);
    }

    public void UpdateWorldConfig(Config.WorldConfig updatePatch)
    {
        if (updatePatch.MobRate.HasValue)
        {
            WorldMobRate = updatePatch.MobRate.Value;

            dropMessage(6, "[Rate] Mob Rate has been changed to " + WorldMobRate + "x.");
        }
        if (updatePatch.MesoRate.HasValue)
        {
            WorldMesoRate = updatePatch.MesoRate.Value;

            dropMessage(6, "[Rate] Meso Rate has been changed to " + WorldMesoRate + "x.");
        }
        if (updatePatch.ExpRate.HasValue)
        {
            WorldExpRate = updatePatch.ExpRate.Value;

            dropMessage(6, "[Rate] Exp Rate has been changed to " + WorldExpRate + "x.");
        }
        if (updatePatch.DropRate.HasValue)
        {
            WorldDropRate = updatePatch.DropRate.Value;

            dropMessage(6, "[Rate] Drop Rate has been changed to " + WorldDropRate + "x.");
        }
        if (updatePatch.QuestRate.HasValue)
        {
            WorldQuestRate = updatePatch.QuestRate.Value;

            dropMessage(6, "[Rate] Quest Rate has been changed to " + WorldQuestRate + "x.");
        }
        if (updatePatch.BossDropRate.HasValue)
        {
            WorldBossDropRate = updatePatch.BossDropRate.Value;

            dropMessage(6, "[Rate] Boss Drop Rate has been changed to " + WorldBossDropRate + "x.");
        }
        if (updatePatch.TravelRate.HasValue)
        {
            WorldTravelRate = updatePatch.TravelRate.Value;

            dropMessage(6, "[Rate] Travel Rate has been changed to " + WorldTravelRate + "x.");
        }
        if (updatePatch.FishingRate.HasValue)
        {
            WorldFishingRate = updatePatch.FishingRate.Value;

            dropMessage(6, "[Rate] Fishing Rate has been changed to " + WorldFishingRate + "x.");
        }
        if (updatePatch.ServerMessage != null)
        {
            setServerMessage(updatePatch.ServerMessage);
        }
    }


    public void reloadEventScriptManager()
    {
        if (!IsRunning)
        {
            return;
        }

        eventSM.dispose();
        eventSM = ActivatorUtilities.CreateInstance<EventScriptManager>(LifeScope.ServiceProvider, this);
        eventSM.ReloadEventScript();
    }
    public void Initialize(Config.RegisterServerResult config)
    {
        log.Information("[{ServerName}] 初始化...", InstanceName);

        UpdateWorldConfig(config.Config);
        log.Information("[{ServerName}] 初始化世界倍率-完成。怪物倍率：x{MobRate}，金币倍率：x{MesoRate}，经验倍率：x{ExpRate}，掉落倍率：x{DropRate}，BOSS掉落倍率：x{BossDropRate}，任务倍率：x{QuestRate}，传送时间倍率：x{TravelRate}，钓鱼倍率：x{FishingRate}。",
            InstanceName, WorldMobRate, WorldMesoRate, WorldExpRate, WorldDropRate, WorldBossDropRate, WorldQuestRate, WorldTravelRate, WorldFishingRate);

        log.Information("[{ServerName}] 初始化事件...", InstanceName);
        var loadedEventsCount = eventSM.ReloadEventScript();
        log.Information("[{ServerName}] 初始化事件（{EventCount}项）...完成", InstanceName, loadedEventsCount);

        _respawnTask = new RespawnTask(this);
        _respawnTask.Register(Node.TimerManager);

        EventRecallManager = new EventRecallManager(this);
        EventRecallManager.Register(Node.TimerManager);

        var inviteHandlerLogger = LifeScope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InviteChannelHandler>>();
        InviteChannelHandlerRegistry.Register(new PartyInviteChannelHandler(this, inviteHandlerLogger));
        InviteChannelHandlerRegistry.Register(new GuildInviteChannelHandler(this, inviteHandlerLogger));
        InviteChannelHandlerRegistry.Register(new AllianceInviteChannelHandler(this, inviteHandlerLogger));
        InviteChannelHandlerRegistry.Register(new MessengerInviteChannelHandler(this, inviteHandlerLogger));

        log.Information("[{ServerName}] 初始化完成", InstanceName);

        log.Information("[{ServerName}] 已启动，当前服务器时间{ServerCurrentTime}，本地时间{LocalCurrentTime}",
            InstanceName,
            DateTimeOffset.FromUnixTimeMilliseconds(Node.getCurrentTime()).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            DateTimeOffset.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public Task StartServer(CancellationToken cancellationToken)
    {
        IsRunning = true;
        log.Information("[{ServerName}] 启动成功：监听端口{Port}", InstanceName, Port);

        CommandLoop.Start(InstanceName);
        return Task.CompletedTask;
    }


    bool isShuttingDown = false;


    public async Task Shutdown(int delaySeconds = -1)
    {
        try
        {
            if (isShuttingDown || !IsRunning)
            {
                return;
            }

            isShuttingDown = true;
            log.Information("[{ServerName}] 停止中...", InstanceName);

            log.Information("[{ServerName}] 停止定时任务...", InstanceName);

            if (_respawnTask != null)
            {
                await _respawnTask.StopAsync();
            }

            log.Information("[{ServerName}] 停止定时任务>>>完成", InstanceName);

            await disconnectAwayPlayers();
            await Players.disconnectAll(true);
            await PlayerShopManager.DisposeAsync();

            eventSM.dispose();

            mapManager.Dispose();

            await closeChannelSchedules();

            await NettyServer.Stop();
            log.Information("[{ServerName}] 停止监听", InstanceName);
            await CommandLoop.DisposeAsync();
            LifeScope.Dispose();

            IsRunning = false;
            log.Information("[{ServerName}] 已停止", InstanceName);
        }
        catch (Exception e)
        {
            log.Error(e, "[{ServerName}] 停止失败", InstanceName);
        }
        finally
        {
            isShuttingDown = false;
        }
    }

    private void closeChannelServices()
    {
        MobClearSkillService.dispose();
        MobMistService.dispose();
        MobStatusService.dispose();
        OverallService.dispose();
    }

    private async Task closeChannelSchedules()
    {
        await DojoInstance.DisposeAsync();

        closeChannelServices();
    }

    public MapManager getMapFactory()
    {
        return mapManager;
    }

    public int GetActiveMapCount()
    {
        return getMapFactory().getMaps().Count + getEventSM().GetEventMaps();
    }

    public int getWorld()
    {
        return world;
    }

    public void addPlayer(Player chr)
    {
        if (PlayersAway.Remove(chr.Id))
            GameMetrics.OnlinePlayerCount.Add(-1);

        Players.AddPlayer(chr);
        GameMetrics.OnlinePlayerCount.Add(1, new KeyValuePair<string, object?>("Channel", InstanceName));
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

    public bool RemovePlayer(int chrId)
    {
        var chr = Players.RemovePlayer(chrId);
        if (chr != null)
        {
            chr.getMap().removePlayer(chr);

            GameMetrics.OnlinePlayerCount.Add(-1, new KeyValuePair<string, object?>("Channel", InstanceName));
            return true;
        }

        return false;
    }

    public int getChannelCapacity()
    {
        return (int)(Math.Ceiling(((float)Players.getAllCharacters().Count / ChannelConfig.MaxSize) * 800));
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
    public void EnterExtralWorld(Player chr)
    {
        // either they in CS or MTS
        if (PlayersAway.TryAdd(chr.Id, chr))
            GameMetrics.OnlinePlayerCount.Add(1);

        RemovePlayer(chr.Id);
    }

    public void RemovePlayerDeep(Player chr)
    {
        if (PlayersAway.Remove(chr.Id))
            GameMetrics.OnlinePlayerCount.Add(-1);

        RemovePlayer(chr.Id);
    }

    public bool IsAwayFromWorld(int id) => PlayersAway.ContainsKey(id);

    public bool canUninstall()
    {
        return Players.Count() == 0 && PlayersAway.Count == 0;
    }

    private async Task disconnectAwayPlayers()
    {
        foreach (var chr in PlayersAway.Values)
        {
            if (chr != null && chr.isLoggedinWorld())
            {
                chr.getClient().ForceDisconnect();
            }
        }
    }

    public bool addExpedition(Expedition exped)
    {
        if (expeditions.TryAdd(exped.getType(), exped))
        {
            exped.beginRegistration();
            return true;
        }
        return false;
    }

    public void removeExpedition(Expedition exped)
    {
        expeditions.Remove(exped.getType());
    }

    public Expedition? getExpedition(ExpeditionType type)
    {
        return expeditions.GetValueOrDefault(type);
    }

    public List<Expedition> getExpeditions()
    {
        return new(expeditions.Values);
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
        ServerMessageManager.resetDisabledServerMessages();
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
    public int lookupPartyDojo(Team? party)
    {
        return DojoInstance.LookupPartyDojo(party);
    }

    public int ingressDojo(bool isPartyDojo, int fromStage)
    {
        return ingressDojo(isPartyDojo, null, fromStage);
    }

    public int ingressDojo(bool isPartyDojo, Team? party, int fromStage)
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

    public void dismissDojoSchedule(int dojoMapId, Team party)
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
        if (dungeons.ContainsKey(dungeonid))
        {
            return false;
        }

        MiniDungeonInfo mmdi = MiniDungeonInfo.getDungeon(dungeonid);
        MiniDungeon mmd = new MiniDungeon(this, mmdi.getBase(), this.getMapFactory().getMap(mmdi.getDungeonId()).getTimeLimit());   // thanks Conrad for noticing hardcoded time limit for minidungeons

        dungeons.Add(dungeonid, mmd);
        return true;
    }

    public MiniDungeon? getMiniDungeon(int dungeonid)
    {
        return dungeons.GetValueOrDefault(dungeonid);
    }

    public void removeMiniDungeon(int dungeonid)
    {
        dungeons.Remove(dungeonid);
    }

    #region wedding
    //public bool isWeddingReserved(int weddingId)
    //{
    //    return WeddingInstance.IsWeddingReserved(weddingId);
    //}

    //public int getWeddingReservationStatus(int? weddingId, bool cathedral)
    //{
    //    return WeddingInstance.GetWeddingReservationStatus(weddingId, cathedral);
    //}

    //public int pushWeddingReservation(int? weddingId, bool cathedral, bool premium, int groomId, int brideId)
    //{
    //    return WeddingInstance.PushWeddingReservation(weddingId, cathedral, premium, groomId, brideId);
    //}

    //public bool isOngoingWeddingGuest(bool cathedral, int playerId)
    //{
    //    return WeddingInstance.IsOngoingWeddingGuest(cathedral, playerId);
    //}

    //public int getOngoingWedding(bool cathedral)
    //{
    //    return WeddingInstance.GetOngoingWedding(cathedral);
    //}

    //public void closeOngoingWedding(bool cathedral)
    //{
    //    WeddingInstance.CloseOngoingWedding(cathedral);
    //}

    //public bool acceptOngoingWedding(bool cathedral)
    //{
    //    return WeddingInstance.AcceptOngoingWedding(cathedral);
    //}

    //public string? getWeddingReservationTimeLeft(int? weddingId)
    //{
    //    return WeddingInstance.GetWeddingReservationTimeLeft(weddingId);
    //}

    #endregion
    public void dropMessage(int type, string message)
    {
        foreach (var player in getPlayerStorage().getAllCharacters())
        {
            player.dropMessage(type, message);
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

    public IPEndPoint GetChannelEndPoint(int channel)
    {
        if (channel == getId())
            return getIP();

        return NodeService.GetChannelEndPoint(channel);
    }

    public void TypedMessage(int type, string messageKey, params string[] param)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.TypedMessage(type, messageKey, param);
        }
    }

    public void Notice(string key, params string[] param)
    {
        TypedMessage(0, key, param);
    }

    public void Popup(string key, params string[] param)
    {
        TypedMessage(1, key, param);
    }

    public void Dialog(string key, params string[] param)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.Dialog(key, param);
        }
    }

    public void Pink(string key, params string[] param)
    {
        TypedMessage(5, key, param);
    }

    public void LightBlue(string key, params string[] param)
    {
        TypedMessage(6, key, param);
    }

    public void LightBlue(Func<ClientCulture, string> action)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.LightBlue(action);
        }
    }

    public void TopScrolling(string key, params string[] param)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.TopScrolling(key, param);
        }
    }

    public void Yellow(string key, params string[] param)
    {
        foreach (var chr in Players.getAllCharacters())
        {
            chr.Yellow(key, param);
        }
    }

    public void Post(ICommand<ChannelCommandContext> command)
    {
        CommandLoop.Register(command);
    }
}
