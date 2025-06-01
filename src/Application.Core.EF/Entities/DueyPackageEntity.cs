namespace Application.EF.Entities;

public partial class DueyPackageEntity
{
    public int PackageId { get; set; }

    public int ReceiverId { get; set; }
    public string SenderName { get; set; } = null!;

    public int Mesos { get; set; }

    public DateTimeOffset TimeStamp { get; set; }

    public string? Message { get; set; }

    public bool Checked { get; set; } = true;

    public bool Type { get; set; } = false;
    private DueyPackageEntity() { }

    public DueyPackageEntity(int receiverId, string senderName, int mesos, string? message, bool @checked, bool type, DateTimeOffset createTime)
    {
        ReceiverId = receiverId;
        SenderName = senderName;
        Mesos = mesos;
        TimeStamp = createTime;
        Message = message;
        Checked = @checked;
        Type = type;

        UpdateSentTime();
    }


    public virtual ICollection<Dueyitem> Dueyitems { get; set; } = new List<Dueyitem>();


    public void UpdateSentTime()
    {
        DateTimeOffset cal = TimeStamp;

        if (Type)
        {
            if (DateTimeOffset.UtcNow - TimeStamp < TimeSpan.FromDays(1))
            {
                // thanks inhyuk for noticing quick delivery packages unavailable to retrieve from the get-go
                cal.AddDays(-1);
            }
        }

        this.TimeStamp = cal;
    }
}
