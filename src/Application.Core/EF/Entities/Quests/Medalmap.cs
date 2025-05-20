namespace Application.EF.Entities;

public partial class Medalmap
{
    private Medalmap() { }
    public Medalmap(int characterid, int queststatusid, int mapid)
    {
        Characterid = characterid;
        Queststatusid = queststatusid;
        Mapid = mapid;
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public int Queststatusid { get; set; }

    public int Mapid { get; set; }
}
