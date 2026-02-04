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
using tools;

namespace Application.Core.Channel.Net.Handlers;

public class PetChatHandler : ChannelHandlerBase
{
    readonly ILogger<PetChatHandler> _logger;
    readonly AutoBanDataManager _autoBanManager;

    public PetChatHandler(ILogger<PetChatHandler> logger, AutoBanDataManager autoBanManager)
    {
        _logger = logger;
        _autoBanManager = autoBanManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var petId = p.readLong();
        p.readByte();
        int act = p.ReadSByte();
        sbyte pet = c.OnlinedCharacter.getPetIndex(petId);
        if ((pet < 0 || pet > 3) || (act < 0 || act > 9))
        {
            return;
        }
        string text = p.readString();
        if (text.Length > sbyte.MaxValue)
        {
            _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit with pets.");
            _logger.LogWarning("Chr {CharacterName} tried to send text with length of {text.Length}", c.OnlinedCharacter.getName(), text.Length);
            c.Disconnect(true, false);
            return;
        }
        c.OnlinedCharacter.getMap().broadcastMessage(c.OnlinedCharacter, PacketCreator.petChat(c.OnlinedCharacter.getId(), pet, act, text), true);
        // ChatLogger.log(c, "Pet", text);
    }
}
