using Application.Shared.Servers;
using Application.Utility.Pipeline;
using Application.Utility.Tickables;

namespace Application.Core.Channel
{
    public interface IChannelServer : ISocketServer, IActorInstance<WorldChannel>, ITickableTree
    {
        int Id { get; }
    }
}
