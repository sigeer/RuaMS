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
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class SpouseChatHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readString();//recipient
        string msg = p.readString();

        int partnerId = c.OnlinedCharacter.getPartnerId();
        if (partnerId > 0)
        {
            // yay marriage
            var spouse = c.getWorldServer().getPlayerStorage().getCharacterById(partnerId);
            if (spouse != null && spouse.isLoggedinWorld())
            {
                spouse.sendPacket(PacketCreator.OnCoupleMessage(c.OnlinedCharacter.getName(), msg, true));
                c.sendPacket(PacketCreator.OnCoupleMessage(c.OnlinedCharacter.getName(), msg, true));
                // ChatLogger.log(c, "Spouse", msg);
            }
            else
            {
                c.OnlinedCharacter.dropMessage(5, "Your spouse is currently offline.");
            }
        }
        else
        {
            c.OnlinedCharacter.dropMessage(5, "You don't have a spouse.");
        }
    }
}
