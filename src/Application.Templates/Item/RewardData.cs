namespace Application.Templates.Item
{
    public sealed class RewardData
    {
        [WZPath("reward/-/item")]
        public int ItemID { get; set; }
        public int Count { get; set; }
        public int Prob { get; set; }
        public int Period { get; set; } = -1;
        public string Effect { get; set; } = string.Empty;
        [WZPath("reward/-/worldMsg")]
        public string? WorldMessage { get; set; }
    }
}