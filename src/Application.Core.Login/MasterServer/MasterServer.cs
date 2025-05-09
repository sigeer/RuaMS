using Application.Core.Gameplay.Wedding;
using Application.Core.Servers;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using net.netty;


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


        public MasterServer(ILogger<MasterServer> logger, IConfiguration configuration)
        {
            _logger = logger;

            InstanceId = Guid.NewGuid().ToString();
            ChannelServerList = new List<ChannelServerWrapper>();
            StartupTime = DateTimeOffset.Now;
            Transport = new MasterServerTransport(this);
            NettyServer = new LoginServer(this);

            WeddingInstance = new WeddingService(this);

            var serverSection = configuration.GetSection("WorldConfig");
            MobRate = serverSection.GetValue<float>("MobRate", 1);
            ExpRate = serverSection.GetValue<float>("ExpRate", 1);
            MesoRate = serverSection.GetValue<float>("MesoRate", 1);
            DropRate = serverSection.GetValue<float>("DropRate", 1);
            BossDropRate = serverSection.GetValue<float>("BossDropRate", 1);
            TravelRate = serverSection.GetValue<float>("TravelRate", 1);
            FishingRate = serverSection.GetValue<float>("FishingRate", 1);

            EventMessage = serverSection.GetValue<string>("EventMessage");
            ServerMessage = serverSection.GetValue<string>("ServerMessage");
        }

        public async Task Shutdown()
        {
            await NettyServer.Stop();
            IsRunning = false;
        }

        public async Task StartServer()
        {
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
    }
}
