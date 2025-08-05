namespace Application.EF.Entities;

public partial class FamilyCharacterEntity
{
    public FamilyCharacterEntity(int cid, int familyid, int seniorid)
    {
        Cid = cid;
        Familyid = familyid;
        Seniorid = seniorid;
    }

    protected FamilyCharacterEntity()
    {
    }

    public int Cid { get; set; }

    public int Familyid { get; set; }

    public int Seniorid { get; set; }

    public int Reputation { get; set; }

    public int Todaysrep { get; set; }

    public int Totalreputation { get; set; }

    public int Reptosenior { get; set; }

    public string? Precepts { get; set; }

    public long Lastresettime { get; set; }

    public virtual CharacterEntity CidNavigation { get; set; } = null!;
}
