using Application.Core.Server;
using Application.Utility.Tickables;

namespace Application.Core.Game.Skills
{
    public class BuffStatValueHolder : ILoopTickable, ILifedTickable
    {
        protected Player _chr;
        StatEffect _effect;
        public StatEffect Effect
        {
            get
            {
                if (!_effect.isSkill())
                {
                    return _effect;
                }
                else
                {
                    // 被动技能带来的buff，在取技能等级时，获取最新的信息
                    if (SkillIds.IsPassiveSkill(_effect.getBuffSourceId()))
                    {
                        return _chr.GetPlayerSkillEffect(_effect.getBuffSourceId());
                    }
                    return _effect;
                }
            }
        }
        public long startTime;
        public int value;
        public bool bestApplied;
        public long ExpiredAt { get; }
        public bool IsExpired { get; protected set; }

        public BuffStatValueHolder(Player chr, StatEffect effect, long startTime, long expiredAt, int value)
        {
            _chr = chr;
            _effect = effect;
            this.startTime = startTime;
            ExpiredAt = expiredAt;
            this.value = value;

            this.bestApplied = false;

            Period = 1_000;
        }

        public long Next { get; protected set; }

        public long Period { get; init; }

        public TickableStatus Status { get; set; }
        public virtual async Task OnTick(long now)
        {
            if (this.IsAvailable())
            {
                if (ExpiredAt <= now)
                {
                    Status = TickableStatus.Remove;
                    return;
                }

                await Process(now);
                Next = now + Period;
            }
        }

        protected virtual Task Process(long now)
        {
            return Task.CompletedTask;
        }
    }
}
