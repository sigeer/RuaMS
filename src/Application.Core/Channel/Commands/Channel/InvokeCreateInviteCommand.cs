using InvitationProto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeCreateInviteCommand : IWorldChannelCommand
    {
        CreateInviteResponse res;

        public InvokeCreateInviteCommand(CreateInviteResponse res)
        {
            this.res = res;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.InviteChannelHandlerRegistry.GetHandler(res.Type)?.OnInvitationCreated(res);
            return;
        }
    }
}
