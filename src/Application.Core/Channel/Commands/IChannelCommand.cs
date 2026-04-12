using Application.Utility.Pipeline;

namespace Application.Core.Channel.Commands
{
    public interface IChannelCommand : ICommand<WorldChannelServer>
    {
    }

    public interface IWorldChannelCommand : ICommand<WorldChannel>
    {
    }
}
