namespace Application.EF.Entities;

public partial class Ipban
{
    public int Ipbanid { get; set; }

    public string Ip { get; set; } = null!;

    public string? Aid { get; set; }
}
