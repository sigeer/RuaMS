/*
	This file is part of the OdinMS Maple Story Server
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


using client;
using client.autoban;
using net.packet;
using server;
using tools;

namespace net.server.channel.handlers;

public class PetChatHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        int petId = p.readInt();
        p.readInt();
        p.readByte();
        int act = p.readByte();
        sbyte pet = c.getPlayer().getPetIndex(petId);
        if ((pet < 0 || pet > 3) || (act < 0 || act > 9))
        {
            return;
        }
        string text = p.readString();
        if (text.Length > sbyte.MaxValue)
        {
            AutobanFactory.PACKET_EDIT.alert(c.getPlayer(), c.getPlayer().getName() + " tried to packet edit with pets.");
            log.Warning("Chr {CharacterName} tried to send text with length of {text.Length}", c.getPlayer().getName(), text.Length);
            c.disconnect(true, false);
            return;
        }
        c.getPlayer().getMap().broadcastMessage(c.getPlayer(), PacketCreator.petChat(c.getPlayer().getId(), pet, act, text), true);
        ChatLogger.log(c, "Pet", text);
    }
}
