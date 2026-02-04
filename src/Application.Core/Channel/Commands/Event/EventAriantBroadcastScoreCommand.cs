using server.partyquest;

namespace Application.Core.Channel.Commands
{
    internal class EventAriantBroadcastScoreCommand : IWorldChannelCommand
    {
        AriantColiseum _evt;

        public EventAriantBroadcastScoreCommand(AriantColiseum evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.broadcastAriantScoreUpdate();
        }
    }
}
