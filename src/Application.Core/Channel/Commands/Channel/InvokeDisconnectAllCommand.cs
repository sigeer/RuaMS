namespace Application.Core.Channel.Commands
{
    internal class InvokeDisconnectAllCommand : IWorldChannelCommand
    {
        bool includeGM;

        public InvokeDisconnectAllCommand(bool includeGM)
        {
            this.includeGM = includeGM;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _ = ctx.WorldChannel.getPlayerStorage().disconnectAll(includeGM);
        }
    }
}
