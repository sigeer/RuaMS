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
using Application.Core.Client.inventory;
using Application.Core.Game.Skills;
using Application.Core.Server;
using Application.Templates.Item.Consume;
using Application.Templates.Skill;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
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
            await applyAttack(attack, chr);
        }
        else if (attack.skill == ThunderBreaker.SHARK_WAVE && chr.getSkillLevel(ThunderBreaker.SHARK_WAVE) > 0)
        {
            await chr.BroadcastMap(
                PacketCreator.rangedAttack(chr, attack.skill, attack.skilllevel, attack.stance, attack.numAttackedAndDamage, 0, attack.targets, attack.speed, attack.direction, attack.display), chr.Id);
            await applyAttack(attack, chr);

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
                await applyAttack(attack, chr);
            }
            else if (attack.skill == Aran.COMBO_FENRIR && chr.getCombo() >= 100)
            {
                await chr.setCombo(0);
                await applyAttack(attack, chr);
            }
            else if (attack.skill == Aran.COMBO_TEMPEST && chr.getCombo() >= 200)
            {
                await chr.setCombo(0);
                await applyAttack(attack, chr);
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
            int bulletCount = 0;
            StatEffect? effect = null;
            if (attack.skill != 0)
            {
                effect = await attack.getAttackEffect(chr, null);
                if (effect == null  || effect.EffectTemplate is not SkillLevelData skillLevelData)
                {
                    return;
                }

                if (skillLevelData.BulletCount > 0)
                {
                    bulletCount += skillLevelData.BulletCount;
                }

                if (skillLevelData.BulletConsume > 0)
                {
                    // 多重飞镖、快枪手
                    // 暗器伤人属于BUFF，不会走这里
                    bulletCount += skillLevelData.BulletConsume;
                }

                if (skillLevelData.ItemConsume > 0)
                {
                    // 使用指定子弹
                    projectile = skillLevelData.ItemConsume;
                }

                int money = skillLevelData.MoneyCon;
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
            bool hasShadowPartner = chr.getBuffedValue(BuffStat.SHADOWPARTNER) != null;
            if (hasShadowPartner)
            {
                bulletCount *= 2;
            }

            Item? usedBullet = null;
            if (projectile  == 0)
            {
                usedBullet = chr.GetProperBulletItem(bulletCount);
                if (usedBullet != null)
                {
                    slot = usedBullet.getPosition();
                    projectile = usedBullet.getItemId();
                }
            }

            bool soulArrow = chr.getBuffedValue(BuffStat.SOULARROW) != null;
            bool shadowClaw = chr.getBuffedValue(BuffStat.SHADOW_CLAW) != null;
            if (projectile > 0 && !soulArrow && !shadowClaw)
            {
                if (usedBullet != null)
                {
                    // 理论上标飞没有无形箭，这里不作区分
                    await InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, (short)bulletCount, false, true);

                    await InventoryManipulator.RechargeBalanceFury(chr, usedBullet);
                }
                else
                {
                    await InventoryManipulator.removeById(c, InventoryType.USE, projectile, bulletCount, false, true);
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
                    // 无形箭 / 不消耗飞镖的技能
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

                if (effect != null)
                {
                    var effectCooldown = effect.getCooldown();
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

                await applyAttack(attack, chr);
            }
        }
    }
}
