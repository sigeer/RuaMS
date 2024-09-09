using constants.net;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using net.encryption;
using net.packet.logging;
using tools;

namespace net.netty;




public abstract class ServerChannelInitializer : ChannelInitializer<ISocketChannel>
{
    private static ILogger log = LogFactory.GetLogger("ServerChannelInitializer");
    private static int IDLE_TIME_SECONDS = 30;
    private static bool LOG_PACKETS = YamlConfig.config.server.USE_DEBUG_SHOW_PACKET;
    private static IChannelHandler sendPacketLogger = new OutPacketLogger();
    private static IChannelHandler receivePacketLogger = new InPacketLogger();

    protected static AtomicLong sessionId = new AtomicLong(7777);

    protected string getRemoteAddress(IChannel channel)
    {
        string remoteAddress = "null";
        try
        {
            remoteAddress = channel.RemoteAddress.GetIPv4Address();
        }
        catch (NullReferenceException npe)
        {
            log.Warning(npe, "Unable to get remote address from netty Channel: {ChannelId}", channel);
        }

        return remoteAddress;
    }

    protected void initPipeline(ISocketChannel socketChannel, Client client)
    {
        InitializationVector sendIv = InitializationVector.generateSend();
        InitializationVector recvIv = InitializationVector.generateReceive();
        writeInitialUnencryptedHelloPacket(socketChannel, sendIv, recvIv);
        setUpHandlers(socketChannel.Pipeline, sendIv, recvIv, client);
    }

    private void writeInitialUnencryptedHelloPacket(ISocketChannel socketChannel, InitializationVector sendIv, InitializationVector recvIv)
    {
        socketChannel.WriteAndFlushAsync(Unpooled.WrappedBuffer(PacketCreator.getHello(ServerConstants.VERSION, sendIv, recvIv).getBytes())).Wait();
    }

    private void setUpHandlers(IChannelPipeline pipeline, InitializationVector sendIv, InitializationVector recvIv,
                               Client client)
    {
        pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, IDLE_TIME_SECONDS));
        pipeline.AddLast("PacketCodec", new PacketCodec(ClientCyphers.of(sendIv, recvIv)));
        pipeline.AddLast("Client", client);

        if (LOG_PACKETS)
        {
            pipeline.AddBefore("Client", "SendPacketLogger", sendPacketLogger);
            pipeline.AddBefore("Client", "ReceivePacketLogger", receivePacketLogger);
        }
    }
}
