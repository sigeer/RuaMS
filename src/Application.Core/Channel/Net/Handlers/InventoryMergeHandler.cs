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


using Application.Core.Client.inventory;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class InventoryMergeHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        p.readInt();
        chr.getAutobanManager().setTimestamp(2, c.CurrentServer.Node.getCurrentTimestamp(), 4);

        if (!YamlConfig.config.server.USE_ITEM_SORT)
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }

        sbyte invType = p.ReadSByte();
        if (invType < 1 || invType > 5)
        {
            await c.Disconnect(false);
            return;
        }

        InventoryType inventoryType = InventoryTypeUtils.getByType(invType);
        var inventory = c.OnlinedCharacter.GetInventory(inventoryType);
        if (inventory == null)
        {
            await c.Disconnect(false);
            return;
        }

        var ops = new BagInventorySorter(inventory).Merge();
        await chr.SyncClientInventory(ops, true);

        await c.SendPacket(PacketCreator.finishedSort(invType));
        await c.SendPacket(PacketCreator.enableActions());
    }
}