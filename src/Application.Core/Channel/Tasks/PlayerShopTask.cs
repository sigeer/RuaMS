using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class PlayerShopTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;

        public PlayerShopTask(WorldChannelServer server) : base($"{server.ServerName}_{nameof(PlayerShopTask)}")
        {
            _server = server;
        }

        public override void HandleRun()
        {
            _server.PushChannelCommand(new SyncPlayerShopCommand());
        }
    }
}
