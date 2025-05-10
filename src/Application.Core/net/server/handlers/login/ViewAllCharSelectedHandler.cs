/*
 This file is part of the OdinMS Maple Story NewServer
 Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
 Matthias Butz <matze@odinms.de>
 Jan Christian Meyer <vimes@odinms.de>

 This program is free software: you can redistribute it and/or modify
 it under the terms of the GNU Affero General Public License as
 published by the Free Software Foundation version 3 as published by
 the Free Software Foundation. You may not use, modify or distribute
 this program under any other version of the GNU Affero General Public
 License.

 This program is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 GNU Affero General Public License for more details.

 You should have received a copy of the GNU Affero General Public License
 along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using net.packet;
using net.server.coordinator.session;
using System.Net;
using tools;
using static net.server.coordinator.session.SessionCoordinator;

namespace net.server.handlers.login;


public class ViewAllCharSelectedHandler : AbstractPacketHandler
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

        try
        {
            int channel = Randomizer.rand(1, wserv.getChannelsSize());
            c.setChannel(channel);
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
            c.setChannel(1);
        }

        var socket = server.GetChannelEndPoint(c, c.getWorld(), c.Channel);
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
