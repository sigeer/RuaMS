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
using Application.Utility.Configs;
using net.packet;
using net.server.coordinator.world;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Jay Estrella
 * @author Ubaware
 */
public class FamilyAddHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (!YamlConfig.config.server.USE_FAMILY_SYSTEM)
        {
            return;
        }
        string toAdd = p.readString();
        var addChr = c.CurrentServer.getPlayerStorage().getCharacterByName(toAdd);
        var chr = c.OnlinedCharacter;
        if (addChr == null)
        {
            c.sendPacket(PacketCreator.sendFamilyMessage(65, 0));
        }
        else if (addChr == chr)
        { //only possible through packet editing/client editing i think?
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (addChr.getMap() != chr.getMap() || (addChr.isHidden()) && chr.gmLevel() < addChr.gmLevel())
        {
            c.sendPacket(PacketCreator.sendFamilyMessage(69, 0));
        }
        else if (addChr.getLevel() <= 10)
        {
            c.sendPacket(PacketCreator.sendFamilyMessage(77, 0));
        }
        else if (Math.Abs(addChr.getLevel() - chr.getLevel()) > 20)
        {
            c.sendPacket(PacketCreator.sendFamilyMessage(72, 0));
        }
        else if (addChr.getFamily() != null && addChr.getFamily() == chr.getFamily())
        { //same family
            c.sendPacket(PacketCreator.enableActions());
        }
        else if (InviteType.FAMILY.HasRequest(addChr.getId()))
        {
            c.sendPacket(PacketCreator.sendFamilyMessage(73, 0));
        }
        else if (chr.getFamily() != null && addChr.getFamily() != null && addChr.getFamily().getTotalGenerations() + chr.getFamily().getTotalGenerations() > YamlConfig.config.server.FAMILY_MAX_GENERATIONS)
        {
            c.sendPacket(PacketCreator.sendFamilyMessage(76, 0));
        }
        else
        {
            InviteType.FAMILY.CreateInvite(new InviteRequest(chr, addChr));
            addChr.getClient().sendPacket(PacketCreator.sendFamilyInvite(chr.getId(), chr.getName()));
            chr.dropMessage("The invite has been sent.");
            c.sendPacket(PacketCreator.enableActions());
        }
    }
}
