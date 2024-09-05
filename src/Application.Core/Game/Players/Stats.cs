using client;
using constants.game;
using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public int[] RemainingSp { get; set; } = new int[10];
        protected int clientmaxhp, clientmaxmp, localmaxhp = 50, localmaxmp = 5;
        protected float transienthp = float.NegativeInfinity, transientmp = float.NegativeInfinity;

        private AbstractCharacterListener? listener = null;
        protected Dictionary<Stat, int> statUpdates = new();

        protected object effLock = new object();
        protected ReaderWriterLockSlim statLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public void RegisterStatsListener()
        {
            listener = new CharacterListener();
            listener.onHpChanged = (oldHp) =>
            {
                hpChangeAction(oldHp);
            };
            listener.onHpmpPoolUpdate = () =>
            {
                var hpmpupdate = recalcLocalStats();
                foreach (var p in hpmpupdate)
                {
                    statUpdates.AddOrUpdate(p.Key, p.Value);
                }

                if (Hp > localmaxhp)
                {
                    setHp(localmaxhp);
                    statUpdates.AddOrUpdate(Stat.HP, Hp);
                }

                if (Mp > localmaxmp)
                {
                    setMp(localmaxmp);
                    statUpdates.AddOrUpdate(Stat.MP, Mp);
                }
            };
            listener.onStatUpdate = () =>
            {
                recalcLocalStats();
            };
            listener.onAnnounceStatPoolUpdate = () =>
            {
                List<KeyValuePair<Stat, int>> statup = new(8);
                foreach (var s in statUpdates)
                {
                    statup.Add(new(s.Key, s.Value));
                }

                sendPacket(PacketCreator.updatePlayerStats(statup, true, this));
            };
        }


        public int getStr()
        {
            statLock.EnterReadLock();
            try
            {
                return Str;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getDex()
        {
            statLock.EnterReadLock();
            try
            {
                return Dex;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getInt()
        {
            statLock.EnterReadLock();
            try
            {
                return Int;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getLuk()
        {
            statLock.EnterReadLock();
            try
            {
                return Luk;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getRemainingAp()
        {
            statLock.EnterReadLock();
            try
            {
                return Ap;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        protected int getRemainingSp(int jobid)
        {
            statLock.EnterReadLock();
            try
            {
                return RemainingSp[GameConstants.getSkillBook(jobid)];
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int[] getRemainingSps()
        {
            statLock.EnterReadLock();
            try
            {
                return Arrays.copyOf(RemainingSp, RemainingSp.Length);
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getHpMpApUsed()
        {
            statLock.EnterReadLock();
            try
            {
                return HpMpUsed;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public bool isAlive()
        {
            statLock.EnterReadLock();
            try
            {
                return Hp > 0;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getHp()
        {
            statLock.EnterReadLock();
            try
            {
                return Hp;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getMp()
        {
            statLock.EnterReadLock();
            try
            {
                return Mp;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getMaxHp()
        {
            statLock.EnterReadLock();
            try
            {
                return Maxhp;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getMaxMp()
        {
            statLock.EnterReadLock();
            try
            {
                return Maxmp;
            }
            finally
            {
                statLock.ExitReadLock();
            }
        }

        public int getClientMaxHp()
        {
            return clientmaxhp;
        }

        public int getClientMaxMp()
        {
            return clientmaxmp;
        }

        public int getCurrentMaxHp()
        {
            return localmaxhp;
        }

        public int getCurrentMaxMp()
        {
            return localmaxmp;
        }

        private void setHpMpApUsed(int mpApUsed)
        {
            this.HpMpUsed = mpApUsed;
        }

        private void dispatchHpChanged(int oldHp)
        {
            listener?.onHpChanged?.Invoke(oldHp);
        }

        private void dispatchHpmpPoolUpdated()
        {
            listener?.onHpmpPoolUpdate?.Invoke();
        }

        private void dispatchStatUpdated()
        {
            listener?.onStatUpdate?.Invoke();
        }

        private void dispatchStatPoolUpdateAnnounced()
        {
            listener?.onAnnounceStatPoolUpdate?.Invoke();
        }

        protected void setHp(int newHp)
        {
            int oldHp = Hp;

            int thp = newHp;
            if (thp < 0)
            {
                thp = 0;
            }
            else if (thp > localmaxhp)
            {
                thp = localmaxhp;
            }

            if (this.Hp != thp)
            {
                this.transienthp = float.NegativeInfinity;
            }
            this.Hp = thp;

            dispatchHpChanged(oldHp);
        }

        protected void setMp(int newMp)
        {
            int tmp = newMp;
            if (tmp < 0)
            {
                tmp = 0;
            }
            else if (tmp > localmaxmp)
            {
                tmp = localmaxmp;
            }

            if (this.Mp != tmp)
            {
                this.transientmp = float.NegativeInfinity;
            }
            this.Mp = tmp;
        }

        private void setRemainingAp(int remainingAp)
        {
            this.Ap = remainingAp;
        }

        private void setRemainingSp(int remainingSp, int skillbook)
        {
            this.RemainingSp[skillbook] = remainingSp;
        }

        protected void setMaxHp(int hp_)
        {
            if (this.Maxhp < hp_)
            {
                this.transienthp = float.NegativeInfinity;
            }
            this.Maxhp = hp_;
            this.clientmaxhp = Math.Min(30000, hp_);
        }

        protected void setMaxMp(int mp_)
        {
            if (this.Maxmp < mp_)
            {
                this.transientmp = float.NegativeInfinity;
            }
            this.Maxmp = mp_;
            this.clientmaxmp = Math.Min(30000, mp_);
        }

        private static long clampStat(int v, int min, int max)
        {
            return (v < min) ? min : ((v > max) ? max : v);
        }

        private static long calcStatPoolNode(int? v, int displacement)
        {
            long r;
            if (v == null)
            {
                r = -32768;
            }
            else
            {
                r = clampStat(v.Value, -32767, 32767);
            }

            return ((r & 0x0FFFF) << displacement);
        }

        private static long calcStatPoolLong(int? v1, int? v2, int? v3, int? v4)
        {
            long ret = 0;

            ret |= calcStatPoolNode(v1, 48);
            ret |= calcStatPoolNode(v2, 32);
            ret |= calcStatPoolNode(v3, 16);
            ret |= calcStatPoolNode(v4, 0);

            return ret;
        }

        private void changeStatPool(long? hpMpPool, long? strDexIntLuk, long? newSp, int newAp, bool silent)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                statUpdates.Clear();
                bool poolUpdate = false;
                bool statUpdate = false;

                if (hpMpPool != null)
                {
                    short newHp = (short)(hpMpPool >> 48);
                    short newMp = (short)(hpMpPool >> 32);
                    short newMaxHp = (short)(hpMpPool >> 16);
                    short newMaxMp = (short)hpMpPool;

                    if (newMaxHp != short.MinValue)
                    {
                        if (newMaxHp < 50)
                        {
                            newMaxHp = 50;
                        }

                        poolUpdate = true;
                        setMaxHp(newMaxHp);
                        statUpdates.AddOrUpdate(Stat.MAXHP, clientmaxhp);
                        statUpdates.AddOrUpdate(Stat.HP, Hp);
                    }

                    if (newHp != short.MinValue)
                    {
                        setHp(newHp);
                        statUpdates.AddOrUpdate(Stat.HP, Hp);
                    }

                    if (newMaxMp != short.MinValue)
                    {
                        if (newMaxMp < 5)
                        {
                            newMaxMp = 5;
                        }

                        poolUpdate = true;
                        setMaxMp(newMaxMp);
                        statUpdates.AddOrUpdate(Stat.MAXMP, clientmaxmp);
                        statUpdates.AddOrUpdate(Stat.MP, Mp);
                    }

                    if (newMp != short.MinValue)
                    {
                        setMp(newMp);
                        statUpdates.AddOrUpdate(Stat.MP, Mp);
                    }
                }

                if (strDexIntLuk != null)
                {
                    short newStr = (short)(strDexIntLuk >> 48);
                    short newDex = (short)(strDexIntLuk >> 32);
                    short newInt = (short)(strDexIntLuk >> 16);
                    short newLuk = (short)strDexIntLuk;

                    if (newStr >= 4)
                    {
                        setStr(newStr);
                        statUpdates.AddOrUpdate(Stat.STR, Str);
                    }

                    if (newDex >= 4)
                    {
                        setDex(newDex);
                        statUpdates.AddOrUpdate(Stat.DEX, Dex);
                    }

                    if (newInt >= 4)
                    {
                        setInt(newInt);
                        statUpdates.AddOrUpdate(Stat.INT, Int);
                    }

                    if (newLuk >= 4)
                    {
                        setLuk(newLuk);
                        statUpdates.AddOrUpdate(Stat.LUK, Luk);
                    }

                    if (newAp >= 0)
                    {
                        setRemainingAp(newAp);
                        statUpdates.AddOrUpdate(Stat.AVAILABLEAP, Ap);
                    }

                    statUpdate = true;
                }

                if (newSp != null)
                {
                    short sp = (short)(newSp >> 16);
                    short skillbook = (short)newSp;

                    setRemainingSp(sp, skillbook);
                    statUpdates.AddOrUpdate(Stat.AVAILABLESP, RemainingSp[skillbook]);
                }

                if (statUpdates.Count > 0)
                {
                    if (poolUpdate)
                    {
                        dispatchHpmpPoolUpdated();
                    }

                    if (statUpdate)
                    {
                        dispatchStatUpdated();
                    }

                    if (!silent)
                    {
                        dispatchStatPoolUpdateAnnounced();
                    }
                }
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void healHpMp()
        {
            updateHpMp(30000);
        }

        public void updateHpMp(int x)
        {
            updateHpMp(x, x);
        }

        public void updateHpMp(int newhp, int newmp)
        {
            changeHpMp(newhp, newmp, false);
        }

        public void changeHpMp(int newhp, int newmp, bool silent)
        {
            changeHpMpPool(newhp, newmp, null, null, silent);
        }

        private void changeHpMpPool(int? hp, int? mp, int? maxhp, int? maxmp, bool silent)
        {
            long hpMpPool = calcStatPoolLong(hp, mp, maxhp, maxmp);
            changeStatPool(hpMpPool, null, null, -1, silent);
        }

        public void updateHp(int hp)
        {
            updateHpMaxHp(hp, null);
        }

        public void updateMaxHp(int maxhp)
        {
            updateHpMaxHp(null, maxhp);
        }

        public void updateHpMaxHp(int? hp, int? maxhp)
        {
            changeHpMpPool(hp, null, maxhp, null, false);
        }

        public void updateMp(int mp)
        {
            updateMpMaxMp(mp, null);
        }

        public void updateMaxMp(int maxmp)
        {
            updateMpMaxMp(null, maxmp);
        }

        public void updateMpMaxMp(int? mp, int? maxmp)
        {
            changeHpMpPool(null, mp, null, maxmp, false);
        }

        public void updateMaxHpMaxMp(int maxhp, int maxmp)
        {
            changeHpMpPool(null, null, maxhp, maxmp, false);
        }

        protected void enforceMaxHpMp()
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                if (Mp > localmaxmp || Hp > localmaxhp)
                {
                    changeHpMp(Hp, Mp, false);
                }
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public int safeAddHP(int delta)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                if (Hp + delta <= 0)
                {
                    delta = -Hp + 1;
                }

                addHP(delta);
                return delta;
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void addHP(int delta)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                updateHp(Hp + delta);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void addMP(int delta)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                updateMp(Mp + delta);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void addMPHP(int hpDelta, int mpDelta)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                updateHpMp(Hp + hpDelta, Mp + mpDelta);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        protected void addMaxMPMaxHP(int hpdelta, int mpdelta, bool silent)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                changeHpMpPool(null, null, Maxhp + hpdelta, Maxmp + mpdelta, silent);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void addMaxHP(int delta)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                updateMaxHp(Maxhp + delta);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void addMaxMP(int delta)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                updateMaxMp(Maxmp + delta);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        private void setStr(int str)
        {
            this.Str = str;
        }

        private void setDex(int dex)
        {
            this.Dex = dex;
        }

        private void setInt(int int_)
        {
            this.Int = int_;
        }

        private void setLuk(int luk)
        {
            this.Luk = luk;
        }

        public bool assignStr(int x)
        {
            return assignStrDexIntLuk(x, null, null, null);
        }

        public bool assignDex(int x)
        {
            return assignStrDexIntLuk(null, x, null, null);
        }

        public bool assignInt(int x)
        {
            return assignStrDexIntLuk(null, null, x, null);
        }

        public bool assignLuk(int x)
        {
            return assignStrDexIntLuk(null, null, null, x);
        }

        public bool assignHP(int deltaHP, int deltaAp)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                if (Ap - deltaAp < 0 || HpMpUsed + deltaAp < 0 || Maxhp >= 30000)
                {
                    return false;
                }

                long hpMpPool = calcStatPoolLong(null, null, Maxhp + deltaHP, Maxmp);
                long strDexIntLuk = calcStatPoolLong(Str, Dex, Int, Luk);

                changeStatPool(hpMpPool, strDexIntLuk, null, Ap - deltaAp, false);
                setHpMpApUsed(HpMpUsed + deltaAp);
                return true;
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public bool assignMP(int deltaMP, int deltaAp)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                if (Ap - deltaAp < 0 || HpMpUsed + deltaAp < 0 || Maxmp >= 30000)
                {
                    return false;
                }

                long hpMpPool = calcStatPoolLong(null, null, Maxhp, Maxmp + deltaMP);
                long strDexIntLuk = calcStatPoolLong(Str, Dex, Int, Luk);

                changeStatPool(hpMpPool, strDexIntLuk, null, Ap - deltaAp, false);
                setHpMpApUsed(HpMpUsed + deltaAp);
                return true;
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        private static int apAssigned(int? x)
        {
            return x ?? 0;
        }

        public bool assignStrDexIntLuk(int? deltaStr, int? deltaDex, int? deltaInt, int? deltaLuk)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                int apUsed = apAssigned(deltaStr) + apAssigned(deltaDex) + apAssigned(deltaInt) + apAssigned(deltaLuk);
                if (apUsed > Ap)
                {
                    return false;
                }

                int newStr = Str, newDex = Dex, newInt = Int, newLuk = Luk;
                if (deltaStr != null)
                {
                    newStr += deltaStr.Value;   // thanks Rohenn for noticing an NPE case after "null" started being used
                }
                if (deltaDex != null)
                {
                    newDex += deltaDex.Value;
                }
                if (deltaInt != null)
                {
                    newInt += deltaInt.Value;
                }
                if (deltaLuk != null)
                {
                    newLuk += deltaLuk.Value;
                }

                if (newStr < 4 || newStr > YamlConfig.config.server.MAX_AP)
                {
                    return false;
                }

                if (newDex < 4 || newDex > YamlConfig.config.server.MAX_AP)
                {
                    return false;
                }

                if (newInt < 4 || newInt > YamlConfig.config.server.MAX_AP)
                {
                    return false;
                }

                if (newLuk < 4 || newLuk > YamlConfig.config.server.MAX_AP)
                {
                    return false;
                }

                int newAp = Ap - apUsed;
                updateStrDexIntLuk(newStr, newDex, newInt, newLuk, newAp);
                return true;
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void updateStrDexIntLuk(int x)
        {
            updateStrDexIntLuk(x, x, x, x, -1);
        }

        public void changeRemainingAp(int x, bool silent)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                changeStrDexIntLuk(Str, Dex, Int, Luk, x, silent);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        public void gainAp(int deltaAp, bool silent)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                changeRemainingAp(Math.Max(0, Ap + deltaAp), silent);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        protected void updateStrDexIntLuk(int str, int dex, int int_, int luk, int remainingAp)
        {
            changeStrDexIntLuk(str, dex, int_, luk, remainingAp, false);
        }

        private void changeStrDexIntLuk(int str, int dex, int int_, int luk, int remainingAp, bool silent)
        {
            long strDexIntLuk = calcStatPoolLong(str, dex, int_, luk);
            changeStatPool(null, strDexIntLuk, null, remainingAp, silent);
        }

        private void changeStrDexIntLukSp(int str, int dex, int int_, int luk, int remainingAp, int remainingSp, int skillbook, bool silent)
        {
            long strDexIntLuk = calcStatPoolLong(str, dex, int_, luk);
            long sp = calcStatPoolLong(0, 0, remainingSp, skillbook);
            changeStatPool(null, strDexIntLuk, sp, remainingAp, silent);
        }

        protected void updateStrDexIntLukSp(int str, int dex, int int_, int luk, int remainingAp, int remainingSp, int skillbook)
        {
            changeStrDexIntLukSp(str, dex, int_, luk, remainingAp, remainingSp, skillbook, false);
        }

        protected void setRemainingSp(int[] sps)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                Array.ConstrainedCopy(sps, 0, RemainingSp, 0, sps.Length);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }

        protected void updateRemainingSp(int remainingSp, int skillbook)
        {
            changeRemainingSp(remainingSp, skillbook, false);
        }

        protected void changeRemainingSp(int remainingSp, int skillbook, bool silent)
        {
            long sp = calcStatPoolLong(0, 0, remainingSp, skillbook);
            changeStatPool(null, null, sp, short.MinValue, silent);
        }

        public void gainSp(int deltaSp, int skillbook, bool silent)
        {
            Monitor.Enter(effLock);
            statLock.EnterWriteLock();
            try
            {
                changeRemainingSp(Math.Max(0, RemainingSp[skillbook] + deltaSp), skillbook, silent);
            }
            finally
            {
                statLock.ExitWriteLock();
                Monitor.Exit(effLock);
            }
        }
    }
}
