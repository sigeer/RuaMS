using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.ChannelEvents;
using Application.Core.scripting.Events.Abstraction;
using Application.Resources.Messages;
using Application.Shared.Login;
using Application.Utility.Performance;
using Application.Utility.Tickables;
using scripting.Event;
using scripting.npc;
using System.Collections.Concurrent;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    /// <summary>
    /// 会生成副本的事件
    /// </summary>
    public abstract class AbstractInstancedEventManager : EventManager
    {
        protected ConcurrentDictionary<string, AbstractEventInstanceManager> instances = new();
        protected Dictionary<int, bool> openedLobbys = new();
        public int MaxLobbys { get; set; }
        public const int DefaultMaxLobbys = 1;


        /// <summary>
        /// 预生成的 EventInstanceManager
        /// </summary>
        private Queue<AbstractEventInstanceManager> readyInstances = new();
        private int readyId = 0;
        protected int onLoadInstances = 0;

        protected HashSet<int> playerPermit = new();
        protected SemaphoreSlim startSemaphore = new SemaphoreSlim(7);


        public int MinCount { get; protected set; } = 1;
        public int MaxCount { get; init; } = 6;

        public int MinLevel { get; protected set; } = 1;
        public int MaxLevel { get; init; } = 255;

        public int EntryMap { get; init; } = MapId.NONE;
        public int EntryPortal { get; init; }
        public int ExitMap { get; init; } = MapId.NONE;
        public int ExitPortal { get; init; }

        public int ClearMap { get; init; }

        public int MinMap { get; init; }
        public int MaxMap { get; init; }

        /// <summary>
        /// 单位：秒
        /// </summary>
        public int EventTime { get; protected set; }
        public bool AllowReconnect { get; init; } = true;


        public AbstractInstancedEventManager(WorldChannel cserv, string name) : base(cserv, name)
        {

        }

        public override void Initialize()
        {
            MinCount = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 1 : MinCount;
        }

        #region Instances
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

        protected AbstractEventInstanceManager CreateInstance(int level, int lobbyId) 
        {
            return Setup(level, lobbyId);
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
            {  // EM already disposed
                return;
            }

            readyInstances.Enqueue(eim);
            onLoadInstances--;

            instantiateQueuedInstance();    // keep filling the queue until reach threshold.
        }
        public virtual string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
        {
            switch (r)
            {
                case CreateInstanceResult.Success:
                    return null;
                case CreateInstanceResult.RequiredParty:
                    return "需要组队";
                case CreateInstanceResult.RequiredLeader:
                    return c.CurrentCulture.GetMessageByKey(nameof(ScriptTalk.PartyQuest_NeedLeaderTalk));
                case CreateInstanceResult.Requirement:
                    return c.CurrentCulture.GetMessageByKey(nameof(ScriptTalk.PartyQuest_CannotStart_Req));
                case CreateInstanceResult.LobbyLimited:
                    return c.CurrentCulture.GetMessageByKey(nameof(ScriptTalk.PartyQuest_CannotStart_ChannelFull));
                case CreateInstanceResult.Disposed:
                case CreateInstanceResult.Unknown:

                default:
                    return "未知错误";
            }
        }
        #endregion

        #region Lobby

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

        #endregion

        public abstract CreateInstanceResult StartInstance(Player leader, int difficulty = 1, int lobbyId = -1);

        #region Events
        public virtual AbstractEventInstanceManager Setup(int level, int lobbyId)
        {
            var eim = newInstance(_name + lobbyId);
            eim.setProperty("level", level);

            OnSetup(eim, level, lobbyId);

            respawnStages(eim);

            eim.startEventTimer(EventTime * 1000);
            setEventRewards(eim);

            return eim;
        }
        protected virtual void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {

        }

        public virtual void AfterSeup(AbstractEventInstanceManager eim)
        {

        }
        protected virtual void respawnStages(AbstractEventInstanceManager eim) { }
        protected virtual void setEventRewards(AbstractEventInstanceManager eim) { }
        public virtual List<Player> GetEligibleParty(Player leader)
        {
            var members = leader.getPartyMembersOnSameMap();

            if (members.Count >= MinCount
                && members.Count <= MaxCount
                && members.All(x => x.Level >= MinLevel && x.Level <= MaxLevel))
            {
                return members;
            }
            return [];
        }

        /// <summary>
        /// 结束FB
        /// </summary>
        /// <param name="eim"></param>
        protected virtual void End(AbstractEventInstanceManager eim)
        {
            var party = eim.getPlayers();
            foreach (Player player in party)
            {
                OnPlayerExit(eim, player);
            }
            eim.Dispose();
        }


        public virtual void OnTimeOut(AbstractEventInstanceManager eim)
        {
            End(eim);
        }

        public virtual void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            chr.changeMap(EntryMap == MapId.NONE ? chr.MapModel.getReturnMapId() : EntryMap, EntryPortal);
        }

        public virtual void OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            eim.unregisterPlayer(player);
            player.changeMap(ExitMap == MapId.NONE ? player.MapModel.getForcedReturnId() : ExitMap, ExitPortal);
        }

        public virtual void OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
        }


        /// <summary>
        /// 切换地图（前）
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="player"></param>
        /// <param name="mapid"></param>
        public virtual void OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (mapid < MinMap || mapid > MaxMap)
            {
                if (eim.isEventTeamLackingNow(true, MinCount, player))
                {
                    eim.unregisterPlayer(player);
                    End(eim);
                }
                else
                {
                    eim.unregisterPlayer(player);
                }
            }
        }

        /// <summary>
        /// 切换地图（后）
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="player"></param>
        /// <param name="mapid"></param>
        public virtual void OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {

        }

        public virtual void OnLeaderChanged(AbstractEventInstanceManager eim, Player leader)
        {
            var mapid = leader.getMapId();
            if (!eim.isEventCleared() && (mapid < MinMap || mapid > MaxMap))
            {
                End(eim);
            }
        }

        public virtual void OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {

        }

        public virtual void OnPlayerDisconnected(AbstractEventInstanceManager eim, Player player)
        {
            if (eim.isEventTeamLackingNow(true, MinCount, player))
            {
                eim.unregisterPlayer(player);
                End(eim);
            }
            else
            {
                eim.unregisterPlayer(player);
            }
        }

        public virtual void OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {
            if (eim.isEventTeamLackingNow(false, MinCount, player))
            {
                End(eim);
            }
            else
            {
                if (!eim.isEventCleared())
                {
                    OnPlayerExit(eim, player);
                }
            }
        }

        public virtual void OnPartyDisband(AbstractEventInstanceManager eim)
        {
            if (!eim.isEventCleared())
            {
                End(eim);
            }
        }

        public virtual bool OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            if (eim.isEventTeamLackingNow(true, MinCount, player))
            {
                eim.unregisterPlayer(player);
                End(eim);
            }
            else
            {
                eim.unregisterPlayer(player);
            }
            return true;
        }

        public virtual void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
        }

        public virtual void OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
        {
        }

        public virtual void OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
        }

        public virtual void OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker)
        {
        }
        public virtual void OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
        }
        public virtual void OnFriendlyMobDrop(AbstractEventInstanceManager eim, Monster mob)
        {
        }

        public virtual void ClearPQ(AbstractEventInstanceManager eim)
        {
            eim.stopEventTimer();
            eim.setEventCleared();
        }
        #endregion

        public override void Dispose()
        {
            base.Dispose();

            var eimList = instances.Values.ToList();
            instances.Clear();

            foreach (var eim in eimList)
            {
                eim.Dispose();
            }

            openedLobbys.Clear();

            Queue<AbstractEventInstanceManager> readyEims;

            readyEims = new(readyInstances);
            readyInstances.Clear();


            foreach (var eim in readyEims)
            {
                eim.Dispose();
            }
            onLoadInstances = 0;
        }

        class DelayedDisposeRequest : DelayedTickable
        {
            AbstractInstancedEventManager _src;
            string _instanceName;

            public DelayedDisposeRequest(AbstractInstancedEventManager src, string instanceName, long next): base(next)
            {
                _src = src;
                _instanceName = instanceName;
            }

            protected override void Handle(long now)
            {
                _src.ProcessDisposeInstanceInternal(_instanceName);
            }
        }
    }
}
