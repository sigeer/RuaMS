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


using Application.Core.Game.Life.Monsters;
using client;
using client.autoban;
using client.inventory;
using client.status;
using constants.skills;
using net.packet;
using server;
using server.life;
using tools;

namespace net.server.channel.handlers;

public class SummonDamageHandler : AbstractDealDamageHandler
{

    public class SummonAttackEntry
    {

        private int monsterOid;
        private int damage;

        public SummonAttackEntry(int monsterOid, int damage)
        {
            this.monsterOid = monsterOid;
            this.damage = damage;
        }

        public int getMonsterOid()
        {
            return monsterOid;
        }

        public int getDamage()
        {
            return damage;
        }

    }

    public override void HandlePacket(InPacket p, IClient c)
    {
        int oid = p.readInt();
        var player = c.OnlinedCharacter;
        if (!player.isAlive())
        {
            return;
        }
        var summon = player.getSummonsValues().FirstOrDefault(x => x.getObjectId() == oid);
        if (summon == null)
        {
            return;
        }
        var summonSkill = SkillFactory.getSkill(summon.getSkill());
        StatEffect summonEffect = summonSkill.getEffect(summon.getSkillLevel());
        p.skip(4);
        List<SummonAttackEntry> allDamage = new();
        byte direction = p.readByte();
        int numAttacked = p.readByte();
        p.skip(8); // I failed lol (mob x,y and summon x,y), Thanks Gerald
        for (int x = 0; x < numAttacked; x++)
        {
            int monsterOid = p.readInt(); // attacked oid
            p.skip(18);
            int damage = p.readInt();
            allDamage.Add(new SummonAttackEntry(monsterOid, damage));
        }
        player.getMap().broadcastMessage(player, PacketCreator.summonAttack(player.getId(), summon.getObjectId(), direction, allDamage), summon.getPosition());

        if (player.getMap().isOwnershipRestricted(player))
        {
            return;
        }

        bool magic = summonEffect.getWatk() == 0;
        int maxDmg = calcMaxDamage(summonEffect, player, magic);    // thanks Darter (YungMoozi) for reporting unchecked max dmg
        foreach (SummonAttackEntry attackEntry in allDamage)
        {
            int damage = attackEntry.getDamage();
            var target = player.getMap().getMonsterByOid(attackEntry.getMonsterOid());
            if (target != null)
            {
                if (damage > maxDmg)
                {
                    AutobanFactory.DAMAGE_HACK.alert(c.OnlinedCharacter, "Possible packet editing summon damage exploit.");
                    string mobName = MonsterInformationProvider.getInstance().getMobNameFromId(target.getId());
                    log.Information("Possible exploit - chr {CharacterName} used a summon of skillId {SkillId} to attack {MobName} with damage {Damage} (max: {MaxDamage})",
                            c.OnlinedCharacter.getName(), summon.getSkill(), mobName, damage, maxDmg);
                    damage = maxDmg;
                }

                if (damage > 0 && summonEffect.getMonsterStati().Count > 0)
                {
                    if (summonEffect.makeChanceResult())
                    {
                        target.applyStatus(player, new MonsterStatusEffect(summonEffect.getMonsterStati(), summonSkill), summonEffect.isPoison(), 4000);
                    }
                }
                player.getMap().damageMonster(player, target, damage);
            }
        }

        if (summon.getSkill() == Outlaw.GAVIOTA)
        {  // thanks Periwinks for noticing Gaviota not cancelling after grenade toss
            player.cancelEffect(summonEffect, false, -1);
        }
    }

    private static int calcMaxDamage(StatEffect summonEffect, IPlayer player, bool magic)
    {
        double maxDamage;

        if (magic)
        {
            int matk = Math.Max(player.getTotalMagic(), 14);
            maxDamage = player.calculateMaxBaseMagicDamage(matk) * (0.05 * summonEffect.getMatk());
        }
        else
        {
            int watk = Math.Max(player.getTotalWatk(), 14);
            var weapon_item = player.getInventory(InventoryType.EQUIPPED).getItem(-11);

            int maxBaseDmg;  // thanks Conrad, Atoot for detecting some summons legitimately hitting over the calculated limit
            if (weapon_item != null)
            {
                maxBaseDmg = player.calculateMaxBaseDamage(watk, ItemInformationProvider.getInstance().getWeaponType(weapon_item.getItemId()));
            }
            else
            {
                maxBaseDmg = player.calculateMaxBaseDamage(watk, WeaponType.SWORD1H);
            }

            float summonDmgMod = (maxBaseDmg >= 438) ? 0.054f : 0.077f;
            maxDamage = maxBaseDmg * (summonDmgMod * summonEffect.getWatk());
        }

        return (int)maxDamage;
    }
}
