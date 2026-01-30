using Application.Utility.Pipeline;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class ServerMessageCommand : IWorldChannelCommand
    {

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.ServerMessageManager.HandleRun();
            return;
        }
    }
}
