using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class GiftModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int To { get; set; }
        public string ToName { get; set; } = null!;
        public int From { get; set; }
        public string FromName { get; set; } = null!;

        public string Message { get; set; } = null!;

        public int Sn { get; set; }
        public int RingSourceId { get; set; }
    }
}
