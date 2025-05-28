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
        string remoteAddress = getRemoteAddress(socketChannel);
        Log.Logger.Debug("Client connected to login server from {ClientIP} ", remoteAddress);

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
