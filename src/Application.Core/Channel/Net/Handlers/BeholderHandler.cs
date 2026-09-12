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
using Application.Core.Game.Maps.AnimatedObjects;
using tools;

namespace Application.Core.Channel.Net.Handlers;




/// <summary>
/// 召唤物释放技能，目前只有 灵魂助力 能够释放技能
/// CSummoned::TryDoingHeal
/// CSummoned::TryDoingAttack
/// </summary>
public class BeholderHandler : ChannelHandlerBase
{
    //Summon Skills noobs

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        int oid = p.readInt();
        var summon = c.OnlinedCharacter.MapModel.getMapObject(oid) as Summon;
        if (summon != null)
        {
            int skillId = p.readInt();

            //var skillEffect = c.OnlinedCharacter.GetPlayerSkillEffect(skillId);
            //if (skillEffect != null)
            //{
            //    await skillEffect.applyTo(c.OnlinedCharacter);
            //}
            // COutPacket::Encode1(&v19, (v13 << 7) | 6);
            // COutPacket::Encode1(&v44, 11 & 0x7F | (v32 << 7));
            sbyte stance = p.ReadSByte();

            await c.OnlinedCharacter.BroadcastMap(
                PacketCreator.summonSkill(c.OnlinedCharacter.Id, oid, stance),
                c.OnlinedCharacter.Id);
            await c.OnlinedCharacter.SendPacket(EffectPacket.SkillAffect(summon.getSkill()));
            await c.OnlinedCharacter.BroadcastMap(PacketCreator.showBuffEffect(c.OnlinedCharacter.Id, summon.getSkill(), 2), c.OnlinedCharacter.Id);
        }
        else
        {
            c.OnlinedCharacter.clearSummons();
        }
    }
}
