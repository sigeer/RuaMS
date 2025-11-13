namespace Application.Templates.Item
{
    public sealed class RewardData
    {
        [WZPath("reward/-/item")]
        public int ItemID { get; set; }
        public int Count { get; set; }
        public int Prob { get; set; }
        public int Period { get; set; }
        [WZPath("reward/-/Effect")]
        public string? Effect { get; set; }
        [WZPath("reward/-/worldMsg")]
        public string? WorldMessage { get; set; }
    }
}