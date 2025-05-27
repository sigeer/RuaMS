using Application.Core.Net;
using net.packet;

namespace Application.Core.Channel.Net
{
    public abstract class ChannelHandlerBase : IChannelHandler
    {
        public abstract void HandlePacket(InPacket p, ChannelClient c);
        public virtual bool ValidateState(ChannelClient c)
        {
            return c.IsOnlined;
        }
    }
}
