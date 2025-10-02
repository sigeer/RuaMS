namespace Application.Shared.Login
{
    public class AccountLoginStatus
    {
        public AccountLoginStatus(int state, DateTimeOffset dateTime)
        {
            State = state;
            DateTime = dateTime;
        }

        public int State { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public int Language { get; set; }

        public static AccountLoginStatus Default = new AccountLoginStatus(0, DateTimeOffset.MinValue);
    }
}
