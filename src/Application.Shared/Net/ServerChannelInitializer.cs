using Application.Shared.Client;
using Application.Shared.Constants;
using Application.Shared.Net;
using Application.Shared.Net.Encryption;
using Application.Shared.Net.Logging;
using Application.Utility.Compatible.Atomics;
using Application.Utility.Configs;
using Application.Utility.Extensions;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Serilog;

namespace net.netty;

public abstract class ServerChannelInitializer : ChannelInitializer<ISocketChannel>
{
    private static int IDLE_TIME_SECONDS = 30;

    protected static AtomicLong sessionId = new AtomicLong(7777);

    protected string getRemoteAddress(IChannel channel)
    {
        string remoteAddress = "null";
        try
        {
            remoteAddress = channel.RemoteAddress.GetIPAddressString();
        }
        catch (NullReferenceException npe)
        {
            Log.Logger.Warning(npe, "Unable to get remote address from netty Channel: {ChannelId}", channel);
        }

        return remoteAddress;
    }

    protected void initPipeline(ISocketChannel socketChannel, SocketClient client)
    {
        InitializationVector sendIv = InitializationVector.generateSend();
        InitializationVector recvIv = InitializationVector.generateReceive();
        writeInitialUnencryptedHelloPacket(socketChannel, sendIv, recvIv);
        setUpHandlers(socketChannel.Pipeline, sendIv, recvIv, client);
    }

    private void writeInitialUnencryptedHelloPacket(ISocketChannel socketChannel, InitializationVector sendIv, InitializationVector recvIv)
    {
        socketChannel.WriteAndFlushAsync(Unpooled.WrappedBuffer(PacketCommon.getHello(ServerConstants.VERSION, sendIv, recvIv).getBytes())).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    private void setUpHandlers(IChannelPipeline pipeline, InitializationVector sendIv, InitializationVector recvIv,
                               SocketClient client)
    {
        pipeline.AddLast("IdleStateHandler", new IdleStateHandler(0, 0, IDLE_TIME_SECONDS));
        pipeline.AddLast("PacketCodec", new PacketCodec(ClientCyphers.of(sendIv, recvIv)));
        pipeline.AddLast("Client", client);

        if (YamlConfig.config.server.USE_DEBUG_SHOW_PACKET)
        {
            pipeline.AddBefore("Client", "SendPacketLogger", new OutPacketLogger());
            pipeline.AddBefore("Client", "ReceivePacketLogger", new InPacketLogger());
        }
    }
}
