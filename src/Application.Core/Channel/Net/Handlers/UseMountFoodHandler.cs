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


using client.inventory;
using client.inventory.manipulator;
using constants.game;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author PurpleMadness
 * @author Ronan
 */
public class UseMountFoodHandler : ChannelHandlerBase
{
    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        p.skip(4);
        short pos = p.readShort();
        int itemid = p.readInt();

        var chr = c.OnlinedCharacter;
        var mount = chr.getMount();
        Inventory useInv = chr.getInventory(InventoryType.USE);

        if (c.tryacquireClient())
        {
            try
            {
                bool? mountLevelup = null;

                useInv.lockInventory();
                try
                {
                    var item = useInv.getItem(pos);
                    if (item != null && item.getItemId() == itemid && mount != null)
                    {
                        int curTiredness = mount.getTiredness();
                        int healedTiredness = Math.Min(curTiredness, 30);

                        float healedFactor = (float)healedTiredness / 30;
                        mount.setTiredness(curTiredness - healedTiredness);

                        if (healedFactor > 0.0f)
                        {
                            mount.setExp(mount.getExp() + (int)Math.Ceiling(healedFactor * (2 * mount.getLevel() + 6)));
                            int level = mount.getLevel();
                            bool levelup = mount.getExp() >= ExpTable.getMountExpNeededForLevel(level) && level < 31;
                            if (levelup)
                            {
                                mount.setLevel(level + 1);
                            }

                            mountLevelup = levelup;
                        }

                        InventoryManipulator.removeById(c, InventoryType.USE, itemid, 1, true, false);
                    }
                }
                finally
                {
                    useInv.unlockInventory();
                }

                if (mountLevelup != null)
                {
                    chr.getMap().broadcastMessage(PacketCreator.updateMount(chr.getId(), mount, mountLevelup.Value));
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
        return Task.CompletedTask;
    }
}