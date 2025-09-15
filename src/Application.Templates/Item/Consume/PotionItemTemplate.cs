namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 治疗、解除debuff、附加增益buff
    /// </summary>
    [GenerateTag]
    public class PotionItemTemplate : ConsumeItemTemplate, IPackagedItem, IMesoUpEffect, IItemUpEffect
    {
        public PotionItemTemplate(int templateId) : base(templateId)
        {
            Reward = Array.Empty<RewardData>();
        }

        [WZPath("reward/-")]
        public RewardData[] Reward { get; set; }

        [WZPath("spec/hp")]
        public int HP { get; set; }

        [WZPath("spec/mp")]
        public int MP { get; set; }

        [WZPath("spec/hpR")]
        public int HPR { get; set; }

        [WZPath("spec/mpR")]
        public int MPR { get; set; }
        /// <summary>
        /// 提升最大HP
        /// </summary>

        [WZPath("spec/mhpR")]
        public int MHPR { get; set; }
        /// <summary>
        /// 提升最大MP
        /// </summary>

        [WZPath("spec/mmpR")]
        public int MMPR { get; set; }

        [WZPath("spec/mhpRRate")]
        public int MHPRate { get; set; }

        [WZPath("spec/mmpRRate")]
        public int MMPRate { get; set; }

        [WZPath("spec/pad")]
        public int PAD { get; set; }

        [WZPath("spec/mad")]
        public int MAD { get; set; }

        [WZPath("spec/pdd")]
        public int PDD { get; set; }

        [WZPath("spec/mdd")]
        public int MDD { get; set; }

        [WZPath("spec/padRate")]
        public int PADRate { get; set; }

        [WZPath("spec/madRate")]
        public int MADRate { get; set; }

        [WZPath("spec/pddRate")]
        public int PDDRate { get; set; }

        [WZPath("spec/mddRate")]
        public int MDDRate { get; set; }

        [WZPath("spec/acc")]
        public int ACC { get; set; }
        [WZPath("spec/accRate")]
        public int ACCRate { get; set; }

        [WZPath("spec/eva")]
        public int EVA { get; set; }
        [WZPath("spec/evaRate")]
        public int EVARate { get; set; }

        [WZPath("spec/speed")]
        public int Speed { get; set; }
        [WZPath("spec/speedRate")]
        public int SpeedRate { get; set; }

        [WZPath("spec/jump")]
        public int Jump { get; set; }
        [WZPath("spec/jumpRate")]
        public int JumpRate { get; set; }


        [WZPath("spec/seal")]
        public bool Cure_Seal { get; set; }

        [WZPath("spec/curse")]
        public bool Cure_Curse { get; set; }

        [WZPath("spec/poison")]
        public bool Cure_Poison { get; set; }

        [WZPath("spec/weakness")]
        public bool Cure_Weakness { get; set; }

        [WZPath("spec/darkness")]
        public bool Cure_Darkness { get; set; }

        [WZPath("spec/mesoupbyitem")]
        public bool MesoUpByItem { get; set; }

        [WZPath("spec/itemupbyitem")]
        public int ItemUpByItem { get; set; }
        [WZPath("spec/prob")]
        public int Prob { get; set; } = 1;

        [WZPath("spec/expBuff")]
        public int ExpBuffRate { get; set; }

        #region MC 里的道具？
        [WZPath("spec/cp")]
        public int CP { get; set; }
        [WZPath("spec/nuffSkill")]
        public int CPSkill { get; set; } // nuffSkill -> exists in Skill.wz
        #endregion

        [WZPath("spec/incFatigue")]
        public int IncFatigue { get; set; }


        //[WZPath("spec/repeatEffect")]
        //public bool RepeatEffect { get; set; }
        [WZPath("spec/barrier")]
        public int Barrier { get; set; }
        [WZPath("spec/booster")]
        public int Booster { get; set; }
        [WZPath("spec/berserk")]
        public int Berserk { get; set; }

        [WZPath("specEx/0")]
        public MobSkillEffectData? MobSkill { get; set; }

        [WZPath("spec/thaw")]
        public int Thaw { get; set; }
    }

    public sealed class MorphRandomData
    {
        public int Morph { get; set; }
        public int Prob { get; set; }
    }

    public sealed class MobSkillEffectData
    {
        public int MobSkill { get; set; }
        public int Level { get; set; }
        public int Target { set; get; }
    }
}
