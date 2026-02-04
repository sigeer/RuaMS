using server.maps;

namespace Application.Core.Channel.Commands
{
    internal class PlayerMapEffectRemoveCommand : IWorldChannelCommand
    {
        Player _chr;
        MapEffect _effect;

        public PlayerMapEffectRemoveCommand(Player chr, MapEffect effect)
        {
            _chr = chr;
            _effect = effect;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _chr.sendPacket(_effect.makeDestroyData());
        }
    }
}
