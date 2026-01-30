using server.partyquest;

namespace Application.Core.Channel.Commands
{
    internal class EventAriantClearCommand : IWorldChannelCommand
    {
        AriantColiseum _evt;

        public EventAriantClearCommand(AriantColiseum evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.showArenaResults();
        }
    }
}
