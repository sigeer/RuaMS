using Microsoft.EntityFrameworkCore;

namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>
        /// player client is currently trying to change maps or log in the game map
        /// </summary>
        private AtomicBoolean mapTransitioning = new AtomicBoolean(true);
        /// <summary>
        /// player is online, but on cash shop or mts
        /// </summary>
        private AtomicBoolean awayFromWorld = new AtomicBoolean(true);

        public void setDisconnectedFromChannelWorld()
        {
            setAwayFromChannelWorld(true);
        }
        public void setAwayFromChannelWorld()
        {
            setAwayFromChannelWorld(false);
        }
        private void setAwayFromChannelWorld(bool disconnect)
        {
            awayFromWorld.Set(true);

            if (!disconnect)
            {
                Client.getChannelServer().insertPlayerAway(Id);
            }
            else
            {
                Client.getChannelServer().removePlayerAway(Id);
            }
        }
        public bool isLoggedinWorld()
        {
            return this.isLoggedin() && !this.isAwayFromWorld();
        }

        public bool isAwayFromWorld()
        {
            return awayFromWorld.Get();
        }

        public void setEnteredChannelWorld(int channel)
        {
            awayFromWorld.Set(false);
            Client.getChannelServer().removePlayerAway(Id);

            if (PartySearch)
            {
                this.getWorldServer().getPartySearchCoordinator().attachPlayer(this);
            }
        }
    }
}
