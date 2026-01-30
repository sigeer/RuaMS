using Application.Core.Channel.Commands;
using server.maps;

namespace Application.Core.Channel
{
    internal class MapTempActiveCheckCommand : IWorldChannelCommand
    {
        MapMonitor _mapMonitor;

        public MapTempActiveCheckCommand(MapMonitor mapMonitor)
        {
            _mapMonitor = mapMonitor;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _mapMonitor.ProcessActive();
        }
    }
}
