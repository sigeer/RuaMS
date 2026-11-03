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
using Application.Core.Game.Skills;
using Application.Core.Server;
using System.Collections;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class SpecialMoveHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        p.readInt();
        chr.getAutobanManager().setTimestamp(4, c.CurrentServer.Node.getCurrentTimestamp(), 28);
        int skillid = p.readInt();
        int clientSkillLevel = p.ReadSByte();

        Point? pos = null;

        if (!(await chr.SkillUsed(skillid, clientSkillLevel)))
        {
            return;
        }

        var skill = SkillFactory.getSkill(skillid);
        StatEffect effect = skill!.getEffect(clientSkillLevel);

        if (skillid == Hero.MONSTER_MAGNET || skillid == Paladin.MONSTER_MAGNET || skillid == DarkKnight.MONSTER_MAGNET)
        { 
            // Monster Magnet
            int num = p.readInt();
            for (int i = 0; i < num; i++)
            {
                int mobOid = p.readInt();
                byte success = p.readByte();
                await chr.BroadcastMap(PacketCreator.catchMonster(mobOid, success), chr.Id);
                var monster = chr.getMap().getMonsterByOid(mobOid);
                if (monster != null)
                {
                    if (!monster.isBoss())
                    {
                        monster.aggroClearDamages();
                        await monster.aggroMonsterDamage(chr, 1);

                        // thanks onechord for pointing out Magnet crashing the caster (issue would actually happen upon failing to catch mob)
                        // thanks Conrad for noticing Magnet crashing when trying to pull bosses and fixed mobs
                        await monster.aggroSwitchController(chr, true);
                    }
                }
            }
            byte direction = p.readByte();   // thanks MedicOP for pointing some 3rd-party related issues with Magnet
            await chr.BroadcastMap(EffectPacket.ForeignSkillEffect(chr.Id, skillid, clientSkillLevel, chr.Level, direction), chr.Id);
            await c.SendPacket(PacketCreator.enableActions());
        }
        else if (skillid == Brawler.MP_RECOVERY)
        {
            // MP Recovery
            int lose = await chr.safeAddHP(-1 * (chr.ActualMaxHP / effect.getX()));
            int gain = (int)(-lose * (effect.getY() / 100.0));
            await chr.UpdateStatsChunk(() =>
            {
                chr.ChangeMP(gain);
            });
        }
        else if (skillid == SuperGM.HEAL_PLUS_DISPEL)
        {
            p.skip(11);
            await chr.BroadcastMap(EffectPacket.ForeignSkillEffect(chr.getId(), skillid, clientSkillLevel, chr.Level), chr.Id);
        }
        else if (skillid % 10000000 == 1004)
        {
            p.readShort();
        }

        if (p.available() == 5)
        {
            pos = new Point(p.readShort(), p.readShort());
        }
        if (chr.isAlive())
        {
            if (skill.getId() != Priest.MYSTIC_DOOR)
            {
                if (skill.getId() % 10000000 != 1005)
                {
                    await effect.applyTo(chr, pos);
                }
                else
                {
                    await effect.applyEchoOfHero(chr);
                }
            }
            else
            {
                {
                    await c.tryacquireClient();
                    try
                    {
                        if (chr.canDoor())
                        {
                            await chr.CancelBuff(BuffStat.MysticDoor);
                            await effect.applyTo(chr, pos);
                        }
                        else
                        {
                            await chr.Pink("Please wait 5 seconds before casting Mystic Door again.");
                        }
                    }
                    finally
                    {
                        c.releaseClient();
                    }
                }

                await c.SendPacket(PacketCreator.enableActions());
            }
        }
        else
        {
            await c.SendPacket(PacketCreator.enableActions());
        }
    }
}
