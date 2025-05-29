namespace Application.EF.Entities;

public partial class PlayernpcsField
{
    public int Id { get; set; }

    public int World { get; set; }

    public int Map { get; set; }

    public sbyte Step { get; set; }

    public short Podium { get; set; }
}
