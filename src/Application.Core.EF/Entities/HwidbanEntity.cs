namespace Application.EF.Entities;

public partial class HwidbanEntity

{
    private HwidbanEntity() { }
    public HwidbanEntity(string hwid, int accountId)
    {
        Hwid = hwid;
        AccountId = accountId;
    }

    public int Hwidbanid { get; set; }

    public string Hwid { get; set; } = null!;
    public int AccountId { get; set; }
}
