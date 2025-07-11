using Application.Core.Login.Shared;

namespace Application.Core.Login.Models
{
    public class RingModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public long RingId1 { get; set; }
        public long RingId2 { get; set; }

        public int CharacterId1 { get; set; }
        public int CharacterId2 { get; set; }
    }

    public class RingSingle
    {
        public long RingId { get; set; }
        public int SourceId { get; set; }

        public long AnotherRingId { get; set; }
        public int AnotherCharacterId { get; set; }
        public string AnotherCharacterName { get; set; }
    }
}
