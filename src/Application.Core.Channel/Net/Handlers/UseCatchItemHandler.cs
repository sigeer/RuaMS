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


using client.autoban;
using client.inventory.manipulator;
using net.packet;
using server;
using tools;

namespace Application.Core.Channel.Net.Handlers;
/**
 * @author kevintjuh93
 */
public class UseCatchItemHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        AutobanManager abm = chr.getAutobanManager();
        p.readInt();
        abm.setTimestamp(5, c.CurrentServer.getCurrentTimestamp(), 4);
        p.readShort();
        int itemId = p.readInt();
        int monsterid = p.readInt();

        var mob = chr.getMap().getMonsterByOid(monsterid);
        if (chr.getInventory(ItemConstants.getInventoryType(itemId)).countById(itemId) <= 0)
        {
            return;
        }
        if (mob == null)
        {
            return;
        }
        switch (itemId)
        {
            case ItemId.PHEROMONE_PERFUME:
                if (mob.getId() == MobId.TAMABLE_HOG)
                {
                    chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                    mob.getMap().killMonster(mob, null, false);
                    InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                    InventoryManipulator.addById(c, ItemId.HOG, 1, "", -1);
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case ItemId.POUCH:
                if (mob.getId() == MobId.GHOST)
                {
                    if ((abm.getLastSpam(10) + 1000) < c.CurrentServer.getCurrentTime())
                    {
                        if (mob.getHp() < ((mob.getMaxHp() / 10) * 4))
                        {
                            chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                            mob.getMap().killMonster(mob, null, false);
                            InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                            InventoryManipulator.addById(c, ItemId.GHOST_SACK, 1, "", -1);
                        }
                        else
                        {
                            abm.spam(10);
                            c.sendPacket(PacketCreator.catchMessage(0));
                        }
                    }
                    c.sendPacket(PacketCreator.enableActions());
                }
                break;
            case ItemId.ARPQ_ELEMENT_ROCK:
                if (mob.getId() == MobId.ARPQ_SCORPION)
                {
                    if ((abm.getLastSpam(10) + 800) < c.CurrentServer.getCurrentTime())
                    {
                        if (mob.getHp() < ((mob.getMaxHp() / 10) * 4))
                        {
                            if (chr.canHold(ItemId.ARPQ_SPIRIT_JEWEL, 1))
                            {
                                if (Randomizer.nextDouble() < 0.5)
                                { // 50% chance
                                    chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                                    mob.getMap().killMonster(mob, null, false);
                                    InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                                    InventoryManipulator.addById(c, ItemId.ARPQ_SPIRIT_JEWEL, 1, "", -1);
                                    chr.updateAriantScore();
                                }
                                else
                                {
                                    chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 0));
                                }
                            }
                            else
                            {
                                chr.dropMessage(5, "Make a ETC slot available before using this item.");
                            }

                            abm.spam(10);
                        }
                        else
                        {
                            c.sendPacket(PacketCreator.catchMessage(0));
                        }
                    }
                    c.sendPacket(PacketCreator.enableActions());
                }
                break;
            case ItemId.MAGIC_CANE:
                if (mob.getId() == MobId.LOST_RUDOLPH)
                {
                    if (mob.getHp() < ((mob.getMaxHp() / 10) * 4))
                    {
                        chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                        mob.getMap().killMonster(mob, null, false);
                        InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                        InventoryManipulator.addById(c, ItemId.TAMED_RUDOLPH, 1, "", -1);
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.catchMessage(0));
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case ItemId.TRANSPARENT_MARBLE_1:
                if (mob.getId() == MobId.KING_SLIME_DOJO)
                {
                    if (mob.getHp() < ((mob.getMaxHp() / 10) * 3))
                    {
                        chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                        mob.getMap().killMonster(mob, null, false);
                        InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                        InventoryManipulator.addById(c, ItemId.MONSTER_MARBLE_1, 1, "", -1);
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.catchMessage(0));
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case ItemId.TRANSPARENT_MARBLE_2:
                if (mob.getId() == MobId.FAUST_DOJO)
                {
                    if (mob.getHp() < ((mob.getMaxHp() / 10) * 3))
                    {
                        chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                        mob.getMap().killMonster(mob, null, false);
                        InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                        InventoryManipulator.addById(c, ItemId.MONSTER_MARBLE_2, 1, "", -1);
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.catchMessage(0));
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case ItemId.TRANSPARENT_MARBLE_3:
                if (mob.getId() == MobId.MUSHMOM_DOJO)
                {
                    if (mob.getHp() < ((mob.getMaxHp() / 10) * 3))
                    {
                        chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                        mob.getMap().killMonster(mob, null, false);
                        InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                        InventoryManipulator.addById(c, ItemId.MONSTER_MARBLE_3, 1, "", -1);
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.catchMessage(0));
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case ItemId.EPQ_PURIFICATION_MARBLE:
                if (mob.getId() == MobId.POISON_FLOWER)
                {
                    if (mob.getHp() < ((mob.getMaxHp() / 10) * 4))
                    {
                        chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                        mob.getMap().killMonster(mob, null, false);
                        InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                        InventoryManipulator.addById(c, ItemId.EPQ_MONSTER_MARBLE, 1, "", -1);
                    }
                    else
                    {
                        c.sendPacket(PacketCreator.catchMessage(0));
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
            case ItemId.FISH_NET:
                if (mob.getId() == MobId.P_JUNIOR)
                {
                    if ((abm.getLastSpam(10) + 3000) < c.CurrentServer.getCurrentTime())
                    {
                        abm.spam(10);
                        chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                        mob.getMap().killMonster(mob, null, false);
                        InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                        InventoryManipulator.addById(c, ItemId.FISH_NET_WITH_A_CATCH, 1, "", -1);
                    }
                    else
                    {
                        chr.message("You cannot use the Fishing Net yet.");
                    }
                    c.sendPacket(PacketCreator.enableActions());
                }
                break;
            default:
                // proper Fish catch, thanks to Dragohe4rt

                ItemInformationProvider ii = ItemInformationProvider.getInstance();
                int itemGanho = ii.getCreateItem(itemId);
                int mobItem = ii.getMobItem(itemId);

                if (itemGanho != 0 && mobItem == mob.getId())
                {
                    int timeCatch = ii.getUseDelay(itemId);
                    int mobHp = ii.getMobHP(itemId);

                    if (timeCatch != 0 && (abm.getLastSpam(10) + timeCatch) < c.CurrentServer.getCurrentTime())
                    {
                        if (mobHp != 0 && mob.getHp() < ((mob.getMaxHp() / 100) * mobHp))
                        {
                            chr.getMap().broadcastMessage(PacketCreator.catchMonster(monsterid, itemId, 1));
                            mob.getMap().killMonster(mob, null, false);
                            InventoryManipulator.removeById(c, InventoryType.USE, itemId, 1, true, true);
                            InventoryManipulator.addById(c, itemGanho, 1, "", -1);
                        }
                        else if (mob.getId() != MobId.P_JUNIOR)
                        {
                            if (mobHp != 0)
                            {
                                abm.spam(10);
                                c.sendPacket(PacketCreator.catchMessage(0));
                            }
                        }
                        else
                        {
                            chr.message("You cannot use the Fishing Net yet.");
                        }
                    }
                }
                c.sendPacket(PacketCreator.enableActions());
                break;
                // Console.WriteLine("UseCatchItemHandler: \r\n" + slea.ToString());
        }
    }
}
