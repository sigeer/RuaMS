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


using Application.Core.Channel.ServerData;
using Application.Core.Game.Skills;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class MagicDamageHandler : AbstractDealDamageHandler
{
    public MagicDamageHandler(ILogger<AbstractDealDamageHandler> logger, AutoBanDataManager autoBanDataManager) : base(logger, autoBanDataManager)
    {
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        /*long timeElapsed = currentServerTime() - chr.getAutobanManager().getLastSpam(8);
		if(timeElapsed < 300) {
			AutobanFactory.FAST_ATTACK.alert(chr, "Time: " + timeElapsed);
		}
		chr.getAutobanManager().spam(8);*/

        var attack = await parseDamage(p, chr, false, true);

        if (!(await chr.SkillUsed(attack.skill, attack.skilllevel, attack)))
        {
            return;
        }

        int charge = (attack.skill == Evan.FIRE_BREATH
            || attack.skill == Evan.ICE_BREATH
            || attack.skill == FPArchMage.BIG_BANG
            || attack.skill == ILArchMage.BIG_BANG
            || attack.skill == Bishop.BIG_BANG) ? attack.charge : -1;

        var packet = PacketCreator.magicAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, attack.targets, charge, attack.speed, attack.direction, attack.display);

        await chr.BroadcastMap(packet, chr.Id);

        await applyAttack(attack, chr);
    }
}
