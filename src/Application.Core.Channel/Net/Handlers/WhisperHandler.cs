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


using Application.Core.Game.Players;
using client.autoban;
using Microsoft.Extensions.Logging;
using tools;
using static tools.PacketCreator;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Chronos
 */
public class WhisperHandler : ChannelHandlerBase
{
    readonly ILogger<WhisperHandler> _logger;

    public WhisperHandler(ILogger<WhisperHandler> logger)
    {
        _logger = logger;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        byte request = p.readByte();
        string name = p.readString();
        var target = c.getWorldServer().getPlayerStorage().getCharacterByName(name);

        if (target == null || !target.IsOnlined)
        {
            c.sendPacket(PacketCreator.getWhisperResult(name, false));
            return;
        }

        switch (request)
        {
            case WhisperFlag.LOCATION | WhisperFlag.REQUEST:
                handleFind(c.OnlinedCharacter, target, WhisperFlag.LOCATION);
                break;
            case WhisperFlag.WHISPER | WhisperFlag.REQUEST:
                string message = p.readString();
                handleWhisper(message, c, target);
                break;
            case WhisperFlag.LOCATION_FRIEND | WhisperFlag.REQUEST:
                handleFind(c.OnlinedCharacter, target, WhisperFlag.LOCATION_FRIEND);
                break;
            default:
                _logger.LogWarning("Unknown request {Request} triggered by {CharacterName}", request, c.OnlinedCharacter.getName());
                break;
        }
    }

    private void handleFind(IPlayer user, IPlayer target, byte flag)
    {
        if (user.gmLevel() >= target.gmLevel())
        {
            if (target.getCashShop().isOpened())
            {
                user.sendPacket(PacketCreator.getFindResult(target, WhisperType.RT_CASH_SHOP, -1, flag));
            }
            else if (target.getClient().getChannel() == user.getClient().getChannel())
            {
                user.sendPacket(PacketCreator.getFindResult(target, WhisperType.RT_SAME_CHANNEL, target.getMapId(), flag));
            }
            else
            {
                user.sendPacket(PacketCreator.getFindResult(target, WhisperType.RT_DIFFERENT_CHANNEL, target.getClient().getChannel() - 1, flag));
            }
        }
        else
        {
            // not found for whisper is the same message
            user.sendPacket(PacketCreator.getWhisperResult(target.getName(), false));
        }
    }

    private void handleWhisper(string message, IChannelClient client, IPlayer target)
    {
        var user = client.OnlinedCharacter;
        if (user.getAutobanManager().getLastSpam(7) + 200 > client.CurrentServer.getCurrentTime())
        {
            return;
        }
        user.getAutobanManager().spam(7);

        if (message.Length > sbyte.MaxValue)
        {
            AutobanFactory.PACKET_EDIT.alert(user, user.getName() + " tried to packet edit with whispers.");
            _logger.LogWarning("Chr {CharacterName} tried to send text with length of {MessageLength}", user.getName(), message.Length);
            user.getClient().Disconnect(true, false);
            return;
        }

        // ChatLogger.log(user.getClient(), "Whisper To " + target.getName(), message);

        target.sendPacket(PacketCreator.getWhisperReceive(user.getName(), user.getClient().getChannel() - 1, user.isGM(), message));

        bool hidden = target.isHidden() && target.gmLevel() > user.gmLevel();
        user.sendPacket(PacketCreator.getWhisperResult(target.getName(), !hidden));
    }
}
