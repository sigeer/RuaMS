using net.packet;

namespace Application.Core.Net
{
    public interface IPacketHandlerBase<in TClient> where TClient : IClientBase
    {
        bool ValidateState(TClient c);
        void HandlePacket(InPacket p, TClient c);
    }
}
