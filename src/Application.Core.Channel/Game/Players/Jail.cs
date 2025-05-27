namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public long getJailExpirationTimeLeft()
        {
            return Jailexpire - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void setFutureJailExpiration(long time)
        {
            Jailexpire = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + time;
        }

        public void addJailExpirationTime(long time)
        {
            long timeLeft = getJailExpirationTimeLeft();

            if (timeLeft <= 0)
            {
                setFutureJailExpiration(time);
            }
            else
            {
                setFutureJailExpiration(timeLeft + time);
            }
        }

        public void removeJailExpirationTime()
        {
            Jailexpire = 0;
        }
    }
}
