namespace Application.EF.Entities;

public partial class Macban
{
    public Macban(string mac, string? aid)
    {
        Mac = mac;
        Aid = aid;
    }
    private Macban() { }
    public int Macbanid { get; set; }

    public string Mac { get; set; } = null!;

    public string? Aid { get; set; }
}
