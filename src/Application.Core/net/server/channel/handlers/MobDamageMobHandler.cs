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
using client.autoban;
using client.status;
using net.packet;
using server.life;
using tools;

namespace net.server.channel.handlers;



/**
 * @author Jay Estrella
 * @author Ronan
 */
public class MobDamageMobHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        int from = p.readInt();
        p.readInt();
        int to = p.readInt();
        bool magic = p.readByte() == 0;
        int dmg = p.readInt();
        var chr = c.OnlinedCharacter;

        var map = chr.getMap();
        var attacker = map.getMonsterByOid(from);
        var damaged = map.getMonsterByOid(to);

        if (attacker != null && damaged != null)
        {
            int maxDmg = calcMaxDamage(attacker, damaged, magic);     // thanks Darter (YungMoozi) for reporting unchecked dmg

            if (dmg > maxDmg)
            {
                AutobanFactory.DAMAGE_HACK.alert(c.OnlinedCharacter, "Possible packet editing hypnotize damage exploit.");   // thanks Rien dev team
                string attackerName = MonsterInformationProvider.getInstance().getMobNameFromId(attacker.getId());
                string damagedName = MonsterInformationProvider.getInstance().getMobNameFromId(damaged.getId());
                log.Warning("Chr {CharacterName} had hypnotized {Attacker} to attack {Damaged} with damage {Damage} (max: {MaxDamage})", c.OnlinedCharacter.getName(),
                        attackerName, damagedName, dmg, maxDmg);
                dmg = maxDmg;
            }

            map.damageMonster(chr, damaged, dmg);
            map.broadcastMessage(chr, PacketCreator.damageMonster(to, dmg), false);
        }
    }

    private static int calcMaxDamage(Monster attacker, Monster damaged, bool magic)
    {
        int attackerAtk, damagedDef, attackerLevel = attacker.getLevel();
        double maxDamage;
        if (magic)
        {
            int atkRate = calcModifier(attacker, MonsterStatus.MAGIC_ATTACK_UP, MonsterStatus.MATK);
            attackerAtk = (attacker.getStats().getMADamage() * atkRate) / 100;

            int defRate = calcModifier(damaged, MonsterStatus.MAGIC_DEFENSE_UP, MonsterStatus.MDEF);
            damagedDef = (damaged.getStats().getMDDamage() * defRate) / 100;

            maxDamage = ((attackerAtk * (1.15 + (0.025 * attackerLevel))) - (0.75 * damagedDef)) * (Math.Log(Math.Abs(damagedDef - attackerAtk)) / Math.Log(12));
        }
        else
        {
            int atkRate = calcModifier(attacker, MonsterStatus.WEAPON_ATTACK_UP, MonsterStatus.WATK);
            attackerAtk = (attacker.getStats().getPADamage() * atkRate) / 100;

            int defRate = calcModifier(damaged, MonsterStatus.WEAPON_DEFENSE_UP, MonsterStatus.WDEF);
            damagedDef = (damaged.getStats().getPDDamage() * defRate) / 100;

            maxDamage = ((attackerAtk * (1.15 + (0.025 * attackerLevel))) - (0.75 * damagedDef)) * (Math.Log(Math.Abs(damagedDef - attackerAtk)) / Math.Log(17));
        }

        return (int)maxDamage;
    }

    private static int calcModifier(Monster monster, MonsterStatus buff, MonsterStatus nerf)
    {
        Dictionary<MonsterStatus, MonsterStatusEffect> monsterStati = monster.getStati();

        var atkBuff = monsterStati.GetValueOrDefault(buff);
        var atkModifier = atkBuff == null ? 100 : atkBuff.getStati().GetValueOrDefault(buff, 100);

        var atkNerf = monsterStati.GetValueOrDefault(nerf);
        if (atkNerf != null)
        {
            atkModifier -= atkNerf.getStati().GetValueOrDefault(nerf);
        }

        return atkModifier;
    }
}
