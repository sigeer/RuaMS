using Application.Shared.Client;

namespace Application.Shared.Net
{
    public interface IPacketProcessor<TClient> where TClient : IClientBase
    {
        IPacketHandlerBase<TClient>? GetPacketHandler(short code);
        void TryAddHandler(short code, IPacketHandlerBase<TClient> handler);
    }
}
