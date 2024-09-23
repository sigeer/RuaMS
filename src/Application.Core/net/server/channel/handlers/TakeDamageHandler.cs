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


using Application.Core.Game.Life;
using Application.Core.Game.Life.Monsters;
using client;
using client.inventory;
using client.inventory.manipulator;
using client.status;
using constants.id;
using constants.inventory;
using constants.skills;
using net.packet;
using server.life;
using tools;
using static server.life.LifeFactory;

namespace net.server.channel.handlers;

public class TakeDamageHandler : AbstractPacketHandler
{

    public override void HandlePacket(InPacket p, IClient c)
    {
        List<IPlayer> banishPlayers = new();

        var chr = c.OnlinedCharacter;
        p.readInt();
        sbyte damagefrom = p.ReadSByte();
        p.readByte(); //Element
        int damage = p.readInt();
        int oid = 0, monsteridfrom = 0, pgmr = 0, direction = 0;
        int pos_x = 0, pos_y = 0, fake = 0;
        bool is_pgmr = false, is_pg = true, is_deadly = false;
        int mpattack = 0;
        Monster? attacker = null;
        var map = chr.getMap();
        if (damagefrom != -3 && damagefrom != -4)
        {
            monsteridfrom = p.readInt();
            oid = p.readInt();

            try
            {
                var mmo = map.getMapObject(oid);
                if (mmo is Monster)
                {
                    attacker = (Monster)mmo;
                    if (attacker.getId() != monsteridfrom)
                    {
                        attacker = null;
                    }
                }

                if (attacker != null)
                {
                    if (attacker.isBuffed(MonsterStatus.NEUTRALISE))
                    {
                        return;
                    }

                    List<loseItem>? loseItems;
                    if (damage > 0)
                    {
                        loseItems = attacker.getStats().loseItem();
                        if (loseItems != null)
                        {
                            if (chr.getBuffEffect(BuffStat.AURA) == null)
                            {
                                InventoryType type;
                                int playerpos = chr.getPosition().X;
                                byte d = 1;
                                Point pos = new Point(0, chr.getPosition().Y);
                                foreach (var loseItem in loseItems)
                                {
                                    type = ItemConstants.getInventoryType(loseItem.getId());

                                    int dropCount = 0;
                                    for (byte b = 0; b < loseItem.getX(); b++)
                                    {
                                        if (Randomizer.nextInt(100) < loseItem.getChance())
                                        {
                                            dropCount += 1;
                                        }
                                    }

                                    if (dropCount > 0)
                                    {
                                        int qty;

                                        Inventory inv = chr.getInventory(type);
                                        inv.lockInventory();
                                        try
                                        {
                                            qty = Math.Min(chr.countItem(loseItem.getId()), dropCount);
                                            InventoryManipulator.removeById(c, type, loseItem.getId(), qty, false, false);
                                        }
                                        finally
                                        {
                                            inv.unlockInventory();
                                        }

                                        if (loseItem.getId() == 4031868)
                                        {
                                            chr.updateAriantScore();
                                        }

                                        for (byte b = 0; b < qty; b++)
                                        {
                                            pos.X = playerpos + ((d % 2 == 0) ? (25 * (d + 1) / 2) : -(25 * (d / 2)));
                                            map.spawnItemDrop(chr, chr, new Item(loseItem.getId(), 0, 1), map.calcDropPos(pos, chr.getPosition()), true, true);
                                            d++;
                                        }
                                    }
                                }
                            }
                            map.removeMapObject(attacker);
                        }
                    }
                }
                else if (damagefrom != 0 || !map.removeSelfDestructive(oid))
                {    // thanks inhyuk for noticing self-destruct damage not being handled properly
                    return;
                }
            }
            catch (InvalidCastException e)
            {
                //this happens due to mob on last map damaging player just before changing maps
                log.Warning(e, "Attack is not a mob-type, rather is a {MapObject} entity", map.getMapObject(oid)?.GetType()?.Name);
                return;
            }

            direction = p.readByte();
        }
        if (damagefrom != -1 && damagefrom != -2 && attacker != null)
        {
            var attackInfo = MobAttackInfoFactory.getMobAttackInfo(attacker, damagefrom);
            if (attackInfo != null)
            {
                if (attackInfo.isDeadlyAttack())
                {
                    mpattack = chr.getMp() - 1;
                    is_deadly = true;
                }
                mpattack += attackInfo.getMpBurn();

                var possibleType = MobSkillTypeUtils.from(attackInfo.getDiseaseSkill());
                var possibleMobSkill = MobSkillFactory.getMobSkillOrThrow(possibleType, attackInfo.getDiseaseLevel());
                if (possibleMobSkill != null && damage > 0)
                {
                    possibleMobSkill.applyEffect(chr, attacker, false, banishPlayers);
                }

                attacker.setMp(attacker.getMp() - attackInfo.getMpCon());
                if (chr.getBuffedValue(BuffStat.MANA_REFLECTION) != null && damage > 0 && !attacker.isBoss())
                {
                    int jobid = chr.getJob().getId();
                    if (jobid == 212 || jobid == 222 || jobid == 232)
                    {
                        int id = jobid * 10000 + 1002;
                        var manaReflectSkill = SkillFactory.GetSkillTrust(id);
                        if (chr.isBuffFrom(BuffStat.MANA_REFLECTION, manaReflectSkill) && chr.getSkillLevel(manaReflectSkill) > 0 && manaReflectSkill.getEffect(chr.getSkillLevel(manaReflectSkill)).makeChanceResult())
                        {
                            int bouncedamage = (damage * manaReflectSkill.getEffect(chr.getSkillLevel(manaReflectSkill)).getX() / 100);
                            if (bouncedamage > attacker.getMaxHp() / 5)
                            {
                                bouncedamage = attacker.getMaxHp() / 5;
                            }
                            map.damageMonster(chr, attacker, bouncedamage);
                            map.broadcastMessage(chr, PacketCreator.damageMonster(oid, bouncedamage), true);
                            chr.sendPacket(PacketCreator.showOwnBuffEffect(id, 5));
                            map.broadcastMessage(chr, PacketCreator.showBuffEffect(chr.getId(), id, 5), false);
                        }
                    }
                }
            }
        }

        if (damage == -1)
        {
            fake = 4020002 + (chr.getJob().getId() / 10 - 40) * 100000;
        }

        if (damage > 0)
        {
            chr.getAutobanManager().resetMisses();
        }
        else
        {
            chr.getAutobanManager().addMiss();
        }

        //in dojo player cannot use pot, so deadly attacks should be turned off as well
        if (is_deadly && MapId.isDojo(chr.getMap().getId()) && !YamlConfig.config.server.USE_DEADLY_DOJO)
        {
            damage = 0;
            mpattack = 0;
        }

        if (damage > 0 && !chr.isHidden())
        {
            if (attacker != null)
            {
                if (damagefrom == -1)
                {
                    if (chr.getBuffedValue(BuffStat.POWERGUARD) != null)
                    { // PG works on bosses, but only at half of the rate.
                        int bouncedamage = damage * (chr.getBuffedValue(BuffStat.POWERGUARD)!.Value / (attacker.isBoss() ? 200 : 100));
                        bouncedamage = Math.Min(bouncedamage, attacker.getMaxHp() / 10);
                        damage -= bouncedamage;
                        map.damageMonster(chr, attacker, bouncedamage);
                        map.broadcastMessage(chr, PacketCreator.damageMonster(oid, bouncedamage), false, true);
                        attacker.aggroMonsterDamage(chr, bouncedamage);
                    }
                    var bPressure = chr.getBuffEffect(BuffStat.BODY_PRESSURE); // thanks Atoot for noticing an issue on Body Pressure neutralise
                    if (bPressure != null)
                    {
                        var skill = SkillFactory.GetSkillTrust(Aran.BODY_PRESSURE);
                        if (!attacker.alreadyBuffedStats().Contains(MonsterStatus.NEUTRALISE))
                        {
                            if (!attacker.isBoss() && bPressure.makeChanceResult())
                            {
                                attacker.applyStatus(chr, new MonsterStatusEffect(Collections.singletonMap(MonsterStatus.NEUTRALISE, 1), skill), false, (bPressure.getDuration() / 10) * 2, false);
                            }
                        }
                    }
                }

                var cBarrier = chr.getBuffEffect(BuffStat.COMBO_BARRIER);  // thanks BHB for noticing Combo Barrier buff not working
                if (cBarrier != null)
                {
                    damage *= (int)(cBarrier.getX() / 1000.0);
                }
            }
            if (damagefrom != -3 && damagefrom != -4)
            {
                int achilles = 0;
                Skill? achilles1 = null;
                int jobid = chr.getJob().getId();
                if (jobid < 200 && jobid % 10 == 2)
                {
                    achilles1 = SkillFactory.getSkill(jobid * 10000 + (jobid == 112 ? 4 : 5));
                    achilles = chr.getSkillLevel(achilles1);
                }
                if (achilles != 0 && achilles1 != null)
                {
                    damage *= (int)(achilles1.getEffect(achilles).getX() / 1000.0);
                }

                var highDef = SkillFactory.GetSkillTrust(Aran.HIGH_DEFENSE);
                int hdLevel = chr.getSkillLevel(highDef);
                if (highDef != null && hdLevel > 0)
                {
                    damage *= (int)Math.Ceiling(highDef.getEffect(hdLevel).getX() / 1000.0);
                }
            }
            var mesoguard = chr.getBuffedValue(BuffStat.MESOGUARD);
            if (chr.getBuffedValue(BuffStat.MAGIC_GUARD) != null && mpattack == 0)
            {
                int mploss = (int)(damage * (chr.getBuffedValue(BuffStat.MAGIC_GUARD)!.Value / 100.0));
                int hploss = damage - mploss;

                int curmp = chr.getMp();
                if (mploss > curmp)
                {
                    hploss += mploss - curmp;
                    mploss = curmp;
                }

                chr.addMPHP(-hploss, -mploss);
            }
            else if (mesoguard != null)
            {
                damage = (int)Math.Round((double)damage / 2);
                int mesoloss = (int)(damage * (mesoguard / 100.0));
                if (chr.getMeso() < mesoloss)
                {
                    chr.gainMeso(-chr.getMeso(), false);
                    chr.cancelBuffStats(BuffStat.MESOGUARD);
                }
                else
                {
                    chr.gainMeso(-mesoloss, false);
                }
                chr.addMPHP(-damage, -mpattack);
            }
            else
            {
                if (chr.isRidingBattleship())
                {
                    chr.decreaseBattleshipHp(damage);
                }
                chr.addMPHP(-damage, -mpattack);
            }
        }
        if (!chr.isHidden())
        {
            map.broadcastMessage(chr, PacketCreator.damagePlayer(damagefrom, monsteridfrom, chr.getId(), damage, fake, direction, is_pgmr, pgmr, is_pg, oid, pos_x, pos_y), false);
        }
        else
        {
            map.broadcastGMMessage(chr, PacketCreator.damagePlayer(damagefrom, monsteridfrom, chr.getId(), damage, fake, direction, is_pgmr, pgmr, is_pg, oid, pos_x, pos_y), false);
        }
        if (MapId.isDojo(map.getId()))
        {
            chr.setDojoEnergy(chr.getDojoEnergy() + YamlConfig.config.server.DOJO_ENERGY_DMG);
            c.sendPacket(PacketCreator.getEnergy("energy", chr.getDojoEnergy()));
        }

        foreach (var player in banishPlayers)
        {  // chill, if this list ever gets non-empty an attacker does exist, trust me :)
            player.changeMapBanish(attacker.getBanish());
        }
    }
}
