namespace Application.Utility
{
    public class TimeUtils
    {
        public static TimeSpan GetTimeLeftForNextHour()
        {
            var nextHour = DateTimeOffset.UtcNow.Date.AddHours(DateTimeOffset.UtcNow.Hour + 1);
            return (nextHour - DateTimeOffset.UtcNow);
        }

        public static TimeSpan GetTimeLeftForNextDay()
        {
            return (DateTimeOffset.UtcNow.AddDays(1).Date - DateTimeOffset.UtcNow);
        }

        public static string GetTimeString(long then)
        {
            long duration = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - then;
            var d = TimeSpan.FromMilliseconds(duration);
            return d.Minutes + " Minutes and " + d.Seconds + " Seconds";
        }

        public static string GetTimeString(DateTimeOffset then)
        {
            var d = DateTimeOffset.UtcNow - then;
            return d.Minutes + " Minutes and " + d.Seconds + " Seconds";
        }

        public static int DayDiff(DateTimeOffset from, DateTimeOffset to)
        {
            return (to - from).Days;
        }

        public static int DayDiff(long from, long to)
        {
            return TimeSpan.FromMilliseconds(to - from).Days;
        }
    }
}
