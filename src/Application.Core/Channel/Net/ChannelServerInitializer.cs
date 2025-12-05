using Application.Shared.Servers;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.DependencyInjection;
using net.netty;

namespace Application.Core.Channel.Net;

public class ChannelServerInitializer : ServerChannelInitializer
{
    WorldChannelServer _server;
    ChannelConfig _config;
    public ChannelServerInitializer(WorldChannelServer server, ChannelConfig config)
    {
        _server = server;
        _config = config;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        if (!_server.ServerConfigMapping.TryGetValue(_config, out var worldChannel) || !worldChannel.IsRunning)
        {
            socketChannel.CloseAsync();
            return;
        }

        string clientIp = getRemoteAddress(socketChannel);
        Log.Logger.Debug("{ClientIP} 发起连接到频道{Channel}", clientIp, worldChannel.Id);

        long clientSessionId = sessionId.getAndIncrement();
        var client = ActivatorUtilities.CreateInstance<ChannelClient>(worldChannel.LifeScope.ServiceProvider, clientSessionId, worldChannel, socketChannel);
        initPipeline(socketChannel, client);
    }
}
