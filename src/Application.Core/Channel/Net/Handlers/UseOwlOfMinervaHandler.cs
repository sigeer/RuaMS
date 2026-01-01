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



/// <summary>
/// @author Ronan
/// 用途？
/// </summary>
public class UseOwlOfMinervaHandler : ChannelHandlerBase
{
    public override Task HandlePacket(InPacket p, IChannelClient c)
    {
        var owlSearched = c.CurrentServer.Service.GetOwlSearchedItems();
        List<int> owlLeaderboards;

        if (owlSearched.Length < 5)
        {
            owlLeaderboards = [.. ItemId.getOwlItems()];
        }
        else
        {
            // descending order
            owlLeaderboards = owlSearched.OrderByDescending(x => x.Count).Select(x => x.ItemId).ToList();
        }

        c.sendPacket(PacketCreator.getOwlOpen(owlLeaderboards));
        return Task.CompletedTask;
    }
}