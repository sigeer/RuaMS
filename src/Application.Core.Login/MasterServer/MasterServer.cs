
using Application.Core.Login.Datas;
using Application.Core.Login.Models;
using Application.Core.Login.Modules;
using Application.Core.Login.Net;
using Application.Core.Login.ServerData;
using Application.Core.Login.Servers;
using Application.Core.Login.Services;
using Application.Core.Login.Session;
using Application.Core.Login.Tasks;
using Application.Resources.Messages;
using Application.Shared.Constants;
using Application.Shared.Servers;
using Application.Utility;
using Application.Utility.Compatible.Atomics;
using Application.Utility.Configs;
using Application.Utility.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using SystemProto;


namespace Application.Core.Login
{
    /// <summary>
    /// 兼顾调度+登录（原先的Server+World），移除大区的概念
    /// </summary>
    public partial class MasterServer : IServerBase<MasterServerTransport>, ISocketServer
    {
        public bool IsRunning { get; private set; }
        public bool IsShuttingdown => isShuttingdown;
        public int Id { get; } = 0;
        readonly ILogger<MasterServer> _logger;
        public int Port { get; set; } = 8484;
        public AbstractNettyServer NettyServer { get; }
        /// <summary>
        /// 频道服务器，Key：频道服务器名
        /// </summary>
        public Dictionary<string, ChannelServerNode> ChannelServerList { get; }
        /// <summary>
        /// 频道（一个频道服务器上可能运行多个频道）
        /// </summary>
        public List<RegisteredChannelConfig> Channels { get; }

        public string ServerName { get; }

        public MasterServerTransport Transport { get; }

        public DateTimeOffset StartupTime { get; private set; }

        private HashSet<int> queuedGuilds = new();

        #region world config
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
        public string ServerMessage { get; set; }

        public string EventMessage { get; set; }
        #endregion

        #region Managers
        readonly Lazy<CouponManager> _couponManager;
        public CouponManager CouponManager => _couponManager.Value;
        readonly Lazy<AccountManager> _accountManager;
        public AccountManager AccountManager => _accountManager.Value;
        readonly Lazy<CharacterManager> _characterManager;
        public CharacterManager CharacterManager => _characterManager.Value;
        readonly Lazy<ServerManager> _serverManager;
        public ServerManager ServerManager => _serverManager.Value;
        readonly Lazy<BuffManager> _buffManager;
        public BuffManager BuffManager => _buffManager.Value;
        readonly Lazy<CashShopDataManager> _cashShopDataManager;
        public CashShopDataManager CashShopDataManager => _cashShopDataManager.Value;
        readonly Lazy<TeamManager> _teamManager;
        public TeamManager TeamManager => _teamManager.Value;
        readonly Lazy<GuildManager> _guildManager;
        public GuildManager GuildManager => _guildManager.Value;

        readonly Lazy<BuddyManager> _buddyManager;
        public BuddyManager BuddyManager => _buddyManager.Value;
        readonly Lazy<InventoryManager> _inventoryManager;
        public InventoryManager InventoryManager => _inventoryManager.Value;
        readonly Lazy<RingManager> _ringManager;
        public RingManager RingManager => _ringManager.Value;
        readonly Lazy<GiftManager> _giftManager;
        public GiftManager GiftManager => _giftManager.Value;

        readonly Lazy<ResourceDataManager> _lazyResourceDataManager;
        public ResourceDataManager ResourceDataManager => _lazyResourceDataManager.Value;
        readonly Lazy<NewYearCardManager> _lazyNewYearCardManager;
        public NewYearCardManager NewYearCardManager => _lazyNewYearCardManager.Value;
        readonly Lazy<PlayerShopManager> _playerShopManager;
        public PlayerShopManager PlayerShopManager => _playerShopManager.Value;
        readonly Lazy<ItemFactoryManager> _itemFactoryManager;
        public ItemFactoryManager ItemFactoryManager => _itemFactoryManager.Value;
        readonly Lazy<SystemManager> _systemManager;
        public SystemManager SystemManager => _systemManager.Value;
        readonly Lazy<AccountHistoryManager> _accountHistoryManager;
        public AccountHistoryManager AccountHistoryManager => _accountHistoryManager.Value;
        readonly Lazy<AccountBanManager> _accountBanManager;
        public AccountBanManager AccountBanManager => _accountBanManager.Value;

        readonly Lazy<CrossServerService> _crossServerService;
        public CrossServerService CrossServerService => _crossServerService.Value;
        readonly Lazy<GachaponManager> _gachaponManager;
        public GachaponManager GachaponManager => _gachaponManager.Value;

        readonly Lazy<DataStorage> _dataStorage;
        public DataStorage DataStorage => _dataStorage.Value;
        readonly Lazy<CDKManager> _cdkManager;
        public CDKManager CDKManager => _cdkManager.Value;
        #endregion

        readonly Lazy<NoteManager> _noteService;
        public NoteManager NoteManager => _noteService.Value;
        public IServiceProvider ServiceProvider { get; }
        readonly Lazy<ChatRoomManager> _chatRoomManager;
        public ChatRoomManager ChatRoomManager => _chatRoomManager.Value;

        readonly Lazy<InvitationManager> _invitationManager;
        public InvitationManager InvitationManager => _invitationManager.Value;

        public List<AbstractMasterModule> Modules { get; private set; }
        public ITimerManager TimerManager { get; private set; } = null!;


        public bool IsDevRoomAvailable { get; set; }
        public MasterServer(IServiceProvider sp)
        {
            ServiceProvider = sp;
            _logger = ServiceProvider.GetRequiredService<ILogger<MasterServer>>();
            Modules = new();

            ChannelServerList = new();
            Channels = new();
            StartupTime = DateTimeOffset.UtcNow;
            Transport = new MasterServerTransport(this);
            NettyServer = new NettyLoginServer(this);

            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            ServerName = configuration.GetValue<string>("ServerName", "中心服务器");

            var serverSection = configuration.GetSection("GameConfig");
            Name = serverSection.GetValue<string>("Name", "RuaMS");
            MobRate = serverSection.GetValue<float>("MobRate", 1);
            ExpRate = serverSection.GetValue<float>("ExpRate", 1);
            MesoRate = serverSection.GetValue<float>("MesoRate", 1);
            DropRate = serverSection.GetValue<float>("DropRate", 1);
            BossDropRate = serverSection.GetValue<float>("BossDropRate", 1);
            TravelRate = serverSection.GetValue<float>("TravelRate", 1);
            FishingRate = serverSection.GetValue<float>("FishingRate", 1);
            QuestRate = serverSection.GetValue<float>("QuestRate", 1);
            EventMessage = serverSection.GetValue<string>("EventMessage", "");
            ServerMessage = serverSection.GetValue<string>("ServerMessage", "");
            WhyAmIRecommended = serverSection.GetValue<string>("WhyAmIRecommended", "");

            _invitationManager = new(() => ServiceProvider.GetRequiredService<InvitationManager>());

            _buddyManager = new(() => ServiceProvider.GetRequiredService<BuddyManager>());
            _serverManager = new(() => ServiceProvider.GetRequiredService<ServerManager>());
            _couponManager = new(() => ServiceProvider.GetRequiredService<CouponManager>());
            _characterManager = new(() => ServiceProvider.GetRequiredService<CharacterManager>());
            _accountManager = new(() => ServiceProvider.GetRequiredService<AccountManager>());
            _buffManager = new(() => ServiceProvider.GetRequiredService<BuffManager>());
            _cashShopDataManager = new(() => ServiceProvider.GetRequiredService<CashShopDataManager>());
            _teamManager = new(() => ServiceProvider.GetRequiredService<TeamManager>());
            _guildManager = new(() => ServiceProvider.GetRequiredService<GuildManager>());
            _chatRoomManager = new(() => ServiceProvider.GetRequiredService<ChatRoomManager>());
            _itemFactoryManager = new(() => ServiceProvider.GetRequiredService<ItemFactoryManager>());
            _inventoryManager = new(() => ServiceProvider.GetRequiredService<InventoryManager>());
            _giftManager = new(() => ServiceProvider.GetRequiredService<GiftManager>());
            _ringManager = new(() => ServiceProvider.GetRequiredService<RingManager>());
            _lazyNewYearCardManager = new(() => ServiceProvider.GetRequiredService<NewYearCardManager>());
            _lazyResourceDataManager = new(() => ServiceProvider.GetRequiredService<ResourceDataManager>());
            _playerShopManager = new(() => ServiceProvider.GetRequiredService<PlayerShopManager>());
            _noteService = new(() => ServiceProvider.GetRequiredService<NoteManager>());
            _systemManager = new(() => ServiceProvider.GetRequiredService<SystemManager>());
            _accountHistoryManager = new(() => ServiceProvider.GetRequiredService<AccountHistoryManager>());
            _accountBanManager = new(() => ServiceProvider.GetRequiredService<AccountBanManager>());
            _crossServerService = new(() => ServiceProvider.GetRequiredService<CrossServerService>());
            _gachaponManager = new(() => ServiceProvider.GetRequiredService<GachaponManager>());
            _dataStorage = new(() => ServiceProvider.GetRequiredService<DataStorage>());
            _cdkManager = new(() => ServiceProvider.GetRequiredService<CDKManager>());
        }

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public async Task CancelShutdown()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_shutdownDelayCtrl != null)
                {
                    _shutdownDelayCtrl.Cancel();
                    _shutdownDelayCtrl.Dispose();
                    _shutdownDelayCtrl = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        CancellationTokenSource? _shutdownDelayCtrl = null;
        public async Task Shutdown(int delaySeconds = -1)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (!IsRunning)
                    return;

                if (IsShuttingdown)
                    return;

                if (_shutdownDelayCtrl != null)
                {
                    _shutdownDelayCtrl.Cancel();
                    _shutdownDelayCtrl.Dispose();
                }
                _shutdownDelayCtrl = new CancellationTokenSource();
                if (delaySeconds <= 0)
                    await ShutdownServer();
                else
                    _ = Task.Delay(TimeSpan.FromSeconds(delaySeconds), _shutdownDelayCtrl.Token).ContinueWith(task => ShutdownServer(), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        AtomicBoolean isShuttingdown = new AtomicBoolean();

        public async Task ShutdownServer()
        {
            if (!IsRunning)
            {
                _logger.LogInformation(SystemMessage.Server_InActive, ServerName);
                return;
            }

            if (isShuttingdown)
            {
                _logger.LogInformation(SystemMessage.Server_Shutingdown, ServerName);
                return;
            }

            isShuttingdown.Set(true);
            _logger.LogInformation(SystemMessage.Server_StopListenStart, ServerName);

            await NettyServer.Stop();
            _logger.LogInformation(SystemMessage.Server_StopListenComplete, ServerName);

            _shutdownTcs = new TaskCompletionSource();
            Transport.BroadcastShutdown();
            if (ChannelServerList.Count == 0)
                _shutdownTcs.SetResult();

            await CompleteMasterShutdown();
        }
        TaskCompletionSource _shutdownTcs = new TaskCompletionSource();
        public void CompleteChannelShutdown(string serverName)
        {
            RemoveChannel(serverName);

            if (ChannelServerList.Count == 0)
                _shutdownTcs.SetResult();
        }

        async Task CompleteMasterShutdown()
        {
            await _shutdownTcs.Task;

            _logger.LogInformation(SystemMessage.Server_AllChannelShutdown, ServerName);
            foreach (var module in Modules)
            {
                await module.UninstallAsync();
            }

            await InvitationManager.DisposeAsync();

            await TimerManager.Stop();

            _logger.LogInformation(SystemMessage.Server_SaveUserDataStart, ServerName);
            await ServerManager.CommitAllImmediately();
            _logger.LogInformation(SystemMessage.Server_SaveUserDataComplete, ServerName);

            IsRunning = false;
            isShuttingdown.Set(false);
            _shutdownDelayCtrl?.Dispose();
            _logger.LogInformation(SystemMessage.Server_ShutdownComplete, ServerName);
        }

        public async Task StartServer()
        {
            try
            {
                Modules = ServiceProvider.GetServices<AbstractMasterModule>().ToList();
                _logger.LogInformation("[{ServerName}] 共安装了{PluginCount}个额外模块", ServerName, Modules.Count);

                await ServerManager.Setup();

                OpcodeConstants.generateOpcodeNames();

                foreach (var module in Modules)
                {
                    await module.InitializeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{ServerName}] 启动{Status}", ServerName, "失败");
                return;
            }

            _logger.LogInformation(SystemMessage.Server_Start, ServerName);
            await NettyServer.Start();
            _logger.LogInformation(SystemMessage.Server_StartSuccess, ServerName, "成功", Port);

            StartupTime = DateTimeOffset.UtcNow;
            ForceUpdateServerTime();

            await RegisterTask();

            IsRunning = true;
            _logger.LogInformation("[{ServerName}] 已启动，当前服务器时间{ServerCurrentTime}，本地时间{LocalCurrentTime}",
                ServerName,
                DateTimeOffset.FromUnixTimeMilliseconds(getCurrentTime()).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }


        public int AddChannel(ChannelServerNode channel)
        {
            if (IsRunning && ChannelServerList.TryAdd(channel.ServerName, channel))
            {
                var started = Channels.Count;
                foreach (var item in channel.ServerConfigs)
                {
                    Channels.Add(new RegisteredChannelConfig
                    {
                        ServerHost = channel.ServerHost,
                        Port = item.Port,
                        ServerName = channel.ServerName,
                        MaxSize = item.MaxSize
                    });
                    _logger.LogInformation("[{ServerName}] 已注册服务器[{ChannelServerName}]：频道{Channel}", ServerName, channel.ServerName, Channels.Count);
                }
                return started + 1;
            }
            return -1;
        }

        private bool RemoveChannel(string instanceId)
        {
            if (ChannelServerList.Remove(instanceId, out var channelServer))
            {
                _logger.LogInformation("[{ServerName}] 移除{Type}服务器{ChannelServerName}", ServerName, channelServer.GetType().Name, instanceId);
                return Channels.RemoveAll(x => x.ServerName == channelServer.ServerName) == channelServer.ServerConfigs.Count;
            }
            return false;
        }

        public HashSet<ChannelServerNode> GroupPlayer(IEnumerable<int> cidList)
        {
            var result = new HashSet<ChannelServerNode>();

            foreach (var cid in cidList)
            {
                var player = CharacterManager.FindPlayerById(cid);
                if (player?.ChannelNode == null)
                    continue;

                result.Add(player.ChannelNode);
            }
            return result;
        }

        public HashSet<ChannelServerNode> GroupPlayer(IEnumerable<CharacterLiveObject> cList)
        {
            var result = new HashSet<ChannelServerNode>();

            foreach (var player in cList)
            {
                if (player?.ChannelNode == null)
                    continue;

                result.Add(player.ChannelNode);
            }
            return result;
        }

        public ChannelServerNode GetChannelServer(int channelId)
        {
            return ChannelServerList.GetValueOrDefault(Channels[channelId - 1].ServerName)!;
        }

        public ChannelConfig? GetChannel(int channelId)
        {
            return Channels.ElementAtOrDefault(channelId - 1);
        }

        public IPEndPoint GetChannelIPEndPoint(int channelId)
        {
            var channel = Channels[channelId - 1];
            return new IPEndPoint(IPAddress.Parse(channel.ServerHost), channel.Port);
        }

        public int GetChannelCapacity(int channelId)
        {
            return (int)(Math.Ceiling(((float)CharacterManager.GetChannelPlayerCount(channelId) / Channels[channelId - 1].MaxSize) * 800));
        }

        public bool IsGuildQueued(int guildId)
        {
            return queuedGuilds.Contains(guildId);
        }

        public void PutGuildQueued(int guildId)
        {
            queuedGuilds.Add(guildId);
        }

        public void RemoveGuildQueued(int guildId)
        {
            queuedGuilds.Remove(guildId);
        }

        public void UpdateWorldConfig(Config.WorldConfig updatePatch)
        {
            // 修改值
            if (updatePatch.MobRate.HasValue)
            {
                MobRate = updatePatch.MobRate.Value;
                updatePatch.MobRate = MobRate;
            }
            if (updatePatch.MesoRate.HasValue)
            {
                MesoRate = updatePatch.MesoRate.Value;
            }
            if (updatePatch.ExpRate.HasValue)
            {
                ExpRate = updatePatch.ExpRate.Value;
            }
            if (updatePatch.DropRate.HasValue)
            {
                DropRate = updatePatch.DropRate.Value;
            }
            if (updatePatch.BossDropRate.HasValue)
            {
                BossDropRate = updatePatch.BossDropRate.Value;
            }
            if (updatePatch.TravelRate.HasValue)
            {
                TravelRate = updatePatch.TravelRate.Value;
            }
            if (updatePatch.FishingRate.HasValue)
            {
                FishingRate = updatePatch.FishingRate.Value;
            }
            if (updatePatch.ServerMessage != null)
            {
                ServerMessage = updatePatch.ServerMessage;
            }
            // 通知频道服务器更新
            Transport.SendWorldConfig(updatePatch);
        }

        private async Task RegisterTask()
        {
            _logger.LogInformation("[{ServerName}] 定时任务加载中...", ServerName);
            var timeLeft = TimeUtils.GetTimeLeftForNextHour();
            TimerManager = await TimerManagerFactory.InitializeAsync(TaskEngine.Quartz, ServerName);
            var sessionCoordinator = ServiceProvider.GetRequiredService<SessionCoordinator>();
            TimerManager.register(new NamedRunnable("ServerTimeUpdate", UpdateServerTime), YamlConfig.config.server.UPDATE_INTERVAL);
            TimerManager.register(new NamedRunnable("ServerTimeForceUpdate", ForceUpdateServerTime), YamlConfig.config.server.PURGING_INTERVAL);

            TimerManager.register(new NamedRunnable("DisconnectIdlesOnLoginState", DisconnectIdlesOnLoginState), TimeSpan.FromMinutes(5));

            TimerManager.register(new LoginCoordinatorTask(sessionCoordinator), TimeSpan.FromHours(1), timeLeft);
            TimerManager.register(new LoginStorageTask(sessionCoordinator, ServiceProvider.GetRequiredService<LoginBypassCoordinator>()), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
            TimerManager.register(ServiceProvider.GetRequiredService<DueyFredrickTask>(), TimeSpan.FromHours(1), timeLeft);
            TimerManager.register(ServiceProvider.GetRequiredService<RankingLoginTask>(), YamlConfig.config.server.RANKING_INTERVAL, (long)timeLeft.TotalMilliseconds);
            TimerManager.register(ServiceProvider.GetRequiredService<RankingCommandTask>(), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            TimerManager.register(ServiceProvider.GetRequiredService<CouponTask>(), YamlConfig.config.server.COUPON_INTERVAL, (long)timeLeft.TotalMilliseconds);
            InvitationManager.Register(TimerManager);
            ServerManager.Register(TimerManager);

            TimerManager.register(() =>
            {
                NewYearCardManager.NotifyNewYearCard();
            }, TimeSpan.FromHours(1), timeLeft);
            foreach (var module in Modules)
            {
                module.RegisterTask(TimerManager);
            }
            _logger.LogInformation("[{ServerName}] 定时任务加载完成", ServerName);
        }

        public bool IsWorldCapacityFull()
        {
            return GetWorldCapacityStatus() == 2;
        }

        public int GetWorldCapacityStatus()
        {
            int worldCap = ChannelServerList.Sum(x => x.Value.ServerConfigs.Sum(y => y.MaxSize));
            int num = CharacterManager.GetOnlinedPlayerCount();

            int status;
            if (num >= worldCap)
            {
                status = 2;
            }
            else if (num >= worldCap * 0.8)
            {
                // More than 80 percent o___o
                status = 1;
            }
            else
            {
                status = 0;
            }

            return status;
        }

        private AtomicLong currentTime = new AtomicLong(0);
        private long serverCurrentTime = 0;
        public int getCurrentTimestamp()
        {
            return (int)(getCurrentTime() - StartupTime.ToUnixTimeMilliseconds());
        }

        public long getCurrentTime()
        {
            return serverCurrentTime;
        }

        public DateTimeOffset GetCurrentTimeDateTimeOffset()
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(getCurrentTime());
        }
        public void UpdateServerTime()
        {
            serverCurrentTime = currentTime.addAndGet(YamlConfig.config.server.UPDATE_INTERVAL);
        }

        public void ForceUpdateServerTime()
        {
            var forceTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            serverCurrentTime = forceTime;
            currentTime.set(forceTime);
        }

        public ServerStateDto GetServerStats()
        {
            return new ServerStateDto { IsDevRoomAvailable = IsDevRoomAvailable };
        }
    }
}
