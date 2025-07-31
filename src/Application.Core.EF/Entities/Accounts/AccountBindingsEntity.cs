namespace Application.EF.Entities;

public class AccountBindingsEntity
{
    protected AccountBindingsEntity() { }
    public AccountBindingsEntity(int id, int accountId, string iP, string mAC, string hWID, DateTimeOffset lastActiveTime)
    {
        Id = id;
        AccountId = accountId;
        IP = iP;
        MAC = mAC;
        HWID = hWID;
        LastActiveTime = lastActiveTime;
    }

    public int Id { get; set; }
    public int AccountId { get; set; }
    public string IP { get; set; } = null!;
    public string MAC { get; set; } = null!;
    public string HWID { get; set; } = null!;
    public DateTimeOffset LastActiveTime { get; set; }
}
