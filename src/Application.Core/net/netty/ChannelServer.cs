

using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace net.netty;

public class ChannelServer : AbstractServer
{
    private int world;
    private int channel;
    private IChannel? nettyChannel;

    public ChannelServer(int port, int world, int channel) : base(port)
    {
        this.world = world;
        this.channel = channel;
    }

    public override async Task Start()
    {
        IEventLoopGroup parentGroup = new MultithreadEventLoopGroup();
        IEventLoopGroup childGroup = new MultithreadEventLoopGroup();
        ServerBootstrap bootstrap = new ServerBootstrap()
                .Group(parentGroup, childGroup)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new ChannelServerInitializer(world, channel));

        this.nettyChannel = await bootstrap.BindAsync(port);
    }

    public override async Task Stop()
    {
        if (nettyChannel == null)
        {
            throw new Exception("Must start ChannelServer before stopping it");
        }

        await nettyChannel.CloseAsync();
    }
}
