using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System.Net;

namespace ServiceTest.Net
{
    public class MockupClient
    {
        Bootstrap bootstrap;
        public MockupClient()
        {
            var group = new MultithreadEventLoopGroup();

            bootstrap = new Bootstrap();
            bootstrap
                .Group(group)
                .Channel<TcpSocketChannel>();
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
