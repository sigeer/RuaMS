namespace Application.EF.Entities;

public partial class BbsReply
{
    protected BbsReply()
    {
    }

    public BbsReply(int threadid, int postercid, long timestamp, string content)
    {
        Threadid = threadid;
        Postercid = postercid;
        Timestamp = timestamp;
        Content = content;
    }

    public int Replyid { get; set; }

    public int Threadid { get; set; }

    public int Postercid { get; set; }

    public long Timestamp { get; set; }

    public string Content { get; set; } = null!;
}
