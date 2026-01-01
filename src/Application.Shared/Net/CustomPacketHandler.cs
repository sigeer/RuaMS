using Application.Shared.Client;

namespace Application.Shared.Net
{
    public class CustomPacketHandler<TClient> : IPacketHandlerBase<TClient> where TClient : IClientBase
    {
        public Task HandlePacket(InPacket p, TClient c)
        {
            if (p.available() > 0 && c.AccountGMLevel == 4)
            {
                //w/e
                c.sendPacket(PacketCommon.customPacket(p.readBytes(p.available())));
            }
            return Task.CompletedTask;
        }

        public bool ValidateState(TClient c)
        {
            return true;
        }
    }
}
