using Application.Shared.Client;

namespace Application.Shared.Net
{
    public class KeepAliveHandler<TClient> : IPacketHandlerBase<TClient> where TClient : IClientBase
    {
        public Task HandlePacket(InPacket p, TClient c)
        {
            c.PongReceived();
            return Task.CompletedTask;
        }

        public bool ValidateState(TClient c)
        {
            return true;
        }
    }

}
