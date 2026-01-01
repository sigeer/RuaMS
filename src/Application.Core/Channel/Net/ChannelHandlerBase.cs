using Application.Core.Net;

namespace Application.Core.Channel.Net
{
    public abstract class ChannelHandlerBase : IChannelHandler
    {
        public abstract Task HandlePacket(InPacket p, IChannelClient c);
        public virtual bool ValidateState(IChannelClient c)
        {
            return c.IsOnlined;
        }
    }
}
