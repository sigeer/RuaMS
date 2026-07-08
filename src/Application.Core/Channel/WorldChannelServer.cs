using Application.Core.Channel.Actor;
using Application.Core.Channel.DataProviders;
using Application.Core.Channel.DueyService;
using Application.Core.Channel.Message;
using Application.Core.Channel.Modules;
using Application.Core.Channel.Net;
using Application.Core.Channel.ServerData;
using Application.Core.Channel.Services;
using Application.Core.Channel.Tasks;
using Application.Core.Game.Skills;
using Application.Core.Gameplay.Plugins;
using Application.Core.ServerTransports;
using Application.Shared.Login;
using Application.Shared.Servers;
using Application.Utility.Pipeline;
using Application.Utility.Tickables;
using AutoMapper;
using Config;
using Google.Protobuf;
using MessageProto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SyncProto;
using System.Diagnostics;
using System.Net;

namespace Application.Core.Channel
{
    public class WorldChannelServer : IServerBase<IChannelServerTransport>, IActorInstance<WorldChannelServer>, IServiceCenter, ITickable
    {
        public IServiceProvider ServiceProvider { get; }
        public IChannelServerTransport Transport { get; }
        public Dictionary<int, IChannelServer> Servers { get; set; }
        public Dictionary<ChannelConfig, WorldChannel> ServerConfigMapping { get; private set; }

        public bool IsRunning { get; private set; }

        public ChannelServerConfig ServerConfig { get; }

        public string InstanceName => ServerConfig.ServerName;
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

        public NodeTickTask NodeTickTask { get; }
        public MapOwnershipTask MapOwnershipTask { get; }
        #endregion

        #region GameConfig
        public Config.WorldConfig? WorldConfigBackup { get; private set; }

        #endregion
        public List<AbstractChannelModule> Modules { get; private set; }

        public ExpeditionService ExpeditionService { get; }
        Lazy<MessageDispatcherNew> _messageDispatcher;
        public MessageDispatcherNew MessageDispatcherV => _messageDispatcher.Value;


        ScheduledFuture? invitationTask;
        ScheduledFuture? playerShopTask;
        ScheduledFuture? timeoutTask;
        ScheduledFuture? checkMapActiveTask;

        public BatchSyncManager<int, SyncProto.MapSyncDto> BatchSynMapManager { get; }
        public BatchSyncManager<int, SyncProto.PlayerSaveDto> BatchSyncPlayerManager { get; }

        public CommandLoop<WorldChannelServer> CommandLoop { get; }

        public TickableStatus Status => throw new NotImplementedException();
        public PluginManager PluginManager { get; }
        public IMapper Mapper { get; }

        public WorldChannelServer(IServiceProvider sp,
            IChannelServerTransport transport,
            IOptions<ChannelServerConfig> serverConfigOptions,
            ILogger<WorldChannelServer> logger,
            CashItemProvider cashItemProvider,
            IMapper mapper
            )
        {
            ServiceProvider = sp;
            Transport = transport;
            _logger = logger;
            Mapper = mapper;

            Modules = new();
            Servers = new();
            ServerConfigMapping = new();
            ServerConfig = serverConfigOptions.Value;

            _skillbookInformationProvider = new(() => ServiceProvider.GetRequiredService<SkillbookInformationProvider>());
            CashItemProvider = cashItemProvider;


            ServerMessageTask = new ServerMessageTask(this);
            NodeTickTask = new NodeTickTask(this);
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

            PluginManager = new(this);

            _messageDispatcher = new(() => new(this));
            CommandLoop = new(this);
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
        public void UpdateServerTime(long v)
        {
            serverCurrentTime = currentTime.addAndGet(v);
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



        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public async Task Shutdown(int delaySeconds = -1)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (!IsRunning)
                {
                    _logger.LogInformation("未启动");
                    return;
                }
                _logger.LogInformation("正在停止...");

                watcher?.Dispose();
                await NodeTickTask.StopAsync();
                await MapOwnershipTask.StopAsync();
                await ServerMessageTask.StopAsync();

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

                foreach (var server in Servers.Values)
                {
                    await server.Shutdown(delaySeconds);
                }
                await PluginManager.DisposeAsync();

                await TimerManager.Stop();
                await CommandLoop.DisposeAsync();

                _logger.LogInformation("停止{Status}", "成功");

                await Transport.CompleteChannelShutdown();
                IsRunning = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止{Status}", "失败");
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

            CommandLoop.Start();

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

        FileSystemWatcher? watcher;
        public async Task<bool> HandleServerRegistered(RegisterServerResult configs, CancellationToken cancellationToken = default)
        {
            if (configs.StartChannel <= 0)
            {
                _logger.LogError("注册服务器失败, {Message}", configs.Message);
                return false;
            }

            TimerManager = await TimerManagerFactory.InitializeAsync(TaskEngine.Quartz, InstanceName);

            OpcodeConstants.generateOpcodeNames();
            ForceUpdateServerTime();


            var channel = configs.StartChannel;
            foreach (var server in effectChannels)
            {
                var scope = ServiceProvider.CreateScope();
                var worldChannel = new WorldChannel(channel, this, scope, ServerConfig.ServerHost, server.Key, server.Value);
                await worldChannel.Initialize(configs);

                Servers[channel] = worldChannel;
                ServerConfigMapping[server.Key] = worldChannel;

                channel++;
                await worldChannel.StartServer(cancellationToken);
            }

            try
            {
                watcher = new FileSystemWatcher(PluginManager.PluginDir, "Application.Plugin.*.dll") { NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName };
                watcher.Changed += OnFileChanged;
                watcher.Created += OnFileChanged;
                watcher.Deleted += OnFileDeleted;
                watcher.EnableRaisingEvents = true;

                await LoadAllPlugins();
            }
            catch (Exception ex)
            {
                _logger.LogError("注册脚本失败, {Message}", ex.Message);
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


            await NodeTickTask.Register(TimerManager);
            await ServerMessageTask.Register(TimerManager);
            await MapOwnershipTask.Register(TimerManager);

            invitationTask = await TimerManager.register(new InvitationTask(this), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
            playerShopTask = await TimerManager.register(new PlayerShopTask(this), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

#if !DEBUG
            timeoutTask = await TimerManager.register(new net.server.task.TimeoutTask(this), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
#endif


            foreach (var module in Modules)
            {
                module.Initialize();
                module.RegisterTask(TimerManager);
            }

            return IsRunning = true;
        }

        private DateTime _lastLoadTime = DateTime.MinValue;

        private async Task LoadAllPlugins()
        {
            var pluginFiles = Directory.GetFiles(PluginManager.PluginDir, "Application.Plugin.*.dll");
            foreach (var pluginFile in pluginFiles)
            {
                var pluginName = Path.GetFileName(pluginFile);
                try
                {
                    _logger.LogInformation("加载插件: {PluginName}...", pluginName);
                    await PluginManager.LoadPlugin(pluginName);
                    _logger.LogInformation("加载插件: {PluginName}...完成", pluginName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载插件失败: {PluginName}", pluginName);
                }
            }
        }
        const string PluginPrefix = "Application.Plugin.";
        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name == null || !e.Name.StartsWith(PluginPrefix)) return;

            var now = DateTime.Now;
            if ((now - _lastLoadTime) < TimeSpan.FromSeconds(1))
                return;

            _lastLoadTime = now;

            await Task.Delay(200);
            _logger.LogInformation("插件更新: {PluginName}", e.Name);

            await PluginManager.LoadPlugin(e.Name);
        }

        private async void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name == null || !e.Name.StartsWith(PluginPrefix)) return;

            _logger.LogInformation("插件删除: {PluginName}", e.Name);

            var pluginName = Path.GetFileNameWithoutExtension(e.Name);
            await PluginManager.UnloadPlugin(pluginName);
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

        public Task SendBroadcastWorldPacket(Packet p, bool onGM = false)
        {
            return Transport.BroadcastMessage(new PacketRequest { Data = ByteString.CopyFrom(p.getBytes()), OnlyGM = onGM });
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

        public void UpdateWorldConfig(Config.WorldConfig updatePatch)
        {
            WorldConfigBackup = updatePatch;
        }


        public IPEndPoint GetChannelEndPoint(int channel)
        {
            return Transport.GetChannelEndPoint(channel);
        }

        public bool CheckCharacterName(string name)
        {
            return Transport.CheckCharacterName(name);
        }

        public Task SendReloadEvents(Player chr)
        {
            return Transport.SendReloadEvents(new Dto.ReloadEventsRequest { MasterId = chr.Id });
        }

        public void PushChannelCommand(ICommand command)
        {
            foreach (var item in Servers.Values)
            {
                item.Send(command);
            }
        }

        public async Task PushChannelCommandAsync(ICommand command)
        {
            await Task.WhenAll(Servers.Values.Select(item => item.Send(command)));
        }

        public void Broadcast(Func<WorldChannel, Task> action)
        {
            foreach (var item in Servers.Values)
            {
                item.Send(action);
            }
        }

        public void Broadcast(Action<WorldChannel> action)
        {
            foreach (var item in Servers.Values)
            {
                item.Send(action);
            }
        }


        public async Task BroadcastAsync(Action<WorldChannel> action)
        {
            foreach (var item in Servers.Values)
            {
                await item.Send(action);
            }
        }

        public async Task BroadcastAsync(Func<WorldChannel, Task> action)
        {
            foreach (var item in Servers.Values)
            {
                await item.Send(action);
            }
        }


        internal DistributeSession<int, PlayerSaveDto> CreateSyncPlayerSession()
        {
            return new DistributeSession<int, PlayerSaveDto>(Servers.Values.Select(x => x.Id));
        }

        public Task Send(ICommand command) => CommandLoop.Register(command);

        public Task Send(Func<WorldChannelServer, Task> action) => Send(new AsyncNodeDelegateCommand(action));

        public Task Send(Action<WorldChannelServer> action) => Send(new NodeDelegateCommand(action));

        public async Task OnTick(long now)
        {
            await Send(new NodeTickCommand(w =>
            {
                foreach (var item in w.Servers.Values)
                {
                    item.OnTick(now);
                }
            }));

        }

        /// <summary>
        /// 在本节点的所有频道中查找目标玩家并执行操作。
        /// 节点内频道数通常很少（1-5），直接遍历即可。
        /// </summary>
        public async Task SendToPlayerAsync(int playerId, Func<Player, Task> action)
        {
            foreach (var ch in Servers.Values)
            {
                if (await ch.SendToPlayerAsync(playerId, action))
                {
                    return;
                }
            }
        }

        public async Task SendToPlayerAsync(int playerId, Action<Player> action)
        {
            foreach (var ch in Servers.Values)
            {
                if (await ch.SendToPlayerAsync(playerId, action))
                {
                    return;
                }
            }
        }


        public async Task SendToPlayerAsync(int channel, int playerId, Func<Player, Task> action)
        {
            var ch = Servers.GetValueOrDefault(channel);
            if (ch != null)
            {
                await ch.SendToPlayerAsync(playerId, action);
            }
        }

        public async Task SendToPlayersAsync(IEnumerable<int> playerIds, Func<Player, Task> action)
        {
            var ids = playerIds.ToList();
            if (ids.Count == 0)
                return;

            var count = 0;
            foreach (var ch in Servers.Values)
            {
                count += await ch.SendToPlayersAsync(ids, action);
                if (count == ids.Count)
                {
                    break;
                }
            }
        }

        public async Task SendToPlayersAsync(IEnumerable<int> playerIds, Action<Player> action)
        {

            var ids = playerIds.ToList();
            if (ids.Count == 0)
                return;

            var count = 0;
            foreach (var ch in Servers.Values)
            {
                count += await ch.SendToPlayersAsync(ids, action);
                if (count == ids.Count)
                {
                    break;
                }
            }
        }

        public async Task BroadcastPlayersAsync(Func<Player, Task> action)
        {
            foreach (var ch in Servers.Values)
            {
                await ch.BroadcastPlayersAsync(action);
            }
        }

    }
}
