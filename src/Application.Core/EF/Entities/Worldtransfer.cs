namespace Application.EF.Entities;

public partial class Worldtransfer
{
    public Worldtransfer(int characterid, sbyte from, sbyte to)
    {
        Characterid = characterid;
        From = from;
        To = to;
    }

    public int Id { get; set; }

    public int Characterid { get; set; }

    public sbyte From { get; set; }

    public sbyte To { get; set; }

    public DateTimeOffset RequestTime { get; set; }

    public DateTimeOffset? CompletionTime { get; set; }
}
