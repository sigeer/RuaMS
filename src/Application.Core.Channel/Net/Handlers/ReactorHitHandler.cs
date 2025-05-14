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


using Application.Core.Client;
using Application.Core.Game.TheWorld;
using Microsoft.Extensions.Logging;
using net.packet;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Lerk
 */
public class ReactorHitHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        //Console.WriteLine(slea); //To see if there are any differences with packets
        //[CD 00] [6B 00 00 00] [01 00 00 00] [03 00] [00 00 20 03] [F7 03 00 00]
        //[CD 00] [66 00 00 00] [00 00 00 00] [02 00] [00 00 19 01] [00 00 00 00]
        int oid = p.readInt();
        int charPos = p.readInt();
        short stance = p.readShort();
        p.skip(4);
        int skillid = p.readInt();
        var reactor = c.OnlinedCharacter.getMap().getReactorByOid(oid);
        if (reactor != null)
        {
            reactor.hitReactor(true, charPos, stance, skillid, c);
        }
    }
}
