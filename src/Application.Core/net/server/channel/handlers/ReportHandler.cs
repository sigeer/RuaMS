/*
	This file is part of the OdinMS Maple Story Server
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


using client;
using net.packet;
using tools;

namespace net.server.channel.handlers;


/*
 *
 * @author BubblesDev
 */
public class ReportHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        int type = p.readByte(); //00 = Illegal program claim, 01 = Conversation claim
        string victim = p.readString();
        int reason = p.readByte();
        string description = p.readString();
        if (type == 0)
        {
            if (c.getPlayer().getPossibleReports() > 0)
            {
                if (c.getPlayer().getMeso() > 299)
                {
                    c.getPlayer().decreaseReports();
                    c.getPlayer().gainMeso(-300, true);
                }
                else
                {
                    c.sendPacket(PacketCreator.reportResponse(4));
                    return;
                }
            }
            else
            {
                c.sendPacket(PacketCreator.reportResponse(2));
                return;
            }
            Server.getInstance().broadcastGMMessage(c.getWorld(), PacketCreator.serverNotice(6, victim + " was reported for: " + description));
            addReport(c.getPlayer().getId(), Character.getIdByName(victim), 0, description, "");
        }
        else if (type == 1)
        {
            string chatlog = p.readString();
            if (chatlog == null)
            {
                return;
            }
            if (c.getPlayer().getPossibleReports() > 0)
            {
                if (c.getPlayer().getMeso() > 299)
                {
                    c.getPlayer().decreaseReports();
                    c.getPlayer().gainMeso(-300, true);
                }
                else
                {
                    c.sendPacket(PacketCreator.reportResponse(4));
                    return;
                }
            }
            Server.getInstance().broadcastGMMessage(c.getWorld(), PacketCreator.serverNotice(6, victim + " was reported for: " + description));
            addReport(c.getPlayer().getId(), Character.getIdByName(victim), reason, description, chatlog);
        }
        else
        {
            Server.getInstance().broadcastGMMessage(c.getWorld(), PacketCreator.serverNotice(6, c.getPlayer().getName() + " is probably packet editing. Got unknown report type, which is impossible."));
        }
    }

    private void addReport(int reporterid, int victimid, int reason, string description, string chatlog)
    {
        using var dbContext = new DBContext();
        dbContext.Reports.Add(new Report
        {
            Reporttime = DateTimeOffset.Now,
            Reporterid = reporterid,
            Victimid = victimid,
            Reason = (sbyte)reason,
            Chatlog = chatlog,
            Description = description
        });
        dbContext.SaveChanges();
    }
}
