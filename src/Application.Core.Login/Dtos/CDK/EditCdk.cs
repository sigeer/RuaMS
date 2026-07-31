using System.ComponentModel.DataAnnotations;

namespace Application.Core.Login.Dtos.CDK
{

    public class RewardDetailRequestDto
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string? Title { get; set; }
        public string? Description { get; set; }

        [StringLength(16, MinimumLength = 16)]
        public string? Code { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int MaxCount { get; set; }
        public bool AccountOnce { get; set; }
        public List<EditCdkItemRequestDto> Items { get; set; } = [];
    }

    public class EditCdkItemRequestDto
    {
        public int ItemId { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
