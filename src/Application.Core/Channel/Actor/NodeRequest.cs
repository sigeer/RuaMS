using Application.Utility.Pipeline;

namespace Application.Core.Channel.Actor
{
    public class NodeRequest : ICommand<WorldChannelServer>
    {
        public NodeRequest(Action<WorldChannelServer> func)
        {
            Func = func;
        }

        public Action<WorldChannelServer> Func { get; }
        public void Execute(WorldChannelServer ctx)
        {
            Func.Invoke(ctx);
        }
    }

    public class AsyncNodeRequest : IAsyncCommand<WorldChannelServer>
    {
        public AsyncNodeRequest(Func<WorldChannelServer, Task> func)
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
