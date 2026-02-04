using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventOlaTimeoutCommand : IWorldChannelCommand
    {
        Ola _evt;

        public EventOlaTimeoutCommand(Ola evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessTimeout();
        }
    }
}
