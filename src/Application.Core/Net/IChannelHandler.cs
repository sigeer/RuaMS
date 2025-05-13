using Application.Core.Client;
using net.packet;

namespace Application.Core.Net
{
    public interface IChannelHandler: IPacketHandlerBase<IChannelClient>
    {
    }
}
