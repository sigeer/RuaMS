

using Application.Core.Channel;
using Application.Shared.Servers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Application.Core.Channel.Net;

public class ChannelServer : AbstractServer
{
    private IChannel? nettyChannel;
    readonly WorldChannel worldChannel;
    public ChannelServer(WorldChannel worldChannel) : base(worldChannel.Port)
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
