using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Microsoft.Extensions.Logging;

namespace Application.Core.Login.Net.Handlers;

public class ServerlistRequestHandler : LoginHandlerBase
{
    public ServerlistRequestHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override Task HandlePacket(InPacket p, ILoginClient c)
    {
        c.sendPacket(LoginPacketCreator.GetServerList(_server));
        c.sendPacket(LoginPacketCreator.GetEndOfServerList());
        c.sendPacket(LoginPacketCreator.SelectWorld(0));
        c.sendPacket(LoginPacketCreator.SendRecommended(_server));
        return Task.CompletedTask;
    }
}