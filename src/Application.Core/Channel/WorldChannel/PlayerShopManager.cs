using Application.Core.Game.Trades;

namespace Application.Core.Channel
{
    public class PlayerShopManager
    {

        private object activePlayerShopsLock = new object();
        /// <summary>
        /// PlayerId - PlayerShop
        /// </summary>
        private Dictionary<int, PlayerShop> activePlayerShops = new();


        readonly WorldChannel _server;

        public PlayerShopManager(WorldChannel server)
        {
            _server = server;
        }


        public void registerPlayerShop(PlayerShop ps)
        {
            Monitor.Enter(activePlayerShopsLock);
            try
            {
                activePlayerShops.AddOrUpdate(ps.getOwner().getId(), ps);
            }
            finally
            {
                Monitor.Exit(activePlayerShopsLock);
            }
        }

        public void unregisterPlayerShop(PlayerShop ps)
        {
            Monitor.Enter(activePlayerShopsLock);
            try
            {
                activePlayerShops.Remove(ps.getOwner().getId());
            }
            finally
            {
                Monitor.Exit(activePlayerShopsLock);
            }
        }

        public List<PlayerShop> getActivePlayerShops()
        {
            Monitor.Enter(activePlayerShopsLock);
            try
            {
                return activePlayerShops.Values.ToList();
            }
            finally
            {
                Monitor.Exit(activePlayerShopsLock);
            }
        }

        public PlayerShop? getPlayerShop(int ownerid)
        {
            Monitor.Enter(activePlayerShopsLock);
            try
            {
                return activePlayerShops.GetValueOrDefault(ownerid);
            }
            finally
            {
                Monitor.Exit(activePlayerShopsLock);
            }
        }
    }
}
