using Application.Core.Game.Maps;
using tools;

namespace Application.Core.Channel.Commands
{
    internal class MapBroadcastJobChangedCommand : IWorldChannelCommand
    {
        IMap _map;
        int _chrId;

        public MapBroadcastJobChangedCommand(IMap map, int chrId)
        {
            _map = map;
            _chrId = chrId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _map.BroadcastAll(chr => chr.sendPacket(PacketCreator.showForeignEffect(_chrId, 8)), _chrId);
        }
    }
}
