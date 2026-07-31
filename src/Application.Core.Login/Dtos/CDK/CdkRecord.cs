namespace Application.Core.Login.Dtos.CDK
{
    public class RewardRecordResponseDto
    {
        public int Id { get; set; }
        public int CodeId { get; set; }
        public int RecipientId { get; set; }
        public string RecipientName { get; set; } = "";
        public DateTimeOffset RecipientTime { get; set; }
    }
}
