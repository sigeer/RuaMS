using Application.Core.Channel;
using Application.Core.Game.Life;
using Application.Core.Game.Maps;
using Application.Core.model;
using Application.Core.scripting.Events.Abstraction;
using Application.Core.scripting.Events.Instances;
using Application.Core.Scripting.Events;
using Application.Resources.Messages;
using Application.Shared.Events;

namespace Application.Core.scripting.Events.Templates
{
    /// <summary>
    /// 副本配置
    /// </summary>
    public abstract class AbstractEventTemplate
    {
        public string Name { get; }

        public int MaxLobbys { get; set; } = 1;


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
        public int EventTime { get; set; }
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
        public EventInstanceType Type { get; init; }
        #endregion

        public AbstractEventTemplate(string name)
        {
            Name = name;
            AllClearRewards = new();
            StageClearRewards = new();
            Type = EventInstanceType.Regular;
        }

        public virtual void OnMounted(WorldChannel worldChannel)
        {
            MinLevel = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 1 : MinLevel;
            MaxLevel = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 255 : MaxLevel;
            MinCount = YamlConfig.config.server.USE_ENABLE_SOLO_EXPEDITIONS ? 1 : MinCount;
        }

        public abstract AbstractEventManager GenerateEventManager(WorldChannel worldChannel);

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
        #region Events
        public virtual Task OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterSeup(AbstractEventInstanceManager eim)
        {
            return Task.CompletedTask;
        }
        public virtual Task respawnStages(AbstractEventInstanceManager eim) { return Task.CompletedTask; }
        public virtual Task setEventRewards(AbstractEventInstanceManager eim) { return Task.CompletedTask; }

        public virtual string GetRequirementDescription(IChannelClient client)
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
        protected virtual async Task End(AbstractEventInstanceManager eim)
        {
            await eim.DisposeAsync();
        }


        public virtual async Task OnTimeOut(AbstractEventInstanceManager eim)
        {
            await End(eim);
        }

        public virtual async Task OnPlayerRegister(AbstractEventInstanceManager eim, Player chr)
        {
            await OnPlayerEntry(eim, chr);
        }

        public virtual async Task OnPlayerEntry(AbstractEventInstanceManager eim, Player chr)
        {
            chr.SaveLocation(SavedLocationType.EVENT);
            await chr.changeMap(EntryMap == MapId.NONE ? chr.MapModel.getForcedReturnId() : EntryMap, EntryPortal);
        }

        public virtual async Task OnPlayerExit(AbstractEventInstanceManager eim, Player player)
        {
            await eim.unregisterPlayer(player);

            if (player.isLoggedin())
            {
                if (ExitMap == MapId.NONE)
                {
                    if (!await player.TryWarpBackSavedLocation(SavedLocationType.EVENT))
                    {
                        await player.ForcedWarpOut();
                    }
                }
                else
                {
                    await player.changeMap(ExitMap, ExitPortal);
                }
                player.clearSavedLocation(SavedLocationType.EVENT);
            }
        }

        public virtual Task OnPlayerUnregister(AbstractEventInstanceManager eim, Player chr)
        {
            return Task.CompletedTask;
        }


        /// <summary>
        /// 切换地图（前）
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="player"></param>
        /// <param name="mapid"></param>
        public virtual async Task OnPlayerMapChanging(AbstractEventInstanceManager eim, Player player, int mapid)
        {
            if (!InInstanceMap(mapid))
            {
                if (IsEventTeamLackingNow(eim, true, player))
                {
                    await eim.unregisterPlayer(player);
                    await End(eim);
                }
                else
                {
                    await eim.unregisterPlayer(player);
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



        public virtual Task OnPlayerDied(AbstractEventInstanceManager eim, Player chr)
        {
            return Task.CompletedTask;
        }

        public virtual async Task OnPlayerDisconnected(AbstractEventInstanceManager eim, Player player)
        {
            if (IsEventTeamLackingNow(eim, true, player))
            {
                await eim.unregisterPlayer(player);
                await End(eim);
            }
            else
            {
                await eim.unregisterPlayer(player);
            }
        }

        public virtual async Task OnLeaderChanged(AbstractEventInstanceManager eim, Player leader)
        {
            var mapid = leader.getMapId();
            if (!eim.isEventCleared() && (!InInstanceMap(mapid)))
            {
                await End(eim);
            }
        }

        public virtual async Task OnPlayerLeftParty(AbstractEventInstanceManager eim, Player player)
        {
            if (IsEventTeamLackingNow(eim, false, player))
            {
                await End(eim);
            }
            else
            {
                if (!eim.isEventCleared())
                {
                    await eim.exitPlayer(player);
                }
            }
        }

        public virtual async Task OnPartyDisband(AbstractEventInstanceManager eim)
        {
            if (!eim.isEventCleared())
            {
                await End(eim);
            }
        }

        public virtual async Task<bool> OnPlayerRevive(AbstractEventInstanceManager eim, Player player)
        {
            if (IsEventTeamLackingNow(eim, true, player))
            {
                await eim.unregisterPlayer(player);
                await End(eim);
            }
            else
            {
                await eim.unregisterPlayer(player);
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

        public virtual Task OnMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnMobRevive(AbstractEventInstanceManager eim, Monster mob)
            => Task.CompletedTask;

        public virtual Task OnMobClear(AbstractEventInstanceManager eim, IMap map)
            => Task.CompletedTask;
        public virtual Task OnFriendlyMobDamaged(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? attacker, int damage)
            => Task.CompletedTask;
        public virtual Task OnFriendlyMobKilled(AbstractEventInstanceManager eim, Monster mob, ICombatantObject? killer)
            => Task.CompletedTask;
        public virtual Task OnFriendlyMobDrop(AbstractEventInstanceManager eim, Monster mob)
            => Task.CompletedTask;

        public virtual async Task ClearPQ(AbstractEventInstanceManager eim)
        {
            await eim.stopEventTimer();
            await eim.setEventCleared();
        }

        protected bool InInstanceMap(int mapId)
        {
            return ((mapId >= MinMap && mapId <= MaxMap) || IncludedMap.Contains(mapId));
        }
        #endregion

        #region Rewards
        public virtual double GetExpRate()
        {
            return Type == EventInstanceType.PartyQuest ? YamlConfig.config.server.PQ_BONUS_EXP_RATE : 1;
        }

        public virtual RewardOptions GetAllClearRewardOptions(Player chr, int point)
        {
            return new RewardOptions(FinalExpRate: (float)GetExpRate());
        }
        /// <summary>
        /// 全部通关的最终奖励
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="player"></param>
        /// <param name="points">得分</param>
        /// <param name="eventLevel">难度</param>
        /// <returns>0. 成功，1. 已领取，2. 奖励不存在，3. 背包空间不足</returns>
        public virtual async Task<ClaimRewardResult> GiveClearReward(AbstractEventInstanceManager eim, Player player, int point)
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

                await player.GainItem(item.ItemId, (short)item.Quantity, show: GainItemShow.ShowInChat);
            }

            var option = GetAllClearRewardOptions(player, point);
            if (pool.ExpPool.Length > 0)
            {
                var baseExp = option.ExpPoolIndex == -1 ? Randomizer.Select(pool.ExpPool) : pool.ExpPool[option.ExpPoolIndex];
                await player.gainExp((int)(baseExp * option.FinalExpRate));
            }

            if (pool.MesoPool.Length > 0)
            {
                var baseMeso = option.MesoPoolIndex == -1 ? Randomizer.Select(pool.MesoPool) : pool.MesoPool[option.MesoPoolIndex];
                await player.GainMeso((int)(baseMeso * option.FinalMesoRate), GainItemShow.ShowInChat);
            }

            eim.SetRewardClaimed(player, -1);
            return ClaimRewardResult.Success;
        }
        public virtual RewardOptions GetStageClearRewardOptions(Player chr, int stageMap)
        {
            return new RewardOptions(FinalExpRate: (float)GetExpRate());
        }
        /// <summary>
        /// 对单个玩家发放关卡通关奖励
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="player"></param>
        /// <param name="stageMap"></param>
        /// <returns></returns>
        public virtual async Task<ClaimRewardResult> GiveStageClearReward(AbstractEventInstanceManager eim, Player player, int stageMap)
        {
            if (StageClearRewards.TryGetValue(stageMap, out var data))
            {
                var option = GetStageClearRewardOptions(player, stageMap);
                if (eim.CanGiveReward(player, stageMap))
                {
                    eim.SetRewardClaimed(player, stageMap);
                    await player.gainExp((int)(data.Exp * option.FinalExpRate), true, true);
                    await player.GainMeso((int)(data.Meso * option.FinalMesoRate), GainItemShow.ShowInChat);

                    return ClaimRewardResult.Success;
                }
                return ClaimRewardResult.Claimed;
            }
            return ClaimRewardResult.Success;
        }
        /// <summary>
        /// 对所有玩家发放关卡通关奖励
        /// </summary>
        /// <param name="eim"></param>
        /// <param name="stageMap"></param>
        /// <returns></returns>
        public virtual async Task GiveStageClearRewardAll(AbstractEventInstanceManager eim, int stageMap)
        {
            if (StageClearRewards.TryGetValue(stageMap, out var data))
            {
                foreach (var player in eim.getPlayers())
                {
                    var option = GetStageClearRewardOptions(player, stageMap);
                    if (eim.CanGiveReward(player, stageMap))
                    {
                        eim.SetRewardClaimed(player, stageMap);
                        await player.gainExp((int)(data.Exp * option.FinalExpRate), true, true);
                        await player.GainMeso((int)(data.Meso * option.FinalMesoRate), GainItemShow.ShowInChat);
                    }
                }
            }
        }
        #endregion
    }
}
