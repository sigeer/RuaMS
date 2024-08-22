/*
	This file is part of the OdinMS Maple Story Server
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
using net.packet;
using server;
using tools;

namespace net.server.channel.handlers;

public class SkillBookHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        if (!c.getPlayer().isAlive())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        p.readInt();
        short slot = p.readShort();
        int itemId = p.readInt();

        bool canuse;
        bool success = false;
        int skill = 0;
        int maxlevel = 0;

        Character player = c.getPlayer();
        if (c.tryacquireClient())
        {
            try
            {
                Inventory inv = c.getPlayer().getInventory(InventoryType.USE);
                var toUse = inv.getItem(slot);
                if (toUse == null || toUse.getItemId() != itemId)
                {
                    return;
                }
                Dictionary<string, int> skilldata = ItemInformationProvider.getInstance().getSkillStats(toUse.getItemId(), c.getPlayer().getJob().getId());
                if (skilldata == null)
                {
                    return;
                }
                Skill skill2 = SkillFactory.GetSkillTrust(skilldata.GetValueOrDefault("skillid"));
                if (skilldata.get("skillid") == 0)
                {
                    canuse = false;
                }
                else if ((player.getSkillLevel(skill2) >= skilldata.get("reqSkillLevel") || skilldata.get("reqSkillLevel") == 0) && player.getMasterLevel(skill2) < skilldata.get("masterLevel"))
                {
                    inv.lockInventory();
                    try
                    {
                        var used = inv.getItem(slot);
                        if (used != toUse || toUse.getQuantity() < 1)
                        {    // thanks ClouD for noticing skillbooks not being usable when stacked
                            return;
                        }

                        InventoryManipulator.removeFromSlot(c, InventoryType.USE, slot, 1, false);
                    }
                    finally
                    {
                        inv.unlockInventory();
                    }

                    canuse = true;
                    if (ItemInformationProvider.rollSuccessChance(skilldata.GetValueOrDefault("success")))
                    {
                        success = true;
                        player.changeSkillLevel(skill2, player.getSkillLevel(skill2), Math.Max(skilldata.GetValueOrDefault("masterLevel"), player.getMasterLevel(skill2)), -1);
                    }
                    else
                    {
                        success = false;
                        //player.dropMessage("The skill book lights up, but the skill winds up as if nothing happened.");
                    }
                }
                else
                {
                    canuse = false;
                }
            }
            finally
            {
                c.releaseClient();
            }

            // thanks Vcoc for noting skill book result not showing for all in area
            player.getMap().broadcastMessage(PacketCreator.skillBookResult(player, skill, maxlevel, canuse, success));
        }
    }
}
