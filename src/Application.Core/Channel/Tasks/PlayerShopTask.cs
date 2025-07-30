using Application.Core.Game.Invites;

namespace Application.Core.Channel.Tasks
{
    public class PlayerShopTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;

        public PlayerShopTask(WorldChannelServer server)
        {
            _server = server;
        }

        public override void HandleRun()
        {
            foreach (var ch in _server.Servers.Values)
            {
                ch.PlayerShopManager.CheckExpired();
            }
        }
    }
}
