using Application.Core.Game.TheWorld;
using Application.Shared.Servers;
using DotNetty.Transport.Channels.Sockets;
using net.server.coordinator.session;

namespace net.netty;

public class LoginServerInitializer : ServerChannelInitializer
{
    readonly IWorldLogin server;

    public LoginServerInitializer(IWorldLogin server)
    {
        this.server = server;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string remoteAddress = getRemoteAddress(socketChannel);
        Log.Logger.Debug("Client connected to login server from {ClientIP} ", remoteAddress);

        PacketProcessor packetProcessor = PacketProcessor.getLoginServerProcessor();
        long clientSessionId = sessionId.getAndIncrement();

        var client = Client.createLoginClient(clientSessionId, socketChannel, packetProcessor, server);

        if (!SessionCoordinator.getInstance().canStartLoginSession(client))
        {
            socketChannel.CloseAsync().Wait();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
