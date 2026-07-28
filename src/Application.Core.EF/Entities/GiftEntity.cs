using Application.Core.EF;

namespace Application.EF.Entities;

public class GiftEntity: IKeyedEntity<int>
{
    public GiftEntity(int id , int to, int from, string message, int sn, int ringid)
    {
        Id = id;
        ToId = to;
        FromId = from;
        Message = message;
        Sn = sn;
        RingSourceId = ringid;
    }

    private GiftEntity() { }

    public int Id { get; set; }

    public int ToId { get; set; }

    public int FromId { get; set; }

    public string Message { get; set; } = "";

    public int Sn { get; set; }

    public int RingSourceId { get; set; }
    public DateTimeOffset? ClaimTime { get; set; }
}
