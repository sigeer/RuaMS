/*
    This file is part of the HeavenMS MapleStory NewServer, commands OdinMS-based
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

/*
   @Author: Arthur L - Refactored command content into modules
*/


using net.server;
using server.events.gm;
using tools;

namespace client.command.commands.gm3;

public class StartEventCommand : Command
{
    public StartEventCommand()
    {
        setDescription("Start an event on current map.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        var player = c.OnlinedCharacter;
        int players = 50;
        if (paramsValue.Length > 1)
        {
            players = int.Parse(paramsValue[0]);
        }
        c.getChannelServer().setEvent(new Event(player.getMapId(), players));
        Server.getInstance().broadcastMessage(c.getWorld(), PacketCreator.earnTitleMessage(
                "[Event] An event has started on "
                        + player.getMap().getMapName()
                        + " and will allow "
                        + players
                        + " players to join. Type @joinevent to participate."));
        Server.getInstance().broadcastMessage(c.getWorld(),
                PacketCreator.serverNotice(6, "[Event] An event has started on "
                        + player.getMap().getMapName()
                        + " and will allow "
                        + players
                        + " players to join. Type @joinevent to participate."));
    }
}
