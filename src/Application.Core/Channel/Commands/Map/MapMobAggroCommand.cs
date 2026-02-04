using Application.Core.Game.Maps;

namespace Application.Core.Channel.Commands
{
    internal class MapMobAggroCommand : IWorldChannelCommand
    {
        IMap _map;

        public MapMobAggroCommand(IMap map)
        {
            _map = map;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.getAggroCoordinator().RunAggro();
        }
    }
}
