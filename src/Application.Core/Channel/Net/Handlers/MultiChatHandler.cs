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
using client.autoban;
using Microsoft.Extensions.Logging;

namespace Application.Core.Channel.Net.Handlers;

public class MultiChatHandler : ChannelHandlerBase
{
    readonly GuildManager _guildManager;
    readonly ILogger<MultiChatHandler> _logger;
    readonly AutoBanDataManager _autoBanManager;
    readonly BuddyManager _buddyManager;

    public MultiChatHandler(ILogger<MultiChatHandler> logger, GuildManager guildManager, AutoBanDataManager autoBanManager, BuddyManager buddyManager)
    {
        _logger = logger;
        _guildManager = guildManager;
        _autoBanManager = autoBanManager;
        _buddyManager = buddyManager;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        if (player.getAutobanManager().getLastSpam(7) + 200 > c.CurrentServerContainer.getCurrentTime())
        {
            return;
        }

        int type = p.readByte(); // 0 for buddys, 1 for partys
        int numRecipients = p.readByte();
        int[] recipients = new int[numRecipients];
        for (int i = 0; i < numRecipients; i++)
        {
            recipients[i] = p.readInt();
        }
        string chattext = p.readString();
        if (chattext.Length > sbyte.MaxValue && !player.isGM())
        {
            await _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit chats.");
            _logger.LogWarning("Chr {CharacterName} tried to send text with length of {ChatContent}", c.OnlinedCharacter.getName(), chattext.Length);
            await c.Disconnect(true, false);
            return;
        }

        if (type == 1 && player.Party <= 0)
            return;

        if (type == 2 && player.GetGuild() == null)
            return;

        if (type == 3 && player.GetAlliance() == null)
            return;

        player.getAutobanManager().spam(7);
        await c.CurrentServerContainer.Transport.SendMultiChatAsync(type, player.Name, chattext, recipients);
    }
}
