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
using Application.Core.Server;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class CloseRangeDamageHandler : AbstractDealDamageHandler
{
    public CloseRangeDamageHandler(ILogger<AbstractDealDamageHandler> logger, AutoBanDataManager autoBanDataManager) : base(logger, autoBanDataManager)
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

        var attack = await parseDamage(p, chr, false, false);

        if (!(await chr.SkillUsed(attack.skill, attack.skilllevel, attack)))
        {
            return;
        }

        await chr.BroadcastMap(
            PacketCreator.closeRangeAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, attack.targets, attack.speed, attack.direction, attack.display),
            chr.Id);
        int numFinisherOrbs = 0;
        var comboBuff = chr.GetBuffStatValue(BuffStat.COMBO);
        if (GameConstants.isFinisherSkill(attack.skill))
        {
            if (comboBuff != null)
            {
                numFinisherOrbs = comboBuff.Value - 1;
            }
            await chr.handleOrbconsume();
        }
        else if (attack.numAttacked > 0)
        {
            if (attack.skill != Crusader.SHOUT && comboBuff != null)
            {
                var orbcount = comboBuff.Value;
                int oSkillId = chr.isCygnus() ? DawnWarrior.COMBO : Crusader.COMBO;
                int advcomboid = chr.isCygnus() ? DawnWarrior.ADVANCED_COMBO : Hero.ADVANCED_COMBO;
                Skill combo = SkillFactory.GetSkillTrust(oSkillId);
                Skill advcombo = SkillFactory.GetSkillTrust(advcomboid);
                StatEffect? ceffect;
                int advComboSkillLevel = chr.getSkillLevel(advcombo);
                if (advComboSkillLevel > 0)
                {
                    ceffect = advcombo.getEffect(advComboSkillLevel);
                }
                else
                {
                    int comboLv = chr.getSkillLevel(combo);
                    if (comboLv <= 0 || chr.isGM())
                    {
                        comboLv = SkillFactory.GetSkillTrust(oSkillId).getMaxLevel();
                    }

                    if (comboLv > 0)
                    {
                        ceffect = combo.getEffect(comboLv);
                    }
                    else
                    {
                        ceffect = null;
                    }
                }
                if (ceffect != null)
                {
                    if (orbcount <= ceffect.getX())
                    {
                        int neworbcount = orbcount + 1;
                        if (advComboSkillLevel > 0 && ceffect.makeChanceResult())
                        {
                            if (neworbcount <= ceffect.getX())
                            {
                                neworbcount++;
                            }
                        }

                        await chr.UpdateBuff(BuffStat.COMBO, holder =>
                        {
                            holder.Value = neworbcount;
                        });
                    }
                }
            }
            else if (chr.getSkillLevel(chr.isCygnus() ? SkillFactory.GetSkillTrust(ThunderBreaker.ENERGY_CHARGE) : SkillFactory.GetSkillTrust(Marauder.ENERGY_CHARGE)) > 0
                && (chr.getJob().isA(Job.MARAUDER) || chr.getJob().isA(Job.THUNDERBREAKER2)))
            {
                for (int i = 0; i < attack.numAttacked; i++)
                {
                    await chr.handleEnergyChargeGain();
                }
            }
        }

        if (numFinisherOrbs == 0 && GameConstants.isFinisherSkill(attack.skill))
        {
            return;
        }

        await applyAttack(attack, chr);
    }
}
