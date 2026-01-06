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


using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author XoticStory
 * @author BubblesDev
 * @author Ronan
 */
public class PartySearchStartHandler : ChannelHandlerBase
{
    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        int min = p.readInt();
        int max = p.readInt();

        c.OnlinedCharacter.dropMessage(1, "该功能已关闭");
        c.sendPacket(PacketCreator.enableActions());
        return Task.CompletedTask;

        var chr = c.OnlinedCharacter;
        if (min > max)
        {
            chr.dropMessage(1, "The min. value is higher than the max!");
            c.sendPacket(PacketCreator.enableActions());
            return Task.CompletedTask;
        }

        if (max - min > 30)
        {
            chr.dropMessage(1, "You can only search for party members within a range of 30 levels.");
            c.sendPacket(PacketCreator.enableActions());
            return Task.CompletedTask;
        }

        if (chr.getLevel() < min || chr.getLevel() > max)
        {
            chr.dropMessage(1, "The range of level for search has to include your own level.");
            c.sendPacket(PacketCreator.enableActions());
            return Task.CompletedTask;
        }

        p.readInt(); // members
        int jobs = p.readInt();

        if (!c.OnlinedCharacter.isPartyLeader())
        {
            return Task.CompletedTask;
        }
        return Task.CompletedTask;
        // var world = c.getWorldServer();
        // world.getPartySearchCoordinator().registerPartyLeader(chr, min, max, jobs);
    }
}