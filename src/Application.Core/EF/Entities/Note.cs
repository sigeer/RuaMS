namespace Application.EF;

public partial class DB_Note
{
    private DB_Note()
    {
    }

    public DB_Note(string to, string from, string message, long timestamp)
    {
        To = to;
        From = from;
        Message = message;
        Timestamp = timestamp;
    }

    public DB_Note(string to, string from, string message, long timestamp, int fame) : this(to, from, message, timestamp)
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
    public static DB_Note createNormal(string message, string from, string to, long timestamp)
    {
        return new DB_Note(to, from, message, timestamp, 0);
    }

    public static DB_Note createGift(string message, string from, string to, long timestamp)
    {
        return new DB_Note(to, from, message, timestamp, 1);
    }
}
