namespace Application.Core.Models
{
    public class NoteObject
    {
        public int Id { get; set; }

        public string To { get; set; } = null!;

        public string From { get; set; } = null!;

        public string Message { get; set; } = null!;

        public long Timestamp { get; set; }

        public int Fame { get; set; }

        public int Deleted { get; set; }
    }
}
