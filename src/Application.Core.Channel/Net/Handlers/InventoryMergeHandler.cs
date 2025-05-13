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


using Application.Utility.Configs;
using client.inventory;
using client.inventory.manipulator;
using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class InventoryMergeHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        p.readInt();
        chr.getAutobanManager().setTimestamp(2, c.CurrentServer.getCurrentTimestamp(), 4);

        if (!YamlConfig.config.server.USE_ITEM_SORT)
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        sbyte invType = p.ReadSByte();
        if (invType < 1 || invType > 5)
        {
            c.Disconnect(false);
            return;
        }

        InventoryType inventoryType = InventoryTypeUtils.getByType(invType);
        Inventory inventory = c.OnlinedCharacter.getInventory(inventoryType);
        inventory.lockInventory();
        try
        {
            //------------------- RonanLana's SLOT MERGER -----------------

            ItemInformationProvider ii = ItemInformationProvider.getInstance();
            Item? srcItem, dstItem;

            for (short dst = 1; dst <= inventory.getSlotLimit(); dst++)
            {
                dstItem = inventory.getItem(dst);
                if (dstItem == null)
                {
                    continue;
                }

                for (short src = (short)(dst + 1); src <= inventory.getSlotLimit(); src++)
                {
                    srcItem = inventory.getItem(src);
                    if (srcItem == null)
                    {
                        continue;
                    }

                    if (dstItem.getItemId() != srcItem.getItemId())
                    {
                        continue;
                    }
                    if (dstItem.getQuantity() == ii.getSlotMax(c, inventory.getItem(dst).getItemId()))
                    {
                        break;
                    }

                    InventoryManipulator.move(c, inventoryType, src, dst);
                }
            }

            //------------------------------------------------------------

            inventory = c.OnlinedCharacter.getInventory(inventoryType);
            bool sorted = false;

            while (!sorted)
            {
                short freeSlot = inventory.getNextFreeSlot();

                if (freeSlot != -1)
                {
                    short itemSlot = -1;
                    for (short i = (short)(freeSlot + 1); i <= inventory.getSlotLimit(); i = (short)(i + 1))
                    {
                        if (inventory.getItem(i) != null)
                        {
                            itemSlot = i;
                            break;
                        }
                    }
                    if (itemSlot > 0)
                    {
                        InventoryManipulator.move(c, inventoryType, itemSlot, freeSlot);
                    }
                    else
                    {
                        sorted = true;
                    }
                }
                else
                {
                    sorted = true;
                }
            }
        }
        finally
        {
            inventory.unlockInventory();
        }

        c.sendPacket(PacketCreator.finishedSort(inventoryType.getType()));
        c.sendPacket(PacketCreator.enableActions());
    }
}