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
using Application.Core.Game.Skills;
using Application.Templates.Item.Consume;
using client.inventory;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 * @author Frz
 */
public class ScrollHandler : ChannelHandlerBase
{

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        {
            await c.tryacquireClient();
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
                Skill LegendarySpirit = SkillFactory.GetSkillTrust(Beginner.LegendarySpirit);
                if (chr.getSkillLevel(LegendarySpirit) > 0 && equipSlot >= 0)
                {
                    legendarySpirit = true;
                    toScroll = chr.getInventory(InventoryType.EQUIP).getItem(equipSlot) as Equip;
                }

                if (toScroll == null)
                {
                    await announceCannotScroll(c, legendarySpirit);
                    return;
                }

                var useInventory = chr.getInventory(InventoryType.USE);
                Item? scroll = useInventory.getItem(scrollSlot);

                if (scroll == null || scroll.getQuantity() < 1 || scroll.SourceTemplate is not ScrollItemTemplate scrollTemplate)
                {
                    await announceCannotScroll(c, legendarySpirit);
                    return;
                }

                if (scrollTemplate.Recover && !ii.canUseCleanSlate(toScroll))
                {
                    // 检查白医是否可用
                    await announceCannotScroll(c, legendarySpirit);
                    return;
                }

                if (ItemConstants.RequireUpgradeSlot(scroll.getItemId()) && toScroll.getUpgradeSlots() < 1)
                {
                    // 检查强化次数
                    await announceCannotScroll(c, legendarySpirit);   // thanks onechord for noticing zero upgrade slots freezing Legendary Scroll UI
                    return;
                }

                var scrollReqs = ii.getScrollReqs(scroll.getItemId());
                if (scrollReqs.Length > 0 && !scrollReqs.Contains(toScroll.getItemId()))
                {
                    // 检查卷轴是否对装备可用（专用卷轴）
                    await announceCannotScroll(c, legendarySpirit);
                    return;
                }

                if (!scrollTemplate.RandStat && !scrollTemplate.Recover)
                {
                    // 检查卷轴是否对装备可用（部位）
                    if (!canScroll(scroll.getItemId(), toScroll.getItemId()))
                    {
                        await announceCannotScroll(c, legendarySpirit);
                        return;
                    }
                }

                Item? wscroll = null;
                if (whiteScroll)
                {
                    if (!ItemConstants.RequireUpgradeSlot(scroll.getItemId()))
                    {
                        // 不消耗强化次数的卷轴，不需要消耗祝福
                        wscroll = null;
                    }
                    else
                    {
                        wscroll = useInventory.findById(ItemId.WHITE_SCROLL);
                    }
                }

                var scrollSuccess = ii.scrollEquipWithId(toScroll, scroll.getItemId(), wscroll != null, 0, chr.isGM());

                await c.OnlinedCharacter.Bag.TryRemoveFromSlot(InventoryType.USE, scroll.getPosition(), 1, false);
                if (wscroll != null)
                    await c.OnlinedCharacter.Bag.TryRemoveFromSlot(InventoryType.USE, wscroll.getPosition(), 1, false);

                if (scrollSuccess == Equip.ScrollResult.CURSE)
                {
                    if (!ItemId.isWeddingRing(toScroll.getItemId()))
                    {
                        if (equipSlot < 0)
                        {
                            var inv = chr.getInventory(InventoryType.EQUIPPED);
                            var (removeRes, _) = await inv.removeItem(toScroll.getPosition());
                            if (removeRes != null)
                                await chr.SyncClientInventory(removeRes);
                        }
                        else
                        {
                            var inv = chr.getInventory(InventoryType.EQUIP);
                            var (removeRes, _) = await inv.removeItem(toScroll.getPosition());
                            if (removeRes != null)
                                await chr.SyncClientInventory(removeRes);
                        }
                    }
                    else
                    {
                        scrollSuccess = Equip.ScrollResult.FAIL;

                        await chr.forceUpdateItem(toScroll);
                    }
                }
                else
                {
                    await chr.forceUpdateItem(toScroll);
                }

                await chr.BroadcastMap(PacketCreator.getScrollEffect(chr.getId(), scrollSuccess, legendarySpirit, wscroll != null));
            }
            finally
            {
                c.releaseClient();
            }
        }
    }

    private static async Task announceCannotScroll(IChannelClient c, bool legendarySpirit)
    {
        if (legendarySpirit)
        {
            await c.SendPacket(PacketCreator.getScrollEffect(c.OnlinedCharacter.getId(), Equip.ScrollResult.FAIL, false, false));
        }
        else
        {
            await c.SendPacket(PacketCreator.getInventoryFull());
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
