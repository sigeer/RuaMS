namespace Application.Templates.Skill
{
    public sealed class MobSkillLevelData
    {
        public MobSkillLevelData(int skillId, int level)
        {
            nSkillID = skillId;
            nSLV = level;
            SummonIDs = Array.Empty<int>();
        }

        public int nSkillID { get; set; }
        public int nSLV { get; set; }
        /// <summary>
        /// Skill cooldown (interval between casts) in seconds.
        /// </summary>
        public int Interval { get; set; }
        public int MpCon { get; set; }
        /// <summary>
        /// Duration of the (de)buff in seconds.
        /// </summary>
        public int Time { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>
        /// Minimum mob HP for this skill to be cast.
        /// </summary>
        public int HP { get; set; }
        public int Prop { get; set; }

        public int[] SummonIDs { get; set; }
        public int SummonEffect { get; set; }
        public int Limit { get; set; }
        public int Count { get; set; }
        public bool RandomTarget { get; set; }
    }
}