using Application.Templates.Item.Data;

namespace Application.Templates.Item.Consume
{
    public class ConsumeItemTemplate : AbstractItemTemplate
    {
        /// <summary>
        /// Used to determine the cost of rechargeable items.
        /// </summary>
        [WZPath("info/unitPrice")]
        public double UnitPrice { get; set; }

        [WZPath("info/masterLevel")]
        public int MasterLevel { get; set; }

        [WZPath("info/reqSkillLevel")]
        public int ReqSkillLevel { get; set; }

        [WZPath("info/monsterBook")]
        public bool MonsterBook { get; set; }

        [WZPath("specEx/consumeOnPickup, spec/consumeOnPickup")]
        public bool ConsumeOnPickup { get; set; }
        [WZPath("info/noCancelMouse")]
        public bool NoCancelMouse { get; set; }

        /// <summary>
        /// Battlefield skill - for sheep v wolf event
        /// Default value: -1
        /// </summary>
        [WZPath("spec/BFSkill")]
        public int BFSkill { get; set; }

        [WZPath("spec/dojangshield")]
        public int DojangShield { get; set; }

        [WZPath("info/inc")]
        public int PetfoodInc { get; set; }

        [WZPath("spec/mesoupbyitem")]
        public bool MesoUpByItem { get; set; }

        [WZPath("spec/itemupbyitem")]
        public bool ItemUpByItem { get; set; }

        [WZPath("spec/expBuff")]
        public bool ExpUpByItem { get; set; }

        [WZPath("spec/expBuff")]
        public int ExpBuffRate { get; set; } // exp rate modifiers use this, meso/drop use prob isntead

        [WZPath("spec/hp")]
        public int HP { get; set; }

        [WZPath("spec/mp")]
        public int MP { get; set; }

        [WZPath("spec/hpR")]
        public int HPR { get; set; }

        [WZPath("spec/mpR")]
        public int MPR { get; set; }


        [WZPath("spec/mhpR")]
        public int MHPR { get; set; }

        [WZPath("spec/mmpR")]
        public int MMPR { get; set; }

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

        [WZPath("spec/eva")]
        public int EVA { get; set; }

        [WZPath("spec/accR")]
        public int ACCRate { get; set; }

        [WZPath("spec/evaR")]
        public int EVARate { get; set; }

        [WZPath("spec/speed")]
        public int Speed { get; set; }

        [WZPath("spec/jump")]
        public int Jump { get; set; }

        [WZPath("spec/speedRate")]
        public int SpeedRate { get; set; }

        [WZPath("spec/jumpRate")]
        public int JumpRate { get; set; }

        [WZPath("spec/morph")]
        public int Morph { get; set; }

        [WZPath("spec/expinc")]
        public int ExpInc { get; set; }

        [WZPath("spec/moveTo")]
        public int MoveTo { get; set; }

        [WZPath("spec/ignoreContinent")]
        public bool IgnoreContinent { get; set; }

        [WZPath("spec/prob")]
        public int Prob { get; set; }

        [WZPath("spec/cp")]
        public int CP { get; set; }

        [WZPath("spec/nuffSkill")]
        public int CPSkill { get; set; } // nuffSkill -> exists in Skill.wz

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

        [WZPath("info/cursed")]
        public int CursedRate { get; set; }

        [WZPath("info/success")]
        public int SuccessRate { get; set; }

        [WZPath("info/incMHP")]
        public int IncMHP { get; set; }

        [WZPath("info/incMMP")]
        public int IncMMP { get; set; }

        [WZPath("info/incPAD")]
        public int IncPAD { get; set; }

        [WZPath("info/incMAD")]
        public int IncMAD { get; set; }

        [WZPath("info/incPDD")]
        public int IncPDD { get; set; }

        [WZPath("info/incMDD")]
        public int IncMDD { get; set; }

        [WZPath("info/incACC")]
        public int IncACC { get; set; }

        [WZPath("info/incEVA")]
        public int IncEVA { get; set; }

        [WZPath("info/incINT")]
        public int IncINT { get; set; }

        [WZPath("info/incDEX")]
        public int IncDEX { get; set; }

        [WZPath("info/incSTR")]
        public int IncSTR { get; set; }

        [WZPath("info/incLUK")]
        public int IncLUK { get; set; }

        [WZPath("info/incSpeed")]
        public int IncSpeed { get; set; }

        [WZPath("info/incJump")]
        public int IncJump { get; set; }

        [WZPath("info/preventslip")]
        public bool PreventSlip { get; set; }

        [WZPath("info/warmsupport")]
        public bool WarmSupport { get; set; }

        [WZPath("info/incCraft")]
        public int IncCraft { get; set; }

        [WZPath("info/recover")]
        public int Recover { get; set; }

        [WZPath("info/randstat")]
        public bool RandStat { get; set; }

        [WZPath("info/incRandVol")]
        public int IncRandVol { get; set; }

        [WZPath("info/type")]
        public int InfoType { get; set; }

        [WZPath("spec/script")]
        public string ItemScript { get; set; }





        // these are for mastery books
        public int[] SkillData { get; set; }
        public int[] PetfoodPets { get; set; }
        public int[] SummoningSackIDs { get; set; }
        public int[] SummoningSackProbs { get; set; }
        public RewardData[] Reward { get; set; } // TODO hook this up to a provider

        public bool ScrollDestroy(Random rand)
            => CursedRate > 0 && rand.Next(100) < CursedRate;
        public bool ScrollSuccess(Random rand)
            => SuccessRate <= 0 || rand.Next(100) < SuccessRate;

        public ConsumeItemTemplate(int templateId) : base(templateId)
        {
            SkillData = new int[0];
            PetfoodPets = new int[0];
            SummoningSackIDs = new int[0];
            SummoningSackProbs = new int[0];
            Reward = new RewardData[0];
            ItemScript = "";
        }
    }
}