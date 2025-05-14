using Application.Core.Client;
using Application.Core.Gameplay.Wedding;
using Application.Core.Login.Datas;
using Application.Core.Login.Net;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.EF.Entities;
using Application.Shared.Configs;
using Application.Utility.Configs;
using Application.Utility.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using net.netty;
using net.server;
using net.server.coordinator.session;
using server;
using System.Net;


namespace Application.Core.Login
{
    /// <summary>
    /// 兼顾调度+登录（原先的Server+World），移除大区的概念
    /// </summary>
    public partial class MasterServer : IMasterServer
    {
        public int Id { get; } = 0;
        readonly ILogger<MasterServer> _logger;
        public int Port { get; set; } = 8484;
        public AbstractServer NettyServer { get; }
        public bool IsRunning { get; private set; }
        public List<ChannelServerWrapper> ChannelServerList { get; }
        public WeddingService WeddingInstance { get; }

        public string InstanceId { get; }

        public IMasterServerTransport Transport { get; }

        public DateTimeOffset StartupTime { get; }

        private HashSet<int> queuedGuilds = new();
        public PlayerStorage Players { get; }

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


        public MasterServer(LoginServer loginServer, ILogger<MasterServer> logger, IConfiguration configuration)
        {
            _logger = logger;

            InstanceId = Guid.NewGuid().ToString();
            ChannelServerList = new List<ChannelServerWrapper>();
            StartupTime = DateTimeOffset.Now;
            Transport = new MasterServerTransport(this);
            NettyServer = loginServer;

            WeddingInstance = new WeddingService(this);

            var serverSection = configuration.GetSection("WorldConfig");
            MobRate = serverSection.GetValue<float>("MobRate");
            ExpRate = serverSection.GetValue<float>("ExpRate");
            MesoRate = serverSection.GetValue<float>("MesoRate");
            DropRate = serverSection.GetValue<float>("DropRate");
            BossDropRate = serverSection.GetValue<float>("BossDropRate");
            TravelRate = serverSection.GetValue<float>("TravelRate");
            FishingRate = serverSection.GetValue<float>("FishingRate");

            EventMessage = serverSection.GetValue<string>("EventMessage");
            ServerMessage = serverSection.GetValue<string>("ServerMessage");

            Players = new PlayerStorage();
            BuffStorage = new PlayerBuffStorage();
        }

        public async Task Shutdown()
        {
            await NettyServer.Stop();
            IsRunning = false;
        }

        public async Task StartServer()
        {
            await RegisterTask();

            _logger.LogInformation("登录服务器启动中...");
            await NettyServer.Start();
            _logger.LogInformation("登录服务器启动成功, 监听端口{Port}", Port);
            IsRunning = true;
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
            _logger.LogInformation("定时任务加载中...");
            await TimerManager.InitializeAsync(TaskEngine.Quartz);
            var tMan = TimerManager.getInstance();
            await tMan.Start();
            tMan.register(new NamedRunnable("Purge", TimerManager.purge), YamlConfig.config.server.PURGING_INTERVAL);
            tMan.register(new NamedRunnable("DisconnectIdlesOnLoginState", DisconnectIdlesOnLoginState), TimeSpan.FromMinutes(5));
            _logger.LogInformation("定时任务加载完成");
        }

        public bool IsWorldCapacityFull()
        {
            return GetWorldCapacityStatus() == 2;
        }

        public int GetWorldCapacityStatus()
        {
            int worldCap = ChannelServerList.Count * YamlConfig.config.server.CHANNEL_LOAD;
            int num = Players.Count;

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



        private void DisconnectIdlesOnLoginState()
        {
            List<ILoginClient> toDisconnect = new();

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

                foreach (var c in toDisconnect)
                {
                    inLoginState.Remove(c);
                }
            }
            finally
            {
                Monitor.Exit(srvLock);
            }

            foreach (var c in toDisconnect)
            {
                // thanks Lei for pointing a deadlock issue with srvLock
                if (c.IsOnlined)
                {
                    c.Disconnect();
                }
                else
                {
                    SessionCoordinator.getInstance().closeSession(c, true);
                }
            }
        }


        public int getCurrentTimestamp()
        {
            return Server.getInstance().getCurrentTimestamp();
        }

        public long getCurrentTime()
        {
            return Server.getInstance().getCurrentTime();
        }

        public bool WarpPlayer(string name, int? channel, int mapId, int? portal)
        {
            return Transport.WrapPlayer(name, channel, mapId, portal);
        }
    }
}
