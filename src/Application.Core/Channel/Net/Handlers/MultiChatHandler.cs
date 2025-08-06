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
using client.autoban;
using Microsoft.Extensions.Logging;
using tools;

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

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        if (player.getAutobanManager().getLastSpam(7) + 200 > c.CurrentServerContainer.getCurrentTimestamp())
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
            _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit chats.");
            _logger.LogWarning("Chr {CharacterName} tried to send text with length of {ChatContent}", c.OnlinedCharacter.getName(), chattext.Length);
            c.Disconnect(true, false);
            return;
        }
        var world = c.getWorldServer();
        if (type == 0)
        {
            _buddyManager.BuddyChat(player, recipients, chattext);
            // ChatLogger.log(c, "Buddy", chattext);
        }
        else if (type == 1 && player.getParty() != null)
        {
            c.CurrentServer.Service.SendTeamChat(player.Name, chattext);
            // ChatLogger.log(c, "Party", chattext);
        }
        else if (type == 2 && player.GuildModel != null)
        {
            _guildManager.SendGuildChat(c.OnlinedCharacter, chattext);
            // ChatLogger.log(c, "Guild", chattext);
        }
        else if (type == 3 && player.getGuild() != null)
        {
            if (player.AllianceModel != null)
            {
                _guildManager.SendAllianceChat(c.OnlinedCharacter, chattext);
                // ChatLogger.log(c, "Ally", chattext);
            }
        }
        player.getAutobanManager().spam(7);
    }
}
