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
using Application.Shared.KeyMaps;
using client.keybind;
using constants.game;

namespace Application.Core.Channel.Net.Handlers;

public class KeymapChangeHandler : ChannelHandlerBase
{
    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        if (p.available() >= 8)
        {
            int mode = p.readInt();
            if (mode == 0)
            {
                int numChanges = p.readInt();
                for (int i = 0; i < numChanges; i++)
                {
                    int key = p.readInt();
                    int type = p.readByte();
                    int action = p.readInt();

                    if (type == KeyBindingType.Skill)
                    {
                        var skill = SkillFactory.getSkill(action);
                        bool isBanndedSkill;
                        if (skill != null)
                        {
                            isBanndedSkill = GameConstants.bannedBindSkills(skill.getId());
                            // 不能绑定的技能（一些玩法中临时获得的技能）
                            // 非管理员能拥有的管理员技能
                            // 非当前职业能拥有的技能
                            if (isBanndedSkill
                                || (!c.OnlinedCharacter.isGM() && GameConstants.isGMSkills(skill.getId()))
                                || (!GameConstants.isInJobTree(skill.getId(), c.OnlinedCharacter.getJob().getId()) && !c.OnlinedCharacter.isGM()))
                            { //for those skills are are "technically" in the beginner tab, like bamboo rain in Dojo or skills you find in PYPQ
                                //AutobanFactory.PACKET_EDIT.alert(c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit keymapping.");
                                //FilePrinter.printError(FilePrinter.EXPLOITS + c.OnlinedCharacter.getName() + ".txt", c.OnlinedCharacter.getName() + " tried to use skill " + skill.getId());
                                //c.disconnect(true, false);
                                //return;

                                continue;   // fk that
                            }
                            /* if (c.OnlinedCharacter.getSkillLevel(skill) < 1) {    HOW WOULD A SKILL EVEN BE AVAILABLE TO KEYBINDING
                                    continue;                                   IF THERE IS NOT EVEN A SINGLE POINT USED INTO IT??
                            } */
                        }
                    }

                    c.OnlinedCharacter.changeKeybinding(key, new KeyBinding(type, action));
                }
            }
            else if (mode == 1)
            {
                // Auto HP Potion
                int itemID = p.readInt();
                if (itemID != 0 && c.OnlinedCharacter.getInventory(InventoryType.USE).findById(itemID) == null)
                {
                    await c.Disconnect(false, false); // Don't let them send a packet with a use item they dont have.
                    return;
                }
                c.OnlinedCharacter.changeKeybinding((int)KeyCode.VirtualAutoPotionHP, new KeyBinding(KeyBindingType.AutoPotion, itemID));
            }
            else if (mode == 2)
            {
                // Auto MP Potion
                int itemID = p.readInt();
                if (itemID != 0 && c.OnlinedCharacter.getInventory(InventoryType.USE).findById(itemID) == null)
                {
                    await c.Disconnect(false, false); // Don't let them send a packet with a use item they dont have.
                    return;
                }
                c.OnlinedCharacter.changeKeybinding((int)KeyCode.VirtualAutoPotionMP, new KeyBinding(KeyBindingType.AutoPotion, itemID));
            }
        }
    }
}
