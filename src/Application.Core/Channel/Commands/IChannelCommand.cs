using Application.Utility.Pipeline;

namespace Application.Core.Channel.Commands
{
    public interface IChannelCommand : ICommand<WorldChannelServer>
    {
    }

    public interface IChannelAsyncCommand : IAsyncCommand<WorldChannelServer>
    {
    }

    public interface IWorldChannelCommand : ICommand<WorldChannel>
    {
    }

    public interface IWorldChannelAsyncCommand : IAsyncCommand<WorldChannel>
    {
    }
}
