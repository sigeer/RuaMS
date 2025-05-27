namespace Application.Shared.Items
{
    public class DropDto
    {
        public int ItemId { get; set; }
        public int Chance { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }
        public short QuestId { get; set; } = -1;
        public DropType Type { get; set; }
        public int DropperId { get; set; }
    }
}
