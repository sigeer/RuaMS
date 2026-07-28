namespace Application.Shared.Login
{
    /// <summary>
    /// 本次登录信息
    /// </summary>
    public class AccountLoginStatus
    {
        public AccountLoginStatus(int state, DateTimeOffset dateTime)
        {
            State = state;
            ProcessTime = dateTime;
        }

        public int State { get; set; }
        public DateTimeOffset ProcessTime { get; set; }
        public int Language { get; set; }

        public static AccountLoginStatus Default = new AccountLoginStatus(0, DateTimeOffset.MinValue);
    }
}
