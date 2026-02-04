using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventSnowballTimeoutCommand : IWorldChannelCommand
    {
        Snowball _evt;

        public EventSnowballTimeoutCommand(Snowball evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessTimeout();
        }
    }
}
