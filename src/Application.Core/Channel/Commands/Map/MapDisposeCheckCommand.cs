namespace Application.Core.Channel.Commands
{
    internal class MapDisposeCheckCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.getMapFactory().CheckActive();
        }
    }
}
