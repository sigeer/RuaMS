namespace Application.Templates.StatEffectProps
{
    /// <summary>
    /// 提升属性
    /// </summary>
    public interface IStatEffectPower : IStatEffectProp
    {
        /// <summary>
        /// 提升最大HP
        /// </summary>
        public int MHPR { get; }
        /// <summary>
        /// 提升最大MP
        /// </summary>
        public int MMPR { get; }

        public int MHPRate { get; }

        public int MMPRate { get; }

        public int PAD { get; }

        public int MAD { get; }
        public int PDD { get; }
        public int MDD { get; }
        public int PADRate { get; }
        public int MADRate { get; }

        public int PDDRate { get; }

        public int MDDRate { get; }
        public int ACC { get; }
        public int ACCRate { get; }

        public int EVA { get; }
        public int EVARate { get; }

        public int Speed { get; }
        public int SpeedRate { get; }

        public int Jump { get; }
        public int JumpRate { get; }
        public int ExpBuffRate { get; }

    }
}
