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


using client.inventory;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class UseItemEffectHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        Item? toUse;
        int itemId = p.readInt();
        if (itemId == ItemId.BUMMER_EFFECT || itemId == ItemId.GOLDEN_CHICKEN_EFFECT)
        {
            toUse = c.OnlinedCharacter.getInventory(InventoryType.ETC).findById(itemId);
        }
        else
        {
            toUse = c.OnlinedCharacter.getInventory(InventoryType.CASH).findById(itemId);
        }
        if (toUse == null || toUse.getQuantity() < 1)
        {
            if (itemId != 0)
            {
                return;
            }
        }
        c.OnlinedCharacter.setItemEffect(itemId);
        c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.itemEffect(c.OnlinedCharacter.getId(), itemId), false);
    }
}
