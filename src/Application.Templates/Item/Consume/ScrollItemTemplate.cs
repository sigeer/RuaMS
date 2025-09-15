namespace Application.Templates.Item.Consume
{
    /// <summary>
    /// 204: 卷轴
    /// </summary>
    [GenerateTag]
    public sealed class ScrollItemTemplate : ConsumeItemTemplate
    {
        public ScrollItemTemplate(int templateId) : base(templateId)
        {
            Req = Array.Empty<int>();
        }

        [WZPath("info/success")]
        public int Success { get; set; }
        public double SuccessRate => Success / 100.0;
        /// <summary>
        /// 破坏几率
        /// </summary>
        [WZPath("info/cursed")]
        public int Cursed { get; set; }
        public double CursedRate => Cursed / 100.0;

        [WZPath("info/incMHP")]
        public int IncMHP { get; set; }

        [WZPath("info/incMMP")]
        public int IncMMP { get; set; }



        [WZPath("info/incMAD")]
        public int IncMAD { get; set; }

        [WZPath("info/incPDD")]
        public int IncPDD { get; set; }
        [WZPath("info/incPAD")]
        public int IncPAD { get; set; }

        [WZPath("info/incMDD")]
        public int IncMDD { get; set; }

        [WZPath("info/incACC")]
        public int IncACC { get; set; }

        [WZPath("info/incEVA")]
        public int IncEVA { get; set; }

        [WZPath("info/incINT")]
        public int IncINT { get; set; }

        [WZPath("info/incDEX")]
        public int IncDEX { get; set; }

        [WZPath("info/incSTR")]
        public int IncSTR { get; set; }

        [WZPath("info/incLUK")]
        public int IncLUK { get; set; }

        [WZPath("info/incSpeed")]
        public int IncSpeed { get; set; }

        [WZPath("info/incJump")]
        public int IncJump { get; set; }
        /// <summary>
        /// 添加防滑
        /// </summary>
        [WZPath("info/preventslip")]
        public bool PreventSlip { get; set; }
        /// <summary>
        /// 添加防寒
        /// </summary>
        [WZPath("info/warmsupport")]
        public bool WarmSupport { get; set; }
        /// <summary>
        /// 对特定道具生效
        /// </summary>
        [WZPath("req/-")]
        public int[] Req { get; set; }
    }
}
