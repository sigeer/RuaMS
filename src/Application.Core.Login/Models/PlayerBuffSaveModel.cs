namespace Application.Core.Login.Models
{
    public class PlayerBuffSaveModel
    {
        public BuffModel[] Buffs { get; set; } = [];
        public DiseaseModel[] Diseases { get; set; } = [];
    }

    public class BuffModel
    {
        public int UsedTime { get; set; }
        public int SourceId { get; set; }
        public bool IsSkill { get; set; }
        public int SkillLevel { get; set; }
    }
    public class DiseaseModel
    {
        public long LeftTime { get; set; }
        public int MobSkillId { get; set; }
        public int MobSkillLevel { get; set; }
        public int DiseaseOrdinal { get; set; }
    }
}
