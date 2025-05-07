

using Application.Core.Game.TheWorld;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace net.netty;

public class ChannelServer : AbstractServer
{
    private IChannel? nettyChannel;
    readonly IWorldChannel worldChannel;
    public ChannelServer(IWorldChannel worldChannel) : base(worldChannel.Port)
    {
        this.worldChannel = worldChannel;
    }

    public override async Task Start()
    {
        IEventLoopGroup parentGroup = new MultithreadEventLoopGroup();
        IEventLoopGroup childGroup = new MultithreadEventLoopGroup();
        ServerBootstrap bootstrap = new ServerBootstrap()
                .Group(parentGroup, childGroup)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new ChannelServerInitializer(worldChannel));

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
