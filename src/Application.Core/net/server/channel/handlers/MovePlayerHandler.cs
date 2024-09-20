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
using tools;
using tools.exceptions;

namespace net.server.channel.handlers;

public class MovePlayerHandler : AbstractMovementPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        p.skip(9);
        try
        {   // thanks Sa for noticing empty movement sequences crashing players
            int movementDataStart = p.getPosition();
            updatePosition(p, c.OnlinedCharacter, 0);
            long movementDataLength = p.getPosition() - movementDataStart; //how many bytes were read by updatePosition
            p.seek(movementDataStart);

            c.OnlinedCharacter.getMap().movePlayer(c.OnlinedCharacter, c.OnlinedCharacter.getPosition());
            if (c.OnlinedCharacter.isHidden())
            {
                c.OnlinedCharacter.getMap().broadcastGMMessage(c.OnlinedCharacter, PacketCreator.movePlayer(c.OnlinedCharacter.getId(), p, movementDataLength), false);
            }
            else
            {
                c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.movePlayer(c.OnlinedCharacter.getId(), p, movementDataLength), false);
            }
        }
        catch (EmptyMovementException e)
        {
            log.Error(e.ToString());
        }
    }
}
