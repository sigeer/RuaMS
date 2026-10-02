using Application.Core.Server.life;
using Application.Shared.Battle.Skills;
using Application.Utility.Tickables;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    public abstract class MonsterBuffBase : ILoopTickable, ILifedTickable
    {
        protected Monster _mob;

        protected MonsterBuffBase(Monster mob, MonsterStatus buffStat, IBuffSource buffSource, int value, long startTime, long expireAt)
        {
            _mob = mob;
            BuffStat = buffStat;
            BuffSource = buffSource;
            Value = value;

            StartTime = startTime;
            ExpiredAt = expireAt;

            Period = 1_000;
        }

        public MonsterStatus BuffStat { get; }
        public IBuffSource BuffSource { get; }
        public int Value { get; init; }

        public long StartTime { get; }
        public long ExpiredAt { get; }
        public TickableStatus Status { get; set; }

        public long Next { get; protected set; }

        public long Period { get; init; }
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

        /// <summary>
        /// 持续触发
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        protected virtual Task Process(long now)
        {
            return Task.CompletedTask;
        }

        public void EncodeForLocalOne(OutPacket p)
        {
            p.writeShort(Value);
            p.writeInt(BuffSource.GetEncodeId());
            // a7 + 500 * CInPacket::Decode2(a6)
            p.writeShort((short)Math.Min((ExpiredAt - _mob.MapModel.ChannelServer.Node.getCurrentTime()) / 500, short.MaxValue));
        }
    }
}
