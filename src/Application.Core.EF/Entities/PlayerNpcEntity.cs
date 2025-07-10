namespace Application.EF.Entities;

public partial class PlayerNpcEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Hair { get; set; }

    public int Face { get; set; }

    public int Skin { get; set; }

    public int Gender { get; set; }

    public int X { get; set; }

    public int Cy { get; set; }

    public int World { get; set; }

    public int Map { get; set; }

    public int Dir { get; set; }

    public int Scriptid { get; set; }

    public int Fh { get; set; }

    public int Rx0 { get; set; }

    public int Rx1 { get; set; }

    public int Worldrank { get; set; }

    public int Overallrank { get; set; }

    public int Worldjobrank { get; set; }

    public int Job { get; set; }
}
