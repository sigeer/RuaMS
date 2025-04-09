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


using Application.Core.Game.Skills;
using client;
using client.inventory;
using client.inventory.manipulator;
using net.packet;
using server;
using tools;

namespace net.server.channel.handlers;

public class SkillBookHandler : AbstractPacketHandler
{
    public override void HandlePacket(InPacket p, IClient c)
    {
        if (!c.OnlinedCharacter.isAlive())
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

        var player = c.OnlinedCharacter;
        if (c.tryacquireClient())
        {
            try
            {
                Inventory inv = c.OnlinedCharacter.getInventory(InventoryType.USE);
                var toUse = inv.getItem(slot);
                if (toUse == null || toUse.getItemId() != itemId)
                {
                    return;
                }
                var skilldata = ItemInformationProvider.getInstance().getSkillStats(toUse.getItemId(), c.OnlinedCharacter.getJob().getId());
                if (skilldata == null)
                {
                    return;
                }

                var targetSkillId = skilldata.GetValueOrDefault("skillid");
                var skill2 = SkillFactory.getSkill(targetSkillId);
                if (targetSkillId == 0)
                {
                    canuse = false;
                }
                else if ((player.getSkillLevel(skill2) >= skilldata.GetValueOrDefault("reqSkillLevel") || skilldata.GetValueOrDefault("reqSkillLevel") == 0) && player.getMasterLevel(skill2) < skilldata.GetValueOrDefault("masterLevel"))
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
