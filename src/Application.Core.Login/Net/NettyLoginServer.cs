using Application.Shared.Servers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;


namespace Application.Core.Login.Net;

public class NettyLoginServer : AbstractNettyServer
{
    public static int WORLD_ID = -1;
    public static int CHANNEL_ID = -1;
    private IChannel? nettyChannel;
    readonly MasterServer server;
    public NettyLoginServer(MasterServer server) : base(server.Port)
    {
        this.server = server;
    }

    public override async Task Start()
    {
        IEventLoopGroup parentGroup = new MultithreadEventLoopGroup();
        IEventLoopGroup childGroup = new MultithreadEventLoopGroup();
        ServerBootstrap bootstrap = new ServerBootstrap()
                .Group(parentGroup, childGroup)
                .Channel<TcpServerSocketChannel>()
                .ChildHandler(new LoginServerInitializer(server));

        this.nettyChannel = await bootstrap.BindAsync(port);
    }

    public override async Task Stop()
    {
        if (nettyChannel == null)
        {
            throw new Exception("Must start LoginServer before stopping it");
        }

        await nettyChannel.CloseAsync();
    }
}
