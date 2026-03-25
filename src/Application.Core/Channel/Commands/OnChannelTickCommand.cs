using Application.Utility.Tickables;

namespace Application.Core.Channel.Commands
{
    public class OnChannelTickCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            var now = ctx.WorldChannel.Node.getCurrentTime();
            ctx.WorldChannel.OnTick(now);
        }
    }
}
