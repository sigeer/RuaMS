using Application.Core.Client;

namespace Application.Core.Net
{
    public interface IPacketProcessor<TClient> where TClient : IClientBase
    {
        IPacketHandlerBase<TClient>? GetPacketHandler(short code);
    }
}
