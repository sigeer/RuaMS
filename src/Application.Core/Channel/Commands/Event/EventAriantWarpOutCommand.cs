using server.partyquest;

namespace Application.Core.Channel.Commands
{
    internal class EventAriantWarpOutCommand : IWorldChannelCommand
    {
        AriantColiseum _evt;

        public EventAriantWarpOutCommand(AriantColiseum evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.enterKingsRoom();
        }
    }
}
