namespace Application.Core.EF.Entities
{
    public class AccountBanEntity: IKeyedEntity<int>
    {
        private AccountBanEntity() { }
        public AccountBanEntity(int accountId, DateTimeOffset startTime, DateTimeOffset endTime, int banLevel, int reason, string reasonDescription, int operatorId)
        {
            AccountId = accountId;
            StartTime = startTime;
            EndTime = endTime;
            BanLevel = banLevel;
            Reason = reason;
            ReasonDescription = reasonDescription;
            OperateAccountId = operatorId;
        }

        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset EndTime { get; set; } = DateTimeOffset.UtcNow;
        public int BanLevel { get; set; }
        public int Reason { get; set; }
        public string ReasonDescription { get; set; }
        public int OperateAccountId { get; set; }

        public int AuditAccountId { get; set; }
        public bool Canceled { get; set; }

        public void UnBan(int accId)
        {
            Canceled = true;
            AuditAccountId = accId;
        }
    }
}
