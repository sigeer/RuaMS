using Application.Core.Client;
using Application.Core.Servers;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using net.netty;
using net.server.coordinator.session;
using Serilog;

namespace Application.Core.Login.Net;

public class LoginServerInitializer : ServerChannelInitializer
{
    IServiceProvider _serviceProvider;
    readonly IMasterServer masterServer;

    public LoginServerInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        masterServer = _serviceProvider.GetRequiredService<IMasterServer>()!;
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        string remoteAddress = getRemoteAddress(socketChannel);
        Log.Logger.Debug("Client connected to login server from {ClientIP} ", remoteAddress);

        long clientSessionId = sessionId.getAndIncrement();

        var client = new LoginClient(sessionId, masterServer, socketChannel,
            _serviceProvider.GetRequiredService<LoginPacketProcessor>(),
            _serviceProvider.GetRequiredService<ILogger<IClientBase>>()!);

        if (!SessionCoordinator.getInstance().canStartLoginSession(client))
        {
            client.CloseSocket();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
