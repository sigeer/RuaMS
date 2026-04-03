using Application.Utility.Performance;
using Application.Utility.Pipeline;

namespace Application.Core.Channel.Actor
{
    public class NodeDelegateCommand : ICommand<WorldChannelServer>
    {
        public string? Name => Func.Target?.ToString();
        public NodeDelegateCommand(Action<WorldChannelServer> func)
        {
            Func = func;
        }

        public Action<WorldChannelServer> Func { get; }
        public void Execute(WorldChannelServer ctx)
        {
            Func.Invoke(ctx);
        }
    }

    public class AsyncNodeDelegateCommand : IAsyncCommand<WorldChannelServer>
    {
        public string? Name => Func.Target?.ToString();
        public AsyncNodeDelegateCommand(Func<WorldChannelServer, Task> func)
        {
            Func = func;
        }

        public Func<WorldChannelServer, Task> Func { get; }
        public Task Execute(WorldChannelServer ctx)
        {
            return Func.Invoke(ctx);
        }
    }
}
