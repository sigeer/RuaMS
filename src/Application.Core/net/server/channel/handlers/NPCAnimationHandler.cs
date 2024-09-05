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


using net.opcodes;
using net.packet;

namespace net.server.channel.handlers;

public class NPCAnimationHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        if (c.OnlinedCharacter.isChangingMaps())
        {   // possible cause of error 38 in some map transition scenarios, thanks Arnah
            return;
        }

        OutPacket op = OutPacket.create(SendOpcode.NPC_ACTION);
        int length = p.available();
        if (length == 6)
        { // NPC Talk
            op.writeInt(p.readInt());
            op.writeByte(p.readByte());   // 2 bytes, thanks resinate
            op.writeByte(p.readByte());
        }
        else if (length > 6)
        { // NPC Move
            byte[] bytes = p.readBytes(length - 9);
            op.writeBytes(bytes);
        }
        c.sendPacket(op);
    }
}
