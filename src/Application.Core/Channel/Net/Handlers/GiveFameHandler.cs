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
using Application.Core.Game.Players;
using client.autoban;
using Microsoft.Extensions.Logging;
using tools;
using static Application.Core.Game.Players.Player;

namespace Application.Core.Channel.Net.Handlers;

public class GiveFameHandler : ChannelHandlerBase
{
    readonly ILogger<GiveFameHandler> _logger;
    readonly AutoBanDataManager _autoBanManager;

    public GiveFameHandler(ILogger<GiveFameHandler> logger, AutoBanDataManager autoBanManager)
    {
        _logger = logger;
        _autoBanManager = autoBanManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var target = c.OnlinedCharacter.getMap().getMapObject(p.readInt()) as IPlayer;
        int mode = p.readByte();
        int famechange = 2 * mode - 1;
        var player = c.OnlinedCharacter;
        if (player.Level < 15)
        {
            player.sendPacket(PacketCreator.giveFameErrorResponse(2));
            return;
        }
        if (target == null || target.getId() == player.getId())
        {
            return;
        }
        else if (famechange != 1 && famechange != -1)
        {
            _autoBanManager.Alert(AutobanFactory.PACKET_EDIT, c.OnlinedCharacter, c.OnlinedCharacter.getName() + " tried to packet edit fame.");
            _logger.LogWarning("Chr {CharacterName} tried to fame hack with famechange {FameChange}", c.OnlinedCharacter.getName(), famechange);
            c.Disconnect(true);
            return;
        }

        var status = player.canGiveFame(target);
        if (status == FameStatus.OK)
        {
            if (target.gainFame(famechange, player, mode))
            {
                if (!player.isGM())
                {
                    player.hasGivenFame(target);
                }
            }
            else
            {
                player.message("Could not process the request, since this character currently has the minimum/maximum level of fame.");
            }
        }
        else
        {
            c.sendPacket(PacketCreator.giveFameErrorResponse(status == FameStatus.NOT_TODAY ? 3 : 4));
        }
    }
}