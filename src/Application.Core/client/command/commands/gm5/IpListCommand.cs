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


using constants.game;
using net.server;

namespace client.command.commands.gm5;



/**
 * @author Mist
 * @author Blood (Tochi)
 * @author Ronan
 */
public class IpListCommand : Command
{
    public IpListCommand()
    {
        setDescription("Show IP of all players.");
    }

    public override void execute(IClient c, string[] paramsValue)
    {
        string str = "Player-IP relation:";

        foreach (var w in Server.getInstance().getWorlds())
        {
            var chars = w.getPlayerStorage().GetAllOnlinedPlayers();

            if (chars.Count > 0)
            {
                str += "\r\n" + GameConstants.WORLD_NAMES[w.getId()] + "\r\n";

                foreach (var chr in chars)
                {
                    str += "  " + chr.getName() + " - " + chr.getClient().getRemoteAddress() + "\r\n";
                }
            }
        }

        c.getAbstractPlayerInteraction().npcTalk(22000, str);
    }

}