namespace Application.EF.Entities;

public partial class AreaInfo
{
    private AreaInfo()
    {
    }

    public AreaInfo(int charid, int area, string info)
    {
        Charid = charid;
        Area = area;
        Info = info;
    }

    public int Id { get; set; }

    public int Charid { get; set; }

    public int Area { get; set; }

    public string Info { get; set; } = null!;
}
