namespace Application.EF.Entities;

public partial class Questaction
{
    public int Questactionid { get; set; }

    public int Questid { get; set; }

    public int Status { get; set; }

    public byte[] Data { get; set; } = null!;
}
