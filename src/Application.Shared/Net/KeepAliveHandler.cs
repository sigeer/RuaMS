using Application.Shared.Client;

namespace Application.Shared.Net
{
    public class KeepAliveHandler<TClient> : IPacketHandlerBase<TClient> where TClient : IClientBase
    {
        public void HandlePacket(InPacket p, TClient c)
        {
            c.PongReceived();
        }

        public bool ValidateState(TClient c)
        {
            return true;
        }
    }

}
