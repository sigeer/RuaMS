namespace Application.Templates
{
    public interface IEffectTemplate
    {
    }

    /// <summary>
    /// 治疗类
    /// </summary>
    public interface IRecoverItemTemplate
    {
        int HP { get; set; }
        int MP { get; set; }
        int HPRate { get; set; }
        int MPRate { get; set; }
    }

    /// <summary>
    /// 解除debuff类
    /// </summary>
    public interface IDispelDebuffTemplate
    {
        [WZPath("spec/seal")]
        public bool Cure_Seal { get; set; }

        [WZPath("spec/curse")]
        public bool Cure_Curse { get; set; }

        [WZPath("spec/poison")]
        public bool Cure_Poison { get; set; }

        [WZPath("spec/weakness")]
        public bool Cure_Weakness { get; set; }

        [WZPath("spec/darkness")]
        public bool Cure_Darkness { get; set; }
    }

    /// <summary>
    /// 增益状态类
    /// </summary>
    public interface IBuffTemplate
    {
        int Time { get; set; }
        int PAD { get; set; }
        int PDD { get; set; }
        int MAD { get; set; }
        int MDD { get; set; }
    }
    /// <summary>
    /// 回城卷轴
    /// </summary>
    public interface ITownScrollItemTemplate : IEffectTemplate
    {
        int MoveTo { get; set; }
    }
}
