using Application.Templates.Item.Consume;
using Application.Templates.StatEffectProps;
using System.Drawing;

namespace Application.Templates.Skill
{
    public sealed class SkillLevelData: ISkillStatEffectProp, IStatEffectHeal, IStatEffectMorph, IStatEffectPower
    {

        [WZPath("~/$name")]
        public int Level { get; set; }
        [WZPath("~/damage")]
        public int Damage { get; set; } = 100;
        [WZPath("~/fixdamage")]
        public int FixDamage { get; set; } = -1;
        [WZPath("~/prop")]
        public int Prop { get; set; } = 100;
        [WZPath("~/attackCount")]
        public int AttackCount { get; set; } = 1;
        [WZPath("~/mobCount")]
        public int MobCount { get; set; } = 1;
        [WZPath("~/time")]
        public int Time { get; set; } = -1;
        [WZPath("~/mpCon")]
        public int MpCon { get; set; }
        [WZPath("~/hpCon")]
        public int HpCon { get; set; }
        [WZPath("~/speed")]
        public int Speed { get; set; }
        [WZPath("~/jump")]
        public int Jump { get; set; }
        [WZPath("~/pad")]
        public int PAD { get; set; }
        [WZPath("~/mad")]
        public int MAD { get; set; }
        [WZPath("~/pdd")]
        public int PDD { get; set; }
        [WZPath("~/mdd")]
        public int MDD { get; set; }
        [WZPath("~/eva")]
        public int EVA { get; set; }
        [WZPath("~/acc")]
        public int ACC { get; set; }
        [WZPath("~/hp")]
        public int HP { get; set; }
        [WZPath("~/mp")]
        public int MP { get; set; }
        [WZPath("~/cooltime")]
        public int Cooltime { get; set; }
        [WZPath("~/lt")]
        public Point? LeftTop { get; set; }
        [WZPath("~/rb")]
        public Point? RightBottom { get; set; }
        [WZPath("~/x")]
        public int X { get; set; }
        [WZPath("~/y")]
        public int Y { get; set; }
        [WZPath("~/morph")]
        public int Morph { get; set; }
        [WZPath("~/bulletCount")]
        public int BulletCount { get; set; } = 1;
        [WZPath("~/bulletConsume")]
        public int BulletConsume { get; set; }
        [WZPath("~/moneyCon")]
        public int MoneyCon { get; set; }
        [WZPath("~/itemCon")]
        public int ItemCon { get; set; }
        [WZPath("~/itemConNo")]
        public int ItemConNo { get; set; }

        [GenerateIgnoreProperty]
        public int HPR { get; set; }
        [GenerateIgnoreProperty]
        public int MPR { get; set; }
        [GenerateIgnoreProperty]
        public int MHPR { get; set; }
        [GenerateIgnoreProperty]
        public int MMPR { get; set; }
        [GenerateIgnoreProperty]
        public int MHPRate { get; set; }
        [GenerateIgnoreProperty]
        public int MMPRate { get; set; }
        [GenerateIgnoreProperty]
        public int PADRate { get; set; }
        [GenerateIgnoreProperty]
        public int MADRate { get; set; }
        [GenerateIgnoreProperty]
        public int PDDRate { get; set; }
        [GenerateIgnoreProperty]
        public int MDDRate { get; set; }
        [GenerateIgnoreProperty]
        public int ACCRate { get; set; }
        [GenerateIgnoreProperty]
        public int EVARate { get; set; }
        [GenerateIgnoreProperty]
        public int SpeedRate { get; set; }
        [GenerateIgnoreProperty]
        public int JumpRate { get; set; }
        [GenerateIgnoreProperty]
        public int ExpInc { get; set; }
        [GenerateIgnoreProperty]
        public MorphRandomData[]? MorphRandom { get; set; }
        [GenerateIgnoreProperty]
        public int ExpBuffRate { get; set; }
    }
}