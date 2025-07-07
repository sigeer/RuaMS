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


using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Module.Duey.Common;
using Application.Shared.Net;
using Application.Utility.Configs;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Module.Duey.Channel.Net.Handlers;

public class DueyHandler : ChannelHandlerBase
{
    readonly DueyManager _dueyManager;
    readonly ILogger<DueyHandler> _logger;

    public DueyHandler(DueyManager dueyManager, ILogger<DueyHandler> logger)
    {
        _dueyManager = dueyManager;
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        byte operation = p.readByte();
        if (operation == DueyProcessorActions.TOSERVER_RECV_ITEM.getCode())
        {
            // on click 'O' Button, thanks inhyuk
            _dueyManager.SendTalk(c, false);
        }
        else if (operation == DueyProcessorActions.TOSERVER_SEND_ITEM.getCode())
        {
            sbyte inventId = p.ReadSByte();
            short itemPos = p.readShort();
            short amount = p.readShort();
            int mesos = p.readInt();
            string recipient = p.readString();
            bool quick = p.readByte() != 0;
            string? message = quick ? p.readString() : null;

            _dueyManager.DueySendItemFromInventory(c, inventId, itemPos, amount, mesos, message, recipient, quick);
        }
        else if (operation == DueyProcessorActions.TOSERVER_REMOVE_PACKAGE.getCode())
        {
            int packageid = p.readInt();

            _dueyManager.RemoveDueyPackage(c.OnlinedCharacter, packageid);
        }
        else if (operation == DueyProcessorActions.TOSERVER_CLAIM_PACKAGE.getCode())
        {
            int packageid = p.readInt();

            _dueyManager.TakePackage(c.OnlinedCharacter, packageid);
        }
        else
        {
            _logger.LogDebug("DueyHandler, Action={Action}, ReadableCount={ReadableCount}", operation, p.available());
        }
    }
}
