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


using Application.Core.Channel.DataProviders;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Skills;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;


public class RangedAttackHandler : AbstractDealDamageHandler
{
    public RangedAttackHandler(ILogger<AbstractDealDamageHandler> logger, AutoBanDataManager autoBanDataManager) : base(logger, autoBanDataManager)
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

        var attack = await parseDamage(p, chr, true, false);

        if (chr.IsMorphWithoutAttack())
        {
            // How are they attacking when the client won't let them?
            await chr.getClient().Disconnect(false, false);
            return;
        }

        if (MapId.isDojo(chr.getMap().getId()) && attack.numAttacked > 0)
        {
            chr.setDojoEnergy(chr.getDojoEnergy() + YamlConfig.config.server.DOJO_ENERGY_ATK);
            await c.SendPacket(PacketCreator.getEnergy("energy", chr.getDojoEnergy()));
        }

        if (attack.skill == Buccaneer.ENERGY_ORB || attack.skill == ThunderBreaker.SPARK || attack.skill == Shadower.TAUNT || attack.skill == NightLord.TAUNT)
        {
            await chr.BroadcastMap(
                PacketCreator.rangedAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, 0, attack.targets, attack.speed, attack.direction, attack.display), chr.Id);
            await applyAttack(attack, chr, 1);
        }
        else if (attack.skill == ThunderBreaker.SHARK_WAVE && chr.getSkillLevel(ThunderBreaker.SHARK_WAVE) > 0)
        {
            await chr.BroadcastMap(
                PacketCreator.rangedAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, 0, attack.targets, attack.speed, attack.direction, attack.display), chr.Id);
            await applyAttack(attack, chr, 1);

            for (int i = 0; i < attack.numAttacked; i++)
            {
                await chr.handleEnergyChargeGain();
            }
        }
        else if (attack.skill == Aran.COMBO_SMASH || attack.skill == Aran.COMBO_FENRIR || attack.skill == Aran.COMBO_TEMPEST)
        {
            await chr.BroadcastMap(
                PacketCreator.rangedAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, 0, attack.targets, attack.speed, attack.direction, attack.display), chr.Id);
            if (attack.skill == Aran.COMBO_SMASH && chr.getCombo() >= 30)
            {
                await chr.setCombo(0);
                await applyAttack(attack, chr, 1);
            }
            else if (attack.skill == Aran.COMBO_FENRIR && chr.getCombo() >= 100)
            {
                await chr.setCombo(0);
                await applyAttack(attack, chr, 2);
            }
            else if (attack.skill == Aran.COMBO_TEMPEST && chr.getCombo() >= 200)
            {
                await chr.setCombo(0);
                await applyAttack(attack, chr, 4);
            }
        }
        else
        {
            var weapon = chr.getInventory(InventoryType.EQUIPPED).getItem(EquipSlot.Weapon)!;
            WeaponType type = ItemInformationProvider.getInstance().getWeaponType(weapon.getItemId());
            if (type == WeaponType.NOT_A_WEAPON)
            {
                return;
            }
            short slot = -1;
            int projectile = 0;
            short bulletCount = 1;
            StatEffect? effect = null;
            if (attack.skill != 0)
            {
                effect = await attack.getAttackEffect(chr, null);
                if (effect == null)
                {
                    return;
                }

                bulletCount = effect.getBulletCount();
                if (effect.getCooldown() > 0)
                {
                    await c.SendPacket(PacketCreator.skillCooldown(attack.skill, effect.getCooldown()));
                }

                if (attack.skill == Hermit.SHADOW_MESO)
                {   // shadow meso
                    bulletCount = 0;

                    int money = effect.getMoneyCon();
                    if (money != 0)
                    {
                        int moneyMod = money / 2;
                        money += Randomizer.nextInt(moneyMod);
                        if (money > chr.getMeso())
                        {
                            money = chr.getMeso();
                        }
                        await chr.GainMeso(-money);
                    }
                }
            }
            bool hasShadowPartner = chr.getBuffedValue(BuffStat.SHADOWPARTNER) != null;
            if (hasShadowPartner)
            {
                bulletCount *= 2;
            }
            var inv = chr.getInventory(InventoryType.USE);
            for (short i = 1; i <= inv.getSlotLimit(); i++)
            {
                var item = inv.getItem(i);
                if (item != null)
                {
                    int id = item.getItemId();
                    slot = item.getPosition();

                    bool bow = ItemConstants.isArrowForBow(id);
                    bool cbow = ItemConstants.isArrowForCrossBow(id);
                    if (item.getQuantity() >= bulletCount)
                    { //Fixes the bug where you can't use your last arrow.
                        if (type == WeaponType.CLAW && ItemConstants.isThrowingStar(id) && weapon.getItemId() != ItemId.MAGICAL_MITTEN)
                        {
                            if (((id == ItemId.HWABI_THROWING_STARS || id == ItemId.BALANCED_FURY) && chr.getLevel() < 70) || (id == ItemId.CRYSTAL_ILBI_THROWING_STARS && chr.getLevel() < 50))
                            {
                            }
                            else
                            {
                                projectile = id;
                                break;
                            }
                        }
                        else if ((type == WeaponType.GUN && ItemConstants.isBullet(id)))
                        {
                            if (id == ItemId.BLAZE_CAPSULE || id == ItemId.GLAZE_CAPSULE)
                            {
                                if (chr.getLevel() >= 70)
                                {
                                    projectile = id;
                                    break;
                                }
                            }
                            else if (chr.getLevel() > (id % 10) * 20 + 9)
                            {
                                projectile = id;
                                break;
                            }
                        }
                        else if ((type == WeaponType.BOW && bow) || (type == WeaponType.CROSSBOW && cbow) || (weapon.getItemId() == ItemId.MAGICAL_MITTEN && (bow || cbow)))
                        {
                            projectile = id;
                            break;
                        }
                    }
                }
            }
            bool soulArrow = chr.getBuffedValue(BuffStat.SOULARROW) != null;
            bool shadowClaw = chr.getBuffedValue(BuffStat.SHADOW_CLAW) != null;
            if (projectile != 0)
            {
                if (!soulArrow && !shadowClaw && attack.skill != DawnWarrior.SOUL_BLADE && attack.skill != ThunderBreaker.SHARK_WAVE && attack.skill != NightWalker.VAMPIRE)
                {
                    short bulletConsume = bulletCount;

                    if (effect != null && effect.getBulletConsume() != 0)
                    {
                        bulletConsume = (byte)(effect.getBulletConsume() * (hasShadowPartner ? 2 : 1));
                    }

                    if (slot < 0)
                    {
                        _logger.LogWarning("<ERROR> Projectile to use was unable to be found.");
                    }
                    else
                    {
                        await InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, bulletConsume, false, true);
                    }
                }
            }

            if (projectile != 0
                || soulArrow
                || attack.skill == DawnWarrior.SOUL_BLADE
                || attack.skill == ThunderBreaker.SHARK_WAVE
                || attack.skill == NightWalker.VAMPIRE
                || attack.skill == Hermit.SHADOW_MESO
                || attack.skill == WindArcher.STORM_BREAK)
            {
                int visProjectile = projectile; //visible projectile sent to players
                if (ItemConstants.isThrowingStar(projectile))
                {
                    var cash = chr.getInventory(InventoryType.CASH);
                    for (int i = 1; i <= cash.getSlotLimit(); i++)
                    { // impose order...
                        var item = cash.getItem((short)i);
                        if (item != null)
                        {
                            if (item.getItemId() / 1000 == 5021)
                            {
                                visProjectile = item.getItemId();
                                break;
                            }
                        }
                    }
                }
                else if (soulArrow
                    || attack.skill == Ranger.ARROW_RAIN
                    || attack.skill == Sniper.ARROW_ERUPTION
                    || attack.skill == DawnWarrior.SOUL_BLADE
                    || attack.skill == ThunderBreaker.SHARK_WAVE
                    || attack.skill == NightWalker.VAMPIRE
                    || attack.skill == WindArcher.STORM_BREAK)
                {
                    visProjectile = 0;
                }

                Packet packet;
                switch (attack.skill)
                {
                    case Bowmaster.HURRICANE: // Hurricane
                    case Marksman.PIERCING_ARROW: // Pierce
                    case Corsair.RAPID_FIRE: // Rapid Fire
                    case WindArcher.HURRICANE: // KoC Hurricane
                        packet = PacketCreator.rangedAttack(chr, attack.skill, attack.skilllevel, attack.rangedirection, attack.numAttackedAndDamage, visProjectile, attack.targets, attack.speed, attack.direction, attack.display);
                        break;
                    default:
                        packet = PacketCreator.rangedAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, visProjectile, attack.targets, attack.speed, attack.direction, attack.display);
                        break;
                }
                await chr.BroadcastMap(packet, chr.Id);

                if (attack.skill != 0)
                {
                    var skill = SkillFactory.GetSkillTrust(attack.skill);
                    StatEffect effect_ = skill.getEffect(chr.getSkillLevel(skill));
                    var effectCooldown = effect_.getCooldown();
                    if (effectCooldown > 0)
                    {
                        if (chr.skillIsCooling(attack.skill))
                        {
                            return;
                        }
                        else
                        {
                            await c.SendPacket(PacketCreator.skillCooldown(attack.skill, effectCooldown));
                            chr.addCooldown(attack.skill, c.CurrentServer.Node.getCurrentTime(), 1000 * effectCooldown);
                        }
                    }
                }

                if (chr.getSkillLevel(SkillFactory.GetSkillTrust(NightWalker.VANISH)) > 0
                    && chr.getBuffedValue(BuffStat.DARKSIGHT) != null
                    && attack.numAttacked > 0
                    && chr.getBuffSource(BuffStat.DARKSIGHT) != SuperGM.HIDE)
                {
                    await chr.cancelEffectFromBuffStat(BuffStat.DARKSIGHT);
                    await chr.cancelBuffStats(BuffStat.DARKSIGHT);
                }
                else if (chr.getSkillLevel(SkillFactory.GetSkillTrust(WindArcher.WIND_WALK)) > 0
                    && chr.getBuffedValue(BuffStat.WIND_WALK) != null
                    && attack.numAttacked > 0)
                {
                    await chr.cancelEffectFromBuffStat(BuffStat.WIND_WALK);
                    await chr.cancelBuffStats(BuffStat.WIND_WALK);
                }

                await applyAttack(attack, chr, bulletCount);
            }
        }
    }
}
