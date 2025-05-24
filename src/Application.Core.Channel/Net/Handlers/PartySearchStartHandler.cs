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

/**
 * @author XoticStory
 * @author BubblesDev
 * @author Ronan
 */
public class PartySearchStartHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int min = p.readInt();
        int max = p.readInt();

        var chr = c.OnlinedCharacter;
        if (min > max)
        {
            chr.dropMessage(1, "The min. value is higher than the max!");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (max - min > 30)
        {
            chr.dropMessage(1, "You can only search for party members within a range of 30 levels.");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (chr.getLevel() < min || chr.getLevel() > max)
        {
            chr.dropMessage(1, "The range of level for search has to include your own level.");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        p.readInt(); // members
        int jobs = p.readInt();

        var party = c.OnlinedCharacter.getParty();
        if (party == null || !c.OnlinedCharacter.isPartyLeader())
        {
            return;
        }

        var world = c.getWorldServer();
        world.getPartySearchCoordinator().registerPartyLeader(chr, min, max, jobs);
    }
}