using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.model;
using Application.Core.scripting.Events;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.scripting.Events.Templates;
using Application.Resources.Messages;
using Application.Shared.Events;
using Application.Utility.Performance;
using Application.Utility.Tickables;
using server.quest;
using System.Collections.Concurrent;
using tools.exceptions;

namespace Application.Core.Scripting.Events
{
    public abstract class AbstractEventManager : ITickableTree, IDisposable
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

        public virtual void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            Status = TickableStatus.Remove;

            var eimList = instances.Values.ToList();
            instances.Clear();

            foreach (var eim in eimList)
            {
                eim.Dispose();
            }

            openedLobbys.Clear();

            Queue<AbstractEventInstanceManager> readyEims = new(readyInstances);
            readyInstances.Clear();

            foreach (var eim in readyEims)
            {
                eim.Dispose();
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

        public IMap GetMap(int mapId)
        {
            var map = getChannelServer().getMapFactory().getMap(mapId);
            return map;
        }

        public string getName()
        {
            return Name;
        }

        public void startQuest(Player chr, int id, int npcid)
        {
            try
            {
                Quest.getInstance(id).forceStart(chr, npcid);
            }
            catch (NullReferenceException ex)
            {
                log.Error(ex.ToString());
            }
        }

        public void completeQuest(Player chr, int id, int npcid)
        {
            try
            {
                Quest.getInstance(id).forceComplete(chr, npcid);
            }
            catch (NullReferenceException ex)
            {
                log.Error(ex.ToString());
            }
        }

        public virtual void OnTick(long now)
        {
            this.ProcessSubTickables(now);
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
            {
                return;
            }

            readyInstances.Enqueue(eim);
            onLoadInstances--;

            instantiateQueuedInstance();
        }

        public virtual string? HandleCreateInstanceResult(CreateInstanceResult r, IChannelClient c)
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

        public abstract CreateInstanceResult StartInstance(Player leader, int difficulty = 1, int lobbyId = -1);

        public virtual AbstractEventInstanceManager Setup(int level, int lobbyId)
        {
            var eim = newInstance(Name + lobbyId);
            eim.setProperty("level", level);

            OnSetup(eim, level, lobbyId);
            respawnStages(eim);
            eim.startEventTimer(EventTime * 1000);
            setEventRewards(eim);

            return eim;
        }

        protected virtual void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId) { }

        public virtual void AfterSeup(AbstractEventInstanceManager eim) { }

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

        public string GetRequirementDescription(IChannelClient client)
        {
            var countRange = MinCount == MaxCount ? MinCount.ToString() : MinCount + " ~ " + MaxCount;
            var levelRange = MinLevel == MaxLevel ? MinLevel.ToString() : MinLevel + " ~ " + MaxLevel;
            return client.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_Requirement),
                countRange,
                levelRange,
                (EventTime / 60).ToString());
        }

        protected virtual void End(AbstractEventInstanceManager eim)
        {
            eim.Dispose();
        }

        public virtual void OnTimeOut(AbstractEventInstanceManager eim)
        {
            Template.OnTimeOut(eim);
        }

        public virtual void OnPlayerRegister(AbstractEventInstanceManager eim, Player chr)
        {
            Template.OnPlayerRegister(eim, chr);
        }

        public virtual void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            Template.OnPlayerEntry(eim, chr);
        }

        public virtual void OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            Template.OnPlayerExit(eim, player);
        }

        public virtual void OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            Template.OnPlayerUnregister(eim, chr);
        }

        public virtual void OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            Template.OnPlayerMapChanging(eim, player, mapid);
        }

        public virtual void OnPlayerMapChanged(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            Template.OnPlayerMapChanged(eim, player, mapid);
        }

        public virtual void OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            Template.OnPlayerDied(eim, chr);
        }

        public virtual void OnPlayerDisconnected(AbstractEventInstanceManager eim, Player player)
        {
            Template.OnPlayerDisconnected(eim, player);
        }

        public virtual void OnLeaderChanged(AbstractEventInstanceManager eim, Player leader)
        {
            Template.OnLeaderChanged(eim, leader);
        }

        public virtual void OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {
            Template.OnPlayerLeftParty(eim, player);
        }

        public virtual void OnPartyDisband(AbstractEventInstanceManager eim)
        {
            Template.OnPartyDisband(eim);
        }

        public virtual bool OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            return Template.OnPlayerRevive(eim, player);
        }

        public virtual bool IsEventTeamLackingNow(AbstractEventInstanceManager eim, bool leavingEventMap, Player quitter)
        {
            return Template.IsEventTeamLackingNow(eim, leavingEventMap, quitter);
        }

        public virtual void OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            Template.OnMobKilled(eim, mob, killer);
        }

        public virtual void OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
        {
            Template.OnMobRevive(eim, mob);
        }

        public virtual void OnMobClear(AbstractEventInstanceManager eim, IMap map)
        {
            Template.OnMobClear(eim, map);
        }

        public virtual void OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
        {
            Template.OnFriendlyMobDamaged(eim, mob, attacker, damage);
        }

        public virtual void OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            Template.OnFriendlyMobKilled(eim, mob, killer);
        }

        public virtual void OnFriendlyMobDrop(AbstractEventInstanceManager eim, Monster mob)
        {
            Template.OnFriendlyMobDrop(eim, mob);
        }

        public virtual void ClearPQ(AbstractEventInstanceManager eim)
        {
            Template.ClearPQ(eim);
        }

        public virtual RewardOptions GetAllClearRewardOptions(Player chr, int point)
        {
            return new RewardOptions();
        }

        public virtual ClaimRewardResult GiveClearReward(AbstractEventInstanceManager eim, Player player, int point)
        {
            if (!eim.CanGiveReward(player, -1))
            {
                return ClaimRewardResult.Claimed;
            }

            var pool = AllClearRewards.GetValueOrDefault(eim.Level);
            if (pool == null)
            {
                pool = AllClearRewards.Values.FirstOrDefault();
                if (pool == null)
                {
                    return ClaimRewardResult.Success;
                }
            }

            if (pool.ItemPool.Length > 0)
            {
                if (!eim.hasRewardSlot(player, eim.Level))
                {
                    return ClaimRewardResult.BagFull;
                }

                var item = Randomizer.Select(pool.ItemPool);
                player.GainItem(item.ItemId, (short)item.Quantity, show: GainItemShow.ShowInChat);
            }

            var option = GetAllClearRewardOptions(player, point);
            if (pool.ExpPool.Length > 0)
            {
                var baseExp = option.ExpPoolIndex == -1 ? Randomizer.Select(pool.ExpPool) : pool.ExpPool[option.ExpPoolIndex];
                player.gainExp((int)(baseExp * option.FinalExpRate));
            }

            if (pool.MesoPool.Length > 0)
            {
                var baseMeso = option.MesoPoolIndex == -1 ? Randomizer.Select(pool.MesoPool) : pool.MesoPool[option.MesoPoolIndex];
                player.GainMeso((int)(baseMeso * option.FinalMesoRate), GainItemShow.ShowInChat);
            }

            eim.SetRewardClaimed(player, -1);
            return ClaimRewardResult.Success;
        }

        public virtual RewardOptions GetStageClearRewardOptions(Player chr, int stageMap)
        {
            return new RewardOptions();
        }

        public virtual ClaimRewardResult GiveStageClearReward(AbstractEventInstanceManager eim, Player player, int stageMap)
        {
            if (StageClearRewards.TryGetValue(stageMap, out var data))
            {
                var expExtraBonus = eim.Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PQ_BONUS_EXP_RATE : 1;
                var option = GetStageClearRewardOptions(player, stageMap);
                if (eim.CanGiveReward(player, stageMap))
                {
                    eim.SetRewardClaimed(player, stageMap);
                    player.gainExp((int)(data.Exp * option.FinalExpRate), true, true);
                    player.GainMeso((int)(data.Meso * option.FinalMesoRate), GainItemShow.ShowInChat);
                    return ClaimRewardResult.Success;
                }
                return ClaimRewardResult.Claimed;
            }
            return ClaimRewardResult.Success;
        }

        public virtual void GiveStageClearRewardAll(AbstractEventInstanceManager eim, int stageMap)
        {
            if (StageClearRewards.TryGetValue(stageMap, out var data))
            {
                foreach (var player in eim.getPlayers())
                {
                    var option = GetStageClearRewardOptions(player, stageMap);
                    if (eim.CanGiveReward(player, stageMap))
                    {
                        eim.SetRewardClaimed(player, stageMap);
                        player.gainExp((int)(data.Exp * option.FinalExpRate), true, true);
                        player.GainMeso((int)(data.Meso * option.FinalMesoRate), GainItemShow.ShowInChat);
                    }
                }
            }
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

            protected override void Handle(long now)
            {
                _src.ProcessDisposeInstanceInternal(_instanceName);
            }
        }
    }
}