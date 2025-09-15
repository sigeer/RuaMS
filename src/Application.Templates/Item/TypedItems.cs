using Application.Templates.Item.Consume;

namespace Application.Templates.Item
{
    public interface IPackagedItem
    {
        public RewardData[] Reward { get; set; }
    }
    /// <summary>
    /// 触发StatEffect的道具
    /// </summary>
    public interface IEffectItem: ITemplate
    {

    }

    public interface IMorphPotionItem : IEffectItem
    {

        public int HP { get; set; }

        public int Morph { get; set; }
        public MorphRandomData[] MorphRandom { get; set; }
    }

    public interface IMesoUpEffect : IEffectItem
    {
        public bool MesoUpByItem { get; set; }
        public int Prob { get; set; }
    }

    public interface IItemUpEffect : IEffectItem
    {
        public int ItemUpByItem { get; set; }
        public int Prob { get; set; }
    }

    public interface IMapProtectEffect: IEffectItem
    {
        public int Thaw { get; set; }
    }
}
