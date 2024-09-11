using DotNetty.Transport.Channels.Sockets;
using net.server.coordinator.session;

namespace net.netty;

public class LoginServerInitializer : ServerChannelInitializer
{
    private static ILogger log = LogFactory.GetLogger("ServerChannelInitializer/Login");

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string clientIp = socketChannel.RemoteAddress.GetIPv4Address();
        log.Debug("Client connected to login server from {ClientIP} ", clientIp);

        PacketProcessor packetProcessor = PacketProcessor.getLoginServerProcessor();
        long clientSessionId = sessionId.getAndIncrement();
        string remoteAddress = getRemoteAddress(socketChannel);
        var client = Client.createLoginClient(clientSessionId, remoteAddress, packetProcessor, LoginServer.WORLD_ID, LoginServer.CHANNEL_ID);

        if (!SessionCoordinator.getInstance().canStartLoginSession(client))
        {
            socketChannel.CloseAsync().Wait();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
