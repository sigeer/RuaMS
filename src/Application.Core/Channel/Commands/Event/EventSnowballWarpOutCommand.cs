using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventSnowballWarpOutCommand : IWorldChannelCommand
    {
        Snowball _evt;

        public EventSnowballWarpOutCommand(Snowball evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessWarpOut();
        }
    }
}
