namespace Application.Utility
{
    public class TimeUtils
    {
        public static TimeSpan GetTimeLeftForNextHour()
        {
            var now = DateTime.UtcNow;
            var nextHour = now.Date.AddHours(now.Hour + 1);
            var remaining = nextHour - now;
            return remaining >= TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        public static TimeSpan GetTimeLeftForNextDay()
        {
            var now = DateTime.UtcNow;
            var nextMidnight = now.AddDays(1).Date;
            return nextMidnight - now;
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
