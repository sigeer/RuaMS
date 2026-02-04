using Application.Core.Game.Maps.Mists;

namespace Application.Core.Channel.Commands
{
    internal class MapMistRemoveCommand : IWorldChannelCommand
    {
        Mist _mist;

        public MapMistRemoveCommand(Mist mist)
        {
            _mist = mist;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mist.MapRemove();
        }
    }
}
