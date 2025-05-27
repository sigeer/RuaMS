namespace Application.EF;

public partial class NoteEntity
{
    private NoteEntity()
    {
    }

    public NoteEntity(string to, string from, string message, long timestamp)
    {
        To = to;
        From = from;
        Message = message;
        Timestamp = timestamp;
    }

    public NoteEntity(string to, string from, string message, long timestamp, int fame) : this(to, from, message, timestamp)
    {
        Fame = fame;
    }

    public int Id { get; set; }

    public string To { get; set; } = null!;

    public string From { get; set; } = null!;

    public string Message { get; set; } = null!;

    public long Timestamp { get; set; }

    public int Fame { get; set; }

    public int Deleted { get; set; }

    private static int PLACEHOLDER_ID = -1;
    public static NoteEntity createNormal(string message, string from, string to, long timestamp)
    {
        return new NoteEntity(to, from, message, timestamp, 0);
    }

    public static NoteEntity createGift(string message, string from, string to, long timestamp)
    {
        return new NoteEntity(to, from, message, timestamp, 1);
    }
}
