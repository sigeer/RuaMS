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
using Application.Core.Game.Skills;
using client.inventory;
using client.inventory.manipulator;
using constants.id;
using constants.inventory;
using Microsoft.Extensions.Logging;
using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 * @author Frz
 */
public class ScrollHandler : ChannelHandlerBase
{
    
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        if (c.tryacquireClient())
        {
            try
            {
                p.readInt(); // whatever...
                short scrollSlot = p.readShort();
                short equipSlot = p.readShort();
                byte ws = (byte)p.readShort();
                bool whiteScroll = false; // white scroll being used?
                bool legendarySpirit = false; // legendary spirit skill
                if ((ws & 2) == 2)
                {
                    whiteScroll = true;
                }

                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                var chr = c.OnlinedCharacter;
                var toScroll = chr.getInventory(InventoryType.EQUIPPED).getItem(equipSlot) as Equip;
                Skill LegendarySpirit = SkillFactory.GetSkillTrust(1003);
                if (chr.getSkillLevel(LegendarySpirit) > 0 && equipSlot >= 0)
                {
                    legendarySpirit = true;
                    toScroll = chr.getInventory(InventoryType.EQUIP).getItem(equipSlot) as Equip;
                }

                if (toScroll == null)
                {
                    announceCannotScroll(c, legendarySpirit);
                    return;
                }

                byte oldLevel = toScroll.getLevel();
                byte oldSlots = toScroll.getUpgradeSlots();
                Inventory useInventory = chr.getInventory(InventoryType.USE);
                Item? scroll = useInventory.getItem(scrollSlot);

                if (scroll == null)
                {
                    announceCannotScroll(c, legendarySpirit);
                    return;
                }

                Item? wscroll = null;

                if (ItemConstants.isCleanSlate(scroll.getItemId()) && !ii.canUseCleanSlate(toScroll))
                {
                    announceCannotScroll(c, legendarySpirit);
                    return;
                }
                else if (!ItemConstants.isModifierScroll(scroll.getItemId()) && toScroll.getUpgradeSlots() < 1)
                {
                    announceCannotScroll(c, legendarySpirit);   // thanks onechord for noticing zero upgrade slots freezing Legendary Scroll UI
                    return;
                }

                List<int> scrollReqs = ii.getScrollReqs(scroll.getItemId());
                if (scrollReqs.Count > 0 && !scrollReqs.Contains(toScroll.getItemId()))
                {
                    announceCannotScroll(c, legendarySpirit);
                    return;
                }
                if (whiteScroll)
                {
                    wscroll = useInventory.findById(ItemId.WHITE_SCROLL);
                    if (wscroll == null)
                    {
                        whiteScroll = false;
                    }
                }

                if (!ItemConstants.isChaosScroll(scroll.getItemId()) && !ItemConstants.isCleanSlate(scroll.getItemId()))
                {
                    if (!canScroll(scroll.getItemId(), toScroll.getItemId()))
                    {
                        announceCannotScroll(c, legendarySpirit);
                        return;
                    }
                }

                var scrolled = (Equip?)ii.scrollEquipWithId(toScroll, scroll.getItemId(), whiteScroll, 0, chr.isGM());
                var scrollSuccess = Equip.ScrollResult.FAIL; // fail
                if (scrolled == null)
                {
                    scrollSuccess = Equip.ScrollResult.CURSE;
                }
                else if (scrolled.getLevel() > oldLevel 
                    || (ItemConstants.isCleanSlate(scroll.getItemId()) && scrolled.getUpgradeSlots() == oldSlots + 1) 
                    || ItemConstants.isFlagModifier(scroll.getItemId(), scrolled.getFlag()))
                {
                    scrollSuccess = Equip.ScrollResult.SUCCESS;
                }

                useInventory.lockInventory();
                try
                {
                    if (scroll.getQuantity() < 1)
                    {
                        announceCannotScroll(c, legendarySpirit);
                        return;
                    }

                    if (whiteScroll && !ItemConstants.isCleanSlate(scroll.getItemId()))
                    {
                        if (wscroll!.getQuantity() < 1)
                        {
                            announceCannotScroll(c, legendarySpirit);
                            return;
                        }

                        c.OnlinedCharacter.Bag.RemoveFromSlot(InventoryType.USE, wscroll.getPosition(), 1, false);
                    }

                    c.OnlinedCharacter.Bag.RemoveFromSlot(InventoryType.USE, scroll.getPosition(), 1, false);
                }
                finally
                {
                    useInventory.unlockInventory();
                }

                List<ModifyInventory> mods = new();
                if (scrollSuccess == Equip.ScrollResult.CURSE)
                {
                    if (!ItemId.isWeddingRing(toScroll.getItemId()))
                    {
                        mods.Add(new ModifyInventory(3, toScroll));
                        if (equipSlot < 0)
                        {
                            Inventory inv = chr.getInventory(InventoryType.EQUIPPED);

                            inv.lockInventory();
                            try
                            {
                                chr.unequippedItem(toScroll);
                                inv.removeItem(toScroll.getPosition());
                            }
                            finally
                            {
                                inv.unlockInventory();
                            }
                        }
                        else
                        {
                            Inventory inv = chr.getInventory(InventoryType.EQUIP);

                            inv.lockInventory();
                            try
                            {
                                inv.removeItem(toScroll.getPosition());
                            }
                            finally
                            {
                                inv.unlockInventory();
                            }
                        }
                    }
                    else
                    {
                        scrolled = toScroll;
                        scrollSuccess = Equip.ScrollResult.FAIL;

                        mods.Add(new ModifyInventory(3, scrolled));
                        mods.Add(new ModifyInventory(0, scrolled));
                    }
                }
                else
                {
                    mods.Add(new ModifyInventory(3, scrolled));
                    mods.Add(new ModifyInventory(0, scrolled));
                }
                c.sendPacket(PacketCreator.modifyInventory(true, mods));
                chr.getMap().broadcastMessage(PacketCreator.getScrollEffect(chr.getId(), scrollSuccess, legendarySpirit, whiteScroll));
                if (equipSlot < 0 && (scrollSuccess == Equip.ScrollResult.SUCCESS || scrollSuccess == Equip.ScrollResult.CURSE))
                {
                    chr.equipChanged();
                }
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    private static void announceCannotScroll(IChannelClient c, bool legendarySpirit)
    {
        if (legendarySpirit)
        {
            c.sendPacket(PacketCreator.getScrollEffect(c.OnlinedCharacter.getId(), Equip.ScrollResult.FAIL, false, false));
        }
        else
        {
            c.sendPacket(PacketCreator.getInventoryFull());
        }
    }

    private static bool canScroll(int scrollid, int itemid)
    {
        int sid = scrollid / 100;

        switch (sid)
        {
            case 20492: //scroll for accessory (pendant, belt, ring)
                return canScroll(ItemId.RING_STR_100_SCROLL, itemid) || canScroll(ItemId.DRAGON_STONE_SCROLL, itemid) ||
                        canScroll(ItemId.BELT_STR_100_SCROLL, itemid);

            default:
                return (scrollid / 100) % 100 == (itemid / 10000) % 100;
        }
    }
}
