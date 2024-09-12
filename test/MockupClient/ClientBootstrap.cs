using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using net.encryption;
using System.Net;
using tools;

namespace MockupClient
{
    public class ClientBootstrap
    {
        Bootstrap bootstrap;
        public ClientBootstrap()
        {
            var group = new MultithreadEventLoopGroup();
            bootstrap = new Bootstrap();
            bootstrap
            .Group(group)
                .Channel<TcpSocketChannel>()
                .Handler(new ClientChannleInitializer());
        }
        IChannel _channel;
        public async Task Initialize()
        {
            _channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7575));
        }

        public async Task Send(byte[] bytes)
        {
            await _channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(bytes));
        }
    }
}
