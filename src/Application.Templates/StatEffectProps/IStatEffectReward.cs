using Application.Templates.Item;

namespace Application.Templates.StatEffectProps
{
    public interface IStatEffectReward : IStatEffectProp
    {
        public RewardData[] Reward { get; }
    }
}
