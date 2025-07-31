
using Application.Core.Login.Datas;
using Application.Core.Login.Events;
using Application.Core.Login.Models;
using Application.Core.Login.Net;
using Application.Core.Login.ServerData;
using Application.Core.Login.Servers;
using Application.Core.Login.Services;
using Application.Core.Login.Session;
using Application.Core.Login.Tasks;
using Application.Shared.Servers;
using Application.Utility;
using Application.Utility.Compatible.Atomics;
using Application.Utility.Configs;
using Application.Utility.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using net.server;
using System.Net;


namespace Application.Core.Login
{
    /// <summary>
    /// 兼顾调度+登录（原先的Server+World），移除大区的概念
    /// </summary>
    public partial class MasterServer : IServerBase<MasterServerTransport>, ISocketServer
    {
        public bool IsRunning { get; private set; }
        public int Id { get; } = 0;
        readonly ILogger<MasterServer> _logger;
        public int Port { get; set; } = 8484;
        public AbstractNettyServer NettyServer { get; }
        /// <summary>
        /// 频道服务器，Key：频道服务器名
        /// </summary>
        public Dictionary<string, ChannelServerWrapper> ChannelServerList { get; }
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
        public CouponManager CouponManager { get; }
        public AccountManager AccountManager { get; }
        public CharacterManager CharacterManager { get; }
        public ServerManager ServerManager { get; }
        public BuffManager BuffManager { get; }
        public CashShopDataManager CashShopDataManager { get; }
        public TeamManager TeamManager { get; }
        public GuildManager GuildManager { get; }
        readonly Lazy<InventoryManager> _inventoryManager;
        public InventoryManager InventoryManager => _inventoryManager.Value;
        readonly Lazy<RingManager> _ringManager;
        public RingManager RingManager => _ringManager.Value;
        readonly Lazy<GiftManager> _giftManager;
        public GiftManager GiftManager => _giftManager.Value;
        public ItemTransactionManager ItemTransactionManager { get; }
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
        #endregion

        readonly Lazy<NoteManager> _noteService;
        public NoteManager NoteManager => _noteService.Value;
        public IServiceProvider ServiceProvider { get; }

        public ChatRoomManager ChatRoomManager { get; }
        public List<MasterModule> Modules { get; private set; }
        public InvitationManager InvitationManager { get; }


        CharacterService _characterSevice;
        public ITimerManager TimerManager { get; private set; } = null!;
        public MasterServer(IServiceProvider sp, CharacterService characterManager)
        {
            ServiceProvider = sp;
            _logger = ServiceProvider.GetRequiredService<ILogger<MasterServer>>();
            Modules = new();

            _characterSevice = characterManager;

            ChannelServerList = new();
            Channels = new();
            StartupTime = DateTimeOffset.UtcNow;
            Transport = new MasterServerTransport(this);
            NettyServer = new NettyLoginServer(this);

            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            ServerName = configuration.GetSection("ServerConfig").GetValue<string>("ServerName", "中心服务器");

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


            InvitationManager = ActivatorUtilities.CreateInstance<InvitationManager>(ServiceProvider, this);

            ServerManager = ActivatorUtilities.CreateInstance<ServerManager>(ServiceProvider, this);
            CouponManager = ActivatorUtilities.CreateInstance<CouponManager>(ServiceProvider, this);
            CharacterManager = ActivatorUtilities.CreateInstance<CharacterManager>(ServiceProvider, this);
            AccountManager = ActivatorUtilities.CreateInstance<AccountManager>(ServiceProvider, this);
            BuffManager = ActivatorUtilities.CreateInstance<BuffManager>(ServiceProvider, this);
            CashShopDataManager = ActivatorUtilities.CreateInstance<CashShopDataManager>(ServiceProvider, this);
            TeamManager = ActivatorUtilities.CreateInstance<TeamManager>(ServiceProvider, this);
            GuildManager = ActivatorUtilities.CreateInstance<GuildManager>(ServiceProvider, this);
            ChatRoomManager = ActivatorUtilities.CreateInstance<ChatRoomManager>(ServiceProvider, this);
            ItemTransactionManager = ActivatorUtilities.CreateInstance<ItemTransactionManager>(ServiceProvider, this);
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
        }

        bool isShuttingdown = false;
        public async Task Shutdown()
        {
            if (!IsRunning)
            {
                _logger.LogInformation("[{ServerName}]未启动", ServerName);
                return;
            }

            if (isShuttingdown)
            {
                _logger.LogInformation("正在停止[{ServerName}]", ServerName);
                return;
            }

            isShuttingdown = true;
            _logger.LogInformation("[{ServerName}] 停止中...", ServerName);

            await NettyServer.Stop();
            _logger.LogInformation("[{ServerName}] 停止监听", ServerName);

            Transport.BroadcastShutdown();

            foreach (var module in Modules)
            {
                await module.UninstallAsync();
            }

            await InvitationManager.DisposeAsync();

            await TimerManager.Stop();
            await Server.getInstance().Stop(false);

            var storageService = ServiceProvider.GetRequiredService<StorageService>();
            _logger.LogInformation("[{ServerName}] 正在保存玩家数据...", ServerName);
            await storageService.CommitAllImmediately();
            _logger.LogInformation("[{ServerName}] 玩家数据已保存", ServerName);

            IsRunning = false;
            isShuttingdown = false;
            _logger.LogInformation("[{ServerName}] 已停止", ServerName);
        }

        public async Task StartServer()
        {
            try
            {
                Modules = ServiceProvider.GetServices<MasterModule>().ToList();
                _logger.LogInformation("[{ServerName}] 共安装了{PluginCount}个额外模块", ServerName, Modules.Count);

                await ServerManager.Setup();

                await Server.getInstance().Start();

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

            _logger.LogInformation("[{ServerName}] 启动中...", ServerName);
            await NettyServer.Start();
            _logger.LogInformation("[{ServerName}] 启动{Status}, 监听端口{Port}", ServerName, "成功", Port);

            StartupTime = DateTimeOffset.UtcNow;
            ForceUpdateServerTime();

            await RegisterTask();

            IsRunning = true;
            _logger.LogInformation("[{ServerName}] 已启动，当前服务器时间{ServerCurrentTime}，本地时间{LocalCurrentTime}",
                ServerName,
                DateTimeOffset.FromUnixTimeMilliseconds(getCurrentTime()).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }


        public int AddChannel(ChannelServerWrapper channel)
        {
            if (ChannelServerList.TryAdd(channel.ServerName, channel))
            {
                var started = Channels.Count;
                foreach (var item in channel.ServerConfigs)
                {
                    Channels.Add(new RegisteredChannelConfig
                    {
                        Host = item.Host,
                        Port = item.Port,
                        ServerName = channel.ServerName,
                        MaxSize = item.MaxSize
                    });
                }
                _serverChannelCache = null;
                return started + 1;
            }
            return -1;
        }

        public bool RemoveChannel(string instanceId)
        {
            if (ChannelServerList.Remove(instanceId, out var channelServer))
            {
                _serverChannelCache = null;
                return Channels.RemoveAll(x => x.ServerName == channelServer.ServerName) == channelServer.ServerConfigs.Count;
            }
            return false;
        }

        List<ServerChannelPair>? _serverChannelCache;
        private List<ServerChannelPair> GetServerChannel()
        {
            return _serverChannelCache ??= Channels.Select((item, idx) => new ServerChannelPair(item.ServerName, idx + 1)).ToList();
        }

        /// <summary>
        /// 对玩家按服务器分组
        /// </summary>
        /// <param name="dataList">玩家id及其所在频道</param>
        /// <param name="exceptServer"></param>
        /// <returns>服务器: 服务器上的玩家</returns>
        public Dictionary<ChannelServerWrapper, int[]> GroupPlayer(IEnumerable<PlayerChannelPair> dataList)
        {
            var channelServerMap = GetServerChannel();
            var result = new Dictionary<ChannelServerWrapper, List<int>>();

            foreach (var player in dataList)
            {
                if (player.Channel <= 0)
                    continue;

                var serverName = Channels[player.Channel - 1].ServerName;
                var serverWrapper = ChannelServerList[serverName];
                if (!result.TryGetValue(serverWrapper, out var idList))
                {
                    idList = new List<int>();
                    result[serverWrapper] = idList;
                }
                idList.Add(player.PlayerId);
            }

            // 转换 List<int> -> int[]
            return result.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public Dictionary<ChannelServerWrapper, int[]> GroupPlayer(IEnumerable<int> cidList)
        {
            var channelServerMap = GetServerChannel();
            var result = new Dictionary<ChannelServerWrapper, List<int>>();

            foreach (var cid in cidList)
            {
                var player = CharacterManager.FindPlayerById(cid);
                if (player == null || player.ActualChannel == 0)
                    continue;

                var serverName = Channels[player.ActualChannel - 1].ServerName;
                var serverWrapper = ChannelServerList[serverName];
                if (!result.TryGetValue(serverWrapper, out var idList))
                {
                    idList = new List<int>();
                    result[serverWrapper] = idList;
                }
                idList.Add(player.Character.Id);
            }

            // 转换 List<int> -> int[]
            return result.ToDictionary(x => x.Key, x => x.Value.ToArray());
        }

        public ChannelServerWrapper GetChannelServer(int channelId)
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
            return new IPEndPoint(IPAddress.Parse(channel.Host), channel.Port);
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
            TimerManager = await server.TimerManager.InitializeAsync(TaskEngine.Quartz, ServerName);
            var sessionCoordinator = ServiceProvider.GetRequiredService<SessionCoordinator>();
            TimerManager.register(new NamedRunnable("ServerTimeUpdate", UpdateServerTime), YamlConfig.config.server.UPDATE_INTERVAL);
            TimerManager.register(new NamedRunnable("ServerTimeForceUpdate", ForceUpdateServerTime), YamlConfig.config.server.PURGING_INTERVAL);

            TimerManager.register(new NamedRunnable("DisconnectIdlesOnLoginState", DisconnectIdlesOnLoginState), TimeSpan.FromMinutes(5));
            TimerManager.register(new CharacterAutosaverTask(ServiceProvider.GetRequiredService<StorageService>()), TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            TimerManager.register(new LoginCoordinatorTask(sessionCoordinator), TimeSpan.FromHours(1), timeLeft);
            TimerManager.register(new LoginStorageTask(sessionCoordinator, ServiceProvider.GetRequiredService<LoginBypassCoordinator>()), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
            TimerManager.register(ServiceProvider.GetRequiredService<DueyFredrickTask>(), TimeSpan.FromHours(1), timeLeft);
            TimerManager.register(ServiceProvider.GetRequiredService<RankingLoginTask>(), YamlConfig.config.server.RANKING_INTERVAL, (long)timeLeft.TotalMilliseconds);
            TimerManager.register(ServiceProvider.GetRequiredService<RankingCommandTask>(), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            TimerManager.register(ServiceProvider.GetRequiredService<CouponTask>(), YamlConfig.config.server.COUPON_INTERVAL, (long)timeLeft.TotalMilliseconds);
            InvitationManager.Register(TimerManager);
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
            int worldCap = ChannelServerList.Count * YamlConfig.config.server.CHANNEL_LOAD;
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



        public bool WarpPlayer(string name, int? channel, int mapId, int? portal)
        {
            return Transport.WrapPlayer(name, channel, mapId, portal);
        }

        public void BroadcastWorldMessage(Packet p)
        {
            Transport.BroadcastMessage(p);
        }
        public void BroadcastWorldGMPacket(Packet packet)
        {
            Transport.BroadcastWorldGMPacket(packet);
        }

        public bool CheckCharacterName(string name)
        {
            return _characterSevice.CheckCharacterName(name);
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
    }
}
