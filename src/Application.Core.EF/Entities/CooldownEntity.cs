namespace Application.EF.Entities;

public partial class CooldownEntity
{
    private CooldownEntity()
    {
    }

    public CooldownEntity(int charid, int skillId, long length, long startTime)
    {
        Charid = charid;
        SkillId = skillId;
        Length = length;
        StartTime = startTime;
    }

    public int Id { get; set; }

    public int Charid { get; set; }

    public int SkillId { get; set; }

    public long Length { get; set; }

    public long StartTime { get; set; }
}
