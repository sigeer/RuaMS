using System.Drawing;

namespace Application.Templates.Skill
{
    public sealed class MobSkillLevelData
    {
        public MobSkillLevelData()
        {
            SummonIDs = Array.Empty<int>();
        }
        [WZPath("level/-/$name")]
        public int nSLV { get; set; }
        /// <summary>
        /// Skill cooldown (interval between casts) in seconds.
        /// </summary>
        [WZPath("level/-/interval")]
        public int Interval { get; set; }
        public int MpCon { get; set; }
        /// <summary>
        /// Duration of the (de)buff in seconds.
        /// </summary>
        public int Time { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        [WZPath("level/-/hp")]
        public int HP { get; set; }
        public int Prop { get; set; }
        [WZPath("level/-/-")]
        public int[] SummonIDs { get; set; }
        public int SummonEffect { get; set; }
        public int Limit { get; set; }
        public int Count { get; set; }
        public Point Lt { get; set; }
        public Point Rb { get; set; }
    }
}