using Application.Core.Channel.Commands;
using Application.Core.Channel.DataProviders;
using Application.Core.Game.Players.Tickables;
using Application.Core.Game.Skills;
using Application.Core.model;
using Application.Utility.Tickables;
using net.server;
using server;
using server.maps;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>
        /// 仅供debug使用
        /// </summary>
        private Dictionary<BuffStat, sbyte> buffEffectsCount = new();
        /// <summary>
        /// buffEffects 同一类型buff取效果最好的那种，当该buff过期时，使用次一级效果且未过期的
        /// </summary>

        public Dictionary<BuffStat, BuffStatValueHolder> ActiveEffects { get; } = new();
        /// <summary>
        /// sourceid - effects
        /// <para>
        /// 玩家所有buff
        /// 同类型的buff可能有多个来源：加成同一种属性的不同药品
        /// </para>
        /// </summary>
        private Dictionary<int, Dictionary<BuffStat, BuffStatValueHolder>> buffEffects = new();


        public bool hasBuffFromSourceid(int sourceid)
        {
            return buffEffects.ContainsKey(sourceid);
        }

        private BuffStatValueHolder? GetBuffStatValue(BuffStat effect)
        {
            return ActiveEffects.GetValueOrDefault(effect);
        }
        public long? getBuffedStarttime(BuffStat effect)
        {
            return GetBuffStatValue(effect)?.startTime;
        }

        public void setBuffedValue(BuffStat effect, int value)
        {
            if (ActiveEffects.TryGetValue(effect, out var mbsvh))
            {
                mbsvh.value = value;
            }
        }

        public int? getBuffedValue(BuffStat effect)
        {
            return GetBuffStatValue(effect)?.value;
        }

        public int getBuffSource(BuffStat stat)
        {
            return GetBuffStatValue(stat)?.effect?.getSourceId() ?? -1;
        }

        public StatEffect? getBuffEffect(BuffStat stat)
        {
            return GetBuffStatValue(stat)?.effect;
        }

        public bool HasBuff(BuffStat stat)
        {
            return ActiveEffects.ContainsKey(stat);
        }

        public List<BuffStatValueHolder> getAllStatups()
        {
            return buffEffects.Values.SelectMany(x => x.Values).ToList();
        }

        public List<PlayerBuffValueHolder> getAllBuffs()
        {
            Dictionary<int, PlayerBuffValueHolder> ret = new();
            foreach (var bel in buffEffects)
            {
                var effectBuffStats = new List<BuffStatValue>();
                foreach (var mbsvh in bel.Value)
                {
                    if (!ret.ContainsKey(bel.Key))
                    {
                        ret.Add(bel.Key, new PlayerBuffValueHolder(mbsvh.Value.startTime, mbsvh.Value.effect, effectBuffStats));
                    }
                    effectBuffStats.Add(new BuffStatValue(mbsvh.Key, mbsvh.Value.value));
                }
            }
            return new(ret.Values);
        }

        private void extractBuffValue(int sourceid, BuffStat stat)
        {
            removeEffectFromItemEffectHolder(sourceid, stat);
        }

        public void debugListAllBuffs()
        {
            Log.Debug("-------------------");
            Log.Debug("CACHED BUFF COUNT: {CachedBuffCount}", string.Join(", ", buffEffectsCount
                    .Select(entry => entry.Key + ": " + entry.Value)));

            Log.Debug("-------------------");
            Log.Debug("CACHED BUFFS: {CachedBuff}", string.Join(", ", buffEffects
                    .Select(entry => entry.Key + ": (" + string.Join(", ", entry.Value
                            .Select(innerEntry => innerEntry.Key.name() + innerEntry.Value.value)) + ")"))
            );

            Log.Debug("-------------------");
            Log.Debug("IN ACTION: {InAction}", string.Join(", ", ActiveEffects
                    .Select(entry => entry.Key.name() + " -> " + 
                    (entry.Value.effect.isSkill() 
                    ? Client.CurrentCulture.GetSkillName(entry.Value.effect.getSourceId()) 
                    : Client.CurrentCulture.GetItemName(entry.Value.effect.getSourceId()))))
            );
        }

        public void ClearExpiredBuffs()
        {
            List<BuffStatValueHolder> toCancel = new();

            foreach (var bel in getAllStatups())
            {
                if (bel.Disabled)
                {
                    toCancel.Add(bel);    //rofl
                }
            }

            foreach (var item in toCancel)
            {
                cancelEffect(item.effect, false);
            }
        }


        public void cancelAllBuffs(bool softcancel)
        {
            if (softcancel)
            {
                cancelEffectFromBuffStat(BuffStat.SUMMON);
                cancelEffectFromBuffStat(BuffStat.PUPPET);
                cancelEffectFromBuffStat(BuffStat.COMBO);

                ActiveEffects.Clear();

                foreach (int srcid in buffEffects.Keys)
                {
                    removeItemEffectHolder(srcid);
                }
            }
            else
            {
                Dictionary<StatEffect, long> mseBuffs = new();

                foreach (var bpl in buffEffects)
                {
                    foreach (var mbse in bpl.Value)
                    {
                        mseBuffs.AddOrUpdate(mbse.Value.effect, mbse.Value.startTime);
                    }
                }

                foreach (var mse in mseBuffs)
                {
                    cancelEffect(mse.Key, false);
                }
            }
        }

        private void dropBuffStats(List<BuffStateValuePair> effectsToCancel)
        {
            foreach (var cancelEffectCancelTasks in effectsToCancel)
            {
                //bool nestedCancel = false;


                /*
                if (buffExpires.get(cancelEffectCancelTasks.getRight().effect.getBuffSourceId()) != null) {
                    nestedCancel = true;
                }*/

                if (cancelEffectCancelTasks.ValueHolder.bestApplied)
                {
                    fetchBestEffectFromItemEffectHolder(cancelEffectCancelTasks.BuffStat);
                }

                /*
                if (nestedCancel) {
                    this.cancelEffect(cancelEffectCancelTasks.getRight().effect, false, -1, false);
                }*/
            }
        }

        private List<BuffStateValuePair> deregisterBuffStats(Dictionary<BuffStat, BuffStatValueHolder> stats)
        {

            List<BuffStateValuePair> effectsToCancel = new(stats.Count);
            foreach (var stat in stats)
            {
                int sourceid = stat.Value.effect.getBuffSourceId();

                BuffStat mbs = stat.Key;
                effectsToCancel.Add(new(mbs, stat.Value));


                if (ActiveEffects.Remove(mbs, out var mbsvh) && mbsvh != null && mbsvh.effect.getBuffSourceId() == sourceid)
                {
                    mbsvh.bestApplied = true;
                    mbsvh.Disabled = true;

                    if (mbs == BuffStat.SUMMON || mbs == BuffStat.PUPPET)
                    {
                        int summonId = mbsvh.effect.getSourceId();

                        if (summons.Remove(summonId, out var summon) && summon != null)
                        {
                            MapModel.broadcastMessage(PacketCreator.removeSummon(summon, true), summon.getPosition());
                            MapModel.removeMapObject(summon);
                            removeVisibleMapObject(summon);

                            if (summon.isPuppet())
                            {
                                MapModel.removePlayerPuppet(this);
                            }
                        }
                    }
                    else if (mbs == BuffStat.HPREC || mbs == BuffStat.MPREC)
                    {
                        if (mbs == BuffStat.HPREC)
                        {
                            extraHpRec = 0;
                        }
                        else
                        {
                            extraMpRec = 0;
                        }

                        if (extraRecoveryTask != null)
                        {
                            extraRecoveryTask.cancel(false);
                            extraRecoveryTask = null;
                        }

                        if (extraHpRec != 0 || extraMpRec != 0)
                        {
                            startExtraTaskInternal(extraHpRec, extraMpRec, extraRecInterval);
                        }
                    }
                }
            }

            return effectsToCancel;
        }

        public void cancelEffect(int itemId)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            cancelEffect(ii.GetItemEffectTrust(itemId), false);
        }

        public bool cancelEffect(StatEffect effect, bool overwrite)
        {
            var ret = cancelEffect(effect, overwrite, true);

            if (effect.isMagicDoor() && ret)
            {
                if (!hasBuffFromSourceid(Priest.MYSTIC_DOOR))
                {
                    Door.attemptRemoveDoor(this);
                }
            }

            return ret;
        }

        private static StatEffect? getEffectFromBuffSource(Dictionary<BuffStat, BuffStatValueHolder> buffSource)
        {
            try
            {
                return buffSource.FirstOrDefault().Value?.effect;
            }
            catch
            {
                return null;
            }
        }

        private bool isUpdatingEffect(HashSet<StatEffect> activeEffects, StatEffect? mse)
        {
            if (mse == null)
            {
                return false;
            }

            // thanks xinyifly for noticing "Speed Infusion" crashing game when updating buffs during map transition
            bool active = mse.isActive(this);
            if (active)
            {
                return !activeEffects.Contains(mse);
            }
            else
            {
                return activeEffects.Contains(mse);
            }
        }

        public void updateActiveEffects()
        {
            HashSet<BuffStat> updatedBuffs = new();
            HashSet<StatEffect> activeEffects = new();

            foreach (BuffStatValueHolder mse in ActiveEffects.Values)
            {
                activeEffects.Add(mse.effect);
            }

            foreach (var buff in buffEffects.Values)
            {
                StatEffect? mse = getEffectFromBuffSource(buff);
                if (isUpdatingEffect(activeEffects, mse))
                {
                    foreach (var p in mse!.getStatups())
                    {
                        updatedBuffs.Add(p.BuffState);
                    }
                }
            }

            foreach (BuffStat mbs in updatedBuffs)
            {
                ActiveEffects.Remove(mbs);
            }

            updateEffects(updatedBuffs);
        }

        private void updateEffects(HashSet<BuffStat> removedStats)
        {
            HashSet<BuffStat> retrievedStats = new();

            foreach (BuffStat mbs in removedStats)
            {
                fetchBestEffectFromItemEffectHolder(mbs);

                BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(mbs);
                if (mbsvh != null)
                {
                    foreach (var statup in mbsvh.effect.getStatups())
                    {
                        retrievedStats.Add(statup.BuffState);
                    }
                }
            }

            propagateBuffEffectUpdates(new(), retrievedStats, removedStats);
        }

        private bool cancelEffect(StatEffect effect, bool overwrite, bool firstCancel)
        {
            HashSet<BuffStat> removedStats = new();
            dropBuffStats(cancelEffectInternal(effect, overwrite, removedStats));
            UpdateLocalStats();
            updateEffects(removedStats);

            return removedStats.Count > 0;
        }

        private List<BuffStateValuePair> cancelEffectInternal(StatEffect effect, bool overwrite, HashSet<BuffStat> removedStats)
        {
            Dictionary<BuffStat, BuffStatValueHolder>? buffstats = null;
            BuffStat? ombs;
            if (!overwrite)
            {   // is removing the source effect, meaning every effect from this srcid is being purged
                buffstats = extractCurrentBuffStats(effect);
            }
            else if ((ombs = getSingletonStatupFromEffect(effect)) != null)
            {   // removing all effects of a buff having non-shareable buff stat.
                BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(ombs);
                if (mbsvh != null)
                {
                    buffstats = extractCurrentBuffStats(mbsvh.effect);
                }
            }

            if (buffstats == null)
            {            // all else, is dropping ALL current statups that uses same stats as the given effect
                buffstats = extractLeastRelevantStatEffectsIfFull(effect);
            }

            if (effect.isMapChair())
            {
                stopChairTask();
            }

            List<BuffStateValuePair> toCancel = deregisterBuffStats(buffstats);
            if (effect.isMonsterRiding())
            {
                Client.CurrentServer.MountTirednessManager.unregisterMountHunger(this);
                this.getMount()?.setActive(false);
            }

            if (!overwrite)
            {
                removedStats.UnionWith(buffstats.Keys);
            }

            return toCancel;
        }

        public void cancelEffectFromBuffStat(BuffStat stat)
        {
            BuffStatValueHolder? effect = ActiveEffects.GetValueOrDefault(stat);

            if (effect != null)
            {
                cancelEffect(effect.effect, false);
            }
        }

        public void cancelBuffStats(BuffStat stat)
        {

            List<KeyValuePair<int, BuffStatValueHolder>> cancelList = new();


            foreach (var bel in this.buffEffects)
            {
                BuffStatValueHolder? beli = bel.Value.GetValueOrDefault(stat);
                if (beli != null)
                {
                    cancelList.Add(new(bel.Key, beli));
                }
            }


            Dictionary<BuffStat, BuffStatValueHolder> buffStatList = new();
            foreach (var p in cancelList)
            {
                buffStatList.AddOrUpdate(stat, p.Value);
                extractBuffValue(p.Key, stat);
                dropBuffStats(deregisterBuffStats(buffStatList));
            }

            cancelPlayerBuffs(Arrays.asList(stat));
        }

        private bool removeEffectFromItemEffectHolder(int sourceid, BuffStat buffStat)
        {
            Dictionary<BuffStat, BuffStatValueHolder>? lbe = buffEffects.GetValueOrDefault(sourceid);

            if (lbe != null && lbe.Remove(buffStat, out var d) && d != null)
            {
                buffEffectsCount.AddOrUpdate(buffStat, (sbyte)(buffEffectsCount.GetValueOrDefault(buffStat) - 1));

                if (lbe.Count == 0)
                {
                    buffEffects.Remove(sourceid);
                }

                return true;
            }

            return false;
        }

        private void removeItemEffectHolder(int sourceid)
        {
            if (buffEffects.Remove(sourceid, out var be) && be != null)
            {
                foreach (var bei in be)
                {
                    buffEffectsCount.AddOrUpdate(bei.Key, (sbyte)(buffEffectsCount.GetValueOrDefault(bei.Key) - 1));
                }
            }
        }

        private Dictionary<BuffStat, BuffStatValueHolder> extractCurrentBuffStats(StatEffect effect)
        {

            Dictionary<BuffStat, BuffStatValueHolder> stats = new();
            buffEffects.Remove(effect.getBuffSourceId(), out var buffList);
            if (buffList != null)
            {
                foreach (var stateffect in buffList)
                {
                    stats.AddOrUpdate(stateffect.Key, stateffect.Value);
                    buffEffectsCount.AddOrUpdate(stateffect.Key, (sbyte)(buffEffectsCount.GetValueOrDefault(stateffect.Key) - 1));
                }
            }

            return stats;
        }

        private Dictionary<BuffStat, BuffStatValueHolder> extractLeastRelevantStatEffectsIfFull(StatEffect effect)
        {
            Dictionary<BuffStat, BuffStatValueHolder> extractedStatBuffs = new();

            Dictionary<BuffStat, Byte> stats = new();
            Dictionary<BuffStat, BuffStatValueHolder> minStatBuffs = new();

            foreach (var mbsvhi in buffEffects)
            {
                foreach (var mbsvhe in mbsvhi.Value)
                {
                    BuffStat mbs = mbsvhe.Key;

                    if (stats.TryGetValue(mbs, out var b))
                    {
                        stats.AddOrUpdate(mbs, (byte)(b + 1));
                        if (mbsvhe.Value.value < (minStatBuffs.GetValueOrDefault(mbs)?.value ?? 0))
                        {
                            minStatBuffs.AddOrUpdate(mbs, mbsvhe.Value);
                        }
                    }
                    else
                    {
                        stats.AddOrUpdate(mbs, (byte)1);
                        minStatBuffs.AddOrUpdate(mbs, mbsvhe.Value);
                    }
                }
            }

            HashSet<BuffStat> effectStatups = new();
            foreach (var efstat in effect.getStatups())
            {
                effectStatups.Add(efstat.BuffState);
            }

            foreach (var it in stats)
            {
                bool uniqueBuff = isSingletonStatup(it.Key);

                if (it.Value >= (!uniqueBuff ? YamlConfig.config.server.MAX_MONITORED_BUFFSTATS : 1) && effectStatups.Contains(it.Key))
                {
                    var mbsvh = minStatBuffs.GetValueOrDefault(it.Key)!;

                    var lpbe = buffEffects.GetValueOrDefault(mbsvh.effect.getBuffSourceId());
                    lpbe?.Remove(it.Key);
                    buffEffectsCount.AddOrUpdate(it.Key, (sbyte)(buffEffectsCount.GetValueOrDefault(it.Key) - 1));

                    if (lpbe == null || lpbe.Count == 0)
                    {
                        buffEffects.Remove(mbsvh.effect.getBuffSourceId());
                    }
                    extractedStatBuffs.AddOrUpdate(it.Key, mbsvh);
                }
            }

            return extractedStatBuffs;
        }

        private void cancelInactiveBuffStats(HashSet<BuffStat> retrievedStats, HashSet<BuffStat> removedStats)
        {
            List<BuffStat> inactiveStats = new();
            foreach (BuffStat mbs in removedStats)
            {
                if (!retrievedStats.Contains(mbs))
                {
                    inactiveStats.Add(mbs);
                }
            }

            if (inactiveStats.Count > 0)
            {
                sendPacket(PacketCreator.cancelBuff(inactiveStats));
                MapModel.broadcastMessage(this, PacketCreator.cancelForeignBuff(getId(), inactiveStats), false);
            }
        }

        private static Dictionary<StatEffect, int> topologicalSortLeafStatCount(Dictionary<BuffStat, Stack<StatEffect>> buffStack)
        {
            Dictionary<StatEffect, int> leafBuffCount = new();

            foreach (var e in buffStack)
            {
                Stack<StatEffect> mseStack = e.Value;
                if (mseStack.Count == 0)
                {
                    continue;
                }

                StatEffect mse = mseStack.Peek();

                leafBuffCount.AddOrUpdate(mse, leafBuffCount.GetValueOrDefault(mse) + 1);
            }

            return leafBuffCount;
        }

        private static List<StatEffect> topologicalSortRemoveLeafStats(Dictionary<StatEffect, HashSet<BuffStat>> stackedBuffStats, Dictionary<BuffStat, Stack<StatEffect>> buffStack, Dictionary<StatEffect, int> leafStatCount)
        {
            List<StatEffect> clearedStatEffects = new();
            HashSet<BuffStat> clearedStats = new();

            foreach (var e in leafStatCount)
            {
                StatEffect mse = e.Key;

                if (stackedBuffStats.GetValueOrDefault(mse)?.Count <= e.Value)
                {
                    clearedStatEffects.Add(mse);

                    foreach (BuffStat mbs in stackedBuffStats.GetValueOrDefault(mse)!)
                    {
                        clearedStats.Add(mbs);
                    }
                }
            }

            foreach (BuffStat mbs in clearedStats)
            {
                if (buffStack.GetValueOrDefault(mbs)!.TryPop(out var mse))
                    stackedBuffStats.GetValueOrDefault(mse)?.Remove(mbs);
            }

            return clearedStatEffects;
        }

        private static void topologicalSortRebaseLeafStats(Dictionary<StatEffect, HashSet<BuffStat>> stackedBuffStats, Dictionary<BuffStat, Stack<StatEffect>> buffStack)
        {
            foreach (var e in buffStack)
            {
                Stack<StatEffect> mseStack = e.Value;

                if (mseStack.Count > 0)
                {
                    if (mseStack.TryPop(out var mse))
                        stackedBuffStats.GetValueOrDefault(mse)?.Remove(e.Key);
                }
            }
        }

        private static List<StatEffect> topologicalSortEffects(Dictionary<BuffStat, List<KeyValuePair<StatEffect, int>>> buffEffects)
        {
            Dictionary<StatEffect, HashSet<BuffStat>> stackedBuffStats = new();
            Dictionary<BuffStat, Stack<StatEffect>> buffStack = new();

            foreach (var e in buffEffects)
            {
                BuffStat mbs = e.Key;

                Stack<StatEffect> mbsStack = new();
                buffStack.AddOrUpdate(mbs, mbsStack);

                foreach (var emse in e.Value)
                {
                    StatEffect mse = emse.Key;
                    mbsStack.Push(mse);

                    HashSet<BuffStat>? mbsStats = stackedBuffStats.GetValueOrDefault(mse);
                    if (mbsStats == null)
                    {
                        mbsStats = new();
                        stackedBuffStats.AddOrUpdate(mse, mbsStats);
                    }

                    mbsStats.Add(mbs);
                }
            }

            List<StatEffect> buffList = new();
            while (true)
            {
                Dictionary<StatEffect, int> leafStatCount = topologicalSortLeafStatCount(buffStack);
                if (leafStatCount.Count == 0)
                {
                    break;
                }

                List<StatEffect> clearedNodes = topologicalSortRemoveLeafStats(stackedBuffStats, buffStack, leafStatCount);
                if (clearedNodes.Count == 0)
                {
                    topologicalSortRebaseLeafStats(stackedBuffStats, buffStack);
                }
                else
                {
                    buffList.AddRange(clearedNodes);
                }
            }

            return buffList;
        }

        private static List<StatEffect> sortEffectsList(Dictionary<StatEffect, int> updateEffectsList)
        {
            Dictionary<BuffStat, List<KeyValuePair<StatEffect, int>>> buffEffects = new();

            foreach (var p in updateEffectsList)
            {
                StatEffect mse = p.Key;

                foreach (var statup in mse.getStatups())
                {
                    BuffStat stat = statup.BuffState;

                    var statBuffs = buffEffects.GetValueOrDefault(stat);
                    if (statBuffs == null)
                    {
                        statBuffs = new();
                        buffEffects.AddOrUpdate(stat, statBuffs);
                    }

                    statBuffs.Add(new(mse, statup.Value));
                }
            }

            foreach (var statBuffs in buffEffects)
            {
                statBuffs.Value.Sort((o1, o2) => o2.Value.CompareTo(o1.Value));
            }

            return topologicalSortEffects(buffEffects);
        }

        private List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> propagatePriorityBuffEffectUpdates(HashSet<BuffStat> retrievedStats)
        {
            List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> priorityUpdateEffects = new();
            Dictionary<BuffStatValueHolder, StatEffect> yokeStats = new();

            // priority buffsources: override buffstats for the client to perceive those as "currently buffed"
            HashSet<BuffStatValueHolder> mbsvhList = new();
            foreach (BuffStatValueHolder mbsvh in getAllStatups())
            {
                mbsvhList.Add(mbsvh);
            }

            foreach (BuffStatValueHolder mbsvh in mbsvhList)
            {
                StatEffect mse = mbsvh.effect;
                int buffSourceId = mse.getBuffSourceId();
                if (isPriorityBuffSourceid(buffSourceId) && !hasActiveBuff(buffSourceId))
                {
                    foreach (var ps in mse.getStatups())
                    {
                        BuffStat mbs = ps.BuffState;
                        if (retrievedStats.Contains(mbs))
                        {
                            BuffStatValueHolder mbsvhe = ActiveEffects.GetValueOrDefault(mbs)!;

                            // this shouldn't even be null...
                            //if (mbsvh != null) {
                            yokeStats.AddOrUpdate(mbsvh, mbsvhe.effect);
                            //}
                        }
                    }
                }
            }

            foreach (var e in yokeStats)
            {
                BuffStatValueHolder mbsvhPriority = e.Key;
                StatEffect mseActive = e.Value;

                priorityUpdateEffects.Add(new(mseActive.getBuffSourceId(), new(mbsvhPriority.effect, mbsvhPriority.startTime)));
            }

            return priorityUpdateEffects;
        }

        private void propagateBuffEffectUpdates(Dictionary<int, KeyValuePair<StatEffect, long>> retrievedEffects, HashSet<BuffStat> retrievedStats, HashSet<BuffStat> removedStats)
        {
            cancelInactiveBuffStats(retrievedStats, removedStats);
            if (retrievedStats.Count == 0)
            {
                return;
            }

            Dictionary<BuffStat, KeyValuePair<int, StatEffect?>?> maxBuffValue = new();
            foreach (BuffStat mbs in retrievedStats)
            {
                BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(mbs);
                if (mbsvh != null)
                {
                    retrievedEffects.AddOrUpdate(mbsvh.effect.getBuffSourceId(), new(mbsvh.effect, mbsvh.startTime));
                }

                maxBuffValue.AddOrUpdate(mbs, new(int.MinValue, null));
            }

            Dictionary<StatEffect, int> updateEffects = new();

            List<StatEffect> recalcMseList = new();
            foreach (var re in retrievedEffects)
            {
                recalcMseList.Add(re.Value.Key);
            }

            bool mageJob = this.getJobStyle() == Job.MAGICIAN;
            do
            {
                List<StatEffect> mseList = recalcMseList;
                recalcMseList = new();

                foreach (StatEffect mse in mseList)
                {
                    int maxEffectiveStatup = int.MinValue;
                    foreach (var st in mse.getStatups())
                    {
                        BuffStat mbs = st.BuffState;

                        bool relevantStatup = true;
                        if (mbs == BuffStat.WATK)
                        {  // not relevant for mages
                            if (mageJob)
                            {
                                relevantStatup = false;
                            }
                        }
                        else if (mbs == BuffStat.MATK)
                        { // not relevant for non-mages
                            if (!mageJob)
                            {
                                relevantStatup = false;
                            }
                        }

                        var mbv = maxBuffValue.GetValueOrDefault(mbs);
                        if (mbv == null)
                        {
                            continue;
                        }

                        if (mbv.Value.Key < st.Value)
                        {
                            var msbe = mbv.Value.Value;
                            if (msbe != null)
                            {
                                recalcMseList.Add(msbe);
                            }

                            maxBuffValue.AddOrUpdate(mbs, new(st.Value, mse));

                            if (relevantStatup)
                            {
                                if (maxEffectiveStatup < st.Value)
                                {
                                    maxEffectiveStatup = st.Value;
                                }
                            }
                        }
                    }

                    updateEffects.AddOrUpdate(mse, maxEffectiveStatup);
                }
            } while (recalcMseList.Count > 0);

            List<StatEffect> updateEffectsList = sortEffectsList(updateEffects);

            List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> toUpdateEffects = new();
            foreach (StatEffect mse in updateEffectsList)
            {
                toUpdateEffects.Add(new(mse.getBuffSourceId(), retrievedEffects.GetValueOrDefault(mse.getBuffSourceId())));
            }

            List<BuffStatValue> activeStatups = new();
            foreach (var lmse in toUpdateEffects)
            {
                KeyValuePair<StatEffect, long> msel = lmse.Value;

                foreach (var statup in getActiveStatupsFromSourceid(lmse.Key))
                {
                    activeStatups.Add(statup);
                }

                msel.Key.updateBuffEffect(this, activeStatups.ToArray(), msel.Value);
                activeStatups.Clear();
            }

            List<KeyValuePair<int, KeyValuePair<StatEffect, long>>> priorityEffects = propagatePriorityBuffEffectUpdates(retrievedStats);
            foreach (var lmse in priorityEffects)
            {
                var msel = lmse.Value;

                foreach (var statup in getActiveStatupsFromSourceid(lmse.Key))
                {
                    activeStatups.Add(statup);
                }

                msel.Key.updateBuffEffect(this, activeStatups.ToArray(), msel.Value);
                activeStatups.Clear();
            }

            if (this.isRidingBattleship())
            {
                this.sendPacket(PacketCreator.giveBuff(ItemId.BATTLESHIP, 5221006, new BuffStatValue(BuffStat.MONSTER_RIDING, 0)));
                this.announceBattleshipHp();
            }
        }

        private static BuffStat? getSingletonStatupFromEffect(StatEffect mse)
        {
            return mse.getStatups().FirstOrDefault(x => isSingletonStatup(x.BuffState))?.BuffState;
        }

        private List<BuffStatValue> getActiveStatupsFromSourceid(int sourceid)
        {
            if (!buffEffects.ContainsKey(sourceid))
                return new List<BuffStatValue>();
            // already under effLock & chrLock
            List<BuffStatValue> ret = new();
            List<BuffStatValue> singletonStatups = new();
            foreach (var bel in buffEffects[sourceid])
            {
                BuffStat mbs = bel.Key;
                BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(bel.Key);

                BuffStatValue p;
                if (mbsvh != null)
                {
                    p = new(mbs, mbsvh.value);
                }
                else
                {
                    p = new(mbs, 0);
                }

                if (!isSingletonStatup(mbs))
                {   // thanks resinate, Daddy Egg for pointing out morph issues when updating it along with other statups
                    ret.Add(p);
                }
                else
                {
                    singletonStatups.Add(p);
                }
            }
            ret.Sort((p1, p2) => p1.BuffState.CompareTo(p2.BuffState));

            if (singletonStatups.Count > 0)
            {
                singletonStatups.Sort((p1, p2) => p1.BuffState.CompareTo(p2.BuffState));

                ret.AddRange(singletonStatups);
            }

            return ret;
        }

        private static bool isSingletonStatup(BuffStat mbs)
        {
            var list = new BuffStat[]
            {
                BuffStat.COUPON_EXP1,
                BuffStat.COUPON_EXP2,
                BuffStat.COUPON_EXP3,
                BuffStat.COUPON_EXP4,
                BuffStat.COUPON_DRP1,
                BuffStat.COUPON_DRP2,
                BuffStat.COUPON_DRP3,
                BuffStat.MESO_UP_BY_ITEM,
                BuffStat.ITEM_UP_BY_ITEM,
                BuffStat.RESPECT_PIMMUNE,
                BuffStat.RESPECT_MIMMUNE,
                BuffStat.DEFENSE_ATT,
                BuffStat.DEFENSE_STATE,
                BuffStat.WATK,
                BuffStat.WDEF,
                BuffStat.MATK,
                BuffStat.MDEF,
                BuffStat.ACC,
                BuffStat.AVOID,
                BuffStat.SPEED,
                BuffStat.JUMP
            };
            return !list.Contains(mbs);
        }

        private static bool isPriorityBuffSourceid(int sourceid)
        {
            switch (sourceid)
            {
                case -ItemId.ROSE_SCENT:
                case -ItemId.FREESIA_SCENT:
                case -ItemId.LAVENDER_SCENT:
                    return true;

                default:
                    return false;
            }
        }

        private void addItemEffectHolder(int sourceid, Dictionary<BuffStat, BuffStatValueHolder> statups)
        {
            buffEffects.AddOrUpdate(sourceid, statups);
        }

        private void addItemEffectHolderCount(BuffStat stat)
        {
            var val = buffEffectsCount.GetValueOrDefault(stat) + 1;

            buffEffectsCount.AddOrUpdate(stat, (sbyte)val);
        }

        public void registerEffect(StatEffect effect, IEnumerable<BuffStatValue> purposeStats, long starttime, long expirationtime, bool isSilent)
        {
            if (effect.getHpRRate() > 0 || effect.getMpRRate() > 0)
            {
                // 不明
                if (effect.getHpRRate() > 0)
                {
                    extraHpRec = effect.getHpR();
                    extraRecInterval = effect.getHpRRate();
                }

                if (effect.getMpRRate() > 0)
                {
                    extraMpRec = effect.getMpR();
                    extraRecInterval = effect.getMpRRate();
                }


                stopExtraTask();
                startExtraTask(extraHpRec, extraMpRec, extraRecInterval);   // HP & MP sharing the same task holder
            }
            else if (effect.isMapChair())
            {
                startChairTask();
            }

            int sourceid = effect.getBuffSourceId();
            Dictionary<BuffStat, BuffStatValueHolder> toDeploy;
            Dictionary<BuffStat, BuffStatValueHolder> appliedStatups = new();

            foreach (var ps in purposeStats)
            {
                appliedStatups[ps.BuffState] = GetPlayerBuffStatValueHolder(this, effect, starttime, expirationtime, ps.Value);
            }

            bool active = effect.isActive(this);
            if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
            {
                toDeploy = new();
                Dictionary<int, KeyValuePair<StatEffect, long>> retrievedEffects = new();
                HashSet<BuffStat> retrievedStats = new();
                foreach (var statup in appliedStatups)
                {
                    BuffStatValueHolder? mbsvh = ActiveEffects.GetValueOrDefault(statup.Key);
                    BuffStatValueHolder statMbsvh = statup.Value;

                    if (active)
                    {
                        if (mbsvh == null || mbsvh.value < statMbsvh.value || (mbsvh.value == statMbsvh.value && mbsvh.effect.getStatups().Count <= statMbsvh.effect.getStatups().Count))
                        {
                            toDeploy.AddOrUpdate(statup.Key, statMbsvh);
                        }
                        else
                        {
                            if (!isSingletonStatup(statup.Key))
                            {
                                foreach (var mbs in mbsvh.effect.getStatups())
                                {
                                    retrievedStats.Add(mbs.BuffState);
                                }
                            }
                        }
                    }

                    addItemEffectHolderCount(statup.Key);
                }

                // should also propagate update from buffs shared with priority sourceids
                var updated = appliedStatups.Keys;
                foreach (BuffStatValueHolder mbsvh in this.getAllStatups())
                {
                    if (isPriorityBuffSourceid(mbsvh.effect.getBuffSourceId()))
                    {
                        foreach (var p in mbsvh.effect.getStatups())
                        {
                            if (updated.Contains(p.BuffState))
                            {
                                retrievedStats.Add(p.BuffState);
                            }
                        }
                    }
                }

                if (!isSilent)
                {
                    addItemEffectHolder(sourceid, appliedStatups);
                    foreach (var statup in toDeploy)
                    {
                        ActiveEffects.AddOrUpdate(statup.Key, statup.Value);
                    }

                    if (active)
                    {
                        retrievedEffects.AddOrUpdate(sourceid, new(effect, starttime));
                    }

                    propagateBuffEffectUpdates(retrievedEffects, retrievedStats, new());
                }
            }
            else
            {
                foreach (var statup in appliedStatups)
                {
                    addItemEffectHolderCount(statup.Key);
                }

                toDeploy = (active ? appliedStatups : new());
            }

            addItemEffectHolder(sourceid, appliedStatups);
            foreach (var statup in toDeploy)
            {
                ActiveEffects.AddOrUpdate(statup.Key, statup.Value);
            }


            UpdateLocalStats();
        }


        public bool hasActiveBuff(int sourceid)
        {
            var allBuffs = ActiveEffects.Values.ToList();

            foreach (BuffStatValueHolder mbsvh in allBuffs)
            {
                if (mbsvh.effect.getBuffSourceId() == sourceid)
                {
                    return true;
                }
            }
            return false;
        }

        public float getCardRate(int itemid)
        {
            float rate = 100.0f;

            if (itemid == 0)
            {
                StatEffect? mseMeso = getBuffEffect(BuffStat.MESO_UP_BY_ITEM);
                if (mseMeso != null)
                {
                    rate += mseMeso.getCardRate(this, itemid);
                }
            }
            else
            {
                StatEffect? mseItem = getBuffEffect(BuffStat.ITEM_UP_BY_ITEM);
                if (mseItem != null)
                {
                    rate += mseItem.getCardRate(this, itemid);
                }
            }

            return rate / 100;
        }

        public bool IsMorphWithoutAttack()
        {
            var morphBuff = getBuffedValue(BuffStat.MORPH);
            if (morphBuff != null)
            {
                return morphBuff > 0 && morphBuff < 100;
            }
            return false;
        }

        static BuffStatValueHolder GetPlayerBuffStatValueHolder(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
        {
            // 这几个技能都是对自己释放，不会触发“相同取最优”的逻辑
            if (effect.isDragonBlood())
            {
                return new EffectDragonBlood(chr, effect, startTime, expiredAt, value);
            }
            else if (effect.isBerserk())
            {
                return new EffectBerserk(chr, effect, startTime, expiredAt, value);
            }
            else if (effect.isBeholder())
            {
                return new EffectBehold(chr, effect, startTime, expiredAt, value);
            }
            else if (effect.isRecovery())
            {
                return new EffectRecovery(chr, effect, startTime, expiredAt, value);
            }
            return new BuffStatValueHolder(chr, effect, startTime, expiredAt, value);
        }

        public bool IsActiveBuff(BuffStatValueHolder v)
        {
            return ActiveEffects.Values.Any(x => x == v);
        }
    }
}
