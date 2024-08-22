/*
    This file is part of the HeavenMS MapleStory Server, commands OdinMS-based
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
using net.server.channel;

namespace client.command.commands.gm3;

public class OnlineTwoCommand : Command
{
    public OnlineTwoCommand()
    {
        setDescription("Show all online players.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        int total = 0;
        foreach (Channel ch in Server.getInstance().getChannelsFromWorld(player.getWorld()))
        {
            int size = ch.getPlayerStorage().getAllCharacters().Count;
            total += size;
            string s = "(Channel " + ch.getId() + " Online: " + size + ") : ";
            if (ch.getPlayerStorage().getAllCharacters().Count < 50)
            {
                foreach (Character chr in ch.getPlayerStorage().getAllCharacters())
                {
                    s += Character.makeMapleReadable(chr.getName()) + ", ";
                }
                player.dropMessage(6, s.Substring(0, s.Length - 2));
            }
        }

        //player.dropMessage(6, "There are a total of " + total + " players online.");
        player.showHint("Players online: #e#r" + total + "#k#n.", 300);
    }
}
