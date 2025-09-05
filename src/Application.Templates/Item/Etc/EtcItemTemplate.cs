namespace Application.Templates.Item.Etc
{
    public sealed class EtcItemTemplate : AbstractItemTemplate
    {
        [WZPath("info/lv")]
        public int lv { get; set; }

        [WZPath("info/exp")]
        public int Exp { get; set; }

        [WZPath("info/questId")]
        public int QuestID { get; set; }

        [WZPath("info/grade")]
        public int Grade { get; set; }

        [WZPath("info/consumeItem/consumeCount")]
        public int ConsumeCount { get; set; }

        [WZPath("info/consumeItem/consumeCountMessage")]
        public string ConsumeMessage { get; set; }

        [WZPath("info/pquest")]
        public bool PQuest { get; set; }

        [WZPath("info/pickUpBlock")]
        public bool PickupBlock { get; set; }

        [WZPath("info/hybrid")]
        public bool Hybrid { get; set; }

        [WZPath("info/shopCoin")]
        public bool ShopCoin { get; set; }

        [WZPath("info/bigSize")]
        public bool BigSize { get; set; }

        public int[] ConsumeItem { get; set; }
        public int[] ConsumeItemExpGain { get; set; }

        public EtcItemTemplate(int templateId) : base(templateId)
        {
            ConsumeItem = new int[0];
            ConsumeItemExpGain = new int[0];
            ConsumeMessage = "";
        }
    }
}