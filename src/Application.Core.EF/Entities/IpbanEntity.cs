namespace Application.EF.Entities;

public partial class IpbanEntity
{
    private IpbanEntity() { }
    public IpbanEntity(string ip, int aid)
    {
        Ip = ip;
        Aid = aid;
    }

    public int Ipbanid { get; set; }

    public string Ip { get; set; } = null!;

    public int Aid { get; set; }
}
