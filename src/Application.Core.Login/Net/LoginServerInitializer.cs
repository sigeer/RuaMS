using Application.Core.Login.Client;
using Application.Core.Login.Session;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Microsoft.Extensions.DependencyInjection;
using net.netty;
using Serilog;

namespace Application.Core.Login.Net;

public class LoginServerInitializer : ServerChannelInitializer
{
    readonly MasterServer masterServer;
    readonly SessionCoordinator sessionCoordinator;

    public LoginServerInitializer(MasterServer masterServer)
    {
        this.masterServer = masterServer;
        this.sessionCoordinator = masterServer.ServiceProvider.GetRequiredService<SessionCoordinator>();
    }

    protected override void InitChannel(ISocketChannel socketChannel)
    {
        if (!masterServer.IsRunning || masterServer.IsShuttingdown)
        {
            socketChannel.CloseAsync().Wait();
            return;
        }

        string remoteAddress = getRemoteAddress(socketChannel);
        Log.Logger.Debug("{ClientIP} 发起连接到登录服务器", remoteAddress);

        long clientSessionId = sessionId.getAndIncrement();

        var client = ActivatorUtilities.CreateInstance<LoginClient>(masterServer.ServiceProvider, clientSessionId, masterServer, socketChannel as IChannel);
        if (!sessionCoordinator.canStartLoginSession(client))
        {
            client.CloseSocket();
            return;
        }

        initPipeline(socketChannel, client);
    }
}
