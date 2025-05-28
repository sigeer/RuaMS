using Application.Core.Login.Client;
using Application.Core.Login.Net.Packets;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server;
using tools;

namespace Application.Core.Login.Net.Handlers;

public class ServerlistRequestHandler : LoginHandlerBase
{
    public ServerlistRequestHandler(MasterServer server, ILogger<LoginHandlerBase> logger)
        : base(server, logger)
    {
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        c.sendPacket(PacketCreator.getServerList(_server.Id, _server.Name, _server.Flag, _server.EventMessage, Server.getInstance().getWorld(0).Channels));
        c.sendPacket(PacketCreator.getEndOfServerList());
        c.sendPacket(PacketCreator.selectWorld(0));
        c.sendPacket(LoginPacketCreator.SendRecommended(_server));
    }
}