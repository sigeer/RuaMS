using Application.Core.Game.Maps;

namespace Application.Core.Channel.Commands
{
    internal class MapItemMonitorCommand : IWorldChannelCommand
    {
        readonly IMap _map;

        public MapItemMonitorCommand(IMap map)
        {
            _map = map;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.ProcessItemMonitor();
        }
    }
}
