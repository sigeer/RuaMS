namespace Application.Core.EF.Entities
{
    public class AccountBanEntity
    {
        private AccountBanEntity() { }
        public AccountBanEntity(int id, int accountId, DateTimeOffset startTime, DateTimeOffset endTime, int banLevel, int reason, string reasonDescription)
        {
            Id = id;
            AccountId = accountId;
            StartTime = startTime;
            EndTime = endTime;
            BanLevel = banLevel;
            Reason = reason;
            ReasonDescription = reasonDescription;
        }

        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int BanLevel { get; set; }
        public int Reason { get; set; }
        public string ReasonDescription { get; set; }
    }
}
