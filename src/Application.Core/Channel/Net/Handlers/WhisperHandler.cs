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
using Application.Shared.Constants.Buddy;
using client.autoban;
using Microsoft.Extensions.Logging;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Chronos
 */
public class WhisperHandler : ChannelHandlerBase
{
    readonly ILogger<WhisperHandler> _logger;
    readonly AutoBanDataManager _autoBanManager;
    readonly BuddyManager _buddyManager;

    public WhisperHandler(ILogger<WhisperHandler> logger, AutoBanDataManager autoBanManager, BuddyManager buddyManager)
    {
        _logger = logger;
        _autoBanManager = autoBanManager;
        _buddyManager = buddyManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        byte request = p.readByte();
        string name = p.readString();

        switch (request)
        {
            case WhisperFlag.LOCATION | WhisperFlag.REQUEST:
                _logger.LogDebug("何时触发");
                _buddyManager.GetLocation(c.OnlinedCharacter, name);
                break;
            case WhisperFlag.WHISPER | WhisperFlag.REQUEST:
                string message = p.readString();
                HandleSendWhisper(c, name, message);
                break;
            case WhisperFlag.LOCATION_FRIEND | WhisperFlag.REQUEST:
                HandleFriendLocation(c, name);
                break;
            default:
                _logger.LogWarning("Unknown request {Request} triggered by {CharacterName}", request, c.OnlinedCharacter.getName());
                break;
        }
    }

    void HandleFriendLocation(IChannelClient c, string targetName)
    {
        var ble = c.OnlinedCharacter.BuddyList.GetByName(targetName);
        if (ble == null)
        {
            c.sendPacket(PacketCreator.getWhisperResult(targetName, false));
            return;
        }

        var sameChannelSearch = c.CurrentServer.Players.getCharacterByName(targetName);
        if (sameChannelSearch != null)
        {
            c.sendPacket(PacketCreator.GetSameChannelFindResult(sameChannelSearch, WhisperFlag.LOCATION_FRIEND));
            return;
        }

        if (ble.ActualChannel == 0)
        {
            c.sendPacket(PacketCreator.getWhisperResult(targetName, false));
            return;
        }

        var type = ble.IsAwayWorld ? WhisperType.RT_CASH_SHOP : WhisperType.RT_DIFFERENT_CHANNEL;
        c.sendPacket(PacketCreator.GetFindResult(targetName, type, ble.ActualChannel - 1, WhisperFlag.LOCATION_FRIEND));
    }

    void HandleSendWhisper(IChannelClient c, string targetName, string message)
    {
        var user = c.OnlinedCharacter;
        if (user.getAutobanManager().getLastSpam(7) + 200 > c.CurrentServer.Node.getCurrentTime())
            return;

        user.getAutobanManager().spam(7);

        if (message.Length > sbyte.MaxValue)
        {
            _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, user, user.getName() + " tried to packet edit with whispers.");
            _logger.LogWarning("Chr {CharacterName} tried to send text with length of {MessageLength}", user.getName(), message.Length);
            user.getClient().Disconnect(true, false);
            return;
        }

        _buddyManager.SendWhisper(user, targetName, message);
    }
}