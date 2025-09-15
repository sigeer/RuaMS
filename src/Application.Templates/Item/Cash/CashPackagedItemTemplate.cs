namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 553
    /// </summary>
    [GenerateTag]
    public class CashPackagedItemTemplate : CashItemTemplate, IPackagedItem
    {
        public CashPackagedItemTemplate(int templateId) : base(templateId)
        {
            Reward = Array.Empty<RewardData>();
        }

        [WZPath("reward/-")]
        public RewardData[] Reward { get; set; }
    }
}
