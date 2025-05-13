using Application.Core.Game.TheWorld;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using net;
using net.netty;
using Serilog;

namespace Application.Core.Channel.Net;

public class ChannelServerInitializer : ServerChannelInitializer
{
    readonly IServiceProvider _serviceProvider;
    readonly IWorldChannel worldChannel;
    public ChannelServerInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        worldChannel = _serviceProvider.GetRequiredService<IWorldChannel>();
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        if (!worldChannel.IsRunning)
        {
            socketChannel.CloseAsync().Wait();
            return;
        }

        string clientIp = getRemoteAddress(socketChannel);
        Log.Logger.Debug("{ClientIP} 发起连接到频道{Channel}", clientIp, worldChannel.getId());

        PacketProcessor packetProcessor = PacketProcessor.getChannelServerProcessor(worldChannel.InstanceId);
        long clientSessionId = sessionId.getAndIncrement();
        var client = new ChannelClient(clientSessionId, worldChannel, socketChannel,
                        _serviceProvider.GetRequiredService<ChannelPacketProcessor>(),
            _serviceProvider.GetRequiredService<ILogger<IClientBase>>()!);

        initPipeline(socketChannel, client);
    }
}
