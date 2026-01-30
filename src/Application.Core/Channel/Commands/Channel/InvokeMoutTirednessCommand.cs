using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Channel
{
    internal class InvokeMoutTirednessCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.MountTirednessManager.HandleRun();
        }
    }
}
