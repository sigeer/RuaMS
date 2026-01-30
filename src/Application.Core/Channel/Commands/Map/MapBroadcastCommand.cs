using Application.Core.Game.Maps;

namespace Application.Core.Channel.Commands
{
    internal class MapBroadcastCommand : IWorldChannelCommand
    {
        IMap _map;
        Packet _packet;

        public MapBroadcastCommand(IMap map, Packet packet)
        {
            _map = map;
            _packet = packet;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.broadcastMessage(_packet);
            return;
        }
    }
}
