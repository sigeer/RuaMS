using Application.Core.Server.life;

namespace Application.Core.Game.Life.Monsters.TemporaryStat
{
    public class MonsterStatusEffect : MonsterBuffBase
    {
        public MobSkill MobSkillEffect { get; }
        public MonsterStatusEffect(Monster mob, MonsterStatus buffStat, MobSkill source, int value, long startTime, long expireAt)
            : base(mob, buffStat, source, value, startTime, expireAt)
        {
            MobSkillEffect = source;
        }
    }

}
