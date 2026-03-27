using Application.Core.Game.Maps;
using Application.Utility.Performance;
using Application.Utility.Pipeline;
using System.Diagnostics;
using System.Threading.Channels;

namespace Application.Core.Channel.Actor
{
    public class ChannelRequest : ICommand<WorldChannel>
    {
        public ChannelRequest(Action<WorldChannel> func)
        {
            Func = func;
        }

        public Action<WorldChannel> Func { get; }
        public void Execute(WorldChannel ctx)
        {
            Func.Invoke(ctx);
        }
    }

    public class AsyncChannelRequest : IAsyncCommand<WorldChannel>
    {
        public AsyncChannelRequest(Func<WorldChannel, Task> func)
        {
            Func = func;
        }

        public Func<WorldChannel, Task> Func { get; }
        public Task Execute(WorldChannel ctx)
        {
            return Func.Invoke(ctx);
        }
    }
}
