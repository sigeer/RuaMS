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
            _server.Broadcast(async w =>
            {
                var r = await w.PlayerShopManager.CheckExpired();

                await w.NodeActor.Send(new InvokeSyncPlayerShopCommand(new DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest>(w.Id, r)));
            });
        }
    }
}
