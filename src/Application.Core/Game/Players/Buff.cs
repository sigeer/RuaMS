using Application.Core.Game.Skills;
using Application.Core.model;
using client;
using constants.id;
using constants.skills;
using net.server;
using server;
using server.maps;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        private ScheduledFuture? _buffExpireTask = null;
        /// <summary>
        /// 和 buffEffects 区别？
        /// </summary>

        private Dictionary<BuffStat, BuffStatValueHolder> effects = new();
        /// <summary>
        /// 似乎没有用
        /// </summary>
        private Dictionary<BuffStat, sbyte> buffEffectsCount = new();
        private Dictionary<Disease, long> diseaseExpires = new();
        /// <summary>
        /// sourceid - effects
        /// </summary>
        private Dictionary<int, Dictionary<BuffStat, BuffStatValueHolder>> buffEffects = new(); // non-overriding buffs thanks to Ronan
        private Dictionary<int, long> buffExpires = new();

        private BuffStatValueHolder? GetBuffStatValue(BuffStat effect)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                if (effects.TryGetValue(effect, out var mbsvh))
                {
                    return mbsvh;
                }
                return null;
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }
        public long? getBuffedStarttime(BuffStat effect)
        {
            return GetBuffStatValue(effect)?.startTime;
        }

        public void setBuffedValue(BuffStat effect, int value)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                if (effects.TryGetValue(effect, out var mbsvh))
                {
                    mbsvh.value = value;
                }
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
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
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                return effects.ContainsKey(stat);
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        private List<BuffStatValueHolder> getAllStatups()
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                return buffEffects.Values.SelectMany(x => x.Values).ToList();
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        public List<PlayerBuffValueHolder> getAllBuffs()
        {
            // buff values will be stored in an arbitrary order
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                long curtime = Server.getInstance().getCurrentTime();

                Dictionary<int, PlayerBuffValueHolder> ret = new();
                foreach (Dictionary<BuffStat, BuffStatValueHolder> bel in buffEffects.Values)
                {
                    foreach (BuffStatValueHolder mbsvh in bel.Values)
                    {
                        int srcid = mbsvh.effect.getBuffSourceId();
                        if (!ret.ContainsKey(srcid))
                        {
                            ret.Add(srcid, new PlayerBuffValueHolder((int)(curtime - mbsvh.startTime), mbsvh.effect));
                        }
                    }
                }
                return new(ret.Values);
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        private void extractBuffValue(int sourceid, BuffStat stat)
        {
            chLock.EnterReadLock();
            try
            {
                removeEffectFromItemEffectHolder(sourceid, stat);
            }
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public void debugListAllBuffs()
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
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
                Log.Debug("IN ACTION: {InAction}", string.Join(", ", effects
                        .Select(entry => entry.Key.name() + " -> " + ItemInformationProvider.getInstance().getName(entry.Value.effect.getSourceId())))
                );
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        public void debugListAllBuffsCount()
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                Log.Debug("ALL BUFFS COUNT: {Buffs}", string.Join(", ", buffEffectsCount.Select(entry => entry.Key.name() + " -> " + entry.Value))
                );
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        public void buffExpireTask()
        {
            if (_buffExpireTask == null)
            {
                _buffExpireTask = TimerManager.getInstance().register(() =>
                {
                    HashSet<KeyValuePair<int, long>> es;
                    List<BuffStatValueHolder> toCancel = new();

                    Monitor.Enter(effLock);
                    chLock.EnterReadLock();
                    try
                    {
                        es = new(buffExpires);

                        long curTime = Server.getInstance().getCurrentTime();
                        foreach (var bel in es)
                        {
                            if (curTime >= bel.Value)
                            {
                                toCancel.Add(buffEffects.GetValueOrDefault(bel.Key)!.Values.First());    //rofl
                            }
                        }
                    }
                    finally
                    {
                        chLock.ExitReadLock();
                        Monitor.Exit(effLock);
                    }

                    foreach (BuffStatValueHolder mbsvh in toCancel)
                    {
                        cancelEffect(mbsvh.effect, false, mbsvh.startTime);
                    }

                }, 1500);
            }
        }

        public void cancelBuffExpireTask()
        {
            if (_buffExpireTask != null)
            {
                _buffExpireTask.cancel(false);
                _buffExpireTask = null;
            }
        }


        public void cancelAllBuffs(bool softcancel)
        {
            if (softcancel)
            {
                Monitor.Enter(effLock);
                chLock.EnterReadLock();
                try
                {
                    cancelEffectFromBuffStat(BuffStat.SUMMON);
                    cancelEffectFromBuffStat(BuffStat.PUPPET);
                    cancelEffectFromBuffStat(BuffStat.COMBO);

                    effects.Clear();

                    foreach (int srcid in buffEffects.Keys)
                    {
                        removeItemEffectHolder(srcid);
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                    Monitor.Exit(effLock);
                }
            }
            else
            {
                Dictionary<StatEffect, long> mseBuffs = new();

                Monitor.Enter(effLock);
                chLock.EnterReadLock();
                try
                {
                    foreach (var bpl in buffEffects)
                    {
                        foreach (var mbse in bpl.Value)
                        {
                            mseBuffs.AddOrUpdate(mbse.Value.effect, mbse.Value.startTime);
                        }
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                    Monitor.Exit(effLock);
                }

                foreach (var mse in mseBuffs)
                {
                    cancelEffect(mse.Key, false, mse.Value);
                }
            }
        }

        private void dropBuffStats(List<BuffStateValuePair> effectsToCancel)
        {
            foreach (var cancelEffectCancelTasks in effectsToCancel)
            {
                //bool nestedCancel = false;

                chLock.EnterReadLock();
                try
                {
                    /*
                    if (buffExpires.get(cancelEffectCancelTasks.getRight().effect.getBuffSourceId()) != null) {
                        nestedCancel = true;
                    }*/

                    if (cancelEffectCancelTasks.ValueHolder.bestApplied)
                    {
                        fetchBestEffectFromItemEffectHolder(cancelEffectCancelTasks.BuffStat);
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                }

                /*
                if (nestedCancel) {
                    this.cancelEffect(cancelEffectCancelTasks.getRight().effect, false, -1, false);
                }*/
            }
        }

        private List<BuffStateValuePair> deregisterBuffStats(Dictionary<BuffStat, BuffStatValueHolder> stats)
        {
            chLock.EnterReadLock();
            try
            {
                List<BuffStateValuePair> effectsToCancel = new(stats.Count);
                foreach (var stat in stats)
                {
                    int sourceid = stat.Value.effect.getBuffSourceId();

                    if (!buffEffects.ContainsKey(sourceid))
                    {
                        buffExpires.Remove(sourceid);
                    }

                    BuffStat mbs = stat.Key;
                    effectsToCancel.Add(new(mbs, stat.Value));

                    BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(mbs);
                    if (mbsvh != null && mbsvh.effect.getBuffSourceId() == sourceid)
                    {
                        mbsvh.bestApplied = true;
                        effects.Remove(mbs);

                        if (mbs == BuffStat.RECOVERY)
                        {
                            if (recoveryTask != null)
                            {
                                recoveryTask.cancel(false);
                                recoveryTask = null;
                            }
                        }
                        else if (mbs == BuffStat.SUMMON || mbs == BuffStat.PUPPET)
                        {
                            int summonId = mbsvh.effect.getSourceId();

                            var summon = summons.GetValueOrDefault(summonId);
                            if (summon != null)
                            {
                                MapModel.broadcastMessage(PacketCreator.removeSummon(summon, true), summon.getPosition());
                                MapModel.removeMapObject(summon);
                                removeVisibleMapObject(summon);

                                summons.Remove(summonId);
                                if (summon.isPuppet())
                                {
                                    MapModel.removePlayerPuppet(this);
                                }
                                else if (summon.getSkill() == DarkKnight.BEHOLDER)
                                {
                                    if (beholderHealingSchedule != null)
                                    {
                                        beholderHealingSchedule.cancel(false);
                                        beholderHealingSchedule = null;
                                    }
                                    if (beholderBuffSchedule != null)
                                    {
                                        beholderBuffSchedule.cancel(false);
                                        beholderBuffSchedule = null;
                                    }
                                }
                            }
                        }
                        else if (mbs == BuffStat.DRAGONBLOOD)
                        {
                            dragonBloodSchedule?.cancel(false);
                            dragonBloodSchedule = null;
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
            finally
            {
                chLock.ExitReadLock();
            }
        }

        public void cancelEffect(int itemId)
        {
            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            cancelEffect(ii.GetItemEffectTrust(itemId), false, -1);
        }

        public bool cancelEffect(StatEffect effect, bool overwrite, long startTime)
        {
            bool ret;

            Monitor.Enter(prtLock);
            Monitor.Enter(effLock);
            try
            {
                ret = cancelEffect(effect, overwrite, startTime, true);
            }
            finally
            {
                Monitor.Exit(effLock);
                Monitor.Exit(prtLock);
            }

            if (effect.isMagicDoor() && ret)
            {
                Monitor.Enter(prtLock);
                Monitor.Enter(effLock);
                try
                {
                    if (!hasBuffFromSourceid(Priest.MYSTIC_DOOR))
                    {
                        Door.attemptRemoveDoor(this);
                    }
                }
                finally
                {
                    Monitor.Exit(effLock);
                    Monitor.Exit(prtLock);
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
            Monitor.Enter(effLock);     // thanks davidlafriniere, maple006, RedHat for pointing a deadlock occurring here
            try
            {
                HashSet<BuffStat> updatedBuffs = new();
                HashSet<StatEffect> activeEffects = new();

                foreach (BuffStatValueHolder mse in effects.Values)
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
                    effects.Remove(mbs);
                }

                updateEffects(updatedBuffs);
            }
            finally
            {
                Monitor.Exit(effLock);
            }
        }

        private void updateEffects(HashSet<BuffStat> removedStats)
        {
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                HashSet<BuffStat> retrievedStats = new();

                foreach (BuffStat mbs in removedStats)
                {
                    fetchBestEffectFromItemEffectHolder(mbs);

                    BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(mbs);
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
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
        }

        private bool cancelEffect(StatEffect effect, bool overwrite, long startTime, bool firstCancel)
        {
            HashSet<BuffStat> removedStats = new();
            dropBuffStats(cancelEffectInternal(effect, overwrite, startTime, removedStats));
            updateLocalStats();
            updateEffects(removedStats);

            return removedStats.Count > 0;
        }

        private List<BuffStateValuePair> cancelEffectInternal(StatEffect effect, bool overwrite, long startTime, HashSet<BuffStat> removedStats)
        {
            Dictionary<BuffStat, BuffStatValueHolder>? buffstats = null;
            BuffStat? ombs;
            if (!overwrite)
            {   // is removing the source effect, meaning every effect from this srcid is being purged
                buffstats = extractCurrentBuffStats(effect);
            }
            else if ((ombs = getSingletonStatupFromEffect(effect)) != null)
            {   // removing all effects of a buff having non-shareable buff stat.
                BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(ombs);
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
                this.getClient().getWorldServer().unregisterMountHunger(this);
                this.getMount()?.setActive(false);
            }

            if (!overwrite)
            {
                removedStats.addAll(buffstats.Keys);
            }

            return toCancel;
        }

        public void cancelEffectFromBuffStat(BuffStat stat)
        {
            BuffStatValueHolder? effect;

            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                effect = effects.GetValueOrDefault(stat);
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }
            if (effect != null)
            {
                cancelEffect(effect.effect, false, -1);
            }
        }

        public void cancelBuffStats(BuffStat stat)
        {
            Monitor.Enter(effLock);
            try
            {
                List<KeyValuePair<int, BuffStatValueHolder>> cancelList = new();

                chLock.EnterReadLock();
                try
                {
                    foreach (var bel in this.buffEffects)
                    {
                        BuffStatValueHolder? beli = bel.Value.GetValueOrDefault(stat);
                        if (beli != null)
                        {
                            cancelList.Add(new(bel.Key, beli));
                        }
                    }
                }
                finally
                {
                    chLock.ExitReadLock();
                }

                Dictionary<BuffStat, BuffStatValueHolder> buffStatList = new();
                foreach (var p in cancelList)
                {
                    buffStatList.AddOrUpdate(stat, p.Value);
                    extractBuffValue(p.Key, stat);
                    dropBuffStats(deregisterBuffStats(buffStatList));
                }
            }
            finally
            {
                Monitor.Exit(effLock);
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
                    buffExpires.Remove(sourceid);
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

            buffExpires.Remove(sourceid);
        }

        private Dictionary<BuffStat, BuffStatValueHolder> extractCurrentBuffStats(StatEffect effect)
        {
            chLock.EnterReadLock();
            try
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
            finally
            {
                chLock.ExitReadLock();
            }
        }

        private Dictionary<BuffStat, BuffStatValueHolder> extractLeastRelevantStatEffectsIfFull(StatEffect effect)
        {
            Dictionary<BuffStat, BuffStatValueHolder> extractedStatBuffs = new();

            chLock.EnterReadLock();
            try
            {
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
            }
            finally
            {
                chLock.ExitReadLock();
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
                            BuffStatValueHolder mbsvhe = effects.GetValueOrDefault(mbs)!;

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
                BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(mbs);
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
                BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(bel.Key);

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

        private void addItemEffectHolder(int sourceid, long expirationtime, Dictionary<BuffStat, BuffStatValueHolder> statups)
        {
            buffEffects.AddOrUpdate(sourceid, statups);
            buffExpires.AddOrUpdate(sourceid, expirationtime);
        }

        private void addItemEffectHolderCount(BuffStat stat)
        {
            var val = buffEffectsCount.GetValueOrDefault(stat) + 1;

            buffEffectsCount.AddOrUpdate(stat, (sbyte)val);
        }

        public void registerEffect(StatEffect effect, long starttime, long expirationtime, bool isSilent)
        {
            if (effect.isDragonBlood())
            {
                prepareDragonBlood(effect);
            }
            else if (effect.isBerserk())
            {
                checkBerserk(isHidden());
            }
            else if (effect.isBeholder())
            {
                int beholder = DarkKnight.BEHOLDER;
                if (beholderHealingSchedule != null)
                {
                    beholderHealingSchedule.cancel(false);
                }
                if (beholderBuffSchedule != null)
                {
                    beholderBuffSchedule.cancel(false);
                }
                Skill bHealing = SkillFactory.GetSkillTrust(DarkKnight.AURA_OF_BEHOLDER);
                int bHealingLvl = getSkillLevel(bHealing);
                if (bHealingLvl > 0)
                {
                    StatEffect healEffect = bHealing.getEffect(bHealingLvl);
                    var healInterval = TimeSpan.FromSeconds(healEffect.getX());
                    beholderHealingSchedule = TimerManager.getInstance().register(() =>
                    {
                        if (awayFromWorld.Get())
                        {
                            return;
                        }

                        ChangeHP(healEffect.getHp());
                        sendPacket(PacketCreator.showOwnBuffEffect(beholder, 2));
                        MapModel.broadcastMessage(this, PacketCreator.summonSkill(getId(), beholder, 5), true);
                        MapModel.broadcastMessage(this, PacketCreator.showOwnBuffEffect(beholder, 2), false);

                    }, healInterval, healInterval);
                }
                Skill bBuff = SkillFactory.GetSkillTrust(DarkKnight.HEX_OF_BEHOLDER);
                if (getSkillLevel(bBuff) > 0)
                {
                    StatEffect buffEffect = bBuff.getEffect(getSkillLevel(bBuff));
                    var buffInterval = TimeSpan.FromSeconds(buffEffect.getX());
                    beholderBuffSchedule = TimerManager.getInstance().register(() =>
                    {
                        if (awayFromWorld.Get())
                        {
                            return;
                        }

                        buffEffect.applyTo(this);
                        sendPacket(PacketCreator.showOwnBuffEffect(beholder, 2));
                        MapModel.broadcastMessage(this, PacketCreator.summonSkill(getId(), beholder, (int)(Randomizer.nextDouble() * 3) + 6), true);
                        MapModel.broadcastMessage(this, PacketCreator.showBuffEffect(getId(), beholder, 2), false);

                    }, buffInterval, buffInterval);
                }
            }
            else if (effect.isRecovery())
            {
                int healInterval = (YamlConfig.config.server.USE_ULTRA_RECOVERY) ? 2000 : 5000;
                byte heal = (byte)effect.getX();

                chLock.EnterReadLock();
                try
                {
                    if (recoveryTask != null)
                    {
                        recoveryTask.cancel(false);
                    }

                    recoveryTask = TimerManager.getInstance().register(() =>
                    {
                        if (getBuffSource(BuffStat.RECOVERY) == -1)
                        {
                            chLock.EnterReadLock();
                            try
                            {
                                if (recoveryTask != null)
                                {
                                    recoveryTask.cancel(false);
                                    recoveryTask = null;
                                }
                            }
                            finally
                            {
                                chLock.ExitReadLock();
                            }

                            return;
                        }

                        ChangeHP(heal);
                        sendPacket(PacketCreator.showOwnRecovery(heal));
                        MapModel.broadcastMessage(this, PacketCreator.showRecovery(Id, heal), false);

                    }, healInterval, healInterval);
                }
                finally
                {
                    chLock.ExitReadLock();
                }
            }
            else if (effect.getHpRRate() > 0 || effect.getMpRRate() > 0)
            {
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

                chLock.EnterReadLock();
                try
                {
                    stopExtraTask();
                    startExtraTask(extraHpRec, extraMpRec, extraRecInterval);   // HP & MP sharing the same task holder
                }
                finally
                {
                    chLock.ExitReadLock();
                }

            }
            else if (effect.isMapChair())
            {
                startChairTask();
            }

            Monitor.Enter(prtLock);
            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                int sourceid = effect.getBuffSourceId();
                Dictionary<BuffStat, BuffStatValueHolder> toDeploy;
                Dictionary<BuffStat, BuffStatValueHolder> appliedStatups = new();

                foreach (var ps in effect.getStatups())
                {
                    appliedStatups.AddOrUpdate(ps.BuffState, new BuffStatValueHolder(effect, starttime, ps.Value));
                }

                bool active = effect.isActive(this);
                if (YamlConfig.config.server.USE_BUFF_MOST_SIGNIFICANT)
                {
                    toDeploy = new();
                    Dictionary<int, KeyValuePair<StatEffect, long>> retrievedEffects = new();
                    HashSet<BuffStat> retrievedStats = new();
                    foreach (var statup in appliedStatups)
                    {
                        BuffStatValueHolder? mbsvh = effects.GetValueOrDefault(statup.Key);
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
                        addItemEffectHolder(sourceid, expirationtime, appliedStatups);
                        foreach (var statup in toDeploy)
                        {
                            effects.AddOrUpdate(statup.Key, statup.Value);
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

                addItemEffectHolder(sourceid, expirationtime, appliedStatups);
                foreach (var statup in toDeploy)
                {
                    effects.AddOrUpdate(statup.Key, statup.Value);
                }
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
                Monitor.Exit(prtLock);
            }

            updateLocalStats();
        }

        private void prepareDragonBlood(StatEffect bloodEffect)
        {
            if (dragonBloodSchedule != null)
            {
                dragonBloodSchedule.cancel(false);
                dragonBloodSchedule = null;
            }
            dragonBloodSchedule = TimerManager.getInstance().register(() =>
            {
                if (awayFromWorld.Get())
                {
                    return;
                }

                if (ChangeHP(-bloodEffect.getX()))
                {
                    sendPacket(PacketCreator.showOwnBuffEffect(bloodEffect.getSourceId(), 5));
                    MapModel.broadcastMessage(this, PacketCreator.showBuffEffect(getId(), bloodEffect.getSourceId(), 5), false);
                }
                else
                {
                    dragonBloodSchedule!.cancel(false);
                    dragonBloodSchedule = null;
                }
            }, 4000, 4000);
        }

        public bool hasActiveBuff(int sourceid)
        {
            LinkedList<BuffStatValueHolder> allBuffs;

            Monitor.Enter(effLock);
            chLock.EnterReadLock();
            try
            {
                allBuffs = new(effects.Values);
            }
            finally
            {
                chLock.ExitReadLock();
                Monitor.Exit(effLock);
            }

            foreach (BuffStatValueHolder mbsvh in allBuffs)
            {
                if (mbsvh.effect.getBuffSourceId() == sourceid)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
