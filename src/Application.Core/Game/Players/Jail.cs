namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public long getJailExpirationTimeLeft()
        {
            return Jailexpire - DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void setFutureJailExpiration(long time)
        {
            Jailexpire = DateTimeOffset.Now.ToUnixTimeMilliseconds() + time;
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
