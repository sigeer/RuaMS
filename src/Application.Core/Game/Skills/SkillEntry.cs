namespace Application.Core.Game.Skills
{
    public class SkillEntry
    {

        public int masterlevel;
        public sbyte skillevel;
        public long expiration;

        public SkillEntry(sbyte skillevel, int masterlevel, long expiration)
        {
            this.skillevel = skillevel;
            this.masterlevel = masterlevel;
            this.expiration = expiration;
        }

        public override string ToString()
        {
            return skillevel + ":" + masterlevel;
        }
    }
}
