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


using Application.Core.Game.Invites;
using net.packet;
using net.server.coordinator.world;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class DenyPartyRequestHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readByte();
        string[] cname = p.readString().Split("PS: ");

        var cfrom = c.CurrentServer.getPlayerStorage().getCharacterByName(cname[cname.Length - 1]);
        if (cfrom != null)
        {
            var chr = c.OnlinedCharacter;

            if (InviteType.PARTY.AnswerInvite(chr.getId(), cfrom.getPartyId(), false).Result == InviteResultType.DENIED)
            {
                chr.updatePartySearchAvailability(chr.getParty() == null);
                cfrom.sendPacket(PacketCreator.partyStatusMessage(23, chr.getName()));
            }
        }
    }
}
