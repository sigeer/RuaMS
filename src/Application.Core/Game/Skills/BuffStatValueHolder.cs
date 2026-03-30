using Application.Utility.Tickables;
using server;
using tools;

namespace Application.Core.Game.Skills
{
    public class BuffStatValueHolder: ILoopTickable, ILifedTickable
    {
        protected Player _chr;
        public StatEffect effect;
        public long startTime;
        public int value;
        public bool bestApplied;
        public long ExpiredAt { get; }
        public bool IsExpired { get; protected set; }

        public BuffStatValueHolder(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
        {
            _chr = chr;
            this.effect = effect;
            this.startTime = startTime;
            ExpiredAt = expiredAt;
            this.value = value;

            this.bestApplied = false;

            Period = 1_000;
        }

        public long Next { get; protected set; }

        public long Period { get; init; }

        public bool IsTickableCancelled { get; set; }
        public virtual void OnTick(long now)
        {
            if (!IsTickableCancelled && !IsExpired && Next <= now)
            {
                if (ExpiredAt <= now)
                {
                    IsExpired = true;
                    return;
                }

                Process(now);
                Next = now + Period;
            }
        }

        protected virtual void Process(long now)
        {
            return;
        }
    }
}
