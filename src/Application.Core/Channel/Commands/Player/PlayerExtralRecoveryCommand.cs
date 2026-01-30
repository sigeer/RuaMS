namespace Application.Core.Channel.Commands
{
    internal class PlayerExtralRecoveryCommand : IWorldChannelCommand
    {
        Player _chr;
        sbyte _hp;
        sbyte _mp;

        public PlayerExtralRecoveryCommand(Player chr, sbyte hp, sbyte mp)
        {
            _chr = chr;
            _hp = hp;
            _mp = mp;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ApplyExtralRecovery(_hp, _mp);
        }
    }
}
