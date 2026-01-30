using server.partyquest;

namespace Application.Core.Channel.Commands
{
    internal class EventPyramidTimeoutCommand : IWorldChannelCommand
    {
        Pyramid _evt;

        public EventPyramidTimeoutCommand(Pyramid evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessTimeout();
        }
    }
}
