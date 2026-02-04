using server.partyquest;

namespace Application.Core.Channel.Commands
{
    internal class EventPyramidGaugeCommand : IWorldChannelCommand
    {
        Pyramid _evt;

        public EventPyramidGaugeCommand(Pyramid evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessGauge();
        }
    }
}
