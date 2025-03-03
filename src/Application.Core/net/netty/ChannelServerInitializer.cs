using DotNetty.Transport.Channels.Sockets;
using net.server;
using net.server.coordinator.session;

namespace net.netty;

public class ChannelServerInitializer : ServerChannelInitializer
{
    private int world;
    private int channel;

    public ChannelServerInitializer(int world, int channel)
    {
        this.world = world;
        this.channel = channel;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string clientIp = socketChannel.RemoteAddress.GetIPv4Address();
        Log.Logger.Debug("Client connecting to world {WorldId}, channel {ChannelId} from {ClientIP}", world, channel, clientIp);

        PacketProcessor packetProcessor = PacketProcessor.getChannelServerProcessor(world, channel);
        long clientSessionId = sessionId.getAndIncrement();
        string remoteAddress = getRemoteAddress(socketChannel);
        Client client = Client.createChannelClient(clientSessionId, remoteAddress, packetProcessor, world, channel);

        if (Server.getInstance().getChannel(world, channel) == null)
        {
            SessionCoordinator.getInstance().closeSession(client, true);
            socketChannel.CloseAsync().Wait();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
