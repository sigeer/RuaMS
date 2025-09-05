namespace Application.Templates.Item.Data
{
    public sealed class RewardData
    {
        public int ItemID { get; set; }
        public int Count { get; set; }
        public int Prob { get; set; }
        public int Period { get; set; }
        public string Effect { get; set; }

        public RewardData(int itemid, int count, int prob, int period, string effect = "")
        {
            ItemID = itemid;
            Count = count;
            Prob = prob;
            Period = period;
            Effect = effect;
        }
    }
}