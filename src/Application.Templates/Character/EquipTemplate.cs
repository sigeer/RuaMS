namespace Application.Templates.Character
{
    [GenerateTag]
    public sealed class EquipTemplate : AbstractItemTemplate
    {
        /// <summary>
        /// 装备位置
        /// </summary>
        [WZPath("info/islot")]
        public string? Islot { get; set; }
        /// <summary>
        /// 可升级次数
        /// </summary>
        [WZPath("info/tuc")]
        public int TUC { get; set; }

        [WZPath("info/reqLevel")]
        public int ReqLevel { get; set; }

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
        [WZPath("info/incMHP")]
        public int IncMHP { get; set; }
        /// <summary>
        /// 提升MaxMP
        /// </summary>

        [WZPath("info/incMMP")]
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

        [WZPath("info/incCraft")]
        public int incCraft { get; set; }

        [WZPath("info/incSpeed")]
        public int IncSpeed { get; set; }

        [WZPath("info/incJump")]
        public int IncJump { get; set; }
        /// <summary>
        /// 装备后不可交易
        /// </summary>

        [WZPath("info/equipTradeBlock")]
        public bool EquipTradeBlock { get; set; }

        [WZPath("info/reqJob")]
        public int ReqJob { get; set; }
        [WZPath("info/reqSTR")]
        public int ReqSTR { get; set; }
        [WZPath("info/reqDEX")]
        public int ReqDEX { get; set; }
        [WZPath("info/reqINT")]
        public int ReqINT { get; set; }
        [WZPath("info/reqLUK")]
        public int ReqLUK { get; set; }
        [WZPath("info/reqPOP")]
        public int ReqPOP { get; set; }
        /// <summary>
        /// 不明
        /// </summary>
        [WZPath("info/fs")]
        public int Fs { get; set; }
        [WZPath("info/level/info/-")]
        public EquipLevelData[] LevelData { get; set; }
        [WZPath("info/replace")]
        public ReplaceItemTemplate? ReplaceItem { get; set; }
        /// <summary>
        /// 等级数据（LevelData）中有增加属性（有不止Exp的其他项存在）
        /// </summary>
        [GenerateIgnoreProperty]
        public bool IsElemental { get; set; }
        public EquipTemplate(int templateId)
            : base(templateId)
        {
            LevelData = Array.Empty<EquipLevelData>();
        }
    }
}
