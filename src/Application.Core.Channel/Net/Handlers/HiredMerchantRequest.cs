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


using Application.Core.Game.Maps;
using client.inventory;
using constants.game;
using net.packet;
using server.maps;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/**
 * @author XoticStory
 */
public class HiredMerchantRequest : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        try
        {
            foreach (IMapObject mmo in chr.getMap().getMapObjectsInRange(chr.getPosition(), 23000, Arrays.asList(MapObjectType.HIRED_MERCHANT, MapObjectType.PLAYER)))
            {
                if (mmo is IPlayer mc)
                {

                    var shop = mc.getPlayerShop();
                    if (shop != null && shop.isOwner(mc))
                    {
                        chr.sendPacket(PacketCreator.getMiniRoomError(13));
                        return;
                    }
                }
                else
                {
                    chr.sendPacket(PacketCreator.getMiniRoomError(13));
                    return;
                }
            }

            Point cpos = chr.getPosition();
            var portal = chr.getMap().findClosestTeleportPortal(cpos);
            if (portal != null && portal.getPosition().distance(cpos) < 120.0)
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(10));
                return;
            }
        }
        catch (Exception e)
        {
            log.Error(e.ToString());
        }

        if (GameConstants.isFreeMarketRoom(chr.getMapId()))
        {
            if (!chr.hasMerchant())
            {
                try
                {
                    if (ItemFactory.MERCHANT.loadItems(chr.getId(), false).Count == 0 && chr.getMerchantMeso() == 0)
                    {
                        c.sendPacket(PacketCreator.hiredMerchantBox());
                    }
                    else
                    {
                        chr.sendPacket(PacketCreator.retrieveFirstMessage());
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }
            else
            {
                chr.dropMessage(1, "You already have a store open.");
            }
        }
        else
        {
            chr.dropMessage(1, "You cannot open your hired merchant here.");
        }
    }
}
