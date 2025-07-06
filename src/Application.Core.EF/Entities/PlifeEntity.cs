namespace Application.EF.Entities;

public partial class PlifeEntity
{
    protected PlifeEntity()
    {
    }

    public PlifeEntity(int mapId, int mobId, int mobTime, int x, int y, int fh, string type)
    {
        Life = mobId;
        F = 0;
        Fh = fh;
        Cy = y;
        Rx0 = x + 50;
        Rx1 = x - 50;
        Type = type;
        X = x;
        Y = y;
        Map = mapId;
        Mobtime = mobTime;
        Hide = 0;
    }

    public int Id { get; set; }

    public int World { get; set; }

    public int Map { get; set; }

    public int Life { get; set; }

    public string Type { get; set; } = null!;

    public int Cy { get; set; }

    public int F { get; set; }

    public int Fh { get; set; }

    public int Rx0 { get; set; }

    public int Rx1 { get; set; }

    public int X { get; set; }

    public int Y { get; set; }

    public int Hide { get; set; }

    public int Mobtime { get; set; }

    public int Team { get; set; }
}
