namespace Application.Core.Game.Players
{
    public partial class Player
    {
        /// <summary>
        /// player client is currently trying to change maps or log in the game map
        /// </summary>
        private AtomicBoolean mapTransitioning = new AtomicBoolean(true);

        public bool isLoggedinWorld()
        {
            return this.isLoggedin() && !this.isAwayFromWorld();
        }

        public bool isAwayFromWorld()
        {
            return Client.CurrentServer.IsAwayFromWorld(Id);
        }
    }
}
