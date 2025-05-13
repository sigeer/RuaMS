using Application.Core.Client;
using Application.Core.Login.Database;
using Application.Core.Login.Net.Packets;
using Application.Core.Servers;
using Microsoft.Extensions.Logging;
using net.packet;
using net.server.coordinator.session;
using tools;
using static net.server.coordinator.session.SessionCoordinator;

namespace Application.Core.Login.Net.Handlers;


public class ViewAllCharRegisterPicHandler : LoginHandlerBase
{
    public ViewAllCharRegisterPicHandler(IMasterServer server, AccountManager accountManager, ILogger<LoginHandlerBase> logger) : base(server, accountManager, logger)
    {
    }

    private static int parseAntiMulticlientError(AntiMulticlientResult res)
    {
        return (res) switch
        {
            AntiMulticlientResult.REMOTE_PROCESSING => 10,
            AntiMulticlientResult.REMOTE_LOGGEDIN => 7,
            AntiMulticlientResult.REMOTE_NO_MATCH => 17,
            AntiMulticlientResult.COORDINATOR_ERROR => 8,
            _ => 9
        };
    }

    public override void HandlePacket(InPacket p, ILoginClient c)
    {
        p.readByte();
        int charId = p.readInt();
        p.readInt(); // please don't let the client choose which world they should login

        string mac = p.readString();
        string hostString = p.readString();

        Hwid hwid;
        try
        {
            hwid = Hwid.fromHostString(hostString);
        }
        catch (ArgumentException e)
        {
            log.Warning(e, "Invalid host string: {Host}", hostString);
            c.sendPacket(PacketCreator.getAfterLoginError(17));
            return;
        }

        c.updateMacs(mac);
        c.updateHwid(hwid);

        if (c.hasBannedMac() || c.hasBannedHWID())
        {
            SessionCoordinator.getInstance().closeSession(c, true);
            return;
        }

        AntiMulticlientResult res = SessionCoordinator.getInstance().attemptGameSession(c, c.getAccID(), hwid);
        if (res != AntiMulticlientResult.SUCCESS)
        {
            c.sendPacket(PacketCreator.getAfterLoginError(parseAntiMulticlientError(res)));
            return;
        }

        Server server = Server.getInstance();
        if (!server.haveCharacterEntry(c.getAccID(), charId))
        {
            SessionCoordinator.getInstance().closeSession(c, true);
            return;
        }

        c.setWorld(server.getCharacterWorld(charId));
        var wserv = c.getWorldServer();
        if (wserv == null || wserv.isWorldCapacityFull())
        {
            c.sendPacket(PacketCreator.getAfterLoginError(10));
            return;
        }

        int channel = Randomizer.rand(1, server.getWorld(c.getWorld()).getChannelsSize());
        c.setChannel(channel);

        string pic = p.readString();
        c.setPic(pic);

        var socket = server.GetChannelEndPoint(c, c.getWorld(), channel);
        if (socket == null)
        {
            c.sendPacket(PacketCreator.getAfterLoginError(10));
            return;
        }

        server.unregisterLoginState(c);
        c.setCharacterOnSessionTransitionState(charId);

        try
        {
            c.sendPacket(PacketCreator.getServerIP(socket, charId));
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }
    }
}
