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


using Application.Core.Managers;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author Ronan
 */
public class DenyAllianceRequestHandler : ChannelHandlerBase
{

    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        p.readByte();
        string inviterName = p.readString();
        string guildName = p.readString();

        var chr = c.getWorldServer().getPlayerStorage().getCharacterByName(inviterName);
        if (chr != null)
        {
            var alliance = chr.getAlliance();
            if (alliance != null)
            {
                AllianceManager.answerInvitation(c.OnlinedCharacter.getId(), guildName, alliance.getId(), false);
            }
        }
    }
}