using Application.Core.Game.TheWorld;
using DotNetty.Transport.Channels.Sockets;
using net.server;
using net.server.coordinator.session;

namespace net.netty;

public class ChannelServerInitializer : ServerChannelInitializer
{
    readonly IWorldChannel worldChannel;
    public ChannelServerInitializer(IWorldChannel worldChannel)
    {
        this.worldChannel = worldChannel;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string clientIp = getRemoteAddress(socketChannel);
        Log.Logger.Debug("{ClientIP} 发起连接 客户端：{InstanceId} from ", clientIp, worldChannel.InstanceId);

        PacketProcessor packetProcessor = PacketProcessor.getChannelServerProcessor(worldChannel.InstanceId);
        long clientSessionId = sessionId.getAndIncrement();
        Client client = Client.createChannelClient(clientSessionId, socketChannel, packetProcessor, worldChannel);

        if (!worldChannel.IsRunning)
        {
            SessionCoordinator.getInstance().closeSession(client, true);
            socketChannel.CloseAsync().Wait();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
