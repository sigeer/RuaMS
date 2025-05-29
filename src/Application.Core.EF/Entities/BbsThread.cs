namespace Application.EF.Entities;

public partial class BbsThread
{
    public int Threadid { get; set; }

    public int Postercid { get; set; }

    public string Name { get; set; } = null!;

    public long Timestamp { get; set; }

    public short Icon { get; set; }

    public short Replycount { get; set; }

    public string Startpost { get; set; } = null!;

    public int Guildid { get; set; }

    public int Localthreadid { get; set; }
}
