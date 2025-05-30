namespace Application.EF.Entities;

public class GiftEntity
{
    public GiftEntity(int to, string from, string message, int sn, long ringid)
    {
        To = to;
        From = from;
        Message = message;
        Sn = sn;
        Ringid = ringid;
    }

    private GiftEntity() { }

    public int Id { get; set; }

    public int To { get; set; }

    public string From { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int Sn { get; set; }

    public long Ringid { get; set; }
}
