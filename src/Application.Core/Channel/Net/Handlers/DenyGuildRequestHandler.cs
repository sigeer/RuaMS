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

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Xterminator
 */
public class DenyGuildRequestHandler : ChannelHandlerBase
{

    readonly GuildManager _guildManager;
    public DenyGuildRequestHandler(GuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var value = p.readByte();
        var invitor = p.readString();
        _guildManager.AnswerInvitation(c.OnlinedCharacter, -1, false);
    }
}
