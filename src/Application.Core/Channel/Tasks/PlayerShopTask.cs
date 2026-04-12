using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class PlayerShopTask : AbstractRunnable
    {
        readonly WorldChannelServer _server;

        public PlayerShopTask(WorldChannelServer server) : base(nameof(PlayerShopTask))
        {
            _server = server;
        }

        public override void HandleRun()
        {
            _server.Broadcast(w =>
            {
                var r = w.PlayerShopManager.CheckExpired();

                w.NodeActor.Send(new InvokeSyncPlayerShopCommand(new DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest>(w.Id, r)));
            });
        }
    }
}
