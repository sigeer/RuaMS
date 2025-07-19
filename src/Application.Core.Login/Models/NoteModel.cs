using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class NoteModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int ToId { get; set; }

        public int FromId { get; set; }
        public string Message { get; set; }

        public long Timestamp { get; set; }

        public int Fame { get; set; }
        public bool IsDeleted { get; set; }
    }
}
