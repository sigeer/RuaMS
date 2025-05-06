using Application.Core.Game.TheWorld;
using Application.Shared.Servers;
using DotNetty.Transport.Channels.Sockets;
using net.server;
using net.server.coordinator.session;

namespace net.netty;

public class ChannelServerInitializer : ServerChannelInitializer
{
    IWorldChannel actualServer;
    public ChannelServerInitializer(IWorldChannel actualServer)
    {
        this.actualServer = actualServer;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string clientIp = getRemoteAddress(socketChannel);
        Log.Logger.Debug("Client connecting to channel {ChannelId} from {ClientIP}", actualServer.getId(), clientIp);

        PacketProcessor packetProcessor = PacketProcessor.getChannelServerProcessor(actualServer.getId());
        long clientSessionId = sessionId.getAndIncrement();
        Client client = Client.CreateChannelClient(clientSessionId, socketChannel, packetProcessor, actualServer);

        if (!actualServer.IsRunning)
        {
            SessionCoordinator.getInstance().closeSession(client, true);
            socketChannel.CloseAsync().Wait();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
