using Application.Core.EF;

namespace Application.EF.Entities;

public partial class DueyPackageEntity: IKeyedEntity<int>
{
    public int Id { get; set; }

    public int ReceiverId { get; set; }
    public int SenderId { get; set; }

    public int Mesos { get; set; }

    public DateTimeOffset CreateTime { get; set; }

    public string? Message { get; set; }

    public bool HasNotified { get; set; } = false;

    public bool Type { get; set; } = false;
    public byte[]? ItemBlob { get; set; }
    public DateTimeOffset? ClaimTime { get; set; }

    public DueyPackageEntity()
    {
    }

    public DueyPackageEntity(int id, int receiverId, int senderId, int mesos, string? message, bool @checked, bool type, DateTimeOffset createTime) : this()
    {
        Id = id;
        ReceiverId = receiverId;
        SenderId = senderId;
        Mesos = mesos;
        CreateTime = createTime;
        Message = message;
        HasNotified = @checked;
        Type = type;
    }


    //public void UpdateSentTime()
    //{
    //    DateTimeOffset cal = TimeStamp;

    //    if (Type)
    //    {
    //        if (DateTimeOffset.UtcNow - TimeStamp < TimeSpan.FromDays(1))
    //        {
    //            // thanks inhyuk for noticing quick delivery packages unavailable to retrieve from the get-go
    //            cal.AddDays(-1);
    //        }
    //    }

    //    this.TimeStamp = cal;
    //}
}
