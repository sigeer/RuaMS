using Application.Core.Server;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    public class MonsterDebuff : MonsterBuffBase
    {
        public int FromPlayerOId { get; }
        public StatEffect PlayerSkillEffect { get; }


        public MonsterDebuff(Monster mob, int fromObject, MonsterStatus buffStat, StatEffect buffSource, int value, long startTime, long expiredAt) 
            : base(mob, buffStat, buffSource, value, startTime, expiredAt)
        {
            FromPlayerOId = fromObject;
            PlayerSkillEffect = buffSource;
        }
    }
}
