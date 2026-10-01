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


using Application.Core.Channel.Net.Packets;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Gameplay;
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

        if (chr.IsMorphWithoutAttack())
        {
            // How are they attacking when the client won't let them?
            await chr.getClient().Disconnect(false, false);
            return;
        }

        if (chr.getDojoEnergy() < 10000
            && (attack.skill == Beginner.BAMBOO_RAIN || attack.skill == Noblesse.BAMBOO_RAIN || attack.skill == Legend.BAMBOO_THRUST)) // PE hacking or maybe just lagging
        {
            return;
        }
        if (MapId.isDojo(chr.getMap().getId()) && attack.numAttacked > 0)
        {
            chr.setDojoEnergy(chr.getDojoEnergy() + YamlConfig.config.server.DOJO_ENERGY_ATK);
            await c.SendPacket(PacketCreator.getEnergy("energy", chr.getDojoEnergy()));
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
        if (attack.numAttacked > 0 && attack.skill == DragonKnight.SACRIFICE)
        {
            int totDamageToOneMonster = attack.targets.Values.FirstOrDefault()?.damageLines?.FirstOrDefault() ?? 0;

            await chr.safeAddHP(-1 * totDamageToOneMonster * (await attack.getAttackEffect(chr, null))!.getX() / 100);
        }
        if (attack.numAttacked > 0 && attack.skill == WhiteKnight.CHARGE_BLOW)
        {
            bool advcharge_prob = false;
            var advchargeSkillEffect = chr.GetPlayerSkillEffect(Paladin.ADVANCED_CHARGE);
            if (advchargeSkillEffect != null)
            {
                advcharge_prob = advchargeSkillEffect.makeChanceResult();
            }
            if (!advcharge_prob)
            {
                await chr.CancelBuff(BuffStat.WK_CHARGE);
            }
        }
        int attackCount = 1;
        if (attack.skill != 0)
        {
            attackCount = (await attack.getAttackEffect(chr, null))!.getAttackCount();
        }
        if (numFinisherOrbs == 0 && GameConstants.isFinisherSkill(attack.skill))
        {
            return;
        }
        if (attack.skill % 10000000 == 1009)
        { // bamboo
            if (chr.getDojoEnergy() < 10000)
            { // PE hacking or maybe just lagging
                return;
            }

            chr.setDojoEnergy(0);
            await c.SendPacket(PacketCreator.getEnergy("energy", chr.getDojoEnergy()));
            await c.SendPacket(PacketCreator.serverNotice(5, "As you used the secret skill, your energy bar has been reset."));
        }
        else if (attack.skill > 0)
        {
            Skill skill = SkillFactory.GetSkillTrust(attack.skill);
            StatEffect effect_ = skill.getEffect(chr.getSkillLevel(skill));
            if (effect_.getCooldown() > 0)
            {
                if (chr.skillIsCooling(attack.skill))
                {
                    return;
                }
                else
                {
                    await c.SendPacket(PacketCreator.skillCooldown(attack.skill, effect_.getCooldown()));
                    chr.addCooldown(attack.skill, c.CurrentServer.Node.getCurrentTime(), 1000 * (effect_.getCooldown()));
                }
            }
        }
        if ((chr.getSkillLevel(SkillFactory.GetSkillTrust(NightWalker.VANISH)) > 0 || chr.getSkillLevel(SkillFactory.GetSkillTrust(Rogue.DARK_SIGHT)) > 0)
            && chr.getBuffedValue(BuffStat.DARKSIGHT) != null)
        {
            // && chr.getBuffSource(BuffStat.DARKSIGHT) != 9101004
            await chr.CancelBuff(BuffStat.DARKSIGHT);
        }
        else if (chr.getSkillLevel(SkillFactory.GetSkillTrust(WindArcher.WIND_WALK)) > 0 && chr.getBuffedValue(BuffStat.WIND_WALK) != null)
        {
            await chr.CancelBuff(BuffStat.WIND_WALK);
        }

        await applyAttack(attack, chr);
    }
}
