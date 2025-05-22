namespace Application.Shared.Dto
{
    public class PlayerBuffSaveDto
    {
        public BuffDto[] Buffs { get; set; } = [];
        public DiseaseDto[] Diseases { get; set; } = [];
    }

    public class BuffDto
    {
        public int UsedTime { get; set; }
        public int SourceId { get; set; }
        public bool IsSkill { get; set; }
        public int SkillLevel { get; set; }
    }
    public class DiseaseDto
    {
        public long LeftTime { get; set; }
        public int MobSkillId { get; set; }
        public int MobSkillLevel { get; set; }
        public int DiseaseOrdinal { get; set; }
    }
}
