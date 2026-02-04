namespace Application.Core.Channel.Commands
{
    internal class PlayerPendantExpRateIncreaseCommand : IWorldChannelCommand
    {
        Player _chr;

        public PlayerPendantExpRateIncreaseCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.IncreasePendantExpRate();
        }
    }
}
