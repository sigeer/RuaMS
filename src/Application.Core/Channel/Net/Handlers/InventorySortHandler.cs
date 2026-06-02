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
using client.inventory;
using tools;

namespace Application.Core.Channel.Net.Handlers;


public class InventorySortHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        var unknown = p.readInt();
        chr.getAutobanManager().setTimestamp(3, c.CurrentServer.Node.getCurrentTimestamp(), 4);

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

        var inventory = c.OnlinedCharacter.GetInventory(InventoryTypeUtils.getByType(invType));
        if (inventory == null)
        {
            c.Disconnect(false);
            return;
        }

        var ops = new BagInventorySorter(inventory).Sort();
        chr.SyncClientInventory(ops, true);

        c.sendPacket(PacketCreator.finishedSort2(invType));
        c.sendPacket(PacketCreator.enableActions());
    }
}
