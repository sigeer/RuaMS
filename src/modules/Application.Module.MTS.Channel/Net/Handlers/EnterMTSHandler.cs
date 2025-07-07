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
using Application.Core.Client;
using Application.Module.MTS.Channel.Net;
using Application.Shared.MapObjects;
using Application.Shared.Net;
using Microsoft.Extensions.Logging;
using server;
using tools;

namespace Application.Module.MTS.Channel.Net.Handlers;

public class EnterMTSHandler : Application.Core.Channel.Net.Handlers.EnterMTSHandler
{
    readonly ILogger<EnterMTSHandler> _logger;
    readonly DataService _dataService;
    readonly MTSManager _manager;

    public EnterMTSHandler(ILogger<EnterMTSHandler> logger, DataService dataService, MTSManager manager)
    {
        _logger = logger;
        _dataService = dataService;
        _manager = manager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var chr = c.OnlinedCharacter;

        if (chr.getEventInstance() != null)
        {
            c.sendPacket(PacketCreator.serverNotice(5, "Entering Cash Shop or MTS are disabled when registered on an event."));
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (MiniDungeonInfo.isDungeonMap(chr.getMapId()))
        {
            c.sendPacket(PacketCreator.serverNotice(5, "Changing channels or entering Cash Shop or MTS are disabled when inside a Mini-Dungeon."));
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (FieldLimit.CANNOTMIGRATE.check(chr.getMap().getFieldLimit()))
        {
            chr.dropMessage(1, "You can't do it here in this map.");
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (!chr.isAlive())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        if (chr.getLevel() < 10)
        {
            c.sendPacket(PacketCreator.blockedMessage2(5));
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        chr.closePlayerInteractions();

        chr.unregisterChairBuff();
        _dataService.SaveBuff(chr);
        chr.setAwayFromChannelWorld();
        chr.notifyMapTransferToPartner(-1);
        chr.removeIncomingInvites();

        chr.StopPlayerTask();

        chr.saveCharToDB(setChannel: -1);

        c.CurrentServer.removePlayer(chr);
        chr.getMap().removePlayer(c.OnlinedCharacter);
        try
        {
            c.sendPacket(MTSPacketCreator.OpenMTS(c));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
        chr.getCashShop().open(true);// xD
        c.enableCSActions();
        c.sendPacket(MTSPacketCreator.MTSWantedListingOver(0, 0));

        c.sendPacket(MTSPacketCreator.showMTSCash(c.OnlinedCharacter));

        _manager.Query(c.OnlinedCharacter, 1, 0, 0);
    }
}
