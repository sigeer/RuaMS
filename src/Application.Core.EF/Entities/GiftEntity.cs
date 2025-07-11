namespace Application.EF.Entities;

public class GiftEntity
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

    public string Message { get; set; } = null!;

    public int Sn { get; set; }

    public int RingSourceId { get; set; }
}
