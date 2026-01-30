namespace Application.Core.Channel.Commands
{
    internal class InvokeCheckMapOwnershipCommand : IWorldChannelCommand
    {

        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.MapOwnershipManager.HandleRun();
        }
    }
}
