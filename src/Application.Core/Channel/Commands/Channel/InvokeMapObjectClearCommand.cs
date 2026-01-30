using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands.Channel
{
    internal class InvokeMapObjectClearCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.MapObjectManager.HandleRun();
        }
    }
}
