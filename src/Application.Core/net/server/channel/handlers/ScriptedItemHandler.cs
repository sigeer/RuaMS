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


using constants.inventory;
using net.packet;
using scripting.item;
using server;

namespace net.server.channel.handlers;

/**
 * @author Jay Estrella
 */
public class ScriptedItemHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        p.readInt(); // trash stamp, thanks RMZero213
        short itemSlot = p.readShort(); // item slot, thanks RMZero213
        int itemId = p.readInt();

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var info = ii.getScriptedItemInfo(itemId);
        if (info == null)
        {
            return;
        }

        var item = c.OnlinedCharacter.getInventory(ItemConstants.getInventoryType(itemId)).getItem(itemSlot);
        if (item == null || item.getItemId() != itemId || item.getQuantity() < 1)
        {
            return;
        }

        ItemScriptManager ism = ItemScriptManager.getInstance();
        ism.runItemScript(c, info);
    }
}
