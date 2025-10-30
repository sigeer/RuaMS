using Application.Templates.Item.Consume;

namespace Application.Templates.StatEffectProps
{
    public interface IStatEffectMorph : IStatEffectProp
    {
        public int HP { get; }

        public int Morph { get; }
        public MorphRandomData[]? MorphRandom { get; }
    }
}
