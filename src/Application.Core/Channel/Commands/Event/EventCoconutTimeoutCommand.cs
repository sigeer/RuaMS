using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventCoconutTimeoutCommand : IWorldChannelCommand
    {
        Coconut _evt;

        public EventCoconutTimeoutCommand(Coconut evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.Check();
        }
    }
}
