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


using client;
using client.inventory;
using client.inventory.manipulator;
using constants.id;
using constants.inventory;
using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 */
public class UseItemHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        if (!chr.isAlive())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        p.readInt();
        short slot = p.readShort();
        int itemId = p.readInt();
        var toUse = chr.getInventory(InventoryType.USE).getItem(slot);
        if (toUse != null && toUse.getQuantity() > 0 && toUse.getItemId() == itemId)
        {
            if (itemId == ItemId.ALL_CURE_POTION)
            {
                chr.dispelDebuffs();
                remove(c, slot);
                return;
            }
            else if (itemId == ItemId.EYEDROP)
            {
                chr.dispelDebuff(Disease.DARKNESS);
                remove(c, slot);
                return;
            }
            else if (itemId == ItemId.TONIC)
            {
                chr.dispelDebuff(Disease.WEAKEN);
                chr.dispelDebuff(Disease.SLOW);
                remove(c, slot);
                return;
            }
            else if (itemId == ItemId.HOLY_WATER)
            {
                chr.dispelDebuff(Disease.SEAL);
                chr.dispelDebuff(Disease.CURSE);
                remove(c, slot);
                return;
            }
            else if (ItemConstants.isTownScroll(itemId))
            {
                if (ii.GetItemEffectTrust(toUse.getItemId()).applyTo(chr))
                {
                    remove(c, slot);
                }
                return;
            }

            remove(c, slot);

            if (toUse.getItemId() != ItemId.HAPPY_BIRTHDAY)
            {
                ii.GetItemEffectTrust(toUse.getItemId()).applyTo(chr);
            }
            else
            {
                var mse = ii.GetItemEffectTrust(toUse.getItemId());
                foreach (var player in chr.getMap().getCharacters())
                {
                    mse.applyTo(player);
                }
            }
        }
    }

    private void remove(IChannelClient c, short slot)
    {
        InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, 1, false);
        c.sendPacket(PacketCreator.enableActions());
    }
}
