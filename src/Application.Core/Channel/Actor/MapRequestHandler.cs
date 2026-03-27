using Application.Core.Game.Maps;
using Application.Utility.Pipeline;
using Polly;

namespace Application.Core.Channel.Actor
{
    public class MapRequest : ICommand<IMap>
    {
        public MapRequest(Action<IMap> func)
        {
            Func = func;
        }

        public Action<IMap> Func { get; }

        public void Execute(IMap map)
        {
            Func.Invoke(map);
        }
    }

    public class AsyncMapRequest : IAsyncCommand<IMap>
    {
        public AsyncMapRequest(Func<IMap, Task> func)
        {
            Func = func;
        }
        public Func<IMap, Task> Func { get; }

        public Task Execute(IMap map)
        {
            return Func.Invoke(map);
        }
    }
}
