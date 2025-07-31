namespace Application.EF.Entities;

public partial class MacbanEntity
{
    public MacbanEntity(string mac, int aid)
    {
        Mac = mac;
        Aid = aid;
    }
    private MacbanEntity() { }
    public int Macbanid { get; set; }

    public string Mac { get; set; } = null!;

    public int Aid { get; set; }
}
