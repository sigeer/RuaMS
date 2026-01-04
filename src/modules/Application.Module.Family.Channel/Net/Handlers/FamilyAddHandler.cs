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


using Application.Core.Channel.Net;
using Application.Core.Client;
using Application.Core.Game.Invites;
using Application.Module.Family.Channel.Net.Packets;
using Application.Module.Family.Common;
using Application.Shared.Net;
using Microsoft.Extensions.Options;
using tools;

namespace Application.Module.Family.Channel.Net.Handlers;

/**
 * @author Jay Estrella
 * @author Ubaware
 */
public class FamilyAddHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;
    readonly IOptions<FamilyConfigs> _options;

    public FamilyAddHandler(FamilyManager familyManager, IOptions<FamilyConfigs> options)
    {
        _familyManager = familyManager;
        _options = options;
    }

    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        string toAdd = p.readString();
        var addChr = c.CurrentServer.getPlayerStorage().getCharacterByName(toAdd);
        var chr = c.OnlinedCharacter;
        if (addChr == null)
        {
            c.sendPacket(FamilyPacketCreator.sendFamilyMessage(65, 0));
            return;
        }
        if (addChr == chr)
        {
            //only possible through packet editing/client editing i think?
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        if (addChr.getMap() != chr.getMap() || addChr.isHidden() && chr.gmLevel() < addChr.gmLevel())
        {
            c.sendPacket(FamilyPacketCreator.sendFamilyMessage(69, 0));
            return;
        }
        if (addChr.getLevel() <= 10)
        {
            c.sendPacket(FamilyPacketCreator.sendFamilyMessage(77, 0));
            return;
        }

        if (Math.Abs(addChr.getLevel() - chr.getLevel()) > 20)
        {
            c.sendPacket(FamilyPacketCreator.sendFamilyMessage(72, 0));
            return;
        }

        if (_familyManager.GetFamilyByPlayerId(addChr.Id) == _familyManager.GetFamilyByPlayerId(chr.Id))
        {
            //same family
            c.sendPacket(PacketCreator.enableActions());
            return;
        }

        await _familyManager.CreateInvite(chr, toAdd);
        c.sendPacket(PacketCreator.enableActions());
    }
}
