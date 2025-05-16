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
using net.packet;
using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author RonanLana
 */
public class UseMapleLifeHandler : ChannelHandlerBase
{
    public override void HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        long timeNow = c.CurrentServer.getCurrentTime();

        if (timeNow - player.getLastUsedCashItem() < 3000)
        {
            player.dropMessage(5, "Please wait a moment before trying again.");
            c.sendPacket(PacketCreator.sendMapleLifeError(3));
            c.sendPacket(PacketCreator.enableActions());
            return;
        }
        player.setLastUsedCashItem(timeNow);

        string name = p.readString();
        if (c.CurrentServer.CheckCharacterName(name))
        {
            c.sendPacket(PacketCreator.sendMapleLifeCharacterInfo());
        }
        else
        {
            c.sendPacket(PacketCreator.sendMapleLifeNameError());
        }
        c.sendPacket(PacketCreator.enableActions());
    }
}
