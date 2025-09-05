namespace Application.Templates.Character
{
    public sealed class EquipTemplate : AbstractItemTemplate
    {
        public override int SlotMax { get; set; } = 1;
        /// <summary>
        /// Total upgrade count
        /// </summary>
        [WZPath("info/tuc")]
        public int TUC { get; set; }

        [WZPath("info/reqLevel")]
        public int ReqLevel { get; set; }

        [WZPath("info/incSTR")]
        public int incSTR { get; set; }

        [WZPath("info/incDEX")]
        public int incDEX { get; set; }

        [WZPath("info/incINT")]
        public int incINT { get; set; }

        [WZPath("info/incLUK")]
        public int incLUK { get; set; }

        [WZPath("info/incMHP")]
        public int incMHP { get; set; }

        [WZPath("info/incMMP")]
        public int incMMP { get; set; }

        [WZPath("info/incPAD")]
        public int incPAD { get; set; }

        [WZPath("info/incMAD")]
        public int incMAD { get; set; }

        [WZPath("info/incPDD")]
        public int incPDD { get; set; }

        [WZPath("info/incMDD")]
        public int incMDD { get; set; }

        [WZPath("info/incACC")]
        public int incACC { get; set; }

        [WZPath("info/incEVA")]
        public int incEVA { get; set; }

        [WZPath("info/incCraft")]
        public int incCraft { get; set; }

        [WZPath("info/incSpeed")]
        public int incSpeed { get; set; }

        [WZPath("info/incJump")]
        public int incJump { get; set; }

        [WZPath("info/equipTradeBlock")]
        public bool EquipTradeBlock { get; set; }

        [WZPath("info/notExtend")]
        public bool NotExtend { get; set; }
        public int ReqJob { get; set; }
        public int ReqSTR { get; set; }
        public int ReqDEX { get; set; }
        public int ReqINT { get; set; }
        public int ReqLUK { get; set; }

        public EquipLevelData[] LevelData { get; set; }
        public EquipTemplate(int templateId)
            : base(templateId)
        {
            LevelData = Array.Empty<EquipLevelData>();
        }
    }
}
