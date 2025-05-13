using Application.Core.Client;
using Application.Core.Game.TheWorld;
using Application.Core.Net;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Channel.Net
{
    public abstract class ChannelHandlerBase : IChannelHandler
    {
        public abstract void HandlePacket(InPacket p, IChannelClient c);
        public virtual bool ValidateState(IChannelClient c)
        {
            return c.IsOnlined;
        }
    }
}
