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

        private async Task changeStatPool(long? strDexIntLuk, long? newSp, int newAp, bool silent)
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
                    await UpdateLocalStats();
                }

                if (!silent)
                {
                    await SendStats();
                }
            }

        }

        public async Task healHpMp()
        {
            await UpdateStatsChunk(async () =>
             {
                 await SetHP(NumericConfig.MaxHP);
                 SetMP(NumericConfig.MaxMP);
             });
        }

        public async Task<int> safeAddHP(int delta)
        {
            if (HP + delta <= 0)
            {
                delta = -HP + 1;
            }

            await UpdateStatsChunk(async () =>
             {
                 await ChangeHP(delta);
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

        public async Task<bool> assignStr(int x)
        {
            return await assignStrDexIntLuk(x, null, null, null);
        }

        public async Task<bool> assignDex(int x)
        {
            return await assignStrDexIntLuk(null, x, null, null);
        }

        public async Task<bool> assignInt(int x)
        {
            return await assignStrDexIntLuk(null, null, x, null);
        }

        public async Task<bool> assignLuk(int x)
        {
            return await assignStrDexIntLuk(null, null, null, x);
        }

        public async Task<bool> assignHP(int deltaHP, int deltaAp)
        {

            if (Ap - deltaAp < 0 || HpMpUsed + deltaAp < 0 || MaxHP >= 30000)
            {
                return false;
            }

            await ChangeMaxHP(deltaHP);
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

        public async Task<bool> assignStrDexIntLuk(int? deltaStr, int? deltaDex, int? deltaInt, int? deltaLuk)
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
            await updateStrDexIntLuk(newStr, newDex, newInt, newLuk, newAp);
            return true;
        }

        public async Task updateStrDexIntLuk(int x)
        {
            await updateStrDexIntLuk(x, x, x, x, -1);
        }

        public async Task changeRemainingAp(int x, bool silent)
        {
            await changeStrDexIntLuk(Str, Dex, Int, Luk, x, silent);
        }

        public async Task gainAp(int deltaAp, bool silent)
        {

            await changeRemainingAp(Math.Max(0, Ap + deltaAp), silent);

        }

        protected async Task updateStrDexIntLuk(int str, int dex, int int_, int luk, int remainingAp)
        {
            await changeStrDexIntLuk(str, dex, int_, luk, remainingAp, false);
        }

        private async Task changeStrDexIntLuk(int str, int dex, int int_, int luk, int remainingAp, bool silent)
        {
            long strDexIntLuk = calcStatPoolLong(str, dex, int_, luk);
            await changeStatPool(strDexIntLuk, null, remainingAp, silent);
        }

        private async Task changeStrDexIntLukSp(int str, int dex, int int_, int luk, int remainingAp, int remainingSp, int skillbook, bool silent)
        {
            long strDexIntLuk = calcStatPoolLong(str, dex, int_, luk);
            long sp = calcStatPoolLong(0, 0, remainingSp, skillbook);
            await changeStatPool(strDexIntLuk, sp, remainingAp, silent);
        }

        protected async Task updateStrDexIntLukSp(int str, int dex, int int_, int luk, int remainingAp, int remainingSp, int skillbook)
        {
            await changeStrDexIntLukSp(str, dex, int_, luk, remainingAp, remainingSp, skillbook, false);
        }

        protected async Task updateRemainingSp(int remainingSp, int skillbook)
        {
            await changeRemainingSp(remainingSp, skillbook, false);
        }

        protected async Task changeRemainingSp(int remainingSp, int skillbook, bool silent)
        {
            long sp = calcStatPoolLong(0, 0, remainingSp, skillbook);
            await changeStatPool(null, sp, short.MinValue, silent);
        }

        public async Task gainSp(int deltaSp, int skillbook, bool silent)
        {

            await changeRemainingSp(Math.Max(0, RemainingSp[skillbook] + deltaSp), skillbook, silent);
        }

        public async Task SendStats()
        {
            if (statUpdates.Count > 0)
            {
                await SendPacket(PacketCreator.updatePlayerStats(statUpdates, true, this));
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

        public async Task UpdateStatsChunk(Action action)
        {
            statUpdates.Clear();

            action();

            await SendStats();
        }

        public async Task<TOut> UpdateStatsChunk<TOut>(Func<TOut> action)
        {
            try
            {
                statUpdates.Clear();

                return action();
            }
            finally
            {
                await SendStats();
            }
        }

        public async Task<TOut> UpdateStatsChunk<TOut>(Func<Task<TOut>> action)
        {
            try
            {
                statUpdates.Clear();

                return await action();
            }
            finally
            {
                await SendStats();
            }
        }

        public async Task MaxStat()
        {
            await loseExp(getExp(), false, false);
            setLevel(NumericConfig.MaxLevel);
            await updateStrDexIntLuk(NumericConfig.MaxStat);
            setFame(NumericConfig.MaxFame);
            await UpdateStatsChunk(async () =>
            {
                await SetMaxHP(NumericConfig.MaxHP);
                SetMaxMP(NumericConfig.MaxMP);
            });
            await updateSingleStat(Stat.LEVEL, NumericConfig.MaxLevel);
            await updateSingleStat(Stat.FAME, NumericConfig.MaxFame);
        }
    }
}
