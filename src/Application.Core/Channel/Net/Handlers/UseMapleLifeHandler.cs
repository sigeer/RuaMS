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


using tools;

namespace Application.Core.Channel.Net.Handlers;

/**
 * @author RonanLana
 */
public class UseMapleLifeHandler : ChannelHandlerBase
{
    public override async Task HandlePacket(InPacket p, IChannelClient c)
    {
        var player = c.OnlinedCharacter;
        long timeNow = c.CurrentServer.Node.getCurrentTime();

        if (timeNow - player.getLastUsedCashItem() < 3000)
        {
            await player.Pink("Please wait a moment before trying again.");
            await c.SendPacket(PacketCreator.sendMapleLifeError(3));
            await c.SendPacket(PacketCreator.enableActions());
            return;
        }
        player.setLastUsedCashItem(timeNow);

        string name = p.readString();
        if (c.CurrentServer.NodeService.CheckCharacterName(name))
        {
            await c.SendPacket(PacketCreator.sendMapleLifeCharacterInfo());
        }
        else
        {
            await c.SendPacket(PacketCreator.sendMapleLifeNameError());
        }
        await c.SendPacket(PacketCreator.enableActions());
    }
}
