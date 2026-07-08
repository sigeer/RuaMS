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


using Application.Core.Channel.ServerData;
using Application.Core.Game.Relation;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Net.Handlers;

public class BuddylistModifyHandler : ChannelHandlerBase
{
    readonly ILogger<BuddylistModifyHandler> _logger;
    readonly BuddyManager _buddyManager;

    public BuddylistModifyHandler(ILogger<BuddylistModifyHandler> logger, BuddyManager buddyManager)
    {
        _logger = logger;
        _buddyManager = buddyManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        int mode = p.readByte();
        var player = c.OnlinedCharacter;
        BuddyList buddylist = player.getBuddylist();
        if (mode == 1)
        {
            // add
            string addName = p.readString();
            string group = p.readString();
            if (group.Length > 16 || addName.Length < 4 || addName.Length > 13)
            {
                return; //hax.
            }

            await _buddyManager.AddBuddy(player, addName, group);
        }
        else if (mode == 2)
        {
            // accept buddy
            int otherCid = p.readInt();
            var unknown = p.available();
            await _buddyManager.AnswerInvite(player, otherCid);
        }
        else if (mode == 3)
        {
            // delete
            int otherCid = p.readInt();
            await _buddyManager.DeleteBuddy(player, otherCid);
        }
        else
        {
            _logger.LogDebug("未知的Mode{Mode}, 可读字节数{AvailableCount}", mode, p.available());
        }
    }
}
