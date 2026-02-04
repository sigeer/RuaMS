using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventFitnessNoticeCommand : IWorldChannelCommand
    {
        Fitness _evt;

        public EventFitnessNoticeCommand(Fitness evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.CheckMessage();
        }
    }
}
