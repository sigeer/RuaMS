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


using client.creator.novice;
using net.packet;
using tools;

namespace net.server.handlers.login;

public class CreateCharHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        string name = p.readString();
        int job = p.readInt();
        int face = p.readInt();

        int hair = p.readInt();
        int haircolor = p.readInt();
        int skincolor = p.readInt();

        int top = p.readInt();
        int bottom = p.readInt();
        int shoes = p.readInt();
        int weapon = p.readInt();
        int gender = p.readByte();

        int status;
        switch (job)
        {
            case 0: // Knights of Cygnus
                status = NoblesseCreator.createCharacter(c, name, face, hair + haircolor, skincolor, top, bottom, shoes, weapon, gender);
                break;
            case 1: // Adventurer
                status = BeginnerCreator.createCharacter(c, name, face, hair + haircolor, skincolor, top, bottom, shoes, weapon, gender);
                break;
            case 2: // Aran
                status = LegendCreator.createCharacter(c, name, face, hair + haircolor, skincolor, top, bottom, shoes, weapon, gender);
                break;
            default:
                c.sendPacket(PacketCreator.deleteCharResponse(0, 9));
                return;
        }

        if (status == -2)
        {
            c.sendPacket(PacketCreator.deleteCharResponse(0, 9));
        }
    }
}