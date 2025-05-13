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


using Application.Core.Game.Maps;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 */
public class DoorHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int ownerid = p.readInt();
        p.readByte(); // specifies if backwarp or not, 1 town to target, 0 target to town

        var chr = c.OnlinedCharacter;
        if (chr.isChangingMaps() || chr.isBanned())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        foreach (var obj in chr.getMap().getMapObjects())
        {
            if (obj is DoorObject door)
            {
                if (door.getOwnerId() == ownerid)
                {
                    door.warp(chr);
                    return;
                }
            }
        }

        c.sendPacket(PacketCreator.blockedMessage(6));
        c.sendPacket(PacketCreator.enableActions());
    }
}
