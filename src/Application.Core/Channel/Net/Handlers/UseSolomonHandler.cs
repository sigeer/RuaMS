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
using client.inventory;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author XoticStory
 * <p>
 * Modified by -- kevintjuh93, Ronan
 */
public class UseSolomonHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readInt();
        short slot = p.readShort();
        int itemId = p.readInt();

        var itemTemplate = ItemInformationProvider.getInstance().GetSolomenItemTemplate(itemId);
        if (itemTemplate == null)
            return;

        if (c.tryacquireClient())
        {
            try
            {
                var chr = c.OnlinedCharacter;
                Inventory inv = chr.getInventory(InventoryType.USE);
                inv.lockInventory();
                try
                {
                    var slotItem = inv.getItem(slot);
                    if (slotItem == null)
                    {
                        return;
                    }

                    if (slotItem.getItemId() != itemId || slotItem.getQuantity() <= 0 || chr.getLevel() > itemTemplate.MaxLevel)
                    {
                        return;
                    }
                    if (itemTemplate.Exp + chr.getGachaExp() > int.MaxValue)
                    {
                        return;
                    }
                    chr.addGachaExp(itemTemplate.Exp);
                    InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, 1, false);
                }
                finally
                {
                    inv.unlockInventory();
                }
            }
            finally
            {
                c.releaseClient();
            }
        }

        c.sendPacket(PacketCreator.enableActions());
    }
}
