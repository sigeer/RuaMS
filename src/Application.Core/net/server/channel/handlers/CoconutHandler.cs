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
using net.packet;
using server.maps;
using tools;

namespace net.server.channel.handlers;

/**
 * @author kevintjuh93
 */
public class CoconutHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        /*CB 00 A6 00 06 01
         * A6 00 = coconut id
         * 06 01 = ?
         */
        int id = p.readShort();
        MapleMap map = c.getPlayer().getMap();
        var evt = map.getCoconut();
        if (evt == null)
        {
            return;
        }

        var nut = evt.getCoconut(id);
        if (!nut.isHittable())
        {
            return;
        }
        if (currentServerTime() < nut.getHitTime())
        {
            return;
        }
        if (nut.getHits() > 2 && Randomizer.nextDouble() < 0.4)
        {
            if (Randomizer.nextDouble() < 0.01 && evt.getStopped() > 0)
            {
                nut.setHittable(false);
                evt.stopCoconut();
                map.broadcastMessage(PacketCreator.hitCoconut(false, id, 1));
                return;
            }
            nut.setHittable(false); // for sure :)
            nut.resetHits(); // For next event (without restarts)
            if (Randomizer.nextDouble() < 0.05 && evt.getBombings() > 0)
            {
                map.broadcastMessage(PacketCreator.hitCoconut(false, id, 2));
                evt.bombCoconut();
            }
            else if (evt.getFalling() > 0)
            {
                map.broadcastMessage(PacketCreator.hitCoconut(false, id, 3));
                evt.fallCoconut();
                if (c.getPlayer().getTeam() == 0)
                {
                    evt.addMapleScore();
                    map.broadcastMessage(PacketCreator.serverNotice(5, c.getPlayer().getName() + " of Team Maple knocks down a coconut."));
                }
                else
                {
                    evt.addStoryScore();
                    map.broadcastMessage(PacketCreator.serverNotice(5, c.getPlayer().getName() + " of Team Story knocks down a coconut."));
                }
                map.broadcastMessage(PacketCreator.coconutScore(evt.getMapleScore(), evt.getStoryScore()));
            }
        }
        else
        {
            nut.hit();
            map.broadcastMessage(PacketCreator.hitCoconut(false, id, 1));
        }
    }
}
