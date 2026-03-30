using Application.Utility.Pipeline;

namespace Application.Core.Login.Commands
{

    public class MasterDelegateCommand : ICommand<MasterServer>
    {
        public MasterDelegateCommand(Action<MasterServer> func)
        {
            Func = func;
        }

        public Action<MasterServer> Func { get; }
        public void Execute(MasterServer ctx)
        {
            Func.Invoke(ctx);
        }
    }

    public class AsyncMasterDelegateCommand : IAsyncCommand<MasterServer>
    {
        public AsyncMasterDelegateCommand(Func<MasterServer, Task> func)
        {
            Func = func;
        }

        public Func<MasterServer, Task> Func { get; }
        public Task Execute(MasterServer ctx)
        {
            return Func.Invoke(ctx);
        }
    }
}
