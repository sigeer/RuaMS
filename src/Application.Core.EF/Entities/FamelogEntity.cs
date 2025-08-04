
namespace Application.EF.Entities;

public partial class FamelogEntity
{
    private FamelogEntity() { }
    public FamelogEntity(int characterid, int characteridTo, DateTimeOffset when)
    {
        Characterid = characterid;
        CharacteridTo = characteridTo;
        When = when;
    }

    public int Famelogid { get; set; }

    public int Characterid { get; set; }

    public int CharacteridTo { get; set; }

    public DateTimeOffset When { get; set; }

    public virtual CharacterEntity Character { get; set; } = null!;
}
