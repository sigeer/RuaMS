namespace Application.Core.Game.Players
{
    public partial class Player
    {
        long jailExpiration = -1;
        public long getJailExpirationTimeLeft()
        {
            return jailExpiration - DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void setFutureJailExpiration(long time)
        {
            jailExpiration = DateTimeOffset.Now.ToUnixTimeMilliseconds() + time;
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
            jailExpiration = 0;
        }
    }
}
