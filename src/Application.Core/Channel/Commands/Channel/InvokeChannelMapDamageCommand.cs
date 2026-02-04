namespace Application.Core.Channel.Commands
{
    internal class InvokeChannelMapDamageCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.CharacterHpDecreaseManager.HandleRun();
        }
    }
}
