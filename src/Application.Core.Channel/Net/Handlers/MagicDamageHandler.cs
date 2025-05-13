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
using Application.Utility.Configs;
using client;
using constants.id;
using constants.skills;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class MagicDamageHandler : AbstractDealDamageHandler
{
    public MagicDamageHandler(ILogger<AbstractDealDamageHandler> logger) : base(logger)
    {
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        /*long timeElapsed = currentServerTime() - chr.getAutobanManager().getLastSpam(8);
		if(timeElapsed < 300) {
			AutobanFactory.FAST_ATTACK.alert(chr, "Time: " + timeElapsed);
		}
		chr.getAutobanManager().spam(8);*/

        var attack = parseDamage(p, chr, false, true);

        if (chr.getBuffEffect(BuffStat.MORPH) != null)
        {
            if (chr.getBuffEffect(BuffStat.MORPH)!.isMorphWithoutAttack())
            {
                // How are they attacking when the client won't let them?
                chr.getClient().disconnect(false, false);
                return;
            }
        }

        if (MapId.isDojo(chr.getMap().getId()) && attack.numAttacked > 0)
        {
            chr.setDojoEnergy(chr.getDojoEnergy() + +YamlConfig.config.server.DOJO_ENERGY_ATK);
            c.sendPacket(PacketCreator.getEnergy("energy", chr.getDojoEnergy()));
        }

        int charge = (attack.skill == Evan.FIRE_BREATH
            || attack.skill == Evan.ICE_BREATH
            || attack.skill == FPArchMage.BIG_BANG
            || attack.skill == ILArchMage.BIG_BANG
            || attack.skill == Bishop.BIG_BANG) ? attack.charge : -1;

        Packet packet = PacketCreator.magicAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, attack.targets, charge, attack.speed, attack.direction, attack.display);

        chr.getMap().broadcastMessage(chr, packet, false, true);
        var effect = attack.getAttackEffect(chr, null);
        var skill = SkillFactory.GetSkillTrust(attack.skill);
        var effect_ = skill.getEffect(chr.getSkillLevel(skill));
        if (effect_.getCooldown() > 0)
        {
            if (chr.skillIsCooling(attack.skill))
            {
                return;
            }
            else
            {
                c.sendPacket(PacketCreator.skillCooldown(attack.skill, effect_.getCooldown()));
                chr.addCooldown(attack.skill, c.CurrentServer.getCurrentTime(), 1000 * (effect_.getCooldown()));
            }
        }
        applyAttack(attack, chr, effect.getAttackCount());
        var eaterSkill = SkillFactory.getSkill((chr.getJob().getId() - (chr.getJob().getId() % 10)) * 10000);// MP Eater, works with right job
        int eaterLevel = chr.getSkillLevel(eaterSkill);
        if (eaterLevel > 0)
        {
            foreach (int singleDamage in attack.targets.Keys)
            {
                eaterSkill!.getEffect(eaterLevel).applyPassive(chr, chr.getMap().getMapObject(singleDamage), 0);
            }
        }
    }
}
