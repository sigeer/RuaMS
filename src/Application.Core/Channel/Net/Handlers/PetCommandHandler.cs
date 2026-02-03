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


using Application.Core.Game.Items;
using client.inventory;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class PetCommandHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        var petId = p.readLong();
        sbyte petIndex = chr.getPetIndex(petId);
        Pet? pet;
        if (petIndex == -1)
        {
            return;
        }
        else
        {
            pet = chr.getPet(petIndex);
        }
        if (pet == null)
            return;

        p.readByte();
        byte command = p.readByte();
        var petCommand = pet.SourceTemplate.InterActsDict.GetValueOrDefault(command);
        if (petCommand == null)
        {
            return;
        }

        if (Randomizer.nextInt(100) < petCommand.Prob)
        {
            pet.gainTamenessFullness(chr, petCommand.Inc, 0, command);
            chr.getMap().broadcastMessage(PacketCreator.commandResponse(chr.getId(), petIndex, false, command, false));
        }
        else
        {
            chr.getMap().broadcastMessage(PacketCreator.commandResponse(chr.getId(), petIndex, true, command, false));
        }
    }
}
