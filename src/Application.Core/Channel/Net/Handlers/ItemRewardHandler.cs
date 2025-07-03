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
 * @author Jay Estrella
 * @author kevintjuh93
 */
public class ItemRewardHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        byte slot = (byte)p.readShort();
        int itemId = p.readInt(); // will load from xml I don't care.

        var it = c.OnlinedCharacter.getInventory(InventoryType.USE).getItem(slot);   // null check here thanks to Thora
        if (it == null || it.getItemId() != itemId || c.OnlinedCharacter.getInventory(InventoryType.USE).countById(itemId) < 1)
        {
            return;
        }

        ItemInformationProvider ii = ItemInformationProvider.getInstance();
        var rewards = ii.getItemReward(itemId);
        foreach (var reward in rewards.Value)
        {
            if (!InventoryManipulator.checkSpace(c, reward.itemid, reward.quantity, ""))
            {
                c.sendPacket(PacketCreator.getShowInventoryFull());
                break;
            }
            if (Randomizer.nextInt(rewards.Key) < reward.prob)
            {//Is it even possible to get an item with prob 1?
                if (ItemConstants.getInventoryType(reward.itemid) == InventoryType.EQUIP)
                {
                    Item item = ii.getEquipById(reward.itemid);
                    if (reward.period != -1)
                    {
                        // TODO is this a bug, meant to be 60 * 60 * 1000?
                        item.setExpiration(c.CurrentServerContainer.getCurrentTime() + reward.period * 60 * 60 * 10);
                    }
                    InventoryManipulator.addFromDrop(c, item, false);
                }
                else
                {
                    InventoryManipulator.addById(c, reward.itemid, reward.quantity, "");
                }
                InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, false, false);
                if (reward.worldmsg != null)
                {
                    string msg = reward.worldmsg;
                    msg = msg.Replace("/name", c.OnlinedCharacter.getName());
                    msg = msg.Replace("/item", ii.getName(reward.itemid));
                    c.CurrentServerContainer.BroadcastWorldMessage(PacketCreator.serverNotice(6, msg));
                }
                break;
            }
        }
        c.sendPacket(PacketCreator.enableActions());
    }
}
