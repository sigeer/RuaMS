using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventSnowballRespawnCommand : IWorldChannelCommand
    {
        Snowball _evt;

        public EventSnowballRespawnCommand(Snowball evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.SnowmanRespawn();
        }
    }
}
