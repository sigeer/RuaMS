using server.quest;

namespace Application.Core.Channel.Commands
{
    internal class SyncPlayerShopCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            var r = ctx.WorldChannel.PlayerShopManager.CheckExpired();

            ctx.WorldChannel.NodeActor.Post(new InvokeSyncPlayerShopCommand(new DistributeSessionDataWrapper<int, ItemProto.SyncPlayerShopRequest>(ctx.WorldChannel.Id, r)));
        }
    }
}
