namespace Application.Templates.StatEffectProps
{
    /// <summary>
    /// 回复效果
    /// </summary>
    public interface IStatEffectHeal : IStatEffectProp
    {
        public int HP { get; }

        public int MP { get; }

        public int HPR { get; }

        public int MPR { get; }
    }
}
