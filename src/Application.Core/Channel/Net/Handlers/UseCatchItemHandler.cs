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
using client.autoban;
using client.inventory.manipulator;
using tools;

namespace Application.Core.Channel.Net.Handlers;
/**
 * @author kevintjuh93
 */
public class UseCatchItemHandler : ChannelHandlerBase
{
    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;
        AutobanManager abm = chr.getAutobanManager();
        p.readInt();
        abm.setTimestamp(5, c.CurrentServerContainer.getCurrentTimestamp(), 4);
        p.readShort();
        int itemId = p.readInt();

        var invType = ItemConstants.getInventoryType(itemId);
        if (chr.getInventory(invType).countById(itemId) <= 0)
        {
            return Task.CompletedTask;
        }

        int objectId = p.readInt();
        var mob = chr.getMap().getMonsterByOid(objectId);
        if (mob == null)
        {
            return Task.CompletedTask;
        }

        var itemTemplate = ItemInformationProvider.getInstance().GetCatchMobItemTemplate(itemId);
        if (itemTemplate == null)
        {
            return Task.CompletedTask;
        }

        if (itemTemplate.UseDelay > 0 && abm.getLastSpam(10) > 0)
        {
            if (abm.getLastSpam(10) + itemTemplate.UseDelay >= c.CurrentServerContainer.getCurrentTime())
            {
                if (!string.IsNullOrEmpty(itemTemplate.DelayMsg))
                {
                    chr.Pink(itemTemplate.DelayMsg);
                }
                if (itemId == ItemId.ARPQ_ELEMENT_ROCK)
                {
                    c.sendPacket(PacketCreator.catchMessage(1));
                }
                return Task.CompletedTask;
            }
        }

        abm.spam(10);

        if (itemTemplate.MobHP > 0 && mob.getHp() < ((mob.getMaxHp() / 100.0) * itemTemplate.MobHP))
        {
            c.sendPacket(PacketCreator.catchMessage(0));
            return Task.CompletedTask;
        }

        if (itemTemplate.Create > 0 && !chr.canHold(itemTemplate.Create, 1))
        {
            chr.Pink("Make a ETC slot available before using this item.");
            return Task.CompletedTask;
        }

        if (itemTemplate.BridleProp == 0 || Random.Shared.Next(100) < itemTemplate.BridleProp)
        {
            chr.getMap().broadcastMessage(PacketCreator.catchMonster(objectId, itemId, true));
            mob.getMap().killMonster(mob, null, false);
            InventoryManipulator.removeById(c, invType, itemId, 1, true, true);

            if (itemTemplate.Create > 0)
            {
                InventoryManipulator.addById(c, itemTemplate.Create, 1);

                if (itemTemplate.Create == ItemId.ARPQ_SPIRIT_JEWEL)
                    chr.updateAriantScore();
            }
        }
        else
        {
            chr.getMap().broadcastMessage(PacketCreator.catchMonster(objectId, itemId, false));
        }

        c.sendPacket(PacketCreator.enableActions());
        return Task.CompletedTask;
    }
}
