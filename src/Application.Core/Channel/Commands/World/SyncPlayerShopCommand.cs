using server.quest;

namespace Application.Core.Channel.Commands
{
    internal class SyncPlayerShopCommand : IWorldChannelCommand
    {
        public void Execute(WorldChannel ctx)
        {
            var r = ctx.PlayerShopManager.CheckExpired();

            ctx.NodeActor.Send(new InvokeSyncPlayerShopCommand(new DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest>(ctx.Id, r)));
        }
    }
}
