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


using Application.Core.Client;
using Application.Core.Game.TheWorld;
using client.autoban;
using client.inventory;
using client.inventory.manipulator;
using Microsoft.Extensions.Logging;
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class PetFoodHandler : ChannelHandlerBase
{
    public PetFoodHandler(IWorldChannel server, ILogger<ChannelHandlerBase> logger) : base(server, logger)
    {
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        AutobanManager abm = chr.getAutobanManager();
        if (abm.getLastSpam(2) + 500 > currentServerTime())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        abm.spam(2);
        p.readInt(); // timestamp issue detected thanks to Masterrulax
        abm.setTimestamp(1, Server.getInstance().getCurrentTimestamp(), 3);
        if (chr.getNoPets() == 0)
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        int previousFullness = 100;
        byte slot = 0;
        var pets = chr.getPets();
        for (byte i = 0; i < 3; i++)
        {
            var petObj = pets[i];
            if (petObj != null)
            {
                if (petObj.Fullness < previousFullness)
                {
                    slot = i;
                    previousFullness = petObj.Fullness;
                }
            }
        }

        var pet = chr.getPet(slot);
        if (pet == null)
        {
            return;
        }

        short pos = p.readShort();
        int itemId = p.readInt();

        if (c.tryacquireClient())
        {
            try
            {
                Inventory useInv = chr.getInventory(InventoryType.USE);
                useInv.lockInventory();
                try
                {
                    var use = useInv.getItem(pos);
                    if (use == null || (itemId / 10000) != 212 || use.getItemId() != itemId || use.getQuantity() < 1)
                    {
                        return;
                    }

                    pet.gainTamenessFullness(chr, (pet.Fullness <= 75) ? 1 : 0, 30, 1);   // 25+ "emptyness" to get +1 tameness
                    InventoryManipulator.removeFromSlot(c, InventoryType.USE, pos, 1, false);
                }
                finally
                {
                    useInv.unlockInventory();
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }
}
