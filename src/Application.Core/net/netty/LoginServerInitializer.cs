using Application.Core.Servers;
using DotNetty.Transport.Channels.Sockets;
using net.server.coordinator.session;

namespace net.netty;

public class LoginServerInitializer : ServerChannelInitializer
{
    readonly IMasterServer masterServer;

    public LoginServerInitializer(IMasterServer masterServer)
    {
        this.masterServer = masterServer;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string remoteAddress = getRemoteAddress(socketChannel);
        Log.Logger.Debug("Client connected to login server from {ClientIP} ", remoteAddress);

        PacketProcessor packetProcessor = PacketProcessor.getLoginServerProcessor(masterServer.InstanceId);
        long clientSessionId = sessionId.getAndIncrement();

        var client = Client.createLoginClient(clientSessionId, socketChannel, packetProcessor, masterServer);

        if (!SessionCoordinator.getInstance().canStartLoginSession(client))
        {
            socketChannel.CloseAsync().Wait();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
