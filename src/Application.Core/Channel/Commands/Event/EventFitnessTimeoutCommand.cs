using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventFitnessTimeoutCommand : IWorldChannelCommand
    {
        Fitness _evt;

        public EventFitnessTimeoutCommand(Fitness evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessTimeout();
        }
    }
}
