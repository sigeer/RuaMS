/*
	This file is part of the OdinMS Maple Story NewServer
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
                       Matthias Butz <matze@odinms.de>
                       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation. You may not use, modify
    or distribute this program under any other version of the
    GNU Affero General Public License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/



using Application.Core.Channel.Services;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Flav
 */
public class EnterCashShopHandler : ChannelHandlerBase
{
    readonly ILogger<EnterCashShopHandler> _logger;
    readonly DataService _dataService;
    readonly ItemService _itemService;

    public EnterCashShopHandler(ILogger<EnterCashShopHandler> logger, DataService dataService, ItemService itemService)
    {
        _logger = logger;
        _dataService = dataService;
        _itemService = itemService;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        try
        {
            var mc = c.OnlinedCharacter;

            if (mc.cannotEnterCashShop())
            {
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getEventInstance() != null)
            {
                c.sendPacket(PacketCreator.serverNotice(5, "Entering Cash Shop or MTS are disabled when registered on an event."));
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (MiniDungeonInfo.isDungeonMap(mc.getMapId()))
            {
                c.sendPacket(PacketCreator.serverNotice(5, "Changing channels or entering Cash Shop or MTS are disabled when inside a Mini-Dungeon."));
                c.sendPacket(PacketCreator.enableActions());
                return;
            }

            if (mc.getCashShop().isOpened())
            {
                return;
            }

            mc.closePlayerInteractions();
            mc.closePartySearchInteractions();

            mc.unregisterChairBuff();
            _dataService.SaveBuff(mc);
            mc.setAwayFromChannelWorld();

            mc.cancelAllBuffs(true);
            mc.cancelAllDebuffs();
            mc.forfeitExpirableQuests();

            mc.StopPlayerTask();

            c.sendPacket(PacketCreator.openCashShop(c, false));
            c.sendPacket(PacketCreator.showCashInventory(c));
            c.sendPacket(PacketCreator.showGifts(_itemService.LoadPlayerGifts(mc)));
            c.sendPacket(PacketCreator.showWishList(mc, false));
            c.sendPacket(PacketCreator.showCash(mc));

            c.CurrentServer.removePlayer(mc);
            mc.getMap().removePlayer(mc);
            mc.getCashShop().open(true);
            mc.saveCharToDB(setChannel: -1);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
}
