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


using Application.Core.Client;
using Application.Core.Game.TheWorld;
using Application.Core.Managers;
using Application.EF;
using Application.EF.Entities;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/*
 *
 * @author BubblesDev
 */
public class ReportHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int type = p.ReadSByte(); //00 = Illegal program claim, 01 = Conversation claim
        string victim = p.readString();
        int reason = p.ReadSByte();
        string description = p.readString();
        if (type == 0)
        {
            if (c.OnlinedCharacter.getPossibleReports() > 0)
            {
                if (c.OnlinedCharacter.getMeso() > 299)
                {
                    c.OnlinedCharacter.decreaseReports();
                    c.OnlinedCharacter.gainMeso(-300, true);
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
            c.CurrentServer.BroadcastWorldGMPacket(PacketCreator.serverNotice(6, victim + " was reported for: " + description));
            addReport(c.OnlinedCharacter.getId(), CharacterManager.getIdByName(victim), 0, description, "");
        }
        else if (type == 1)
        {
            string chatlog = p.readString();
            if (chatlog == null)
            {
                return;
            }
            if (c.OnlinedCharacter.getPossibleReports() > 0)
            {
                if (c.OnlinedCharacter.getMeso() > 299)
                {
                    c.OnlinedCharacter.decreaseReports();
                    c.OnlinedCharacter.gainMeso(-300, true);
                }
                else
                {
                    c.sendPacket(PacketCreator.reportResponse(4));
                    return;
                }
            }
            c.CurrentServer.BroadcastWorldGMPacket(PacketCreator.serverNotice(6, victim + " was reported for: " + description));
            addReport(c.OnlinedCharacter.getId(), CharacterManager.getIdByName(victim), reason, description, chatlog);
        }
        else
        {
            c.CurrentServer.BroadcastWorldGMPacket(PacketCreator.serverNotice(6, c.OnlinedCharacter.getName() + " is probably packet editing. Got unknown report type, which is impossible."));
        }
    }

    private void addReport(int reporterid, int victimid, int reason, string description, string chatlog)
    {
        using var dbContext = new DBContext();
        dbContext.Reports.Add(new Report
        {
            Reporttime = DateTimeOffset.UtcNow,
            Reporterid = reporterid,
            Victimid = victimid,
            Reason = (sbyte)reason,
            Chatlog = chatlog,
            Description = description
        });
        dbContext.SaveChanges();
    }
}
