/*
    This file is part of the HeavenMS MapleStory NewServer
    Copyleft (L) 2016 - 2019 RonanLana

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
using Application.Module.Family.Channel;
using Application.Module.Family.Channel.Net.Packets;
using Application.Shared.Net;

namespace Application.Module.Family.Channel.Net.Handlers;

/**
 * @author Ubaware
 */
public class OpenFamilyPedigreeHandler : ChannelHandlerBase
{
    readonly FamilyManager _familyManager;

    public OpenFamilyPedigreeHandler(FamilyManager familyManager)
    {
        _familyManager = familyManager;
    }

    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        var target = c.CurrentServer.Players.getCharacterByName(p.readString());

        if (target != null)
        {
            var family = _familyManager.GetFamilyByPlayerId(target.Id);
            if (family != null)
            {
                c.sendPacket(FamilyPacketCreator.showPedigree(family, target.Id));
            }
        }
        return Task.CompletedTask;
    }
}

