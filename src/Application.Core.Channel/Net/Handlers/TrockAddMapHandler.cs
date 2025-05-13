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
using server.maps;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class TrockAddMapHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        byte type = p.readByte();
        bool vip = p.readByte() == 1;
        if (type == 0x00)
        {
            int mapId = p.readInt();
            if (vip)
            {
                chr.deleteFromVipTrocks(mapId);
            }
            else
            {
                chr.deleteFromTrocks(mapId);
            }
            c.sendPacket(PacketCreator.trockRefreshMapList(chr, true, vip));
        }
        else if (type == 0x01)
        {
            if (!FieldLimit.CANNOTVIPROCK.check(chr.getMap().getFieldLimit()))
            {
                if (vip)
                {
                    chr.addVipTrockMap();
                }
                else
                {
                    chr.addTrockMap();
                }

                c.sendPacket(PacketCreator.trockRefreshMapList(chr, false, vip));
            }
            else
            {
                chr.message("You may not save this map.");
            }
        }
    }
}
