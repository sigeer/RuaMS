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


using Application.Core.Channel.DataProviders;
using scripting.item;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Jay Estrella
 */
public class ScriptedItemHandler : ChannelHandlerBase
{
    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        p.readInt(); // trash stamp, thanks RMZero213
        short itemSlot = p.readShort(); // item slot, thanks RMZero213
        int itemId = p.readInt();

        var info = ItemInformationProvider.getInstance().GetScriptItemTemplate(itemId);
        if (info == null)
        {
            return Task.CompletedTask;
        }

        var item = c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(itemId)).getItem(itemSlot);
        if (item == null || item.getItemId() != itemId || item.getQuantity() < 1)
        {
            return Task.CompletedTask;
        }

        ItemScriptManager.getInstance().runItemScript(c, info);
        return Task.CompletedTask;
    }
}
