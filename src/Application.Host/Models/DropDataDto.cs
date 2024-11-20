namespace Application.Host.Models
{
    public class DropDataDto
    {
        public int Id { get; set; }
        public int DropperId { get; set; }
        public string? DropperName { get; set; }

        public int ItemId { get; set; }
        public string? ItemName { get; set; }

        public int MinimumQuantity { get; set; }

        public int MaximumQuantity { get; set; }

        public int QuestId { get; set; }
        public string? QuestName { get; set; }

        public int ContinentId { get; set; }
        public string? ContinentName { get; set; }

        public int Chance { get; set; }
    }
}
