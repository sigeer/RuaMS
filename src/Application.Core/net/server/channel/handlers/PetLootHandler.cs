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
 * @author TheRamon
 * @author Ronan
 */
public class PetLootHandler : AbstractPacketHandler
{
    public override void handlePacket(InPacket p, Client c)
    {
        Character chr = c.getPlayer();

        int petIndex = chr.getPetIndex(p.readInt());
        var pet = chr.getPet(petIndex);
        if (pet == null || !pet.isSummoned())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        p.skip(13);
        int oid = p.readInt();
        var ob = chr.getMap().getMapObject(oid);
        try
        {
            MapItem mapitem = (MapItem)ob;
            if (mapitem.getMeso() > 0)
            {
                if (!chr.isEquippedMesoMagnet())
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (chr.isEquippedPetItemIgnore())
                {
                    HashSet<int> petIgnore = chr.getExcludedItems();
                    if (petIgnore.Count > 0 && petIgnore.Contains(int.MaxValue))
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                }
            }
            else
            {
                if (!chr.isEquippedItemPouch())
                {
                    c.sendPacket(PacketCreator.enableActions());
                    return;
                }

                if (chr.isEquippedPetItemIgnore())
                {
                    HashSet<int> petIgnore = chr.getExcludedItems();
                    if (petIgnore.Count > 0 && petIgnore.Contains(mapitem.getItem().getItemId()))
                    {
                        c.sendPacket(PacketCreator.enableActions());
                        return;
                    }
                }
            }

            chr.pickupItem(ob, petIndex);
        }
        catch (Exception e)
        {
            c.sendPacket(PacketCreator.enableActions());
        }
    }
}
