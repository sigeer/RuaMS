using Application.Core.Game.Maps;

namespace Application.Core.Channel.Commands
{
    internal class MapClearMapEffectCommand : IWorldChannelCommand
    {
        IMap _map;

        public MapClearMapEffectCommand(IMap map)
        {
            _map = map;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            if (_map.MapEffect != null)
            {
                _map.BroadcastAll(e => e.sendPacket(_map.MapEffect.makeDestroyData()));
                _map.MapEffect = null;
            }
        }
    }
}
