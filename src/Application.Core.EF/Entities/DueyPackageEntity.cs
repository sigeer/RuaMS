using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Application.EF.Entities;

public partial class DueyPackageEntity
{
    public int PackageId { get; set; }

    public int ReceiverId { get; set; }
    public int SenderId { get; set; }

    public int Mesos { get; set; }

    public DateTimeOffset TimeStamp { get; set; }

    public string? Message { get; set; }

    public bool Checked { get; set; } = true;

    public bool Type { get; set; } = false;
    [NotMapped]
    public bool IsFrozen { get; set; }
    private DueyPackageEntity() { }

    public DueyPackageEntity(int id, int receiverId, int senderId, int mesos, string? message, bool @checked, bool type, DateTimeOffset createTime)
    {
        PackageId = id;
        ReceiverId = receiverId;
        SenderId = senderId;
        Mesos = mesos;
        TimeStamp = createTime;
        Message = message;
        Checked = @checked;
        Type = type;
    }


    public virtual ICollection<Dueyitem> Dueyitems { get; set; } = new List<Dueyitem>();


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
