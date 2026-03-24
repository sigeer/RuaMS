namespace Application.Utility.Extensions
{
    public static class NumberExtensions
    {
        public static long GetExpirationFromMinutes(this int value)
        {
            if (value < 0)
                return -1;

            return value * 60_000;
        }

        public static long GetExpirationFromSeconds(this int value)
        {
            if (value < 0)
                return -1;

            return value * 1000;
        }

        public static long GetExpirationFromDays(this int value)
        {
            if (value < 0)
                return -1;

            return value * 86_400_000;
        }
    }
}
