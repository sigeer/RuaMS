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

        public static string GetTimeString(long then)
        {
            long duration = DateTimeOffset.Now.ToUnixTimeMilliseconds() - then;
            var d = TimeSpan.FromMilliseconds(duration);
            return d.Minutes + " Minutes and " + d.Seconds + " Seconds";
        }

        public static int DayDiff(DateTimeOffset from, DateTimeOffset to)
        {
            return (to - from).Days;
        }
    }
}
