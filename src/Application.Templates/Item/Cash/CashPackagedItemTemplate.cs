using Application.Templates.StatEffectProps;

namespace Application.Templates.Item.Cash
{
    /// <summary>
    /// 553
    /// </summary>
    [GenerateTag]
    public class CashPackagedItemTemplate : CashItemTemplate, IStatEffectReward
    {
        public CashPackagedItemTemplate(int templateId) : base(templateId)
        {
            Reward = Array.Empty<RewardData>();
        }

        [WZPath("reward/-")]
        public RewardData[] Reward { get; set; }
    }
}
