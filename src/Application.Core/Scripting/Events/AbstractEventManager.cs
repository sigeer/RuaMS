using Application.Core.Channel;
using Application.Core.Game.Maps;
using Application.Core.model;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Utility.Performance;
using Application.Utility.Tickables;
using System.Collections.Concurrent;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public abstract class AbstractEventManager : ITickableTree, IAsyncDisposable
    {
        protected ILogger log = LogFactory.GetLogger(LogType.EventManager);

        protected WorldChannel cserv;
        public WorldChannel ChannelServer => cserv;
        public string Name { get; }

        public TickableStatus Status { get; private set; }
        public List<ITickable> SubTickables { get; private set; }

        protected ConcurrentDictionary<string, AbstractEventInstanceManager> instances = new();
        protected Dictionary<int, bool> openedLobbys = new();
        public int MaxLobbys => Template.MaxLobbys;

        private Queue<AbstractEventInstanceManager> readyInstances = new();
        private int readyId = 0;
        protected int onLoadInstances = 0;

        protected HashSet<int> playerPermit = new();
        protected SemaphoreSlim startSemaphore = new SemaphoreSlim(7);

        public AbstractEventTemplate Template { get; set; }
        public virtual AbstractEventTemplate GetTemplate => Template;
        public int MinCount => Template.MinCount;
        public int MaxCount => Template.MaxCount;

        public int MinLevel => Template.MinLevel;
        public int MaxLevel => Template.MaxLevel;

        public int EntryMap => Template.EntryMap;
        public int EntryPortal => Template.EntryPortal;
        public int ExitMap => Template.ExitMap;
        public int ExitPortal => Template.ExitPortal;

        public int ClearMap => Template.ClearMap;

        public int MinMap => Template.MinMap;
        public int MaxMap => Template.MaxMap;
        public int[] IncludedMap => Template.IncludedMap;

        public int EventTime => Template.EventTime;
        public bool AllowReconnect => Template.AllowReconnect;

        public Dictionary<int, RewardPools> AllClearRewards => Template.AllClearRewards;
        public Dictionary<int, (int Exp, int Meso)> StageClearRewards => Template.StageClearRewards;

        public AbstractEventManager(WorldChannel cserv, AbstractEventTemplate template)
        {
            this.cserv = cserv;
            Name = template.Name;
            Template = template;
            SubTickables = [];
        }

        bool disposed = false;
        protected bool isDisposed()
        {
            return disposed;
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (disposed)
                return;

            disposed = true;
            Status = TickableStatus.Remove;

            var eimList = instances.Values.ToList();
            instances.Clear();

            foreach (var eim in eimList)
            {
                await eim.DisposeAsync();
            }

            openedLobbys.Clear();

            Queue<AbstractEventInstanceManager> readyEims = new(readyInstances);
            readyInstances.Clear();

            foreach (var eim in readyEims)
            {
                await eim.DisposeAsync();
            }
            onLoadInstances = 0;
        }

        public long getLobbyDelay()
        {
            return YamlConfig.config.server.EVENT_LOBBY_DELAY;
        }

        public WorldChannel getChannelServer()
        {
            return cserv;
        }

        public async Task<IMap> GetMap(int mapId)
        {
            var map = await getChannelServer().getMapFactory().getMap(mapId);
            return map;
        }

        public string getName()
        {
            return Name;
        }

        public virtual async Task OnTick(long now)
        {
            await this.ProcessSubTickables(now);
        }

        protected virtual AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new EventInstanceManager(this, instanceName);
        }

        public AbstractEventInstanceManager? getInstance(string name)
        {
            return instances.GetValueOrDefault(name);
        }

        public List<AbstractEventInstanceManager> getInstances()
        {
            return instances.Values.ToList();
        }

        protected AbstractEventInstanceManager newInstance(string instanceName)
        {
            var ret = getReadyInstance() ?? CreateNewInstance(instanceName);
            ret.setName(instanceName);
            return ret;
        }

        bool RegisterInstanceInternal(string instanceName, AbstractEventInstanceManager eim)
        {
            if (instances.TryAdd(instanceName, eim))
            {
                SubTickables.Add(eim);

                GameMetrics.ChannelEventInstanceCount.Add(1,
                    new KeyValuePair<string, object?>("Channel", cserv.InstanceName),
                    new KeyValuePair<string, object?>("Event", getName()));

                return true;
            }
            return false;
        }

        protected virtual void DisposeInstanceInternal(string name)
        {
            if (instances.TryRemove(name, out var eim))
            {
                eim.Status = TickableStatus.Remove;

                GameMetrics.ChannelEventInstanceCount.Add(-1,
                    new KeyValuePair<string, object?>("Channel", cserv.InstanceName),
                    new KeyValuePair<string, object?>("Event", getName()));

                if (eim != null)
                    UnregisterLobby(eim.LobbyId);
            }
        }

        public void ProcessDisposeInstanceInternal(string name) => DisposeInstanceInternal(name);

        public void DisposeInstance(string instanceName)
        {
            SubTickables.Add(new DelayedDisposeRequest(this, instanceName, getChannelServer().Node.getCurrentTime() + YamlConfig.config.server.EVENT_LOBBY_DELAY * 1000));
        }

        protected async Task<AbstractEventInstanceManager> CreateInstance(int level, int lobbyId)
        {
            return await Setup(level, lobbyId);
        }

        protected void registerEventInstance(AbstractEventInstanceManager eim, int lobbyId)
        {
            if (!RegisterInstanceInternal(eim.getName(), eim))
                throw new EventInstanceInProgressException(eim.getName(), this.getName());
            eim.LobbyId = lobbyId;
        }

        private AbstractEventInstanceManager? getReadyInstance()
        {
            try
            {
                if (readyInstances.TryDequeue(out var eim))
                    return eim;
                return null;
            }
            finally
            {
                fillEimQueue();
            }
        }

        private void fillEimQueue()
        {
            Task.Run(instantiateQueuedInstance);
        }

        public void instantiateQueuedInstance()
        {
            int nextEventId;

            if (this.isDisposed() || readyInstances.Count + onLoadInstances >= Math.Ceiling(MaxLobbys / 3.0))
            {
                return;
            }

            onLoadInstances++;
            nextEventId = readyId;
            readyId++;

            var eim = CreateNewInstance("sampleName" + nextEventId);

            if (this.isDisposed())
            {
                return;
            }

            readyInstances.Enqueue(eim);
            onLoadInstances--;

            instantiateQueuedInstance();
        }

        public string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            return Template.HandleCreateInstanceResult(r, c);
        }

        protected void UnregisterLobby(int lobbyId)
        {
            openedLobbys[lobbyId] = false;
        }

        protected bool TryRegisterLobby(int lobbyId)
        {
            if (lobbyId < 0)
            {
                lobbyId = 0;
            }

            if (!openedLobbys.TryGetValue(lobbyId, out var value) || !value)
            {
                openedLobbys[lobbyId] = true;
                return true;
            }

            return false;
        }

        protected int GetAvailableLobbyInstance()
        {
            int maxLobbies = MaxLobbys;

            if (maxLobbies > 0)
            {
                for (int i = 0; i < maxLobbies; i++)
                {
                    if (TryRegisterLobby(i))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public abstract Task<CreateInstanceResult> StartInstance(Player leader, int difficulty = 1, int lobbyId = -1);

        public virtual async Task<AbstractEventInstanceManager> Setup(int level, int lobbyId)
        {
            var eim = newInstance(Name + lobbyId);
            eim.setProperty("level", level);

            await Template.OnSetup(eim, level, lobbyId);
            await Template.respawnStages(eim);
            await eim.startEventTimer(EventTime * 1000);
            await Template.setEventRewards(eim);

            return eim;
        }


        class DelayedDisposeRequest : DelayedTickable
        {
            AbstractEventManager _src;
            string _instanceName;

            public DelayedDisposeRequest(AbstractEventManager src, string instanceName, long next) : base(next)
            {
                _src = src;
                _instanceName = instanceName;
            }

            protected override Task Handle(long now)
            {
                _src.ProcessDisposeInstanceInternal(_instanceName);
                return Task.CompletedTask;
            }
        }
    }
}