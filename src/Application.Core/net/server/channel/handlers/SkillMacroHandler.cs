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


using Application.Core.Game.Skills;
using client.autoban;
using net.packet;

namespace net.server.channel.handlers;

public class SkillMacroHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        var chr = c.OnlinedCharacter;
        int num = p.readByte();
        if (num > 5)
        {
            return;
        }

        for (int i = 0; i < num; i++)
        {
            string name = p.readString();
            if (name.Length > 12)
            {
                AutobanFactory.PACKET_EDIT.alert(chr, "Invalid name length " + name + " (" + name.Length + ") for skill macro.");
                c.disconnect(false, false);
                break;
            }

            int shout = p.readByte();
            int skill1 = p.readInt();
            int skill2 = p.readInt();
            int skill3 = p.readInt();
            SkillMacro macro = new SkillMacro(skill1, skill2, skill3, name, shout, i);
            chr.updateMacros(i, macro);
        }
    }
}
