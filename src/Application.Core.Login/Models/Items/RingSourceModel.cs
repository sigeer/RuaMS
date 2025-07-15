using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class RingSourceModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public long RingId1 { get; set; }
        public long RingId2 { get; set; }

        public int CharacterId1 { get; set; }
        public int CharacterId2 { get; set; }
    }
}
