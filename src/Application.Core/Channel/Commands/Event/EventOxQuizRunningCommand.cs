using server.events.gm;

namespace Application.Core.Channel.Commands
{
    internal class EventOxQuizRunningCommand : IWorldChannelCommand
    {
        OxQuiz _evt;

        public EventOxQuizRunningCommand(OxQuiz evt)
        {
            _evt = evt;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _evt.ProcessSendQuestion();
        }
    }
}
