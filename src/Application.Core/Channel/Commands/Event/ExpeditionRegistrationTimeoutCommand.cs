using server.expeditions;

namespace Application.Core.Channel.Commands
{
    internal class ExpeditionRegistrationTimeoutCommand : IWorldChannelCommand
    {
        Expedition _expd;

        public ExpeditionRegistrationTimeoutCommand(Expedition expd)
        {
            _expd = expd;
        }

        public void Execute(ChannelCommandContext ctx)
        {
            _expd.ProcessRegistrationTimeout();
        }
    }
}
