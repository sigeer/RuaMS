namespace Application.Core.Channel.Commands
{
    internal class PlayerCouponRecalcCommand : IWorldChannelCommand
    {
        readonly Player _player;

        public PlayerCouponRecalcCommand(Player player)
        {
            _player = player;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _player.updateCouponRates();
        }
    }
}
