using server;

namespace Application.Core.Channel.Commands
{
    internal class PlayerBuffHealCommand : IWorldChannelCommand
    {
        Player _chr;
        StatEffect _buffEffect;

        public PlayerBuffHealCommand(Player chr, StatEffect buffEffect)
        {
            _chr = chr;
            _buffEffect = buffEffect;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.ApplyBuffHeal(_buffEffect);
        }
    }
}
