using Application.Utility.Tickables;
using server;
using tools;

namespace Application.Core.Game.Skills
{
    public class BuffStatValueHolder: ITickable
    {
        protected Player _chr;
        public StatEffect effect;
        public long startTime;
        public int value;
        public bool bestApplied;
        public long ExpiredAt { get; }

        public BuffStatValueHolder(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
        {
            _chr = chr;
            this.effect = effect;
            this.startTime = startTime;
            ExpiredAt = expiredAt;
            this.value = value;

            this.bestApplied = false;
            Next = long.MaxValue;
        }

        public long Next { get; protected set; }

        public long Period { get; init; }

        public bool Disabled { get; set; }
        public virtual void OnTick(long now)
        {
            if (!Disabled && ExpiredAt <= now)
            {
                Disabled = true;
                return;
            }
        }
    }
}
