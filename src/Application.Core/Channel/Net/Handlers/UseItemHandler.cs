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
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 */
public class UseItemHandler : ChannelHandlerBase
{
    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        if (!chr.isAlive())
        {
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        p.readInt();
        short slot = p.readShort();
        int itemId = p.readInt();
        var toUse = chr.getInventory(InventoryType.USE).getItem(slot);
        if (toUse != null && toUse.getQuantity() > 0 && toUse.getItemId() == itemId)
        {
            var itemEffect = ii.GetItemEffectTrust(toUse.getItemId());
            if (toUse.getItemId() != ItemId.HAPPY_BIRTHDAY)
            {
                if (await itemEffect.applyTo(chr))
                {
                    await remove(c, slot);
                }
            }
            else
            {
                foreach (var player in chr.getMap().getAllPlayers())
                {
                    await itemEffect.applyTo(player);
                }
                await remove(c, slot);
            }
        }
    }

    private async Task remove(IChannelClient c, short slot)
    {
        await InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, 1, false);
        await c.SendPacket(PacketCreator.enableActions());
    }
}
