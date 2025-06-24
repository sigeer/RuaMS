using Application.Core.Channel.Events;
using Application.Core.Channel.Invitation;
using Application.Core.Channel.Message;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Channel.Tasks;
using Application.Core.ServerTransports;
using Application.Shared.Configs;
using Application.Shared.Login;
using Application.Shared.Message;
using Application.Shared.Servers;
using Config;
using Dto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.server.guild;
using Polly;
using server;
using System.Diagnostics;
using System.Net;
using tools;

namespace Application.Core.Channel
{
    public class WorldChannelServer : IServerBase<IChannelServerTransport>
    {
        readonly IServiceProvider _sp;
        public IChannelServerTransport Transport { get; }
        public Dictionary<int, WorldChannel> Servers { get; set; }
        public bool IsRunning { get; private set; }

        public ChannelServerConfig ServerConfig { get; set; }
        public string ServerName => ServerConfig.ServerName;
        public SkillbookInformationProvider SkillbookInformationProvider { get; }
        readonly ILogger<WorldChannelServer> _logger;

        public DateTimeOffset StartupTime { get; private set; }

        #region Data
        public GuildManager GuildManager { get; private set; } = null!;
        public TeamManager TeamManager { get; private set; } = null!;
        public ChatRoomService ChatRoomService { get; private set; } = null!;
        public NewYearCardService NewYearCardService { get; private set; } = null!;
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
        public List<ChannelModule> Modules { get; }
        public InviteChannelHandlerRegistry InviteChannelHandlerRegistry { get; }
        public ITimerManager TimerManager { get; private set; } = null!;

        public ExpeditionService ExpeditionService { get; }
        ScheduledFuture? invitationTask;
        public WorldChannelServer(IServiceProvider sp, IChannelServerTransport transport, IOptions<ChannelServerConfig> serverConfigOptions, ILogger<WorldChannelServer> logger)
        {
            _sp = sp;
            Transport = transport;
            _logger = logger;

            Servers = new();
            ServerConfig = serverConfigOptions.Value;

            SkillbookInformationProvider = _sp.GetRequiredService<SkillbookInformationProvider>();
            Modules = _sp.GetServices<ChannelModule>().ToList();

            CharacterDiseaseManager = new CharacterDiseaseManager(this);
            PetHungerManager = new PetHungerManager(this);
            ServerMessageManager = new ServerMessageManager(this);
            CharacterHpDecreaseManager = new CharacterHpDecreaseManager(this);
            MapObjectManager = new MapObjectManager(this);
            MountTirednessManager = new MountTirednessManager(this);
            MapOwnershipManager = new MapOwnershipManager(this);

            ExpeditionService = _sp.GetRequiredService<ExpeditionService>();

            InviteChannelHandlerRegistry = _sp.GetRequiredService<InviteChannelHandlerRegistry>();
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

            foreach (var ch in Servers.Values)
            {
                foreach (var chr in ch.getPlayerStorage().getAllCharacters())
                {
                    chr.updateCouponRates();
                }
            }
        }

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public async Task Shutdown()
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

                InviteChannelHandlerRegistry.Dispose();

                foreach (var channel in Servers.Values)
                {
                    await channel.Shutdown();
                }
                await TimerManager.Stop();
                _logger.LogInformation("[{ServerName}] 停止{Status}", ServerName, "成功");

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

            _ = Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                _logger.LogInformation("[{ServerName}]加载WZ - 能手册...", ServerName);
                SkillbookInformationProvider.LoadAllSkillbookInformation();
                _logger.LogInformation("[{ServerName}]加载WZ - 能手册加载完成, 耗时{Cost}", ServerName, sw.Elapsed.TotalSeconds);
            });

            GuildManager = _sp.GetRequiredService<GuildManager>();
            TeamManager = _sp.GetRequiredService<TeamManager>();
            ChatRoomService = _sp.GetRequiredService<ChatRoomService>();
            NewYearCardService = _sp.GetRequiredService<NewYearCardService>();

            TimerManager = await server.TimerManager.InitializeAsync(TaskEngine.Quartz, ServerName);

            CharacterDiseaseManager.Register(TimerManager);
            PetHungerManager.Register(TimerManager);
            ServerMessageManager.Register(TimerManager);
            CharacterHpDecreaseManager.Register(TimerManager);
            MapObjectManager.Register(TimerManager);
            MountTirednessManager.Register(TimerManager);
            MapOwnershipManager.Register(TimerManager);

            invitationTask = TimerManager.register(new InvitationTask(this), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            InviteChannelHandlerRegistry.Register(_sp.GetServices<InviteChannelHandler>());
            InitializeMessage();

            foreach (var module in Modules)
            {
                module.Initialize();
            }

            List<WorldChannel> localServers = [];
            foreach (var config in ServerConfig.ChannelConfig)
            {
                var scope = _sp.CreateScope();
                var channel = new WorldChannel(this, scope, config);
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
            var configs = await registerPolicy.ExecuteAsync(async () => await Transport.RegisterServer(this, localServers));

            if (configs.StartChannel > 0)
            {
                ForceUpdateServerTime();

                foreach (var server in localServers)
                {
                    var channel = configs.StartChannel++;
                    server.Register(channel);
                    Servers[channel] = server;
                }

                UpdateWorldConfig(new WorldConfigPatch
                {
                    MobRate = configs.Config.MobRate,
                    MesoRate = configs.Config.MesoRate,
                    ExpRate = configs.Config.ExpRate,
                    DropRate = configs.Config.DropRate,
                    BossDropRate = configs.Config.BossDropRate,
                    QuestRate = configs.Config.QuestRate,
                    TravelRate = configs.Config.TravelRate,
                    FishingRate = configs.Config.FishingRate,
                    ServerMessage = configs.Config.ServerMessage
                });
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

        public IPlayer? FindPlayerById(int cid)
        {
            foreach (var item in Servers.Values)
            {
                var chr = item.Players.getCharacterById(cid);
                if (chr != null)
                    return chr;
            }
            return null;
        }

        public IPlayer? FindPlayerById(int channel, int cid)
        {
            if (Servers.TryGetValue(channel, out var ch))
                return ch.Players.getCharacterById(cid);

            return null;
        }

        internal WorldChannel? GetChannel(int channel)
        {
            return Servers.GetValueOrDefault(channel);
        }

        public void CommitAccountEntity(AccountCtrl accountEntity)
        {
            Transport.UpdateAccount(accountEntity);
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

        public void BroadcastWorldMessage(Packet p)
        {
            Transport.BroadcastMessage(p);
        }
        public void BroadcastWorldGMPacket(Packet packet)
        {
            Transport.BroadcastGMMessage(packet);
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

        public void SendMultiChat(int type, string nameFrom, int[] value, string chatText)
        {
            foreach (var server in Servers.Values)
            {
                foreach (var cid in value)
                {
                    var chr = server.Players.getCharacterById(cid);
                    if (chr != null)
                    {
                        chr.sendPacket(PacketCreator.multiChat(nameFrom, chatText, type));
                    }
                }
            }
        }

        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return Transport.GetChannelEndPoint(channel);
        }

        public bool WarpPlayer(string name, int? channel, int mapId, int? portal)
        {
            return Transport.WarpPlayer(name, channel, mapId, portal);
        }

        public string GetExpeditionInfo()
        {
            return Transport.LoadExpeditionInfo();
        }

        public bool CheckCharacterName(string name)
        {
            return Transport.CheckCharacterName(name);
        }

        public void NotifyPartner(int id)
        {
            Transport.NotifyPartner(id);
        }

        public void DropMessage(DropMessageDto msg)
        {
            foreach (var ch in Servers.Values)
            {
                if (msg.PlayerId.Contains(-1))
                {
                    foreach (var player in ch.Players.getAllCharacters())
                    {
                        player.dropMessage(msg.Type, msg.Message);
                    }
                }
                else
                {
                    foreach (var id in msg.PlayerId)
                    {
                        ch.Players.getCharacterById(id)?.dropMessage(msg.Type, msg.Message);
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
        }

        /// <summary>
        /// 成功：向受邀者发送请求，失败：向邀请者发送失败原因
        /// </summary>
        /// <param name="data"></param>
        private void OnSendInvitation(Dto.CreateInviteResponse data)
        {
            InviteChannelHandlerRegistry.GetHandler(data.Type)?.OnInvitationCreated(data);
        }


        private void OnAnswerInvitation(AnswerInviteResponse data)
        {
            InviteChannelHandlerRegistry.GetHandler(data.Type)?.OnInvitationAnswered(data);
        }


        private void InitializeMessage()
        {
            MessageDispatcher.Register<Empty>(BroadcastType.OnShutdown, async data => await Shutdown());

            MessageDispatcher.Register<CreateInviteResponse>(BroadcastType.OnInvitationSend, OnSendInvitation);
            MessageDispatcher.Register<AnswerInviteResponse>(BroadcastType.OnInvitationAnswer, OnAnswerInvitation);

            MessageDispatcher.Register<PlayerLevelJobChange>(BroadcastType.OnPlayerLevelChanged, OnPlayerLevelChanged);
            MessageDispatcher.Register<PlayerLevelJobChange>(BroadcastType.OnPlayerJobChanged, OnPlayerJobChanged);
            MessageDispatcher.Register<PlayerOnlineChange>(BroadcastType.OnPlayerLoginOff, OnPlayerLoginOff);

            #region Guild
            MessageDispatcher.Register<UpdateGuildNoticeResponse>(BroadcastType.OnGuildNoticeUpdate, GuildManager.OnGuildNoticeUpdate);
            MessageDispatcher.Register<UpdateGuildGPResponse>(BroadcastType.OnGuildGpUpdate, GuildManager.OnGuildGPUpdate);
            MessageDispatcher.Register<UpdateGuildCapacityResponse>(BroadcastType.OnGuildCapacityUpdate, GuildManager.OnGuildCapacityIncreased);
            MessageDispatcher.Register<UpdateGuildEmblemResponse>(BroadcastType.OnGuildEmblemUpdate, GuildManager.OnGuildEmblemUpdate);
            MessageDispatcher.Register<UpdateGuildRankTitleResponse>(BroadcastType.OnGuildRankTitleUpdate, GuildManager.OnGuildRankTitleUpdate);
            MessageDispatcher.Register<UpdateGuildMemberRankResponse>(BroadcastType.OnGuildRankChanged, GuildManager.OnChangePlayerGuildRank);
            MessageDispatcher.Register<JoinGuildResponse>(BroadcastType.OnPlayerJoinGuild, GuildManager.OnPlayerJoinGuild);
            MessageDispatcher.Register<LeaveGuildResponse>(BroadcastType.OnPlayerLeaveGuild, GuildManager.OnPlayerLeaveGuild);
            MessageDispatcher.Register<ExpelFromGuildResponse>(BroadcastType.OnGuildExpelMember, GuildManager.OnGuildExpelMember);
            MessageDispatcher.Register<GuildDisbandResponse>(BroadcastType.OnGuildDisband, GuildManager.OnGuildDisband);
            #endregion

            #region Alliance
            MessageDispatcher.Register<GuildJoinAllianceResponse>(BroadcastType.OnGuildJoinAlliance, GuildManager.OnGuildJoinAlliance);
            MessageDispatcher.Register<GuildLeaveAllianceResponse>(BroadcastType.OnGuildLeaveAlliance, GuildManager.OnGuildLeaveAlliance);
            MessageDispatcher.Register<AllianceExpelGuildResponse>(BroadcastType.OnAllianceExpelGuild, GuildManager.OnAllianceExpelGuild);
            MessageDispatcher.Register<IncreaseAllianceCapacityResponse>(BroadcastType.OnAllianceCapacityUpdate, GuildManager.OnAllianceCapacityIncreased);
            MessageDispatcher.Register<DisbandAllianceResponse>(BroadcastType.OnAllianceDisband, GuildManager.OnAllianceDisband);
            MessageDispatcher.Register<UpdateAllianceNoticeResponse>(BroadcastType.OnAllianceNoticeUpdate, GuildManager.OnAllianceNoticeChanged);
            MessageDispatcher.Register<ChangePlayerAllianceRankResponse>(BroadcastType.OnAllianceRankChange, GuildManager.OnPlayerAllianceRankChanged);
            MessageDispatcher.Register<UpdateAllianceRankTitleResponse>(BroadcastType.OnAllianceRankTitleUpdate, GuildManager.OnAllianceRankTitleChanged);
            MessageDispatcher.Register<AllianceChangeLeaderResponse>(BroadcastType.OnAllianceChangeLeader, GuildManager.OnAllianceLeaderChanged);
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

            MessageDispatcher.Register<UpdateTeamResponse>(BroadcastType.OnTeamUpdate, msg => TeamManager.ProcessUpdateResponse(msg));

            MessageDispatcher.Register<DropMessageDto>(BroadcastType.OnDropMessage, DropMessage);
            MessageDispatcher.Register<CouponConfig>(BroadcastType.OnCouponConfigUpdate, UpdateCouponConfig);
        }

        public void OnMessageReceived(string type, object message)
        {
            MessageDispatcher.Dispatch(type, message);
        }
    }
}
