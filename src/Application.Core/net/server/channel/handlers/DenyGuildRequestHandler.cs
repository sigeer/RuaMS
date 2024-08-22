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
using net.server.guild;

namespace net.server.channel.handlers;

/**
 * @author Xterminator
 */
public class DenyGuildRequestHandler : AbstractPacketHandler
{

    public override void handlePacket(InPacket p, Client c)
    {
        p.readByte();
        var cfrom = c.getWorldServer().getPlayerStorage().getCharacterByName(p.readString());
        if (cfrom != null)
        {
            Guild.answerInvitation(c.getPlayer().getId(), c.getPlayer().getName(), cfrom.getGuildId(), false);
        }
    }
}
