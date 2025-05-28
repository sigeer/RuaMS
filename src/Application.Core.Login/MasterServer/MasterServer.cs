using Application.Core.Login.Datas;
using Application.Core.Login.Net;
using Application.Core.Login.Services;
using Application.Core.Login.Session;
using Application.Core.Login.Tasks;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Net;
using Application.Shared.Servers;
using Application.Utility;
using Application.Utility.Compatible.Atomics;
using Application.Utility.Configs;
using Application.Utility.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using net.server;
using server;
using System.Net;


namespace Application.Core.Login
{
    /// <summary>
    /// 兼顾调度+登录（原先的Server+World），移除大区的概念
    /// </summary>
    public partial class MasterServer : IServerBase<IMasterServerTransport>
    {
        public int Id { get; } = 0;
        readonly ILogger<MasterServer> _logger;
        public int Port { get; set; } = 8484;
        public AbstractServer NettyServer { get; }
        public bool IsRunning { get; private set; }
        public List<ChannelServerWrapper> ChannelServerList { get; }
        public WeddingManager WeddingInstance { get; }

        public string InstanceId { get; }

        public IMasterServerTransport Transport { get; }

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
        #endregion

        public IServiceProvider ServiceProvider { get; }

        public InvitationController InvitationController { get; }

        CharacterService _characterSevice;
        public MasterServer(IServiceProvider sp, CharacterService characterManager)
        {
            ServiceProvider = sp;
            _logger = ServiceProvider.GetRequiredService<ILogger<MasterServer>>();

            _characterSevice = characterManager;

            InstanceId = Guid.NewGuid().ToString();
            ChannelServerList = new List<ChannelServerWrapper>();
            StartupTime = DateTimeOffset.UtcNow;
            Transport = new MasterServerTransport(this);
            NettyServer = new LoginServer(this);


            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            var serverSection = configuration.GetSection("WorldConfig");
            MobRate = serverSection.GetValue<float>("MobRate", 1);
            ExpRate = serverSection.GetValue<float>("ExpRate", 1);
            MesoRate = serverSection.GetValue<float>("MesoRate", 1);
            DropRate = serverSection.GetValue<float>("DropRate", 1);
            BossDropRate = serverSection.GetValue<float>("BossDropRate", 1);
            TravelRate = serverSection.GetValue<float>("TravelRate", 1);
            FishingRate = serverSection.GetValue<float>("FishingRate", 1);

            Name = serverSection.GetValue<string>("Name", "RuaMS");
            EventMessage = serverSection.GetValue<string>("EventMessage", "");
            ServerMessage = serverSection.GetValue<string>("ServerMessage", "");
            WhyAmIRecommended = serverSection.GetValue<string>("WhyAmIRecommended", "");

            BuffStorage = new PlayerBuffStorage();

            InvitationController = new InvitationController(this);
            ServerManager = ActivatorUtilities.CreateInstance<ServerManager>(ServiceProvider, this);
            CouponManager = ActivatorUtilities.CreateInstance<CouponManager>(ServiceProvider, this);
            CharacterManager = ActivatorUtilities.CreateInstance<CharacterManager>(ServiceProvider, this);
            AccountManager = ActivatorUtilities.CreateInstance<AccountManager>(ServiceProvider, this);
            WeddingInstance = ActivatorUtilities.CreateInstance<WeddingManager>(ServiceProvider, this);
        }

        bool isShuttingdown = false;
        public async Task Shutdown()
        {
            if (!IsRunning)
            {
                _logger.LogInformation("服务器未启动");
                return;
            }

            if (isShuttingdown)
            {
                _logger.LogInformation("正在停止服务器[{ServerName}]", InstanceId);
                return;
            }

            isShuttingdown = true;
            _logger.LogInformation("[{ServerName}] 停止中...", "登录/中心服务器");
            await NettyServer.Stop();
            _logger.LogInformation("[{ServerName}] 停止监听", "登录服务器");

            await InvitationController.DisposeAsync();

            await Server.getInstance().Stop(false);

            var storageService = ServiceProvider.GetRequiredService<StorageService>();
            _logger.LogInformation("[{ServerName}] 正在保存玩家数据...", "中心服务器");
            await storageService.CommitAllImmediately();
            _logger.LogInformation("[{ServerName}] 玩家数据已保存", "中心服务器");

            IsRunning = false;
            isShuttingdown = false;
            _logger.LogInformation("[{ServerName}] 已停止", "登录/中心服务器");
        }

        public async Task StartServer()
        {
            await ServerManager.SetupDataBase();
            await Server.getInstance().Start();

            _logger.LogInformation("[{ServerName}] 启动中...", "登录服务器");
            await NettyServer.Start();
            _logger.LogInformation("[{ServerName}] 启动成功, 监听端口{Port}", "登录服务器", Port);

            StartupTime = DateTimeOffset.UtcNow;
            ForceUpdateServerTime();

            await RegisterTask();

            IsRunning = true;
            _logger.LogInformation("[{ServerName}] 已启动，当前服务器时间{ServerCurrentTime}，本地时间{LocalCurrentTime}",
                "登录/中心服务器",
                DateTimeOffset.FromUnixTimeMilliseconds(getCurrentTime()).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                DateTimeOffset.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        }


        public int AddChannel(ChannelServerWrapper channel)
        {
            ChannelServerList.Add(channel);
            return ChannelServerList.Count;
        }

        public bool RemoveChannel(string instanceId)
        {
            var item = ChannelServerList.FirstOrDefault(x => x.InstanceId == instanceId);
            if (item == null)
                return false;

            return ChannelServerList.Remove(item);
        }

        public ChannelServerWrapper GetChannel(int channelId)
        {
            return ChannelServerList[channelId - 1];
        }

        public IPEndPoint GetChannelIPEndPoint(int channelId)
        {
            var channel = GetChannel(channelId);
            return new IPEndPoint(IPAddress.Parse(channel.ServerConfig.Host), channel.ServerConfig.Port);
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

        public void UpdateWorldConfig(WorldConfigPatch updatePatch)
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
            _logger.LogInformation("[{ServerName}] 定时任务加载中...", "中心服务器");
            var timeLeft = TimeUtils.GetTimeLeftForNextHour();
            var tMan = TimerManager.getInstance();
            await tMan.Start();
            var sessionCoordinator = ServiceProvider.GetRequiredService<SessionCoordinator>();
            tMan.register(new NamedRunnable("ServerTimeUpdate", UpdateServerTime), YamlConfig.config.server.UPDATE_INTERVAL);
            tMan.register(new NamedRunnable("ServerTimeForceUpdate", ForceUpdateServerTime), YamlConfig.config.server.PURGING_INTERVAL);

            tMan.register(new NamedRunnable("DisconnectIdlesOnLoginState", DisconnectIdlesOnLoginState), TimeSpan.FromMinutes(5));
            tMan.register(new CharacterAutosaverTask(ServiceProvider.GetRequiredService<StorageService>()), TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            tMan.register(new LoginCoordinatorTask(sessionCoordinator), TimeSpan.FromHours(1), timeLeft);
            tMan.register(new LoginStorageTask(sessionCoordinator, ServiceProvider.GetRequiredService<LoginBypassCoordinator>()), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
            tMan.register(ServiceProvider.GetRequiredService<DueyFredrickTask>(), TimeSpan.FromHours(1), timeLeft);
            tMan.register(ServiceProvider.GetRequiredService<RankingLoginTask>(), YamlConfig.config.server.RANKING_INTERVAL, (long)timeLeft.TotalMilliseconds);
            tMan.register(ServiceProvider.GetRequiredService<RankingCommandTask>(), TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            tMan.register(ServiceProvider.GetRequiredService<CouponTask>(), YamlConfig.config.server.COUPON_INTERVAL, (long)timeLeft.TotalMilliseconds);
            InvitationController.Register();
            _logger.LogInformation("[{ServerName}] 定时任务加载完成", "中心服务器");
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
