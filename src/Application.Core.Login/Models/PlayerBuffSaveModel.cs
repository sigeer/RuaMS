namespace Application.Core.Login.Models
{
    public class PlayerBuffSaveModel
    {
        public BuffModel[] Buffs { get; set; } = [];
        public DiseaseModel[] Diseases { get; set; } = [];
    }

    public class BuffModel
    {
        public long StartTime { get; set; }
        public int SourceId { get; set; }
        public int SkillLevel { get; set; }
        public BuffStatValueModel[] Stats { get; set; }
    }

    public record BuffStatValueModel
    {
        public string BuffStat { get; set; }
        public int Value { get; set; }
    }
    public class DiseaseModel
    {
        public long StartTime { get; set; }
        public long Length { get; set; }
        public int MobSkillId { get; set; }
        public int MobSkillLevel { get; set; }
        public int DiseaseOrdinal { get; set; }
    }
}
