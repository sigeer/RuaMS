using System.ComponentModel.DataAnnotations;

namespace Application.Core.Login.Dtos.Drop
{
    public class DropRequestDto
    {
        public int Id { get; set; }
        [Range(1, int.MaxValue)]
        public int DropperId { get; set; }
        public int ItemId { get; set; }
        [Range(1, 1000000)]
        public int MinCount { get; set; }
        [Range(1, 1000000)]
        public int MaxCount { get; set; }

        public int QuestId { get; set; }
        [Range(1, 1000000)]
        public int Chance { get; set; }
    }
}
