namespace Application.Templates.StatEffectProps
{
    public interface IStatEffectSource
    {
        public int SourceId { get; }
    }
    public interface IStatEffectProp
    {
        public int Time { get; }
    }

    public interface ISkillStatEffectProp : IStatEffectProp
    {
        public int Level { get; }
    }

    public interface IItemStatEffectProp : IStatEffectProp, IStatEffectSource
    {

    }
}
