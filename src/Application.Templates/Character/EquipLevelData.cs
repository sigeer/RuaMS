namespace Application.Templates.Character
{
    public sealed class EquipLevelData
    {
        public EquipLevelData(int level)
        {
            Level = level;
        }

        public int Level { get; set; }
        public int Exp { get; set; }
        public int IncSTRMin { get; set; }
        public int IncDEXMin { get; set; }
        public int IncINTMin { get; set; }
        public int IncLUKMin { get; set; }
        public int IncMHPMin { get; set; }
        public int IncMMPMin { get; set; }
        public int IncPADMin { get; set; }
        public int IncPDDMin { get; set; }
        public int IncMADMin { get; set; }
        public int IncMDDMin { get; set; }
        public int IncACCMin { get; set; }
        public int IncEVAMin { get; set; }
        public int IncSpeedMin { get; set; }
        public int IncJumpMin { get; set; }

        public int IncSTRMax { get; set; }
        public int IncDEXMax { get; set; }
        public int IncINTMax { get; set; }
        public int IncLUKMax { get; set; }
        public int IncMHPMax { get; set; }
        public int IncMMPMax { get; set; }
        public int IncPADMax { get; set; }
        public int IncPDDMax { get; set; }
        public int IncMADMax { get; set; }
        public int IncMDDMax { get; set; }
        public int IncACCMax { get; set; }
        public int IncEVAMax { get; set; }
        public int IncSpeedMax { get; set; }
        public int IncJumpMax { get; set; }
    }
}
