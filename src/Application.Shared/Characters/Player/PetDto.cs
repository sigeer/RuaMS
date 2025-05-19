namespace Application.Shared.Characters
{
    public class RingDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }

        public int PartnerRingId { get; set; }

        public int PartnerChrId { get; set; }
        public string PartnerName { get; set; } = null!;
    }
    public class PetDto
    {
        public int Petid { get; set; }

        public string? Name { get; set; }

        public int Level { get; set; }

        public int Closeness { get; set; }

        public int Fullness { get; set; }

        public bool Summoned { get; set; }

        public int Flag { get; set; }
    }
}
