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
using server;

namespace client.command.commands.gm6;

public class ShutdownCommand : Command
{
    public ShutdownCommand()
    {
        setDescription("Shut down the server.");
    }

    public override void execute(Client c, string[] paramsValue)
    {
        Character player = c.getPlayer();
        if (paramsValue.Length < 1)
        {
            player.yellowMessage("Syntax: !shutdown [<time>|NOW]");
            return;
        }

        int time = 60000;
        if (paramsValue[0].Equals("now", StringComparison.OrdinalIgnoreCase))
        {
            time = 1;
        }
        else
        {
            time *= int.Parse(paramsValue[0]);
        }

        if (time > 1)
        {
            var dur = TimeSpan.FromMilliseconds(time);


            string strTime = "";
            if (dur.Days > 0)
            {
                strTime += dur.Days + " days, ";
            }
            if (dur.Hours > 0)
            {
                strTime += dur.Hours + " hours, ";
            }
            strTime += dur.Minutes + " minutes, ";
            strTime += dur.Seconds + " seconds";

            foreach (World w in Server.getInstance().getWorlds())
            {
                foreach (Character chr in w.getPlayerStorage().getAllCharacters())
                {
                    chr.dropMessage("Server is undergoing maintenance process, and will be shutdown in " + strTime + ". Prepare yourself to quit safely in the mean time.");
                }
            }
        }

        TimerManager.getInstance().schedule(Server.getInstance().shutdown(false), time);
    }
}
