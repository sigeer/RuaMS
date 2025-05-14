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
using net.packet;
using System.Drawing;

namespace Application.Core.Channel.Net.Handlers;



/**
 * @author Matze
 * @author Ronan
 */
public class ItemPickupHandler : ChannelHandlerBase
{
    readonly ILogger<ItemPickupHandler> _logger;

    public ItemPickupHandler(ILogger<ItemPickupHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readInt(); //Timestamp
        p.readByte();
        p.readPos(); //cpos
        int oid = p.readInt();
        var chr = c.OnlinedCharacter;
        var ob = chr.getMap().getMapObject(oid);
        if (ob == null)
        {
            return;
        }

        Point charPos = chr.getPosition();
        Point obPos = ob.getPosition();
        if (Math.Abs(charPos.X - obPos.X) > 800 || Math.Abs(charPos.Y - obPos.Y) > 600)
        {
            _logger.LogWarning("Chr {CharacterName} tried to pick up an item too far away. Mapid: {MapId}, player pos: {PlayerPosition}, object pos: {ObjectPosition}",
                    c.OnlinedCharacter.getName(), chr.getMapId(), charPos, obPos);
            return;
        }

        chr.pickupItem(ob);
    }
}
