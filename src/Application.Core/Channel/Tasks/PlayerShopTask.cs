using Application.Core.Channel.Commands;

namespace Application.Core.Channel.Tasks
{
    public class PlayerShopTask : ActorTask<WorldChannelServer>
    {
        public PlayerShopTask(WorldChannelServer server) : base(server, nameof(PlayerShopTask), TimeSpan.FromMinutes(1))
        {
        }

        protected override void HandleRun()
        {
            _actor.Broadcast(w =>
            {
                var r = w.PlayerShopManager.CheckExpired();

                w.NodeActor.Send(new InvokeSyncPlayerShopCommand(new DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest>(w.Id, r)));
            });
        }
    }
}
