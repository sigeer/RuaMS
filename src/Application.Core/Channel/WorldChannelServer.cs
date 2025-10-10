using Application.Core.Channel.DataProviders;
using Application.Core.Channel.Invitation;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Skills;
using Application.Core.ServerTransports;
using Application.Resources.Messages;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Config;
using constants.game;
using Dto;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.server.guild;
using net.server.task;
using Polly;
using Serilog;
using server;
using server.quest;
using System.Diagnostics;
using System.Net;
using tools;
using XmlWzReader;

namespace Application.Core.Channel
{
    public class WorldChannelServer : IServerBase<IChannelServerTransport>
    {
        public IServiceProvider ServiceProvider { get; }
        public IChannelServerTransport Transport { get; }
        public Dictionary<int, WorldChannel> Servers { get; set; }
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
        public List<ChannelModule> Modules { get; private set; }
        public InviteChannelHandlerRegistry InviteChannelHandlerRegistry { get; }

        public ExpeditionService ExpeditionService { get; }
        public ChannelPlayerStorage PlayerStorage { get; }

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
                    await channel.ShutdownServer();
                }
                // 有些玩家在CashShop
                PlayerStorage.disconnectAll();

                await TimerManager.Stop();
                ThreadManager.getInstance().stop();
                _logger.LogInformation("[{ServerName}] 停止{Status}", ServerName, "成功");
                Transport.CompleteChannelShutdown(ServerName);
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

        public async Task StartServer()
        {
            await Start();
            StartupTime = DateTimeOffset.UtcNow;
            ForceUpdateServerTime();
        }


        private async Task Start()
        {
            if (IsRunning)
                return;

            if (!Directory.Exists(ScriptResFactory.ScriptDirName) || !Directory.Exists(WZFiles.DIRECTORY))
                throw new DirectoryNotFoundException("没有找到Script/WZ");

            if (ServerConfig.ChannelConfig.Count == 0)
                throw new BusinessFatalException("必须包含频道");

            foreach (var item in ServiceProvider.GetServices<DataBootstrap>())
            {
                _ = Task.Run(() =>
                {
                    item.LoadData();
                });
            }

            _ = Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                SkillFactory.LoadAllSkills();
                sw.Stop();
                _logger.LogDebug("WZ - 技能加载耗时 {StarupCost}s", sw.Elapsed.TotalSeconds);
            });


            DataService.LoadAllPLife();

            Modules = ServiceProvider.GetServices<ChannelModule>().ToList();

            OpcodeConstants.generateOpcodeNames();


            TimerManager = await TimerManagerFactory.InitializeAsync(TaskEngine.Quartz, ServerName);

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
            InitializeMessage();

            foreach (var module in Modules)
            {
                module.Initialize();
                module.RegisterTask(TimerManager);
            }

            List<WorldChannel> localServers = [];
            foreach (var config in ServerConfig.ChannelConfig)
            {
                var scope = ServiceProvider.CreateScope();
                var channel = new WorldChannel(this, scope, ServerConfig.ServerHost, config);
                await channel.StartServer();
                if (channel.IsRunning)
                {
                    localServers.Add(channel);
                }
            }

            var registerPolicy = Policy.HandleResult<RegisterServerResult>(x => x.StartChannel <= 0)
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromMilliseconds(2000),
                onRetry: (result, timespan, retryCount, context) =>
                {
                    _logger.LogError($"第 {retryCount} 次重试，返回值是 {result.Result.StartChannel}");
                });
            var configs = await registerPolicy.ExecuteAsync(async () => await Transport.RegisterServer(localServers));

            if (configs.StartChannel > 0)
            {
                ForceUpdateServerTime();

                foreach (var server in localServers)
                {
                    var channel = configs.StartChannel++;
                    server.Register(channel);
                    Servers[channel] = server;
                }

                UpdateWorldConfig(configs.Config);
                UpdateCouponConfig(configs.Coupon);

                foreach (var server in Servers.Values)
                {
                    server.Initialize();
                }

                IsRunning = true;
            }
            else
            {
                _logger.LogError("注册服务器失败, {Message}", configs.Message);
                IsRunning = false;
            }

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

        public void BroadcastGMPacket(Packet p)
        {
            foreach (var ch in Servers.Values)
            {
                ch.broadcastGMPacket(p);
            }
        }

        void BroadcastSetTimer(MessageProto.SetTimer data)
        {
            BroadcastPacket(PacketCreator.getClock(data.Seconds));
        }

        void BroadcastRemoveTimer(MessageProto.RemoveTimer data)
        {
            BroadcastPacket(PacketCreator.removeClock());
        }

        public void SendBroadcastWorldPacket(Packet p)
        {
            Transport.BroadcastMessage(new PacketRequest { Data = ByteString.CopyFrom(p.getBytes()) });
        }
        public void SendBroadcastWorldGMPacket(Packet p)
        {
            Transport.BroadcastMessage(new PacketRequest { Data = ByteString.CopyFrom(p.getBytes()), OnlyGM = true });
        }

        void OnReceivedPacket(MessageProto.PacketBroadcast data)
        {
            var packet = new ByteBufOutPacket(data.Data.ToByteArray());
            foreach (var ch in Servers.Values)
            {
                if (data.Receivers.Contains(-1))
                {
                    foreach (var player in ch.Players.getAllCharacters())
                    {
                        player.sendPacket(packet);
                    }
                }
                else
                {
                    foreach (var id in data.Receivers)
                    {
                        ch.Players.getCharacterById(id)?.sendPacket(packet);
                    }

                }
            }
        }


        public void SendDropMessage(int type, string message)
        {
            Transport.DropWorldMessage(new MessageProto.DropMessageRequest { Type = type, Message = message });
        }

        public void SendDropGMMessage(int type, string message)
        {
            Transport.DropWorldMessage(new MessageProto.DropMessageRequest { Type = type, Message = message, OnlyGM = true });
        }

        public void SendYellowTip(string message, bool onlyGM)
        {
            Transport.SendYellowTip(new MessageProto.YellowTipRequest { Message = message, OnlyGM = onlyGM });
        }

        private void UpdateWorldConfig(Config.WorldConfig updatePatch)
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

        void OnDropMessage(DropMessageBroadcast msg)
        {
            foreach (var ch in Servers.Values)
            {
                if (msg.Receivers.Contains(-1))
                {
                    foreach (var player in ch.Players.getAllCharacters())
                    {
                        player.dropMessage(msg.Type, msg.Message);
                    }
                }
                else
                {
                    foreach (var id in msg.Receivers)
                    {
                        ch.Players.getCharacterById(id)?.dropMessage(msg.Type, msg.Message);
                    }

                }
            }
        }

        void OnYellowTip(YellowTipBroadcast msg)
        {
            foreach (var ch in Servers.Values)
            {
                if (msg.Receivers.Contains(-1))
                {
                    foreach (var player in ch.Players.getAllCharacters())
                    {
                        player.yellowMessage(msg.Message);
                    }
                }
                else
                {
                    foreach (var id in msg.Receivers)
                    {
                        ch.Players.getCharacterById(id)?.yellowMessage(msg.Message);
                    }

                }
            }
        }

        private void OnPlayerJobChanged(Dto.PlayerLevelJobChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberJob(data.Id, data.JobId);
                    guild.broadcast(PacketCreator.jobMessage(0, data.JobId, data.Name), data.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(data.GuildId, data.Id, data.Level, data.JobId));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = GuildManager.GetAllianceById(guild.AllianceId);
                        if (alliance != null)
                        {
                            alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, data.Id, data.Level, data.JobId), data.Id, -1);
                        }
                    }
                }
            }
            foreach (var module in Modules)
            {
                module.OnPlayerChangeJob(data);
            }
        }

        private void OnPlayerLevelChanged(Dto.PlayerLevelJobChange data)
        {
            if (data.GuildId > 0)
            {
                var guild = GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberLevel(data.Id, data.Level);
                    guild.broadcast(PacketCreator.levelUpMessage(0, data.Level, data.Name), data.Id);
                    guild.broadcast(GuildPackets.guildMemberLevelJobUpdate(data.GuildId, data.Id, data.Level, data.JobId));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = GuildManager.GetAllianceById(guild.AllianceId);
                        if (alliance != null)
                        {
                            alliance.broadcastMessage(GuildPackets.updateAllianceJobLevel(guild, data.Id, data.Level, data.JobId), data.Id, -1);
                        }
                    }

                }
            }

            if (data.Level == JobFactory.GetById(data.JobId).MaxLevel)
            {
                foreach (var ch in Servers.Values)
                {
                    ch.LightBlue(nameof(ClientMessage.Levelup_Congratulation), CharacterViewDtoUtils.GetPlayerNameWithMedal(data.Name, data.MedalItemId), data.Level.ToString(), data.Name);
                }
            }

            foreach (var module in Modules)
            {
                module.OnPlayerLevelUp(data);
            }
            //if (data.TeamId > 0)
            //{
            //    var team = TeamManager.GetParty(data.TeamId);
            //    if (team != null)
            //    {
            //        team.UpdateMemberLevel(data.Id, data.Level);
            //    }
            //}
        }

        private void OnPlayerLoginOff(Dto.PlayerOnlineChange data)
        {
            // 切换频道也会被调用
            bool isLogin = data.IsNewComer;
            bool isLogoff = data.Channel == 0;
            if (data.GuildId > 0)
            {
                var guild = GuildManager.GetGuildById(data.GuildId);
                if (guild != null)
                {
                    guild.SetMemberChannel(data.Id, data.Channel);
                    guild.setOnline(data.Id, data.Channel > 0, data.Channel);

                    var chr = FindPlayerById(data.Channel, data.Id);
                    if (chr != null)
                        chr.sendPacket(GuildPackets.showGuildInfo(chr));

                    if (guild.AllianceId > 0)
                    {
                        var alliance = GuildManager.GetAllianceById(guild.AllianceId);
                        if (alliance != null)
                        {
                            if (chr != null)
                            {
                                chr.sendPacket(GuildPackets.updateAllianceInfo(alliance));
                                chr.sendPacket(GuildPackets.allianceNotice(alliance.AllianceId, alliance.getNotice()));
                            }

                            if (data.IsNewComer)
                                alliance.broadcastMessage(GuildPackets.allianceMemberOnline(guild, data.Id, true), data.Id);
                        }
                    }

                }
            }

            foreach (var module in Modules)
            {
                if (isLogin)
                    module.OnPlayerLogin(data);
            }
        }

        /// <summary>
        /// 成功：向受邀者发送请求，失败：向邀请者发送失败原因
        /// </summary>
        /// <param name="data"></param>
        private void OnSendInvitation(InvitationProto.CreateInviteResponse data)
        {
            InviteChannelHandlerRegistry.GetHandler(data.Type)?.OnInvitationCreated(data);
        }


        private void OnAnswerInvitation(InvitationProto.AnswerInviteResponse data)
        {
            InviteChannelHandlerRegistry.GetHandler(data.Type)?.OnInvitationAnswered(data);
        }


        private void InitializeMessage()
        {
            MessageDispatcher.Register<MessageProto.SetTimer>(BroadcastType.Broadcast_SetTimer, BroadcastSetTimer);
            MessageDispatcher.Register<MessageProto.RemoveTimer>(BroadcastType.Broadcast_RemoveTimer, BroadcastRemoveTimer);
            MessageDispatcher.Register<MessageProto.DropMessageBroadcast>(BroadcastType.Broadcast_DropMessage, OnDropMessage);
            MessageDispatcher.Register<MessageProto.PacketBroadcast>(BroadcastType.Broadcast_Packet, OnReceivedPacket);
            MessageDispatcher.Register<MessageProto.YellowTipBroadcast>(BroadcastType.Broadcast_YellowTip, OnYellowTip);

            MessageDispatcher.Register<Dto.SendWhisperMessageBroadcast>(BroadcastType.Whisper_Chat, BuddyManager.OnWhisperReceived);

            MessageDispatcher.Register<Dto.AddBuddyBroadcast>(BroadcastType.Buddy_Added, BuddyManager.OnAddBuddyBroadcast);
            MessageDispatcher.Register<Dto.BuddyChatBroadcast>(BroadcastType.Buddy_Chat, BuddyManager.OnBuddyChatReceived);
            MessageDispatcher.Register<Dto.NotifyBuddyWhenLoginoffBroadcast>(BroadcastType.Buddy_NotifyChannel, BuddyManager.OnBuddyNotifyChannel);
            MessageDispatcher.Register<Dto.SendBuddyNoticeMessageDto>(BroadcastType.Buddy_NoticeMessage, BuddyManager.OnBuddyNoticeMessageReceived);
            MessageDispatcher.Register<Dto.DeleteBuddyBroadcast>(BroadcastType.Buddy_Delete, BuddyManager.OnBuddyDeleted);

            MessageDispatcher.Register<Dto.MultiChatMessage>(BroadcastType.OnMultiChat, OnMulitiChat);

            var adminSrv = ServiceProvider.GetRequiredService<AdminService>();
            MessageDispatcher.Register<Empty>(BroadcastType.SaveAll, adminSrv.OnSaveAll);
            MessageDispatcher.Register<Empty>(BroadcastType.SendPlayerDisconnectAll, adminSrv.OnDisconnectAll);
            MessageDispatcher.Register<SystemProto.DisconnectPlayerByNameBroadcast>(BroadcastType.SendPlayerDisconnect, adminSrv.OnReceivedDisconnectCommand);
            MessageDispatcher.Register<SystemProto.SummonPlayerByNameBroadcast>(BroadcastType.SendWrapPlayerByName, adminSrv.OnPlayerSummoned);
            MessageDispatcher.Register<SystemProto.BanBroadcast>(BroadcastType.BroadcastBan, adminSrv.OnBannedNotify);
            MessageDispatcher.Register<SystemProto.SetGmLevelBroadcast>(BroadcastType.OnGmLevelSet, adminSrv.OnSetGmLevelNotify);

            MessageDispatcher.Register<Config.AutoBanIgnoredChangedNotifyDto>(BroadcastType.OnAutoBanIgnoreChangedNotify, AutoBanManager.OnIgoreDataChanged);
            MessageDispatcher.Register<Config.MonitorDataChangedNotifyDto>(BroadcastType.OnMonitorChangedNotify, MonitorManager.OnMonitorDataChanged);

            var reportSrv = ServiceProvider.GetRequiredService<ReportService>();
            MessageDispatcher.Register<Dto.SendReportBroadcast>(BroadcastType.OnReportReceived, reportSrv.OnGMReceivedReport);

            var itemSrc = ServiceProvider.GetRequiredService<ItemService>();

            MessageDispatcher.Register<Empty>(BroadcastType.OnShutdown, async data => await Shutdown());
            MessageDispatcher.Register<ItemProto.UseItemMegaphoneBroadcast>(BroadcastType.OnItemMegaphone, itemSrc.OnItemMegaphon);
            MessageDispatcher.Register<ItemProto.CreateTVMessageBroadcast>(BroadcastType.OnTVMessage, itemSrc.OnBroadcastTV);
            MessageDispatcher.Register<Empty>(BroadcastType.OnTVMessageFinish, itemSrc.OnBroadcastTVFinished);
            MessageDispatcher.Register<Dto.ReloadEventsResponse>(BroadcastType.OnEventsReloaded, OnEventsReloaded);
            MessageDispatcher.Register<Config.WorldConfig>(BroadcastType.OnWorldConfigUpdate, UpdateWorldConfig);
            MessageDispatcher.Register<CouponConfig>(BroadcastType.OnCouponConfigUpdate, UpdateCouponConfig);

            MessageDispatcher.Register<InvitationProto.CreateInviteResponse>(BroadcastType.OnInvitationSend, OnSendInvitation);
            MessageDispatcher.Register<InvitationProto.AnswerInviteResponse>(BroadcastType.OnInvitationAnswer, OnAnswerInvitation);

            MessageDispatcher.Register<PlayerLevelJobChange>(BroadcastType.OnPlayerLevelChanged, OnPlayerLevelChanged);
            MessageDispatcher.Register<PlayerLevelJobChange>(BroadcastType.OnPlayerJobChanged, OnPlayerJobChanged);
            MessageDispatcher.Register<PlayerOnlineChange>(BroadcastType.OnPlayerLoginOff, OnPlayerLoginOff);

            #region Guild
            MessageDispatcher.Register<GuildProto.UpdateGuildNoticeResponse>(BroadcastType.OnGuildNoticeUpdate, GuildManager.OnGuildNoticeUpdate);
            MessageDispatcher.Register<GuildProto.UpdateGuildGPResponse>(BroadcastType.OnGuildGpUpdate, GuildManager.OnGuildGPUpdate);
            MessageDispatcher.Register<GuildProto.UpdateGuildCapacityResponse>(BroadcastType.OnGuildCapacityUpdate, GuildManager.OnGuildCapacityIncreased);
            MessageDispatcher.Register<GuildProto.UpdateGuildEmblemResponse>(BroadcastType.OnGuildEmblemUpdate, GuildManager.OnGuildEmblemUpdate);
            MessageDispatcher.Register<GuildProto.UpdateGuildRankTitleResponse>(BroadcastType.OnGuildRankTitleUpdate, GuildManager.OnGuildRankTitleUpdate);
            MessageDispatcher.Register<GuildProto.UpdateGuildMemberRankResponse>(BroadcastType.OnGuildRankChanged, GuildManager.OnChangePlayerGuildRank);
            MessageDispatcher.Register<GuildProto.JoinGuildResponse>(BroadcastType.OnPlayerJoinGuild, GuildManager.OnPlayerJoinGuild);
            MessageDispatcher.Register<GuildProto.LeaveGuildResponse>(BroadcastType.OnPlayerLeaveGuild, GuildManager.OnPlayerLeaveGuild);
            MessageDispatcher.Register<GuildProto.ExpelFromGuildResponse>(BroadcastType.OnGuildExpelMember, GuildManager.OnGuildExpelMember);
            MessageDispatcher.Register<GuildProto.GuildDisbandResponse>(BroadcastType.OnGuildDisband, GuildManager.OnGuildDisband);
            #endregion

            #region Alliance
            MessageDispatcher.Register<AllianceProto.GuildJoinAllianceResponse>(BroadcastType.OnGuildJoinAlliance, GuildManager.OnGuildJoinAlliance);
            MessageDispatcher.Register<AllianceProto.GuildLeaveAllianceResponse>(BroadcastType.OnGuildLeaveAlliance, GuildManager.OnGuildLeaveAlliance);
            MessageDispatcher.Register<AllianceProto.AllianceExpelGuildResponse>(BroadcastType.OnAllianceExpelGuild, GuildManager.OnAllianceExpelGuild);
            MessageDispatcher.Register<AllianceProto.IncreaseAllianceCapacityResponse>(BroadcastType.OnAllianceCapacityUpdate, GuildManager.OnAllianceCapacityIncreased);
            MessageDispatcher.Register<AllianceProto.DisbandAllianceResponse>(BroadcastType.OnAllianceDisband, GuildManager.OnAllianceDisband);
            MessageDispatcher.Register<AllianceProto.UpdateAllianceNoticeResponse>(BroadcastType.OnAllianceNoticeUpdate, GuildManager.OnAllianceNoticeChanged);
            MessageDispatcher.Register<AllianceProto.ChangePlayerAllianceRankResponse>(BroadcastType.OnAllianceRankChange, GuildManager.OnPlayerAllianceRankChanged);
            MessageDispatcher.Register<AllianceProto.UpdateAllianceRankTitleResponse>(BroadcastType.OnAllianceRankTitleUpdate, GuildManager.OnAllianceRankTitleChanged);
            MessageDispatcher.Register<AllianceProto.AllianceChangeLeaderResponse>(BroadcastType.OnAllianceChangeLeader, GuildManager.OnAllianceLeaderChanged);
            #endregion

            #region ChatRoom
            MessageDispatcher.Register<SendChatRoomMessageResponse>(BroadcastType.OnChatRoomMessageSend, ChatRoomService.OnReceiveMessage);
            MessageDispatcher.Register<JoinChatRoomResponse>(BroadcastType.OnJoinChatRoom, ChatRoomService.OnPlayerJoinChatRoom);
            MessageDispatcher.Register<LeaveChatRoomResponse>(BroadcastType.OnLeaveChatRoom, ChatRoomService.OnPlayerLeaveChatRoom);
            #endregion

            #region NewYearCard
            MessageDispatcher.Register<Dto.SendNewYearCardResponse>(BroadcastType.OnNewYearCardSend, NewYearCardService.OnNewYearCardSend);
            MessageDispatcher.Register<Dto.ReceiveNewYearCardResponse>(BroadcastType.OnNewYearCardReceived, NewYearCardService.OnNewYearCardReceived);
            MessageDispatcher.Register<Dto.NewYearCardNotifyDto>(BroadcastType.OnNewYearCardNotify, NewYearCardService.OnNewYearCardNotify);
            MessageDispatcher.Register<Dto.DiscardNewYearCardResponse>(BroadcastType.OnNewYearCardDiscard, NewYearCardService.OnNewYearCardDiscard);
            #endregion

            MessageDispatcher.Register<TeamProto.UpdateTeamResponse>(BroadcastType.OnTeamUpdate, msg => TeamManager.ProcessUpdateResponse(msg));

            MessageDispatcher.Register<Dto.SendNoteResponse>(BroadcastType.OnNoteSend, NoteService.OnNoteReceived);

            MessageDispatcher.Register<LifeProto.CreatePLifeRequest>(BroadcastType.OnPLifeCreated, DataService.OnPLifeCreated);
            MessageDispatcher.Register<LifeProto.RemovePLifeResponse>(BroadcastType.OnPLifeRemoved, DataService.OnPLifeRemoved);
        }

        public void OnMessageReceived(BaseProto.MessageWrapper message)
        {
            MessageDispatcher.Dispatch(message);
        }

        public void OnMessageReceived(string type, IMessage message)
        {
            MessageDispatcher.Dispatch(type, message);
        }

        internal void SendReloadEvents(IPlayer chr)
        {
            Transport.SendReloadEvents(new Dto.ReloadEventsRequest { MasterId = chr.Id });
        }

        private void OnEventsReloaded(Dto.ReloadEventsResponse data)
        {
            IPlayer? sender = null;
            foreach (var ch in Servers.Values)
            {
                ch.reloadEventScriptManager();

                if (sender == null)
                {
                    sender = ch.Players.getCharacterById(data.Request.MasterId);
                    sender?.dropMessage(5, "Reloaded Events");
                }
            }
        }

        void OnMulitiChat(MultiChatMessage data)
        {
            foreach (var cid in data.Receivers)
            {
                var chr = FindPlayerById(cid);
                if (chr != null && !chr.isAwayFromWorld())
                {
                    chr.sendPacket(PacketCreator.multiChat(data.FromName, data.Text, data.Type));
                }
            }
        }
    }
}
