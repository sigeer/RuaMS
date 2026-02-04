using tools;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public int[] RemainingSp { get; set; } = new int[10];

        protected Dictionary<Stat, int> statUpdates = new();



        public int getStr() => Str;

        public int getDex() => Dex;

        public int getInt() => Int;
        public int getLuk() => Luk;

        public int getRemainingAp() => Ap;

        protected int getRemainingSp(int jobid)
        {
            return RemainingSp[GameConstants.getSkillBook(jobid)];
        }

        public int[] getRemainingSps()
        {
            return Arrays.copyOf(RemainingSp, RemainingSp.Length);
        }

        public int getHpMpApUsed()
        {
            return HpMpUsed;
        }

        public bool isAlive() => HP > 0;


        private void setHpMpApUsed(int mpApUsed)
        {
            this.HpMpUsed = mpApUsed;
        }

        private void setRemainingAp(int remainingAp)
        {
            this.Ap = remainingAp;
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
                r = short.MinValue;
            }
            else
            {
                r = clampStat(v.Value, short.MinValue, short.MaxValue);
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

        private void changeStatPool(long? strDexIntLuk, long? newSp, int newAp, bool silent)
        {

            statUpdates.Clear();
            bool statUpdate = false;

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

                this.RemainingSp[skillbook] = sp;
                statUpdates.AddOrUpdate(Stat.AVAILABLESP, sp);
            }

            if (statUpdates.Count > 0)
            {
                if (statUpdate)
                {
                    UpdateLocalStats();
                }

                if (!silent)
                {
                    SendStats();
                }
            }

        }

        public void healHpMp()
        {
            UpdateStatsChunk(() =>
            {
                SetHP(NumericConfig.MaxHP);
                SetMP(NumericConfig.MaxMP);
            });
        }

        public int safeAddHP(int delta)
        {
            if (HP + delta <= 0)
            {
                delta = -HP + 1;
            }

            UpdateStatsChunk(() =>
            {
                ChangeHP(delta);
            });
            return delta;
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

            if (Ap - deltaAp < 0 || HpMpUsed + deltaAp < 0 || MaxHP >= 30000)
            {
                return false;
            }

            ChangeMaxHP(deltaHP);
            setHpMpApUsed(HpMpUsed + deltaAp);
            return true;
        }

        public bool assignMP(int deltaMP, int deltaAp)
        {

            if (Ap - deltaAp < 0 || HpMpUsed + deltaAp < 0 || MaxMP >= 30000)
            {
                return false;
            }

            ChangeMaxMP(deltaMP);
            setHpMpApUsed(HpMpUsed + deltaAp);
            return true;
        }

        private static int apAssigned(int? x)
        {
            return x ?? 0;
        }

        public bool assignStrDexIntLuk(int? deltaStr, int? deltaDex, int? deltaInt, int? deltaLuk)
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

        public void updateStrDexIntLuk(int x)
        {
            updateStrDexIntLuk(x, x, x, x, -1);
        }

        public void changeRemainingAp(int x, bool silent)
        {
            changeStrDexIntLuk(Str, Dex, Int, Luk, x, silent);
        }

        public void gainAp(int deltaAp, bool silent)
        {

            changeRemainingAp(Math.Max(0, Ap + deltaAp), silent);

        }

        protected void updateStrDexIntLuk(int str, int dex, int int_, int luk, int remainingAp)
        {
            changeStrDexIntLuk(str, dex, int_, luk, remainingAp, false);
        }

        private void changeStrDexIntLuk(int str, int dex, int int_, int luk, int remainingAp, bool silent)
        {
            long strDexIntLuk = calcStatPoolLong(str, dex, int_, luk);
            changeStatPool(strDexIntLuk, null, remainingAp, silent);
        }

        private void changeStrDexIntLukSp(int str, int dex, int int_, int luk, int remainingAp, int remainingSp, int skillbook, bool silent)
        {
            long strDexIntLuk = calcStatPoolLong(str, dex, int_, luk);
            long sp = calcStatPoolLong(0, 0, remainingSp, skillbook);
            changeStatPool(strDexIntLuk, sp, remainingAp, silent);
        }

        protected void updateStrDexIntLukSp(int str, int dex, int int_, int luk, int remainingAp, int remainingSp, int skillbook)
        {
            changeStrDexIntLukSp(str, dex, int_, luk, remainingAp, remainingSp, skillbook, false);
        }

        protected void updateRemainingSp(int remainingSp, int skillbook)
        {
            changeRemainingSp(remainingSp, skillbook, false);
        }

        protected void changeRemainingSp(int remainingSp, int skillbook, bool silent)
        {
            long sp = calcStatPoolLong(0, 0, remainingSp, skillbook);
            changeStatPool(null, sp, short.MinValue, silent);
        }

        public void gainSp(int deltaSp, int skillbook, bool silent)
        {

            changeRemainingSp(Math.Max(0, RemainingSp[skillbook] + deltaSp), skillbook, silent);
        }

        public void SendStats()
        {
            if (statUpdates.Count > 0)
            {
                sendPacket(PacketCreator.updatePlayerStats(statUpdates, true, this));
                // PrintStatsUpdated();
            }
        }

        public void PrintStatsUpdated()
        {
            Log.Debug($"== MaxHP: {ActualMaxHP}, HP: {HP}");
            Log.Debug($"== MaxMP: {ActualMaxMP}, MP: {MP}");
            foreach (var x in statUpdates)
            {
                Log.Debug($"<{x.Key}>=<{x.Value}>");
            }

        }

        public void UpdateStatsChunk(Action action)
        {
            statUpdates.Clear();

            action();

            SendStats();
        }

        public TOut UpdateStatsChunk<TOut>(Func<TOut> action)
        {
            try
            {
                statUpdates.Clear();

                return action();
            }
            finally
            {
                SendStats();
            }
        }

        public void MaxStat()
        {
            loseExp(getExp(), false, false);
            setLevel(NumericConfig.MaxLevel);
            updateStrDexIntLuk(NumericConfig.MaxStat);
            setFame(NumericConfig.MaxFame);
            UpdateStatsChunk(() =>
            {
                SetMaxHP(NumericConfig.MaxHP);
                SetMaxMP(NumericConfig.MaxMP);
            });
            updateSingleStat(Stat.LEVEL, NumericConfig.MaxLevel);
            updateSingleStat(Stat.FAME, NumericConfig.MaxFame);
        }
    }
}
