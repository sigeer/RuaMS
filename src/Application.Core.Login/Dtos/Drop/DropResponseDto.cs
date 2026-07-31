namespace Application.Core.Login.Dtos.Drop
{
    public class DropResponseDto
    {
        public int Id { get; set; }
        public int DropperId { get; set; }
        public string MobName { get; set; } = "";
        public int ItemId { get; set; }
        public string ItemName { get; set; } = "";
        public int MinCount { get; set; }
        public int MaxCount { get; set; }

        public int QuestId { get; set; }
        public string? QuestName { get; set; }
        public int Chance { get; set; }
    }
}
