using Application.Templates.Item.Data;

namespace Application.Templates.Item.Consume
{
    public class ConsumeItemTemplate : AbstractItemTemplate
    {

        [WZPath("info/monsterBook")]
        public bool MonsterBook { get; set; }

        [WZPath("specEx/consumeOnPickup")]
        [WZPath("spec/consumeOnPickup")]
        public bool ConsumeOnPickup { get; set; }
        [WZPath("info/noCancelMouse")]
        public bool NoCancelMouse { get; set; }


        [WZPath("spec/expinc")]
        public int ExpInc { get; set; }


        [WZPath("spec/prob")]
        public int Prob { get; set; }

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

        [WZPath("reward/-")]
        public RewardData[] Reward { get; set; } // TODO hook this up to a provider

        public ConsumeItemTemplate(int templateId) : base(templateId)
        {
            Reward = Array.Empty<RewardData>();
        }
    }
}