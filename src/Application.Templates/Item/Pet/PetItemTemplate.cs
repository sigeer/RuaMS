using Application.Templates.Item.Cash;

namespace Application.Templates.Item.Pet
{
    [GenerateTag]
    public sealed class PetItemTemplate : CashItemTemplate
    {
        public override int SlotMax { get; set; } = 1;

        [WZPath("info/hungry")]
        public int Hungry { get; set; } = 1;
        [WZPath("info/permanent")]
        public bool Permanent { get; set; }
        [WZPath("info/life")]
        public int Life { get; set; }

        [WZPath("interact/-")]
        public PetInterActData[] InterActs
        {
            set
            {
                InterActsDict = value.ToDictionary(x => x.Id);
            }
        }
        [GenerateIgnoreProperty]
        public Dictionary<int, PetInterActData> InterActsDict { get; private set; }
        #region Evolve
        [WZPath("info/evol")]
        public bool CanEvol { get; set; }
        [WZPath("info/evolReqPetLvl")]
        public bool EvolRequireLevel { get; set; }
        [WZPath("info/evolReqItemID")]
        public bool EvolRequireItem { get; set; }
        [GenerateIgnoreProperty]
        public int[] Evols { get; set; }
        [GenerateIgnoreProperty]
        public int[] EvolProbs { get; set; }
        [WZPath("info/evol1")]
        public int Evol1 { get; set; }
        [WZPath("info/evolProb1")]
        public int EvolProb1 { get; set; }

        [WZPath("info/evol2")]
        public int Evol2 { get; set; }
        [WZPath("info/evolProb2")]
        public int EvolProb2 { get; set; }

        [WZPath("info/evol3")]
        public int Evol3 { get; set; }
        [WZPath("info/evolProb3")]
        public int EvolProb3 { get; set; }

        [WZPath("info/evol4")]
        public int Evol4 { get; set; }
        [WZPath("info/evolProb4")]
        public int EvolProb4 { get; set; }

        [WZPath("info/evol5")]
        public int Evol5 { get; set; }
        [WZPath("info/evolProb5")]
        public int EvolProb5 { get; set; }

        [WZPath("info/evol6")]
        public int Evol6 { get; set; }
        [WZPath("info/evolProb6")]
        public int EvolProb6 { get; set; }

        [WZPath("info/evol7")]
        public int Evol7 { get; set; }
        [WZPath("info/evolProb7")]
        public int EvolProb7 { get; set; }

        [WZPath("info/evol8")]
        public int Evol8 { get; set; }
        [WZPath("info/evolProb8")]
        public int EvolProb8 { get; set; }

        [WZPath("info/evol9")]
        public int Evol9 { get; set; }
        [WZPath("info/evolProb9")]
        public int EvolProb9 { get; set; }

        [WZPath("info/evol10")]
        public int Evol10 { get; set; }
        [WZPath("info/evolProb10")]
        public int EvolProb10 { get; set; }
        #endregion
        public PetItemTemplate(int templateId)
            : base(templateId)
        {
            InterActs = Array.Empty<PetInterActData>();
            InterActsDict = new Dictionary<int, PetInterActData>();

            Evols = [];
            EvolProbs = [];
        }

        public void Adjust()
        {
            Evols = new int[] { Evol1, Evol2, Evol3, Evol4, Evol5, Evol6, Evol7, Evol8, Evol9, Evol10 }.Where(x => x > 0).ToArray();
            EvolProbs = new int[] { EvolProb1, EvolProb2, EvolProb3, EvolProb4, EvolProb5, EvolProb6, EvolProb7, EvolProb8, EvolProb9, EvolProb10 }.Where(x => x > 0).ToArray();
        }
    }
}
