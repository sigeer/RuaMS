using Application.Utility.Tickables;

namespace Application.Core.Channel.Commands
{
    public class OnChannelTickCommand : IWorldChannelCommand
    {
        public void Execute(WorldChannel ctx)
        {
            var now = ctx.Node.getCurrentTime();
            ctx.OnTick(now);
        }
    }
}
