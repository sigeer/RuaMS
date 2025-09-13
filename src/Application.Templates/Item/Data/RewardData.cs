namespace Application.Templates.Item.Data
{
    public sealed class RewardData
    {
        [WZPath("reward/-/item")]
        public int ItemID { get; set; }
        public int Count { get; set; }
        public int Prob { get; set; }
        public int Period { get; set; }
        public string Effect { get; set; }
    }
}