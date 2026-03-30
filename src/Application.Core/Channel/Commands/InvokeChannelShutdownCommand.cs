using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Core.Channel.Commands
{
    internal class InvokeChannelShutdownCommand : IWorldChannelCommand
    {
        public void Execute(WorldChannel ctx)
        {
            _ = ctx.Shutdown();
        }
    }
}
