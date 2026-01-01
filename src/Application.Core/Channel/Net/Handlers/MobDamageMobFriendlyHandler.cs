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


using Application.Resources.Messages;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Xotic (XoticStory) & BubblesDev
 */

public class MobDamageMobFriendlyHandler : ChannelHandlerBase
{
    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        int attacker = p.readInt();
        p.readInt();
        int damaged = p.readInt();

        var map = c.OnlinedCharacter.getMap();
        var monster = map.getMonsterByOid(damaged);

        if (monster == null || map.getMonsterByOid(attacker) == null)
        {
            return Task.CompletedTask;
        }

        int damage = Randomizer.nextInt(((monster.getMaxHp() / 13 + monster.getPADamage() * 10)) * 2 + 500) / 10; // Formula planned by Beng.

        if (monster.getHp() - damage < 1)
        {     // friendly dies
            switch (monster.getId())
            {
                case MobId.WATCH_HOG:
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_WatchHog), e.GetMobName(monster.getId())));
                    break;
                case MobId.MOON_BUNNY: //moon bunny
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_MoonBunny), e.GetMobName(monster.getId())));
                    break;
                case MobId.TYLUS: //tylus
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Tylus), e.GetMobName(monster.getId())));
                    break;
                case MobId.JULIET: //juliet
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Juliet), e.GetMobName(monster.getId())));
                    break;
                case MobId.ROMEO: //romeo
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Romeo), e.GetMobName(monster.getId())));
                    break;
                case MobId.GIANT_SNOWMAN_LV1_EASY:
                case MobId.GIANT_SNOWMAN_LV1_MEDIUM:
                case MobId.GIANT_SNOWMAN_LV1_HARD:
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Snownman)));
                    break;
                case MobId.DELLI: //delli
                    map.LightBlue(e => e.GetMessageByKey(nameof(ClientMessage.FriendMob_Damaged_Delli), e.GetMobName(monster.getId())));
                    break;
            }

            map.killFriendlies(monster);
        }
        else
        {
            var eim = map.getEventInstance();
            if (eim != null)
            {
                eim.friendlyDamaged(monster);
            }
        }

        monster.applyAndGetHpDamage(damage, false);
        int remainingHp = monster.getHp();
        if (remainingHp <= 0)
        {
            remainingHp = 0;
            map.removeMapObject(monster);
        }

        map.broadcastMessage(PacketCreator.MobDamageMobFriendly(monster, damage, remainingHp), monster.getPosition());
        c.sendPacket(PacketCreator.enableActions());
        return Task.CompletedTask;
    }
}