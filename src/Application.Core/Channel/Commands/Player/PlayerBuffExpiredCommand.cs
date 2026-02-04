namespace Application.Core.Channel.Commands
{
    internal class PlayerBuffExpiredCommand : IWorldChannelCommand
    {
        Player _chr;
        public PlayerBuffExpiredCommand(Player chr)
        {
            _chr = chr;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ClearExpiredBuffs();
        }
    }
}
