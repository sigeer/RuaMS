using Application.Shared.Client;

namespace Application.Shared.Net
{
    public class CustomPacketHandler<TClient> : IPacketHandlerBase<TClient> where TClient : IClientBase
    {
        public async Task HandlePacket(InPacket p, TClient c)
        {
            if (p.available() > 0 && c.AccountGMLevel == 4)
            {
                //w/e
                await c.SendPacket(PacketCommon.customPacket(p.readBytes(p.available())));
            }
        }

        public bool ValidateState(TClient c)
        {
            return true;
        }
    }
}
