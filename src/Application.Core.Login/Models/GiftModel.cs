using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class GiftModel: ITrackableEntityKey<int>
    {
        public int Id { get; set; }

        public int To { get; set; }
        public int From { get; set; }

        public string Message { get; set; } = null!;

        public int Sn { get; set; }
        public int RingSourceId { get; set; }
    }
}
