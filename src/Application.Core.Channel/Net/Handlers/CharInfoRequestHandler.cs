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

namespace Application.Core.Channel.Net.Handlers;

public class CharInfoRequestHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.skip(4);
        int cid = p.readInt();
        var target = c.OnlinedCharacter.getMap().getMapObject(cid);
        if (target != null)
        {
            if (target is IPlayer player)
            {
                if (c.OnlinedCharacter.getId() != player.getId())
                {
                    player.exportExcludedItems(c);
                }
                c.sendPacket(PacketCreator.charInfo(player));
            }
        }
    }
}
