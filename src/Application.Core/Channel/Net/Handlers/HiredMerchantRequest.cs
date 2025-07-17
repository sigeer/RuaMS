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


using Application.Core.Channel.Services;
using Application.Core.Game.Maps;
using Application.Core.Game.Players;
using Application.Utility.Compatible;
using Application.Utility.Extensions;
using client.inventory;
using constants.game;
using Microsoft.Extensions.Logging;
using System.Drawing;
using tools;

namespace Application.Core.Channel.Net.Handlers;


/**
 * @author XoticStory
 */
public class HiredMerchantRequest : ChannelHandlerBase
{
    readonly ILogger<HiredMerchantRequest> _logger;
    readonly PlayerShopService _service;
    public HiredMerchantRequest(ILogger<HiredMerchantRequest> logger, PlayerShopService service)
    {
        _logger = logger;
        _service = service;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        try
        {
            if (chr.getMap().getMapObjectsInRange(chr.getPosition(), 23000, Arrays.asList(MapObjectType.HIRED_MERCHANT, MapObjectType.SHOP)).Count > 0)
            {
                chr.sendPacket(PacketCreator.getMiniRoomError(13));
                return;
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
            _logger.LogError(e.ToString());
        }

        if (GameConstants.isFreeMarketRoom(chr.getMapId()))
        {
            var status = _service.CanHiredMerchant(chr);
            if (status == PlayerHiredMerchantStatus.Unavailable_Opening)
            {
                chr.dropMessage(1, "You already have a store open.");
                return;
            }

            if (status == PlayerHiredMerchantStatus.Unavailable_NeedRetrieve)
            {
                chr.sendPacket(PacketCreator.retrieveFirstMessage());
                return;
            }

            chr.sendPacket(PacketCreator.hiredMerchantBox());
        }
        else
        {
            chr.dropMessage(1, "You cannot open your hired merchant here.");
        }
    }
}
