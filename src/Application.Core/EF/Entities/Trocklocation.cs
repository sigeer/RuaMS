namespace Application.EF.Entities;

public partial class Trocklocation
{
    private Trocklocation()
    {
    }

    public Trocklocation(int characterid, int mapid, int vip)
    {
        Characterid = characterid;
        Mapid = mapid;
        Vip = vip;
    }

    public int Trockid { get; set; }

    public int Characterid { get; set; }

    public int Mapid { get; set; }

    public int Vip { get; set; }
}
