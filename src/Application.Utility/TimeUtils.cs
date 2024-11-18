namespace Application.Utility
{
    public class TimeUtils
    {
        public static TimeSpan GetTimeLeftForNextHour()
        {
            var nextHour = DateTimeOffset.Now.Date.AddHours(DateTimeOffset.Now.Hour + 1);
            return (nextHour - DateTimeOffset.Now);
        }

        public static TimeSpan GetTimeLeftForNextDay()
        {
            return (DateTimeOffset.Now.AddDays(1).Date - DateTimeOffset.Now);
        }
    }
}
