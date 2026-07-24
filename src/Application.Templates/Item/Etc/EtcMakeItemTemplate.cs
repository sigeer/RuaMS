namespace Application.Templates.Item.Etc
{
    /// <summary>
    /// 425
    /// </summary>
    [GenerateTag]
    public class EtcMakeItemTemplate : EtcItemTemplate, IEnhanceEquipItem
    {
        public EtcMakeItemTemplate(int templateId) : base(templateId)
        {
        }
        [WZPath("info/randStat")]
        public int RandStat { get; set; }
        [WZPath("info/randOption")]
        public int RandOption { get; set; }
        [WZPath("info/incReqLevel")]
        public int IncReqLevel { get; set; }

        [WZPath("info/incSTR")]
        public int IncSTR { get; set; }

        [WZPath("info/incDEX")]
        public int IncDEX { get; set; }

        [WZPath("info/incINT")]
        public int IncINT { get; set; }

        [WZPath("info/incLUK")]
        public int IncLUK { get; set; }

        [WZPath("info/incPAD")]
        public int IncPAD { get; set; }

        /// <summary>
        /// 提升MaxHP
        /// </summary>
        [WZPath("info/incMaxHP")]
        public int IncMHP { get; set; }
        /// <summary>
        /// 提升MaxMP
        /// </summary>

        [WZPath("info/incMaxMP")]
        public int IncMMP { get; set; }

        [WZPath("info/incMAD")]
        public int IncMAD { get; set; }

        [WZPath("info/incPDD")]
        public int IncPDD { get; set; }

        [WZPath("info/incMDD")]
        public int IncMDD { get; set; }

        [WZPath("info/incACC")]
        public int IncACC { get; set; }

        [WZPath("info/incEVA")]
        public int IncEVA { get; set; }

        [WZPath("info/incSpeed")]
        public int IncSpeed { get; set; }

        [WZPath("info/incJump")]
        public int IncJump { get; set; }

    }
}
