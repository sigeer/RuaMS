namespace Application.Core.Login.Dtos.CDK
{
    public class RewardResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? Code { get; set; }

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int MaxCount { get; set; }
        public bool AccountOnce { get; set; }
        public int UsedCount { get; set; }
    }

    public class RewardDetailResponseDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public string? Code { get; set; }

        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public int MaxCount { get; set; }
        public bool AccountOnce { get; set; }
        public List<RewardItemResponseDto> Items { get; set; } = [];
    }

}
