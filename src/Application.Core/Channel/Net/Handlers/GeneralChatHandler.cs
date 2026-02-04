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
using Application.Core.Game.Commands;
using client.autoban;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class GeneralChatHandler : ChannelHandlerBase
{
    readonly ILogger<GeneralChatHandler> _logger;
    readonly CommandExecutor commandExecutor;
    readonly AutoBanDataManager _autobanManager;

    public GeneralChatHandler(ILogger<GeneralChatHandler> logger, CommandExecutor commandExecutor, AutoBanDataManager autoBanManager)
    {
        _logger = logger;
        this.commandExecutor = commandExecutor;
        _autobanManager = autoBanManager;
    }

    private const char COMMAND_HEADING = '!';
    bool isCommand(string content)
    {
        char heading = content.ElementAt(0);
        return heading == COMMAND_HEADING;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        string s = p.readString();
        var chr = c.OnlinedCharacter;
        if (chr.getAutobanManager().getLastSpam(7) + 200 > c.CurrentServer.Node.getCurrentTime())
        {
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        if (s.Length > sbyte.MaxValue && !chr.isGM())
        {
            _autobanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit in General Chat.");
            _logger.LogWarning("Chr {CharacterName} tried to send text with length of {StringLength}", c.OnlinedCharacter.getName(), s.Length);
            c.Disconnect(true);
            return;
        }
        char heading = s.ElementAt(0);
        if (isCommand(s))
        {
            commandExecutor.handle(c, s);
        }
        else if (heading != '/')
        {
            int show = p.readByte();
            if (chr.getMap().isMuted() && !chr.isGM())
            {
                chr.dropMessage(5, "The map you are in is currently muted. Please try again later.");
                return;
            }

            if (!chr.isHidden())
            {
                chr.getMap().broadcastMessage(PacketCreator.getChatText(chr.getId(), s, chr.getWhiteChat(), show));
                // ChatLogger.log(c, "General", s);
            }
            else
            {
                chr.getMap().broadcastGMMessage(PacketCreator.getChatText(chr.getId(), s, chr.getWhiteChat(), show));
                // ChatLogger.log(c, "GM General", s);
            }

            chr.getAutobanManager().spam(7);
        }
    }
}