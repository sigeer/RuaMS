using Application.Shared.Client;

namespace Application.Shared.Net
{
    public interface IPacketHandlerBase<in TClient> where TClient : IClientBase
    {
        bool ValidateState(TClient c);
        Task HandlePacket(InPacket p, TClient c);
    }
}
