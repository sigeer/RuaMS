namespace Application.EF.Entities;

public partial class ReactorDropEntity
{
    private ReactorDropEntity() { }
    public ReactorDropEntity(int reactorid, int itemid, int chance, int questid)
    {
        Reactorid = reactorid;
        Itemid = itemid;
        Chance = chance;
        Questid = questid;
    }

    public int Reactordropid { get; set; }

    public int Reactorid { get; set; }

    public int Itemid { get; set; }

    public int Chance { get; set; }

    public int Questid { get; set; }
}
