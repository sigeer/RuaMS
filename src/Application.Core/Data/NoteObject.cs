using server.life;

namespace Application.Core.Models
{
    public class NoteObject
    {
        public int Id { get; set; }

        public string To { get; set; } = null!;

        public int FromId { get; set; }
        public string? From { get; set; }

        public string Message { get; set; } = null!;

        public long Timestamp { get; set; }

        public int Fame { get; set; }

        public int Deleted { get; set; }
        public string GetFromName(IChannelClient client)
        {
            return FromId < 0 ? client.CurrentCulture.GetNpcName(FromId) : From!;
        }
    }
}
