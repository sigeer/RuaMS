namespace Application.EF.Entities;

public partial class SkillEntity
{
    public int Id { get; set; }

    public int Skillid { get; set; }

    public int Characterid { get; set; }

    public int Skilllevel { get; set; }

    public int Masterlevel { get; set; }

    public long Expiration { get; set; }

    public virtual CharacterEntity Character { get; set; } = null!;
}
