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

    public override async Task HandlePacket(InPacket p, ILoginClient c)
    {
        await c.SendPacket(LoginPacketCreator.GetServerList(_server));
        await c.SendPacket(LoginPacketCreator.GetEndOfServerList());
        await c.SendPacket(LoginPacketCreator.SelectWorld(0));
        await c.SendPacket(LoginPacketCreator.SendRecommended(_server));
    }
}