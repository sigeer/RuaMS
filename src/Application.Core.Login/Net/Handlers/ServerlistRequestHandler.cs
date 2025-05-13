using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Login.Net.Packets;
using Application.Core.Servers;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Login.Net.Handlers;

public class ServerlistRequestHandler : LoginHandlerBase
{
    public ServerlistRequestHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger)
        : base(server, accountManager, logger)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        c.sendPacket(PacketCreator.getServerList(_server.Id, _server.Name, _server.Flag, _server.EventMessage, _server.Channels));
        c.sendPacket(PacketCreator.getEndOfServerList());
        c.sendPacket(PacketCreator.selectWorld(0));
        c.sendPacket(_packetCreator.SendRecommended());
    }
}