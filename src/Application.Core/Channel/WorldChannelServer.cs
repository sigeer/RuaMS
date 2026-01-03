using Application.Core.Channel.DataProviders;
using Application.Core.Channel.DueyService;
using Application.Core.Channel.Internal;
using Application.Core.Channel.Invitation;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Net;
using Application.Core.Channel.Performance;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Skills;
using Application.Core.ServerTransports;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Config;
using Dto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.server.task;
using Polly;
using server;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using tools;

namespace Application.Core.Channel
{
    public class WorldChannelServer : IServerBase<IChannelServerTransport>
    {
        public IServiceProvider ServiceProvider { get; }
        public IChannelServerTransport Transport { get; }
        public Dictionary<int, WorldChannel> Servers { get; set; }
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
        readonly Lazy<NoteService> _noteService;
        public NoteService NoteService => _noteService.Value;
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
        #endregion

        #region Task
        public CharacterDiseaseManager CharacterDiseaseManager { get; }
        public PetHungerManager PetHungerManager { get; }
        public ServerMessageManager ServerMessageManager { get; }
        public CharacterHpDecreaseManager CharacterHpDecreaseManager { get; }
        public MapObjectManager MapObjectManager { get; }
        public MountTirednessManager MountTirednessManager { get; }
        public MapOwnershipManager MapOwnershipManager { get; }
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
        public InviteChannelHandlerRegistry InviteChannelHandlerRegistry { get; }

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

            CharacterDiseaseManager = new CharacterDiseaseManager(this);
            PetHungerManager = new PetHungerManager(this);
            ServerMessageManager = new ServerMessageManager(this);
            CharacterHpDecreaseManager = new CharacterHpDecreaseManager(this);
            MapObjectManager = new MapObjectManager(this);
            MountTirednessManager = new MountTirednessManager(this);
            MapOwnershipManager = new MapOwnershipManager(this);

            ExpeditionService = ServiceProvider.GetRequiredService<ExpeditionService>();

            InviteChannelHandlerRegistry = ServiceProvider.GetRequiredService<InviteChannelHandlerRegistry>();

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
            _noteService = new(() => ServiceProvider.GetRequiredService<NoteService>());
            _dataService = new(() => ServiceProvider.GetRequiredService<DataService>());
            _playerNPCService = new(() => ServiceProvider.GetRequiredService<IPlayerNPCService>());
            _itemService = new(() => ServiceProvider.GetRequiredService<ItemService>());
            _playerShopService = new(() => ServiceProvider.GetRequiredService<PlayerShopService>());
            _remoteCallService = new(() => ServiceProvider.GetRequiredService<CrossServerCallbackService>());

            BatchSynMapManager = new BatchSyncManager<int, SyncProto.MapSyncDto>(50, 100, x => x.MasterId, data => Transport.BatchSyncMap(data));
            BatchSyncPlayerManager = new BatchSyncManager<int, SyncProto.PlayerSaveDto>(50, 100, x => x.Character.Id, data => Transport.BatchSyncPlayer(data));

            _messageDispatcher = new(() => new(this));
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

        public DateTimeOffset GetCurrentTimeDateTimeOffSet()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(serverCurrentTime);
        }
        public void UpdateServerTime()
        {
            serverCurrentTime = currentTime.addAndGet(YamlConfig.config.server.UPDATE_INTERVAL);
        }

        public bool canEnterDeveloperRoom()
        {
            return AdminService.GetServerStats().IsDevRoomAvailable;
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

            foreach (var ch in Servers.Values)
            {
                foreach (var chr in ch.getPlayerStorage().getAllCharacters())
                {
                    chr.updateCouponRates();
                }
            }
        }

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

                await CharacterDiseaseManager.StopAsync();
                await PetHungerManager.StopAsync();
                await MapOwnershipManager.StopAsync();
                await ServerMessageManager.StopAsync();
                await CharacterHpDecreaseManager.StopAsync();
                await MapObjectManager.StopAsync();
                await MountTirednessManager.StopAsync();

                if (invitationTask != null)
                    await invitationTask.CancelAsync(false);
                if (playerShopTask != null)
                    await playerShopTask.CancelAsync(false);
                if (timeoutTask != null)
                    await timeoutTask.CancelAsync(false);
                if (checkMapActiveTask != null)
                    await checkMapActiveTask.CancelAsync(false);

                InviteChannelHandlerRegistry.Dispose();

                foreach (var module in Modules)
                {
                    await module.UninstallAsync();
                }

                foreach (var channel in Servers.Values)
                {
                    await channel.Shutdown(delaySeconds);
                }

                await TimerManager.Stop();
                ThreadManager.getInstance().stop();
                _logger.LogInformation("[{ServerName}] 停止{Status}", ServerName, "成功");

                // 有些玩家在CashShop
                PlayerStorage.disconnectAll();

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
            GameMetrics.RegisterChannel(this);
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


            CharacterDiseaseManager.Register(TimerManager);
            PetHungerManager.Register(TimerManager);
            ServerMessageManager.Register(TimerManager);
            CharacterHpDecreaseManager.Register(TimerManager);
            MapObjectManager.Register(TimerManager);
            MountTirednessManager.Register(TimerManager);
            MapOwnershipManager.Register(TimerManager);

            invitationTask = TimerManager.register(new InvitationTask(this), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            playerShopTask = TimerManager.register(new PlayerShopTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
            if (ServerConfig.SystemConfig.AutoClearMap)
            {
                checkMapActiveTask = TimerManager.register(new DisposeCheckTask(this), TimeSpan.FromMinutes(3), TimeSpan.FromMinutes(3));
            }
#if !DEBUG
            timeoutTask = TimerManager.register(new TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
#endif

            InviteChannelHandlerRegistry.Register(ServiceProvider.GetServices<InviteChannelHandler>());

            Modules = ServiceProvider.GetServices<AbstractChannelModule>().ToList();
            foreach (var module in Modules)
            {
                module.Initialize();
                module.RegisterTask(TimerManager);
            }

            return true;
        }
        public void RemovePlayer(int chrId)
        {
            if (chrId <= 0)
                return;

            PlayerStorage.RemovePlayer(chrId);
            foreach (var ch in Servers.Values)
            {
                if (ch.RemovePlayer(chrId))
                    return;
            }
        }

        public IPlayer? FindPlayerById(int cid)
        {
            if (cid <= 0)
                return null;

            return PlayerStorage.getCharacterById(cid);
        }

        public IPlayer? FindPlayerById(int channel, int cid)
        {
            if (cid <= 0)
                return null;

            if (Servers.TryGetValue(channel, out var ch))
                return ch.Players.getCharacterById(cid);

            return null;
        }

        internal WorldChannel? GetChannel(int channel)
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

        public void BroadcastPacket(Packet p)
        {
            foreach (var ch in Servers.Values)
            {
                ch.broadcastPacket(p);
            }
        }


        public async Task SendBroadcastWorldPacket(Packet p)
        {
            await Transport.BroadcastMessage(new PacketRequest { Data = ByteString.CopyFrom(p.getBytes()) });
        }


        public async Task SendDropMessage(int type, string message, bool onlyGM = false)
        {
            await Transport.DropWorldMessage(new MessageProto.DropMessageRequest { Type = type, Message = message, OnlyGM = onlyGM });
        }

        public async Task SendDropGMMessage(int type, string message)
        {
            await SendDropMessage(type, message, true);
        }

        public async Task SendYellowTip(string message, bool onlyGM)
        {
            await SendDropMessage(-1, message, onlyGM);
        }

        public async Task EarnTitleMessage(string message, bool onlyGM)
        {
            await SendDropMessage(-2, message, onlyGM);
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
            foreach (var server in Servers.Values)
            {
                server.UpdateWorldConfig(updatePatch);
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

        public void OnMessageReceived(BaseProto.MessageWrapper message)
        {
            MessageDispatcher.Dispatch(message);
        }

        public void OnMessageReceived(string type, IMessage message)
        {
            MessageDispatcher.Dispatch(type, message);
        }

        internal async Task SendReloadEvents(IPlayer chr)
        {
            await Transport.SendReloadEvents(new Dto.ReloadEventsRequest { MasterId = chr.Id });
        }
    }
}
