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


using Application.Core.Channel.Net;
using Microsoft.Extensions.Logging;
using tools;

public class SkillEffectHandler : ChannelHandlerBase
{
    readonly ILogger<SkillEffectHandler> _logger;

    public SkillEffectHandler(ILogger<SkillEffectHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int skillId = p.readInt();
        int level = p.ReadSByte();
        byte flags = p.readByte();
        int speed = p.ReadSByte();
        byte aids = p.readByte();//Mmmk
        switch (skillId)
        {
            case FPMage.EXPLOSION:
            case FPArchMage.BIG_BANG:
            case ILArchMage.BIG_BANG:
            case Bishop.BIG_BANG:
            case Bowmaster.HURRICANE:
            case Marksman.PIERCING_ARROW:
            case ChiefBandit.CHAKRA:
            case Brawler.CORKSCREW_BLOW:
            case Gunslinger.GRENADE:
            case Corsair.RAPID_FIRE:
            case WindArcher.HURRICANE:
            case NightWalker.POISON_BOMB:
            case ThunderBreaker.CORKSCREW_BLOW:
            case Paladin.MONSTER_MAGNET:
            case DarkKnight.MONSTER_MAGNET:
            case Hero.MONSTER_MAGNET:
            case Evan.FIRE_BREATH:
            case Evan.ICE_BREATH:
                c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.skillEffect(c.OnlinedCharacter, skillId, level, flags, speed, aids), false);
                return;
            default:
                _logger.LogWarning("Chr {CharacterName} entered SkillEffectHandler without being handled using {SkillId}", c.OnlinedCharacter, skillId);
                return;
        }
    }
}
