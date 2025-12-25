using Application.Shared.Servers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Application.Core.Channel.Net;

public class NettyChannelServer : AbstractNettyServer
{
    private IChannel? nettyChannel;
    WorldChannelServer _server;
    ChannelConfig _channelConfig;
    public NettyChannelServer(WorldChannelServer server, ChannelConfig channelConfig) : base(channelConfig.Port)
    {
        _server = server;
        _channelConfig = channelConfig;
    }

    public override async Task Start()
    {
        IEventLoopGroup parentGroup = new MultithreadEventLoopGroup();
        IEventLoopGroup childGroup = new MultithreadEventLoopGroup();
        ServerBootstrap bootstrap = new ServerBootstrap()
                .Group(parentGroup, childGroup)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new ChannelServerInitializer(_server, _channelConfig));

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

public record ChannelServerRecord(ChannelConfig Config, NettyChannelServer NettyServer);