using Application.Core.Login.Dtos.Account;

namespace Application.Core.Login.Dtos.Ban
{
    public class BanResponseDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public AccountPreviewResponseDto? Account { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int BanLevel { get; set; }
        public int Reason { get; set; }
        public string ReasonDescription { get; set; }
        public int OperateAccountId { get; set; }
        public AccountPreviewResponseDto? OperateAccount { get; set; }
        public int AuditAccountId { get; set; }
        public AccountPreviewResponseDto? AuditAccount { get; set; }
        public bool Canceled { get; set; }
    }
}
