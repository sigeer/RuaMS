namespace Application.Core.Game.Players
{
    public partial class Player
    {
        public long getJailExpirationTimeLeft()
        {
            return Jailexpire - Client.CurrentServer.Node.getCurrentTime();
        }

        private void setFutureJailExpiration(long time)
        {
            Jailexpire = Client.CurrentServer.Node.getCurrentTime() + time;
        }

        public async Task addJailExpirationTime(long time)
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

            if (getMapId() != MapId.JAIL)
            {
                saveLocationOnWarp();
                await changeMap(MapId.JAIL);
            }
        }

        public async Task CheckJail()
        {
            if (Jailexpire > Client.CurrentServer.Node.getCurrentTime())
            {
                if (getMapId() != MapId.JAIL)
                {
                    saveLocationOnWarp();
                    await changeMap(MapId.JAIL);
                }
            }
        }

        public void removeJailExpirationTime()
        {
            Jailexpire = 0;
        }
    }
}
