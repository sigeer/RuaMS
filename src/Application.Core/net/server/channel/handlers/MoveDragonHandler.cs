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
public class MoveDragonHandler : AbstractMovementPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        var chr = c.OnlinedCharacter;
        var startPos = p.readPos();
        var dragon = chr.getDragon();
        if (dragon != null)
        {
            try
            {
                int movementDataStart = p.getPosition();
                updatePosition(p, dragon, 0);
                long movementDataLength = p.getPosition() - movementDataStart; //how many bytes were read by updatePosition
                p.seek(movementDataStart);

                if (chr.isHidden())
                {
                    chr.getMap().broadcastGMPacket(chr, PacketCreator.moveDragon(dragon, startPos, p, movementDataLength));
                }
                else
                {
                    chr.getMap().broadcastMessage(chr, PacketCreator.moveDragon(dragon, startPos, p, movementDataLength), dragon.getPosition());
                }
            }
            catch (EmptyMovementException e)
            {
                log.Error(e.ToString());
            }
        }
    }
}