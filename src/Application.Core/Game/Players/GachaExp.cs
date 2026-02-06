using constants.game;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public AtomicInteger GachaExpValue { get; set; }

        public void setGachaExp(int amount)
        {
            this.GachaExpValue.set(amount);
        }
        public int getGachaExp()
        {
            return GachaExpValue.get();
        }

        public void gainGachaExp()
        {
            int expgain = 0;
            long currentgexp = GachaExpValue.get();
            int levelUpNeed = ExpTable.getExpNeededForLevel(Level) - ExpValue.get();
            if (currentgexp >= levelUpNeed)
            {
                expgain += Math.Max(0, levelUpNeed);

                int nextneed = ExpTable.getExpNeededForLevel(Level + 1);
                if (currentgexp - expgain >= nextneed)
                {
                    expgain += nextneed;
                }

                this.GachaExpValue.set((int)(currentgexp - expgain));
            }
            else
            {
                expgain = this.GachaExpValue.getAndSet(0);
            }
            gainExp(expgain, false, true);
            updateSingleStat(Stat.GACHAEXP, this.GachaExpValue.get());
        }

        public void addGachaExp(int gain)
        {
            updateSingleStat(Stat.GACHAEXP, GachaExpValue.addAndGet(gain));
        }
    }
}
