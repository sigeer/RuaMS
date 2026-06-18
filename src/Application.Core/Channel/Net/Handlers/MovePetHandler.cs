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


using Microsoft.Extensions.Logging;
using server.movement;
using tools;
using tools.exceptions;

namespace Application.Core.Channel.Net.Handlers;



public class MovePetHandler : AbstractMovementPacketHandler
{
    public MovePetHandler(ILogger<AbstractMovementPacketHandler> logger) : base(logger)
    {
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var petId = p.readLong();
        var pos = p.readPos();

        var mapPet = c.OnlinedCharacter.GetPetById(petId);
        if (mapPet == null)
        {
            return;
        }
        var serverStartPos = mapPet.getPosition();
        List<LifeMovementFragment> res;

        try
        {
            res = parseMovement(p);
            mapPet.updatePosition(res);
        }
        catch (EmptyMovementException e)
        {
            _logger.LogError(e.ToString());
            return;
        }


        await mapPet.BroadcastMovement(PacketCreator.movePet(c.OnlinedCharacter.Id, mapPet.Index, pos, res), serverStartPos);
        await mapPet.MapModel.MoveMapObject(mapPet);
    }
}
