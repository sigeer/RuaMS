namespace Application.EF.Entities;

public partial class Savedlocation
{
    private Savedlocation()
    {
    }

    public Savedlocation(int map, int portal)
    {
        Map = map;
        Portal = portal;
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public string Locationtype { get; set; } = null!;

    public int Map { get; set; }

    public int Portal { get; set; }
}
