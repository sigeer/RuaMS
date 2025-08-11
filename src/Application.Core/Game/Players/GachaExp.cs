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
            if ((currentgexp + ExpValue.get()) >= ExpTable.getExpNeededForLevel(Level))
            {
                expgain += ExpTable.getExpNeededForLevel(Level) - ExpValue.get();

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
