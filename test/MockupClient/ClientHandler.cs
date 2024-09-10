using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace MockupClient
{
    public class ClientChannleInitializer : ChannelInitializer<ISocketChannel>
    {
        protected override void InitChannel(ISocketChannel channel)
        {
            // throw new NotImplementedException();
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            //if (message is byte[] bytes)
            //{
            //    var clientCrypto = new ClientCryptography(bytes);
            //}
        }
    }
}
