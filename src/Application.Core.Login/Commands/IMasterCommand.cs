using Application.Utility.Pipeline;

namespace Application.Core.Login.Commands
{
    public interface IMasterCommand : ICommand<MasterServer>
    {
    }

    public interface IMasterAsyncCommand : IAsyncCommand<MasterServer>
    {
    }
}
