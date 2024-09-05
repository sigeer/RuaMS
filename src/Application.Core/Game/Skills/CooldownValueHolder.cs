namespace Application.Core.Game.Skills
{
    public class CooldownValueHolder
    {

        public int skillId;
        public long startTime, length;

        public CooldownValueHolder(int skillId, long startTime, long length)
        {
            this.skillId = skillId;
            this.startTime = startTime;
            this.length = length;
        }
    }
}
