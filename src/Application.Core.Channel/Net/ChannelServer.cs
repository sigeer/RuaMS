

using Application.Core.Game.TheWorld;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using net.netty;

namespace Application.Core.Channel.Net;

public class ChannelServer : AbstractServer
{
    private IChannel? nettyChannel;
    readonly ChannelServerInitializer _initializer;
    public ChannelServer(IWorldChannel worldChannel, ChannelServerInitializer initializer) : base(worldChannel.Port)
    {
        _initializer = initializer;
    }

    public override async Task Start()
    {
        IEventLoopGroup parentGroup = new MultithreadEventLoopGroup();
        IEventLoopGroup childGroup = new MultithreadEventLoopGroup();
        ServerBootstrap bootstrap = new ServerBootstrap()
                .Group(parentGroup, childGroup)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(_initializer);

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
