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

namespace Application.Core.Channel.Net.Handlers;

public class FaceExpressionHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        int emote = p.readInt();

        if (emote > 7)
        {
            int itemid = 5159992 + emote;   // thanks RajanGrewal (Darter) for reporting unchecked emote itemid
            if (!ItemId.isFaceExpression(itemid) || chr.getInventory(ItemConstants.getInventoryType(itemid)).findById(itemid) == null)
            {
                return;
            }
        }
        else if (emote < 1)
        {
            return;
        }

        if (c.tryacquireClient())
        {
            try
            {   // expecting players never intends to wear the emote 0 (default face, that changes back after 5sec timeout)
                if (chr.isLoggedinWorld())
                {
                    chr.changeFaceExpression(emote);
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
