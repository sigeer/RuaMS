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
using constants.skills;
using net.packet;
using tools;

namespace net.server.channel.handlers;

public class CancelBuffHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        int sourceid = p.readInt();

        switch (sourceid)
        {
            case FPArchMage.BIG_BANG:
            case ILArchMage.BIG_BANG:
            case Bishop.BIG_BANG:
            case Bowmaster.HURRICANE:
            case Marksman.PIERCING_ARROW:
            case Corsair.RAPID_FIRE:
            case WindArcher.HURRICANE:
            case Evan.FIRE_BREATH:
            case Evan.ICE_BREATH:
                c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.skillCancel(c.OnlinedCharacter, sourceid), false);
                break;

            default:
                c.OnlinedCharacter.cancelEffect(SkillFactory.getSkill(sourceid).getEffect(1), false, -1);
                break;
        }
    }
}
