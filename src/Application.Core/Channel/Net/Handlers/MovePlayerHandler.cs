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
using tools;
using tools.exceptions;

namespace Application.Core.Channel.Net.Handlers;

public class MovePlayerHandler : AbstractMovementPacketHandler
{
    public MovePlayerHandler(ILogger<AbstractMovementPacketHandler> logger) : base(logger)
    {
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        p.skip(5);
        try
        {
            var clientStartPos = p.readPos();
            var serverStartPos = c.OnlinedCharacter.getPosition();
            // thanks Sa for noticing empty movement sequences crashing players
            int movementDataStart = p.getPosition();
            updatePosition(p, c.OnlinedCharacter, 0);
            int movementDataLength = p.getPosition() - movementDataStart; //how many bytes were read by updatePosition
            p.seek(movementDataStart);

            await c.OnlinedCharacter.getMap().movePlayer(c.OnlinedCharacter, c.OnlinedCharacter.getPosition());
            await c.OnlinedCharacter.BroadcastMovement(PacketCreator.movePlayer(c.OnlinedCharacter.getId(), clientStartPos, p, movementDataLength), serverStartPos);
        }
        catch (EmptyMovementException e)
        {
            _logger.LogError(e.ToString());
        }
    }
}
