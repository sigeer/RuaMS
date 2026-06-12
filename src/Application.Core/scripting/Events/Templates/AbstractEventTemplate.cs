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
        #endregion

        public AbstractEventTemplate(string name)
        {
            Name = name;
            AllClearRewards = new();
            StageClearRewards = new();
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
        #region Events
        public virtual void OnSetup(AbstractEventInstanceManager eim, int level, int lobbyId)
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
    }
}
