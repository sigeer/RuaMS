using Application.Core.Game.TheWorld;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.DependencyInjection;
using net.netty;
using Serilog;

namespace Application.Core.Channel.Net;

public class ChannelServerInitializer : ServerChannelInitializer
{
    IWorldChannel worldChannel;

    public ChannelServerInitializer(IWorldChannel channel)
    {
        this.worldChannel = channel;
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

        long clientSessionId = sessionId.getAndIncrement();
        var client = ActivatorUtilities.CreateInstance<ChannelClient>(worldChannel.LifeScope.ServiceProvider, sessionId, socketChannel, worldChannel);
        initPipeline(socketChannel, client);
    }
}
