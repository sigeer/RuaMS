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



using Application.Core.Game.Maps.Specials;
using Application.Resources.Messages;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author kevintjuh93
 */
public class CoconutHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        /*CB 00 A6 00 06 01
         * A6 00 = coconut id
         * 06 01 = ?
         */
        int id = p.readShort();
        var map = c.OnlinedCharacter.getMap();
        if (map is not ICoconutMap coconutMap || coconutMap.Coconut == null)
        {
            return;
        }

        var nut = coconutMap.Coconut.getCoconut(id);
        if (!nut.isHittable())
        {
            return;
        }
        if (c.CurrentServerContainer.getCurrentTime() < nut.getHitTime())
        {
            return;
        }
        if (nut.getHits() > 2 && Randomizer.nextDouble() < 0.4)
        {
            if (Randomizer.nextDouble() < 0.01 && coconutMap.Coconut.getStopped() > 0)
            {
                nut.setHittable(false);
                coconutMap.Coconut.stopCoconut();
                map.broadcastMessage(PacketCreator.hitCoconut(false, id, 1));
                return;
            }
            nut.setHittable(false); // for sure :)
            nut.resetHits(); // For next event (without restarts)
            if (Randomizer.nextDouble() < 0.05 && coconutMap.Coconut.getBombings() > 0)
            {
                map.broadcastMessage(PacketCreator.hitCoconut(false, id, 2));
                coconutMap.Coconut.bombCoconut();
            }
            else if (coconutMap.Coconut.getFalling() > 0)
            {
                map.broadcastMessage(PacketCreator.hitCoconut(false, id, 3));
                coconutMap.Coconut.fallCoconut();
                if (c.OnlinedCharacter.getTeam() == 0)
                {
                    coconutMap.Coconut.addMapleScore();
                    map.Pink(nameof(ClientMessage.Coconut_Effect_Maple), c.OnlinedCharacter.Name);
                }
                else
                {
                    coconutMap.Coconut.addStoryScore();
                    map.Pink(nameof(ClientMessage.Coconut_Effect_Story), c.OnlinedCharacter.Name);
                }
                map.broadcastMessage(PacketCreator.coconutScore(coconutMap.Coconut.getMapleScore(), coconutMap.Coconut.getStoryScore()));
            }
        }
        else
        {
            nut.hit();
            map.broadcastMessage(PacketCreator.hitCoconut(false, id, 1));
        }
    }
}
