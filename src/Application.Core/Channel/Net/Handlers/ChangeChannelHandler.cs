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


using Application.Core.Channel.Commands;
using Application.Core.Channel.ServerData;
using Application.Core.Game.Trades;
using client.autoban;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Matze
 */
public class ChangeChannelHandler : ChannelHandlerBase
{

    readonly AutoBanDataManager _autobanManager;

    public ChangeChannelHandler(AutoBanDataManager autobanManager)
    {
        _autobanManager = autobanManager;
    }

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        int channel = p.readByte() + 1;
        p.readInt();
        c.OnlinedCharacter.getAutobanManager().setTimestamp(6, c.CurrentServer.Node.getCurrentTimestamp(), 3);
        if (c.Channel == channel)
        {
            _autobanManager.Alert(AutobanFactory.GENERAL, c.OnlinedCharacter, "CCing to same channel.");
            c.Disconnect(false, false);
            return;
        }
        else if (c.OnlinedCharacter.getCashShop().isOpened() || c.OnlinedCharacter.getMiniGame() != null || c.OnlinedCharacter.VisitingShop is PlayerShop)
        {
            return;
        }

        c.ChangeChannel(channel);
    }
}