using Application.Core.Scripting.Events;

namespace Application.Core.Channel.Commands
{
    internal class EventInstanceDismissTimerCommand : IWorldChannelCommand
    {
        AbstractEventInstanceManager _eim;

        public EventInstanceDismissTimerCommand(AbstractEventInstanceManager eim)
        {
            _eim = eim;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _eim.DismissEventTimer();
        }
    }
}
