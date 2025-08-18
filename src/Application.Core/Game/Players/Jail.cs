namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public long getJailExpirationTimeLeft()
        {
            return Jailexpire - Client.CurrentServerContainer.getCurrentTime();
        }

        private void setFutureJailExpiration(long time)
        {
            Jailexpire = Client.CurrentServerContainer.getCurrentTime() + time;
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
