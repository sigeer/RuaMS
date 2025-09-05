using System.Drawing;

namespace Application.Templates.Skill
{
    public sealed class SkillLevelData
    {
        public int Level { get; set; }
        public int Damage { get; set; } = 100;
        public int Prop { get; set; } = 100;
        public int AttackCount { get; set; } = 1;
        public int MobCount { get; set; } = 1;
        public int Time { get; set; } = -1;
        public int MpCon { get; set; }
        public int HpCon { get; set; }
        public int Speed { get; set; }
        public int Jump { get; set; }
        public int PAD { get; set; }
        public int MAD { get; set; }
        public int PDD { get; set; }
        public int MDD { get; set; }
        public int EVA { get; set; }
        public int ACC { get; set; }
        public int HP { get; set; }
        public int MP { get; set; }
        public int Cooltime { get; set; }
        public Point? LeftTop { get; set; }
        public Point? RightBottom { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Morph { get; set; }
        public int BulletCount { get; set; } = 1;
        public int BulletConsume { get; set; }

        public int MoneyCon { get; set; }
        public int ItemCon { get; set; }
        public int ItemConNo { get; set; }
    }
}