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
using server.maps;
using tools;
using tools.exceptions;

namespace net.server.channel.handlers;

public class MoveSummonHandler : AbstractMovementPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        int oid = p.readInt();
        Point startPos = p.readPos();
        Character player = c.getPlayer();
        var summons = player.getSummonsValues();
        Summon? summon = summons.FirstOrDefault(x => x.getObjectId() == oid);
        if (summon != null)
        {
            try
            {
                int movementDataStart = p.getPosition();
                updatePosition(p, summon, 0);
                long movementDataLength = p.getPosition() - movementDataStart; //how many bytes were read by updatePosition
                p.seek(movementDataStart);

                player.getMap().broadcastMessage(player, PacketCreator.moveSummon(player.getId(), oid, startPos, p, movementDataLength), summon.getPosition());
            }
            catch (EmptyMovementException e)
            {
            }
        }
    }
}
