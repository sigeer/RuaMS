using net.packet;
using net.server.coordinator.session;
using System.Net;
using tools;
using static net.server.coordinator.session.SessionCoordinator;

namespace net.server.handlers.login;




public class ViewAllCharSelectedWithPicHandler : AbstractPacketHandler
{
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

    public override void HandlePacket(InPacket p, IClient c)
    {

        string pic = p.readString();
        int charId = p.readInt();
        p.readInt(); // please don't let the client choose which world they should login

        string macs = p.readString();
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

        c.updateMacs(macs);
        c.updateHwid(hwid);

        if (c.hasBannedMac() || c.hasBannedHWID())
        {
            SessionCoordinator.getInstance().closeSession(c, true);
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

        int channel = Randomizer.rand(1, wserv.getChannelsSize());
        c.setChannel(channel);

        if (c.checkPic(pic))
        {
            string[] socket = server.getInetSocket(c, c.getWorld(), c.getChannel());
            if (socket == null)
            {
                c.sendPacket(PacketCreator.getAfterLoginError(10));
                return;
            }

            AntiMulticlientResult res = SessionCoordinator.getInstance().attemptGameSession(c, c.getAccID(), hwid);
            if (res != AntiMulticlientResult.SUCCESS)
            {
                c.sendPacket(PacketCreator.getAfterLoginError(parseAntiMulticlientError(res)));
                return;
            }

            server.unregisterLoginState(c);
            c.setCharacterOnSessionTransitionState(charId);

            try
            {
                c.sendPacket(PacketCreator.getServerIP(IPAddress.Parse(socket[0]), int.Parse(socket[1]), charId));
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
            }

        }
        else
        {
            c.sendPacket(PacketCreator.wrongPic());
        }
    }
}
