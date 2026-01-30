using Application.Core.Channel.Commands;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.DueyService;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Net;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Skills;
using Application.Core.ServerTransports;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Utility.Pipeline;
using Config;
using Google.Protobuf;
using ItemProto;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncProto;
using System.Diagnostics;
using System.Net;

namespace Application.Core.Channel
{
    public class WorldChannelServer : IServerBase<IChannelServerTransport>, IActor<ChannelNodeCommandContext>, IServiceCenter
    {
        public IServiceProvider ServiceProvider { get; }
        public IChannelServerTransport Transport { get; }
        public Dictionary<int, WorldChannel> Servers { get; set; }
        public DistributeSession<int, SyncProto.PlayerSaveDto>? SyncPlayerSession { get; set; }
        public DistributeSession<int, ItemProto.SyncPlayerShopRequest>? SyncPlayerShopSession { get; set; }
        public Dictionary<ChannelConfig, WorldChannel> ServerConfigMapping { get; private set; }

        public bool IsRunning { get; private set; }

        public ChannelServerConfig ServerConfig { get; set; }
        public string ServerName => ServerConfig.ServerName;
        Lazy<SkillbookInformationProvider> _skillbookInformationProvider;
        public SkillbookInformationProvider SkillbookInformationProvider => _skillbookInformationProvider.Value;
        public CashItemProvider CashItemProvider { get; }
        readonly ILogger<WorldChannelServer> _logger;

        public DateTimeOffset StartupTime { get; private set; }
        public ITimerManager TimerManager { get; private set; } = null!;

        #region Data
        readonly Lazy<GuildManager> _guildManager;
        public GuildManager GuildManager => _guildManager.Value;

        readonly Lazy<TeamManager> _teamManager;
        public TeamManager TeamManager => _teamManager.Value;
        readonly Lazy<BuddyManager> _buddyManager;
        public BuddyManager BuddyManager => _buddyManager.Value;

        readonly Lazy<ShopManager> _shopManager;
        public ShopManager ShopManager => _shopManager.Value;


        readonly Lazy<ChatRoomService> _chatRoomService;
        public ChatRoomService ChatRoomService => _chatRoomService.Value;
        readonly Lazy<NewYearCardService> _newYearService;
        public NewYearCardService NewYearCardService => _newYearService.Value;

        readonly Lazy<DataService> _dataService;
        public DataService DataService => _dataService.Value;
        readonly Lazy<IPlayerNPCService> _playerNPCService;
        public IPlayerNPCService PlayerNPCService => _playerNPCService.Value;
        readonly Lazy<IMarriageService> _marriageService;
        public IMarriageService MarriageService => _marriageService.Value;

        readonly Lazy<ItemService> _itemService;
        public ItemService ItemService => _itemService.Value;
        readonly Lazy<PlayerShopService> _playerShopService;
        public PlayerShopService PlayerShopService => _playerShopService.Value;
        readonly Lazy<MonitorManager> _monitorManager;
        public MonitorManager MonitorManager => _monitorManager.Value;
        readonly Lazy<AutoBanDataManager> _autoBanManager;
        public AutoBanDataManager AutoBanManager => _autoBanManager.Value;
        readonly Lazy<AdminService> _adminService;
        public AdminService AdminService => _adminService.Value;
        readonly Lazy<CrossServerCallbackService> _remoteCallService;
        public CrossServerCallbackService RemoteCallService => _remoteCallService.Value;

        readonly Lazy<GachaponManager> _gachaponManager;
        public GachaponManager GachaponManager => _gachaponManager.Value;

        readonly Lazy<DueyManager> _dueyManager;
        public DueyManager DueyManager => _dueyManager.Value;

        Lazy<IItemDistributeService> _itemDistributeService;
        public IItemDistributeService ItemDistributeService => _itemDistributeService.Value;
        Lazy<IFishingService> _fishingService;
        public IFishingService FishingService => _fishingService.Value;
        #endregion

        #region Task
        public ServerMessageTask ServerMessageTask { get; }

        public MountTirednessTask MountTirednessTask { get; }
        public MapObjectTask MapObjectTask { get; }
        public CharacterDiseaseTask CharacterDiseaseTask { get; }
        public CharacterHpDecreaseTask CharacterHpDecreaseTask { get; }
        public PetHungerTask PetHungerTask { get; }
        public MapOwnershipTask MapOwnershipTask { get; }
        #endregion

        #region GameConfig
        public float WorldMobRate { get; private set; }

        public float WorldMesoRate { get; private set; }
        public float WorldExpRate { get; private set; }
        public float WorldDropRate { get; private set; }
        public float WorldBossDropRate { get; private set; }
        public float WorldQuestRate { get; private set; }
        public float WorldTravelRate { get; private set; }
        public float WorldFishingRate { get; private set; }
        public string WorldServerMessage { get; private set; }

        public Dictionary<int, int> CouponRates { get; set; } = new(30);
        public List<int> ActiveCoupons { get; set; } = new();

        #endregion
        public List<AbstractChannelModule> Modules { get; private set; }

        public ExpeditionService ExpeditionService { get; }
        public ChannelPlayerStorage PlayerStorage { get; }
        Lazy<MessageDispatcherNew> _messageDispatcher;
        public MessageDispatcherNew MessageDispatcherV => _messageDispatcher.Value;


        ScheduledFuture? invitationTask;
        ScheduledFuture? playerShopTask;
        ScheduledFuture? timeoutTask;
        ScheduledFuture? checkMapActiveTask;

        public BatchSyncManager<int, SyncProto.MapSyncDto> BatchSynMapManager { get; }
        public BatchSyncManager<int, SyncProto.PlayerSaveDto> BatchSyncPlayerManager { get; }

        public ChannelNodeCommandLoop CommandLoop { get; }
        public WorldChannelServer(IServiceProvider sp,
            IChannelServerTransport transport,
            IOptions<ChannelServerConfig> serverConfigOptions,
            ILogger<WorldChannelServer> logger,
            CashItemProvider cashItemProvider
            )
        {
            ServiceProvider = sp;
            Transport = transport;
            _logger = logger;

            Modules = new();
            Servers = new();
            ServerConfigMapping = new();
            ServerConfig = serverConfigOptions.Value;
            PlayerStorage = new();

            _skillbookInformationProvider = new(() => ServiceProvider.GetRequiredService<SkillbookInformationProvider>());
            CashItemProvider = cashItemProvider;


            ServerMessageTask = new ServerMessageTask(this);
            MountTirednessTask = new MountTirednessTask(this);
            MapObjectTask = new MapObjectTask(this);
            CharacterDiseaseTask = new CharacterDiseaseTask(this);
            CharacterHpDecreaseTask = new CharacterHpDecreaseTask(this);
            PetHungerTask = new(this);
            MapOwnershipTask = new(this);

            ExpeditionService = ServiceProvider.GetRequiredService<ExpeditionService>();

            _buddyManager = new(() => ServiceProvider.GetRequiredService<BuddyManager>());
            _guildManager = new Lazy<GuildManager>(() => ServiceProvider.GetRequiredService<GuildManager>());
            _teamManager = new(() => ServiceProvider.GetRequiredService<TeamManager>());
            _shopManager = new(() => ServiceProvider.GetRequiredService<ShopManager>());
            _monitorManager = new(() => ServiceProvider.GetRequiredService<MonitorManager>());
            _autoBanManager = new(() => ServiceProvider.GetRequiredService<AutoBanDataManager>());
            _gachaponManager = new(() => ServiceProvider.GetRequiredService<GachaponManager>());
            _dueyManager = new(() => ServiceProvider.GetRequiredService<DueyManager>());

            _adminService = new(() => ServiceProvider.GetRequiredService<AdminService>());
            _marriageService = new(() => ServiceProvider.GetRequiredService<IMarriageService>());
            _chatRoomService = new Lazy<ChatRoomService>(() => ServiceProvider.GetRequiredService<ChatRoomService>());
            _newYearService = new(() => ServiceProvider.GetRequiredService<NewYearCardService>());
            _dataService = new(() => ServiceProvider.GetRequiredService<DataService>());
            _playerNPCService = new(() => ServiceProvider.GetRequiredService<IPlayerNPCService>());
            _itemService = new(() => ServiceProvider.GetRequiredService<ItemService>());
            _playerShopService = new(() => ServiceProvider.GetRequiredService<PlayerShopService>());
            _remoteCallService = new(() => ServiceProvider.GetRequiredService<CrossServerCallbackService>());
            _itemDistributeService = new(() => ServiceProvider.GetRequiredService<IItemDistributeService>());
            _fishingService = new(() => ServiceProvider.GetRequiredService<IFishingService>());

            BatchSynMapManager = new BatchSyncManager<int, SyncProto.MapSyncDto>(50, 100, x => x.MasterId, data => Transport.BatchSyncMap(data));
            BatchSyncPlayerManager = new BatchSyncManager<int, SyncProto.PlayerSaveDto>(50, 100, x => x.Character.Id, data => Transport.BatchSyncPlayer(data));

            _messageDispatcher = new(() => new(this));
            CommandLoop = new ChannelNodeCommandLoop(this);
        }

        #region 时间
        private AtomicLong currentTime = new AtomicLong(0);
        private long serverCurrentTime = 0;

        public int getCurrentTimestamp()
        {
            return Transport.GetCurrentTimestamp();
        }

        public long getCurrentTime()
        {
            return serverCurrentTime;
        }

        public DateTimeOffset GetCurrentTimeDateTimeOffset()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(serverCurrentTime);
        }
        public void UpdateServerTime()
        {
            serverCurrentTime = currentTime.addAndGet(YamlConfig.config.server.UPDATE_INTERVAL);
        }

        public void ForceUpdateServerTime()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var forceTime = Transport.GetCurrentTime();
            sw.Stop();
            forceTime = forceTime + sw.ElapsedMilliseconds;
            serverCurrentTime = forceTime;
            currentTime.set(forceTime);
        }
        #endregion
        public void UpdateCouponConfig(Config.CouponConfig config)
        {
            ActiveCoupons = config.ActiveCoupons.ToList();
            CouponRates = config.CouponRates.ToDictionary();
        }

        public List<int> GetActiveCoupons() => ActiveCoupons;
        public Dictionary<int, int> GetCouponRates() => CouponRates;

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public async Task Shutdown(int delaySeconds = -1)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (!IsRunning)
                {
                    _logger.LogInformation("[{ServerName}] 未启动", ServerName);
                    return;
                }
                _logger.LogInformation("[{ServerName}] 正在停止...", ServerName);

                await CharacterDiseaseTask.StopAsync();
                await PetHungerTask.StopAsync();
                await MapOwnershipTask.StopAsync();
                await ServerMessageTask.StopAsync();
                await CharacterHpDecreaseTask.StopAsync();
                await MapObjectTask.StopAsync();
                await MountTirednessTask.StopAsync();

                if (invitationTask != null)
                    await invitationTask.CancelAsync(false);
                if (playerShopTask != null)
                    await playerShopTask.CancelAsync(false);
                if (timeoutTask != null)
                    await timeoutTask.CancelAsync(false);
                if (checkMapActiveTask != null)
                    await checkMapActiveTask.CancelAsync(false);

                foreach (var module in Modules)
                {
                    await module.UninstallAsync();
                }

                PushChannelCommand(new InvokeChannelShutdownCommand());

                await TimerManager.Stop();
                await CommandLoop.DisposeAsync();
                _logger.LogInformation("[{ServerName}] 停止{Status}", ServerName, "成功");

                // 有些玩家在CashShop
                await PlayerStorage.disconnectAll(true);

                await Transport.CompleteChannelShutdown();
                IsRunning = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ServerName}] 停止{Status}", ServerName, "失败");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        Dictionary<ChannelConfig, NettyChannelServer> effectChannels = new();
        public async Task StartServer(CancellationToken cancellationToken)
        {
            if (IsRunning)
                return;

            if (!Directory.Exists(ScriptSource.Instance.BaseDir))
                throw new DirectoryNotFoundException("没有找到Script目录");

            CommandLoop.Start(ServerName);

            foreach (var item in ServerConfig.ChannelConfig)
            {
                var nettyServer = new NettyChannelServer(this, item);
                try
                {
                    await nettyServer.Start();
                    effectChannels[item] = nettyServer;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "频道服务器监听失败，已跳过端口{Port}", item.Port);
                }
            }

            if (effectChannels.Count == 0)
                throw new BusinessFatalException("必须包含有效的频道");

            await Transport.RegisterServer(effectChannels.Keys.ToList());
        }

        public async Task<bool> HandleServerRegistered(RegisterServerResult configs, CancellationToken cancellationToken = default)
        {
            if (configs.StartChannel <= 0)
            {
                _logger.LogError("注册服务器失败, {Message}", configs.Message);
                return false;
            }

            IsRunning = true;
            TimerManager = await TimerManagerFactory.InitializeAsync(TaskEngine.Quartz, ServerName);

            OpcodeConstants.generateOpcodeNames();
            ForceUpdateServerTime();

            var channel = configs.StartChannel;
            foreach (var server in effectChannels)
            {
                var scope = ServiceProvider.CreateScope();
                var worldChannel = new WorldChannel(channel, this, scope, ServerConfig.ServerHost, server.Key, server.Value);
                worldChannel.Initialize(configs);

                Servers[channel] = worldChannel;
                ServerConfigMapping[server.Key] = worldChannel;

                channel++;
                await worldChannel.StartServer(cancellationToken);
            }

            DataService.LoadAllPLife();
            DataService.LoadAllReactorDrops();

            foreach (var item in ServiceProvider.GetServices<DataBootstrap>())
            {
                _ = Task.Run(() =>
                {
                    item.LoadData();
                }, cancellationToken);
            }

            _ = Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                SkillFactory.LoadAllSkills();
                sw.Stop();
                _logger.LogDebug("WZ - 技能加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
            }, cancellationToken);


            CharacterDiseaseTask.Register(TimerManager);
            PetHungerTask.Register(TimerManager);
            ServerMessageTask.Register(TimerManager);
            CharacterHpDecreaseTask.Register(TimerManager);
            MapObjectTask.Register(TimerManager);
            MountTirednessTask.Register(TimerManager);
            MapOwnershipTask.Register(TimerManager);

            invitationTask = TimerManager.register(new InvitationTask(this), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            playerShopTask = TimerManager.register(new PlayerShopTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            if (ServerConfig.SystemConfig.AutoClearMap)
            {
                checkMapActiveTask = TimerManager.register(new DisposeCheckTask(this), TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            }
#if !DEBUG
            timeoutTask = TimerManager.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
#endif


            foreach (var module in Modules)
            {
                module.Initialize();
                module.RegisterTask(TimerManager);
            }

            return true;
        }

        public Player? FindPlayerById(int channel, int cid)
        {
            if (cid <= 0)
                return null;

            if (Servers.TryGetValue(channel, out var ch))
                return ch.Players.getCharacterById(cid);

            return null;
        }

        public WorldChannel? GetChannel(int channel)
        {
            return Servers.GetValueOrDefault(channel);
        }

        public AccountLoginStatus UpdateAccountState(int accId, sbyte state)
        {
            return Transport.UpdateAccountState(accId, state);
        }

        public void SetCharacteridInTransition(string v, int cid)
        {
            if (YamlConfig.config.server.USE_IP_VALIDATION)
                Transport.SetCharacteridInTransition(v, cid);
        }

        public bool HasCharacteridInTransition(string clientSession)
        {
            return Transport.HasCharacteridInTransition(clientSession);
        }

        public void InvokeBroadcastPacket(Packet p)
        {
            PushChannelCommand(new InvokeChannelBroadcastCommand([-1], p));
        }


        public void SendBroadcastWorldPacket(Packet p, bool onGM = false)
        {
            _ = Transport.BroadcastMessage(new PacketRequest { Data = ByteString.CopyFrom(p.getBytes()), OnlyGM = onGM });
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">-1. yellow, -2. earntitle, 1. popup 2.</param>
        /// <param name="message"></param>
        /// <param name="onlyGM"></param>
        /// <returns></returns>
        public void SendDropMessage(int type, string message, bool onlyGM = false)
        {
            _ = Transport.DropWorldMessage(new MessageProto.DropMessageRequest { Type = type, Message = message, OnlyGM = onlyGM });
        }

        public void EarnTitleMessage(string message, bool onlyGM)
        {
            SendDropMessage(-2, message, onlyGM);
        }

        public void UpdateWorldConfig(Config.WorldConfig updatePatch)
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
            if (updatePatch.QuestRate.HasValue)
            {
                WorldQuestRate = updatePatch.QuestRate.Value;
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
                WorldServerMessage = updatePatch.ServerMessage;
            }
        }


        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return Transport.GetChannelEndPoint(channel);
        }

        public bool CheckCharacterName(string name)
        {
            return Transport.CheckCharacterName(name);
        }

        public void SendReloadEvents(Player chr)
        {
            _ = Transport.SendReloadEvents(new Dto.ReloadEventsRequest { MasterId = chr.Id });
        }

        public void PushChannelCommand(IWorldChannelCommand command)
        {
            foreach (var item in Servers.Values)
            {
                item.Post(command);
            }
        }

        public void Post(ICommand<ChannelNodeCommandContext> command)
        {
            CommandLoop.Register(command);
        }

        internal DistributeSession<int, PlayerSaveDto> CreateSyncPlayerSession()
        {
            return new DistributeSession<int, PlayerSaveDto>(Servers.Values.Select(x => x.Id));
        }

        internal DistributeSession<int, SyncPlayerShopRequest> CreateSyncPlayerShopSession()
        {
            return new DistributeSession<int, SyncPlayerShopRequest>(Servers.Values.Select(x => x.Id));
        }
    }
}
