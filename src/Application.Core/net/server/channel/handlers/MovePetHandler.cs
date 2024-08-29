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
using server.movement;
using tools;
using tools.exceptions;

namespace net.server.channel.handlers;



public class MovePetHandler : AbstractMovementPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        int petId = p.readInt();
        p.readLong();
        //        Point startPos = StreamUtil.readShortPoint(slea);
        List<LifeMovementFragment> res;

        try
        {
            res = parseMovement(p);
        }
        catch (EmptyMovementException e)
        {
            return;
        }
        var player = c.OnlinedCharacter;
        sbyte slot = player.getPetIndex(petId);
        if (slot == -1)
        {
            return;
        }
        player.getPet(slot)?.updatePosition(res);
        player.getMap().broadcastMessage(player, PacketCreator.movePet(player.getId(), petId, slot, res), false);
    }
}
