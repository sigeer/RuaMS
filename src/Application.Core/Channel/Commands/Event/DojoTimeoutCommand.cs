using Application.Core.Gameplay.ChannelEvents;

namespace Application.Core.Channel.Commands
{
    internal class DojoTimeoutCommand : IWorldChannelCommand
    {
        DojoInstance _instance;
        int _mapId;

        public DojoTimeoutCommand(DojoInstance instance, int mapId)
        {
            _instance = instance;
            _mapId = mapId;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _instance.ProcessTimeout(_mapId);
        }
    }
}
