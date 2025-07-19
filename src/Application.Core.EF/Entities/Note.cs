namespace Application.EF;

public partial class NoteEntity
{
    private NoteEntity()
    {
    }

    public NoteEntity(int id, int to, int from, string message, long timestamp)
    {
        Id = id;
        ToId = to;
        FromId = from;
        Message = message;
        Timestamp = timestamp;
    }

    public NoteEntity(int id, int to, int from, string message, long timestamp, int fame) : this(id, to, from, message, timestamp)
    {
        Fame = fame;
    }

    public int Id { get; set; }

    public int ToId { get; set; }

    public int FromId { get; set; }

    public string Message { get; set; } = null!;

    public long Timestamp { get; set; }

    public int Fame { get; set; }

    public int Deleted { get; set; }
}
