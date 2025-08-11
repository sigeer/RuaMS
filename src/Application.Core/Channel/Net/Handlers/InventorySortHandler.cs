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


public class InventorySortHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        p.readInt();
        chr.getAutobanManager().setTimestamp(3, c.CurrentServerContainer.getCurrentTimestamp(), 4);

        if (!YamlConfig.config.server.USE_ITEM_SORT)
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        sbyte invType = p.ReadSByte();
        if (invType < 1 || invType > 5)
        {
            c.Disconnect(false, false);
            return;
        }

        List<Item> itemarray = new();
        List<ModifyInventory> mods = new();

        Inventory inventory = chr.getInventory(InventoryTypeUtils.getByType(invType));
        inventory.lockInventory();
        try
        {
            for (short i = 1; i <= inventory.getSlotLimit(); i++)
            {
                var item = inventory.getItem(i);
                if (item != null)
                {
                    // 为什么用copy？
                    itemarray.Add(item.copy());
                }
            }

            foreach (Item item in itemarray)
            {
                inventory.removeSlot(item.getPosition());
                mods.Add(new ModifyInventory(3, item));
            }

            int invTypeCriteria = (InventoryTypeUtils.getByType(invType) == InventoryType.EQUIP) ? 3 : 1;
            int sortCriteria = YamlConfig.config.server.USE_ITEM_SORT_BY_NAME ? 2 : 0;
            itemarray = InventorySorter.Sort(itemarray, sortCriteria, invTypeCriteria);

            foreach (Item item in itemarray)
            {
                inventory.addItem(item);
                mods.Add(new ModifyInventory(0, item.copy()));//to prevent crashes
            }
            itemarray.Clear();
        }
        finally
        {
            inventory.unlockInventory();
        }

        c.sendPacket(PacketCreator.modifyInventory(true, mods));
        c.sendPacket(PacketCreator.finishedSort2(invType));
        c.sendPacket(PacketCreator.enableActions());
    }
}
