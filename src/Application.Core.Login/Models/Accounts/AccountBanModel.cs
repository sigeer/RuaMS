using Application.Core.Login.Shared;
using Application.Shared.Login;

namespace Application.Core.Login.Models.Accounts
{
    public class AccountBanModel : ITrackableEntityKey<int>
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public int Reason { get; set; }
        public BanLevel BanLevel { get; set; }
        public string ReasonDescription { get; set; }
    }
}
