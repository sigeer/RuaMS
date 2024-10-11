using Application.Core.Game.Skills;
using client.status;
using server.life;

namespace Application.Core.Game.Life.Monsters
{
    public class MonsterStatusEffect
    {
        private Dictionary<MonsterStatus, int> stati;
        private ISkill skill;

        public MonsterStatusEffect(Dictionary<MonsterStatus, int> stati, ISkill skill)
        {
            this.stati = new(stati);
            this.skill = skill;
        }

        public Dictionary<MonsterStatus, int> getStati()
        {
            return stati;
        }

        public int setValue(MonsterStatus status, int newVal)
        {
            stati.AddOrUpdate(status, newVal);
            return newVal;
        }

        public Skill? getSkill()
        {
            return skill as Skill;
        }

        public bool isMonsterSkill()
        {
            return skill is MobSkill;
        }

        public void removeActiveStatus(MonsterStatus stat)
        {
            stati.Remove(stat);
        }

        public MobSkill? getMobSkill()
        {
            return skill as MobSkill;
        }
    }

}
