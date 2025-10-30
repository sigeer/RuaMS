namespace Application.Templates.StatEffectProps
{
    /// <summary>
    /// 解除异常状态
    /// </summary>
    public interface IStatEffectCure : IStatEffectProp
    {
        public bool Cure_Seal { get; }

        public bool Cure_Curse { get; }

        public bool Cure_Poison { get; }

        public bool Cure_Weakness { get; }

        public bool Cure_Darkness { get; }
    }
}
