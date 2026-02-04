namespace Application.Core.Channel.Commands
{
    internal class PlayerDiseaseExpiredCommand : IWorldChannelCommand
    {
        Player _chr;
        public PlayerDiseaseExpiredCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ClearExpiredDisease();
        }
    }
}
