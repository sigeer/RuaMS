namespace Application.EF.Entities;

public class Gift
{
    public Gift(int to, string from, string message, int sn, int ringid)
    {
        To = to;
        From = from;
        Message = message;
        Sn = sn;
        Ringid = ringid;
    }

    private Gift() { }

    public int Id { get; set; }

    public int To { get; set; }

    public string From { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int Sn { get; set; }

    public int Ringid { get; set; }
}
