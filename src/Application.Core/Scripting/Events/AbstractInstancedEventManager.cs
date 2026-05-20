using Application.Core.Channel;
using Application.Core.Channel.Commands;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.Gameplay.ChannelEvents;
using Application.Core.model;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Resources.Messages;
using Application.Shared.Events;
using Application.Shared.Login;
using Application.Utility.Performance;
using Application.Utility.Tickables;
using scripting.Event;
using scripting.npc;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
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
        public int MaxLobbys { get; set; } = 1;

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
        public int MaxLevel { get; protected set; } = 255;

        public int EntryMap { get; init; } = MapId.NONE;
        public int EntryPortal { get; init; }
        public int ExitMap { get; init; } = MapId.NONE;
        public int ExitPortal { get; init; }

        public int ClearMap { get; init; }

        public int MinMap { get; init; }
        public int MaxMap { get; init; }
        public int[] IncludedMap { get; init; } = [];

        /// <summary>
        /// 单位：秒
        /// </summary>
        public int EventTime { get; protected set; }
        public bool AllowReconnect { get; init; } = true;
        #region Rewards
        /// <summary>
        /// 全部通关奖励。Key: Level
        /// </summary>
        public Dictionary<int, RewardPools> AllClearRewards { get; init; }
        /// <summary>
        /// 关卡通关奖励。Key: 关卡
        /// </summary>
        public Dictionary<int, (int Exp, int Meso)> StageClearRewards { get; init; }
        #endregion

        public AbstractInstancedEventManager(WorldChannel cserv, string name) : base(cserv, name)
        {
            AllClearRewards = new();
            StageClearRewards = new();
        }

        public override void Initialize()
        {
            MinLevel = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 1 : MinLevel;
            MaxLevel = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 255 : MaxLevel;
            MinCount = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 1 : MinCount;
        }

        #region Instances
        protected virtual AbstractEventInstanceManager CreateNewInstance(string instanceName)
        {
            return new EventInstanceManager(ChannelServer, Name, instanceName);
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
                    return c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_NeedLeaderTalk));
                case CreateInstanceResult.Requirement:
                    return c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_CannotStart_Req));
                case CreateInstanceResult.LobbyLimited:
                    return c.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_CannotStart_ChannelFull));
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

        public string GetRequirementDescription(IChannelClient client)
        {
            var countRange = MinCount == MaxCount ? MinCount.ToString() : MinCount + " ~ " + MaxCount;
            var levelRange = MinLevel == MaxLevel ? MinLevel.ToString() : MinLevel + " ~ " + MaxLevel;
            return client.CurrentCulture.GetScriptTalkByKey(nameof(ScriptTalk.PartyQuest_Requirement),
                countRange,
                levelRange,
                (EventTime / 60).ToString());
        }

        /// <summary>
        /// 结束FB
        /// </summary>
        /// <param name="eim"></param>
        protected virtual void End(AbstractEventInstanceManager eim)
        {
            eim.Dispose();
        }


        public virtual void OnTimeOut(AbstractEventInstanceManager eim)
        {
            End(eim);
        }

        public virtual void OnPlayerRegister(AbstractEventInstanceManager eim, Player chr)
        {
            OnPlayerEntry(eim, chr);
        }

        public virtual void OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            chr.SaveLocation(SavedLocationType.EVENT);
            chr.changeMap(EntryMap == MapId.NONE ? chr.MapModel.getForcedReturnId() : EntryMap, EntryPortal);
        }

        public virtual void OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            eim.unregisterPlayer(player);

            if (player.isLoggedin())
            {
                if (ExitMap == MapId.NONE)
                {
                    if (!player.TryWarpBackSavedLocation(SavedLocationType.EVENT))
                    {
                        player.ForcedWarpOut();
                    }
                }
                else
                {
                    player.changeMap(ExitMap, ExitPortal);
                }
                player.clearSavedLocation(SavedLocationType.EVENT);
            }
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
            if (!InInstanceMap(mapid))
            {
                if (IsEventTeamLackingNow(eim, true, player))
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



        public virtual void OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {

        }

        public virtual void OnPlayerDisconnected(AbstractEventInstanceManager eim, Player player)
        {
            if (IsEventTeamLackingNow(eim, true, player))
            {
                eim.unregisterPlayer(player);
                End(eim);
            }
            else
            {
                eim.unregisterPlayer(player);
            }
        }

        public virtual void OnLeaderChanged(AbstractEventInstanceManager eim, Player leader)
        {
            var mapid = leader.getMapId();
            if (!eim.isEventCleared() && (!InInstanceMap(mapid)))
            {
                End(eim);
            }
        }

        public virtual void OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {
            if (IsEventTeamLackingNow(eim, false, player))
            {
                End(eim);
            }
            else
            {
                if (!eim.isEventCleared())
                {
                    eim.exitPlayer(player);
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
            if (IsEventTeamLackingNow(eim, true, player))
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

        /// <summary>
        /// 有成员退出时（离开队伍，离线，死亡等），判断队伍是否仍然可以继续任务
        /// </summary>
        /// <param name="leavingEventMap">正在离开任务地图（包含离线、死亡）ELSE: 离开队伍</param>
        /// <param name="quitter">离开者</param>
        /// <returns>true：缺员，不能继续任务</returns>
        public virtual bool IsEventTeamLackingNow(AbstractEventInstanceManager eim, bool leavingEventMap, Player quitter)
        {
            if (eim.InstanceStatus == InstanceStatus.Cleared)
            {
                return leavingEventMap && eim.getPlayerCount() <= 1;
            }
            else
            {
                if (leavingEventMap && eim.getLeaderId() == quitter.getId())
                {
                    return true;
                }
                return eim.getPlayerCount() <= MinCount;
            }
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

        public virtual void OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
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

        protected bool InInstanceMap(int mapId)
        {
            return ((mapId >= MinMap && mapId <= MaxMap) || IncludedMap.Contains(mapId));
        }
        #endregion

        #region Rewards
        public virtual RewardOptions GetAllClearRewardOptions(Player chr, int point)
        {
            return new RewardOptions();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="player"></param>
        /// <param name="points">得分</param>
        /// <param name="eventLevel">难度</param>
        /// <returns>0. 成功，1. 已领取，2. 奖励不存在，3. 背包空间不足</returns>
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
                    // 没有奖励
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
