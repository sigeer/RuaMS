using InvitationProto;

namespace Application.Core.Channel.Commands
{
    internal class InvokeAnswerInviteCommand : IWorldChannelCommand
    {
        AnswerInviteResponse res;
        public InvokeAnswerInviteCommand(AnswerInviteResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.InviteChannelHandlerRegistry.GetHandler(res.Type)?.OnInvitationAnswered(res);
            return;
        }
    }
}
