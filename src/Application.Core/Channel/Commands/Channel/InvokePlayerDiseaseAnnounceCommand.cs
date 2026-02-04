namespace Application.Core.Channel.Commands.Channel
{
    internal class InvokePlayerDiseaseAnnounceCommand : IWorldChannelCommand
    {
        public void Execute(ChannelCommandContext ctx)
        {
            ctx.WorldChannel.CharacterDiseaseManager.HandleRun();
        }
    }
}
