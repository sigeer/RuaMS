using Application.Core.Game.Invites;

namespace Application.Core.Channel.Commands
{
    internal class InvitationExpireCheckCommand : IChannelCommand
    {
        public void Execute(ChannelNodeCommandContext ctx)
        {
            InviteType.TRADE.CheckExpired(ctx.Server.getCurrentTime());
        }
    }
}
